using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.Core.LobbyManager;

var builder = WebApplication.CreateBuilder(args);
Startup startup = new();
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app, app.Environment);


app.Run();
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        // Add WebSocket manager service
        services.AddSingleton<WebSocketConnectionManager>();
        services.AddTransient<WebSocketHandler>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        // Enable WebSocket support
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
            ReceiveBufferSize = 4 * 1024
        };
        app.UseWebSockets(webSocketOptions);

        // Map WebSocket endpoints
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                    await handler.HandleAsync(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });
        });
    }
}

public class WebSocketHandler
{
    private readonly WebSocketConnectionManager _connectionManager;
    private readonly ILogger<WebSocketHandler> _logger;

    public WebSocketHandler(
        WebSocketConnectionManager connectionManager,
        ILogger<WebSocketHandler> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        var connectionId = _connectionManager.AddConnection(webSocket);
        _logger.LogInformation($"WebSocket connection established: {connectionId}");

        try
        {
            await ReceiveAsync(connectionId, webSocket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in WebSocket connection {connectionId}");
        }
        finally
        {
            _connectionManager.RemoveConnection(connectionId);
            _logger.LogInformation($"WebSocket connection closed: {connectionId}");
        }
    }

    private async Task ReceiveAsync(string connectionId, WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    await HandleTextMessage(connectionId, message);
                    break;

                case WebSocketMessageType.Binary:
                    await HandleBinaryMessage(connectionId, buffer.Array.Take(result.Count).ToArray());
                    break;

                case WebSocketMessageType.Close:
                    await webSocket.CloseAsync(
                        result.CloseStatus.Value,
                        result.CloseStatusDescription,
                        CancellationToken.None
                    );
                    break;
            }
        }
    }

    private async Task HandleTextMessage(string connectionId, string message)
    {
        _logger.LogInformation($"Received from {connectionId}: {message}");

        try
        {
            var json = JsonDocument.Parse(message);
            var type = json.RootElement.GetProperty("type").GetString();

            switch (type)
            {
                case "broadcast":
                    await BroadcastMessage(connectionId, message);
                    break;
                case "private":
                    await SendPrivateMessage(connectionId, json.RootElement);
                    break;
                case "create_lobby":
                    int lobbyCode = LobbyManager.Instance.CreateLobby(connectionId);
                    await _connectionManager.SendAsync(connectionId, $"{{\"type\":\"lobby_created\", \"lobby_code\": {lobbyCode.ToString()}}}");
                    break;
                case "join_lobby":
                    string response = LobbyManager.Instance.JoinLobby(connectionId, message);
                    await _connectionManager.SendAsync(connectionId, $"{{\"type\":\"joined_lobby\"}}");
                    break;
                default:
                    await Echo(connectionId, message);
                    break;
            }
        }
        catch (JsonException)
        {
            await Echo(connectionId, message);
        }
    }

    private async Task HandleBinaryMessage(string connectionId, byte[] data)
    {
        _logger.LogInformation($"Received binary from {connectionId}: {data.Length} bytes");
        // Process binary data
    }

    private async Task Echo(string connectionId, string message)
    {
        await _connectionManager.SendAsync(connectionId, $"Echo: {message}");
    }

    private async Task BroadcastMessage(string senderId, string message)
    {
        await _connectionManager.BroadcastAsync(message, senderId);
    }

    private async Task SendPrivateMessage(string senderId, JsonElement json)
    {
        var targetId = json.GetProperty("target").GetString();
        var content = json.GetProperty("content").GetString();

        await _connectionManager.SendAsync(targetId, content);
    }
}

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public string AddConnection(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);
        return connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public WebSocket GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    public IEnumerable<string> GetAllConnectionIds()
    {
        return _connections.Keys;
    }

    public async Task SendAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var webSocket))
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }
    }

    public async Task BroadcastAsync(string message, string excludeConnectionId = null)
    {
        var tasks = new List<Task>();

        foreach (var pair in _connections)
        {
            if (pair.Key != excludeConnectionId && pair.Value.State == WebSocketState.Open)
            {
                tasks.Add(SendAsync(pair.Key, message));
            }
        }

        await Task.WhenAll(tasks);
    }
}


