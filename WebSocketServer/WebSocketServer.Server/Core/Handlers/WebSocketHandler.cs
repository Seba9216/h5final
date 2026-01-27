using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketServer.Core.Connections;
using WebSocketServer.Core.LobbyManager;

namespace WebSocketServer.Core.Handlers;

public class WebSocketHandler : IWebSocketHandler
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly IMessageHandler _messageHandler;
    private readonly ILobbyManager _lobbyManager;
    private readonly ILogger<WebSocketHandler> _logger;

    public WebSocketHandler(
        IWebSocketConnectionManager connectionManager,
        IMessageHandler messageHandler,
        ILobbyManager lobbyManager,
        ILogger<WebSocketHandler> logger)
    {
        _connectionManager = connectionManager;
        _messageHandler = messageHandler;
        _lobbyManager = lobbyManager;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        var connectionId = _connectionManager.AddConnection(webSocket);
        _logger.LogInformation("WebSocket connection established: {ConnectionId}", connectionId);

        try
        {
            await ReceiveMessagesAsync(connectionId, webSocket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket connection {ConnectionId}", connectionId);
        }
        finally
        {
            // Clean up: remove from lobbies and connection manager
            _lobbyManager.RemoveConnectionFromAllLobbies(connectionId);
            _connectionManager.RemoveConnection(connectionId);
            _logger.LogInformation("WebSocket connection closed: {ConnectionId}", connectionId);
        }
    }

    private async Task ReceiveMessagesAsync(string connectionId, WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    await HandleTextMessageAsync(connectionId, buffer, result.Count);
                    break;

                case WebSocketMessageType.Close:
                    await webSocket.CloseAsync(
                        result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        result.CloseStatusDescription,
                        CancellationToken.None
                    );
                    break;

                case WebSocketMessageType.Binary:
                    _logger.LogWarning("Binary messages not supported for connection {ConnectionId}", connectionId);
                    break;
            }
        }
    }

    private async Task HandleTextMessageAsync(string connectionId, ArraySegment<byte> buffer, int count)
    {
        var message = Encoding.UTF8.GetString(buffer.Array!, 0, count);
        _logger.LogInformation("Received from {ConnectionId}: {Message}", connectionId, message);

        try
        {
            using var json = JsonDocument.Parse(message);
            var messageType = json.RootElement.GetProperty("type").GetString();

            if (string.IsNullOrWhiteSpace(messageType))
            {
                _logger.LogWarning("Message from {ConnectionId} has no type", connectionId);
                await _messageHandler.HandleMessageAsync(connectionId, "echo", message);
                return;
            }

            await _messageHandler.HandleMessageAsync(connectionId, messageType, message);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON from {ConnectionId}, echoing back", connectionId);
            await _messageHandler.HandleMessageAsync(connectionId, "echo", message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Message from {ConnectionId} missing 'type' property", connectionId);
            await _messageHandler.HandleMessageAsync(connectionId, "echo", message);
        }
    }
}