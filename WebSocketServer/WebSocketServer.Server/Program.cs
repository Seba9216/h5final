using Microsoft.EntityFrameworkCore;
using WebSocketServer;
using WebSocketServer.Core.context;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DuckingContext>
 (options => options.UseSqlServer("",
    b => b.MigrationsAssembly("WebSocketServer.Server")));
var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);
app.Run();