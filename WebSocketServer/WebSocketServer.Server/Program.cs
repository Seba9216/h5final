using Microsoft.EntityFrameworkCore;
using System;
using WebSocketServer;
using WebSocketServer.Core.context;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DuckingContext>
 (options => options.UseMySQL("server=localhost;database=testdb;user=user;password=password"));

var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);
app.Run();