using System.Net.WebSockets;

namespace WebSocketServer.Core.Connections;

public interface IWebSocketConnectionManager
{
    string AddConnection(WebSocket webSocket);
    void RemoveConnection(string connectionId);
    WebSocket? GetConnection(string connectionId);
    IEnumerable<string> GetAllConnectionIds();
    Task SendAsync(string connectionId, string message);
    Task BroadcastAsync(string message, string? excludeConnectionId = null);
}