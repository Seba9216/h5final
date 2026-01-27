using System.Text.Json;
using WebSocketServer.Core.Connections;
using WebSocketServer.Core.LobbyManager;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Core.Handlers;

public class MessageHandler : IMessageHandler
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILobbyManager _lobbyManager;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(
        IWebSocketConnectionManager connectionManager,
        ILobbyManager lobbyManager,
        ILogger<MessageHandler> logger)
    {
        _connectionManager = connectionManager;
        _lobbyManager = lobbyManager;
        _logger = logger;
    }

    public async Task HandleMessageAsync(string connectionId, string messageType, string messageJson)
    {
        _logger.LogInformation("Handling message type '{MessageType}' from {ConnectionId}", 
            messageType, connectionId);

        switch (messageType)
        {
            case "broadcast":
                await HandleBroadcastAsync(connectionId, messageJson);
                break;

            case "create_lobby":
                await HandleCreateLobbyAsync(connectionId);
                break;

            case "join_lobby":
                await HandleJoinLobbyAsync(connectionId, messageJson);
                break;

            default:
                await HandleEchoAsync(connectionId, messageJson);
                break;
        }
    }

    private async Task HandleBroadcastAsync(string connectionId, string message)
    {
        await _connectionManager.BroadcastAsync(message, connectionId);
    }

    private async Task HandleCreateLobbyAsync(string connectionId)
    {
        var lobbyCode = _lobbyManager.CreateLobby(connectionId);
        
        var response = new LobbyCreatedResponse
        {
            LobbyCode = lobbyCode
        };

        var responseJson = JsonSerializer.Serialize(response);
        await _connectionManager.SendAsync(connectionId, responseJson);
    }

    private async Task HandleJoinLobbyAsync(string connectionId, string messageJson)
    {
        try
        {
            var joinMessage = JsonSerializer.Deserialize<JoinLobbyMessage>(messageJson);
            
            if (joinMessage == null || string.IsNullOrWhiteSpace(joinMessage.DuckerName))
            {
                await SendErrorAsync(connectionId, "Invalid join lobby request");
                return;
            }

            var joined = _lobbyManager.JoinLobby(
                connectionId, 
                joinMessage.LobbyCode, 
                joinMessage.DuckerName
            );

            if (joined)
            {
                // Send confirmation to the joining player
                string ConnectedPlayers;
                foreach (var player in connectionsInLobby)
                {
                    if (player != newPlayerConnectionId)
                    {
                        ConnectedPlayers += player.Value;
                    }
                }

                var joinedResponse = new JoinedLobbyResponse();
                await _connectionManager.SendAsync(
                    connectionId, 
                    JsonSerializer.Serialize(joinedResponse)
                );

                // Notify other players in the lobby
                await NotifyLobbyPlayersAsync(connectionId, joinMessage.LobbyCode, joinMessage.DuckerName);
            }
            else
            {
                await SendErrorAsync(connectionId, "Lobby not found");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing join lobby message from {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "Invalid message format");
        }
    }

    private async Task NotifyLobbyPlayersAsync(string newPlayerConnectionId, int lobbyCode, string playerName)
    {
        var connectionsInLobby = _lobbyManager.GetConnectionsFromLobbyCode(lobbyCode);
        
        var playerJoinedResponse = new PlayerJoinedResponse
        {
            PlayerName = playerName
        };
        
        var responseJson = JsonSerializer.Serialize(playerJoinedResponse);

        foreach (var connection in connectionsInLobby)
        {
            if (connection.Key != newPlayerConnectionId)
            {
                await _connectionManager.SendAsync(connection.Key, responseJson);
            }
        }
    }

    private async Task HandleEchoAsync(string connectionId, string message)
    {
        await _connectionManager.SendAsync(connectionId, $"Echo: {message}");
    }

    private async Task SendErrorAsync(string connectionId, string errorMessage)
    {
        var errorResponse = new ErrorResponse
        {
            Message = errorMessage
        };

        var responseJson = JsonSerializer.Serialize(errorResponse);
        await _connectionManager.SendAsync(connectionId, responseJson);
    }
}