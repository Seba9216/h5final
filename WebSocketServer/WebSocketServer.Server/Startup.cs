using WebSocketServer.Core.Auth;
using WebSocketServer.Core.Connections;
using WebSocketServer.Core.Handlers;
using WebSocketServer.Core.LobbyManager;
using WebSocketServer.Server.Repositorys;
using WebSocketServer.Server.Services;

namespace WebSocketServer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(Startup).Assembly);
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.WithOrigins("*")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        services.AddSingleton<ILobbyManager, LobbyManager>();
        services.AddTransient<IMessageHandler, MessageHandler>();
        services.AddTransient<IWebSocketHandler, WebSocketHandler>();
        services.AddScoped<IDuckingLoginService, DuckingLoginService>();
        services.AddScoped<IDuckingLoginRepository, DuckingLoginRepository>();
        services.AddScoped<IGameHistoryRepository, GameHistoryRepository>();
        services.AddScoped<IGameHistoryService, GameHistoryService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors("AllowAll");

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
            ReceiveBufferSize = 4 * 1024
        };
        
        app.UseWebSockets(webSocketOptions);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers().RequireCors("AllowAll");
            
            endpoints.MapGet("/health", async context =>
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("healthy");
            });

            endpoints.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    var allowedOrigins = new[] { "*" };

                    if (!allowedOrigins.Contains(origin))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = context.RequestServices.GetRequiredService<IWebSocketHandler>();
                    await handler.HandleAsync(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });
        });
    }
}