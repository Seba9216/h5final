using Microsoft.EntityFrameworkCore;
using WebSocketServer;
using WebSocketServer.Core.context;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DuckingContext>
 (options => options.UseMySQL("server=host.docker.internal;database=testdb;user=user;password=password;",
    b => b.MigrationsAssembly("WebSocketServer.Server")));
var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    
    await next();
});

startup.Configure(app, app.Environment);
app.Run();