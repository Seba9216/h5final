using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer.Core.Connections;

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
    }

    public string AddConnection(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);
        _logger.LogDebug("Added connection: {ConnectionId}", connectionId);
        return connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out _))
        {
            _logger.LogDebug("Removed connection: {ConnectionId}", connectionId);
        }
    }

    public WebSocket? GetConnection(string connectionId)
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
        if (!_connections.TryGetValue(connectionId, out var webSocket))
        {
            _logger.LogWarning("Connection not found: {ConnectionId}", connectionId);
            return;
        }

        if (webSocket.State != WebSocketState.Open)
        {
            _logger.LogWarning("WebSocket not open for connection: {ConnectionId}", connectionId);
            return;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None
            );
            
            _logger.LogDebug("Sent message to {ConnectionId}: {Message}", connectionId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to {ConnectionId}", connectionId);
        }
    }

    public async Task BroadcastAsync(string message, string? excludeConnectionId = null)
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
        _logger.LogDebug("Broadcast message to {Count} connections", tasks.Count);
    }
}