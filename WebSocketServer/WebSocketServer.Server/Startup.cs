using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebSocketServer.Core.Connections;
using WebSocketServer.Core.Handlers;
using WebSocketServer.Core.LobbyManager;

namespace WebSocketServer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        services.AddSingleton<ILobbyManager, LobbyManager>();
        services.AddTransient<IMessageHandler, MessageHandler>();
        services.AddTransient<IWebSocketHandler, WebSocketHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
            ReceiveBufferSize = 4 * 1024
        };
        
        app.UseWebSockets(webSocketOptions);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            
            endpoints.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
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