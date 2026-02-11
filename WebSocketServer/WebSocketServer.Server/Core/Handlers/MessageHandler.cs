using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Text.Json;
using WebSocketServer.Core.Auth;
using WebSocketServer.Core.Configuration;
using WebSocketServer.Core.Connections;
using WebSocketServer.Core.context;
using WebSocketServer.Core.LobbyManager;
using WebSocketServer.Core.Models;

namespace WebSocketServer.Core.Handlers;

public class MessageHandler : IMessageHandler
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly ILobbyManager _lobbyManager;
    private readonly ILogger<MessageHandler> _logger;
    private readonly DuckingContext _dbContext;
    private readonly ITokenService _tokenService;

    public MessageHandler(
        IWebSocketConnectionManager connectionManager,
        ILobbyManager lobbyManager,
        ILogger<MessageHandler> logger,
        DuckingContext dbContext,
        ITokenService tokenService)
    {
        _connectionManager = connectionManager;
        _lobbyManager = lobbyManager;
        _logger = logger;
        _dbContext = dbContext;
        _tokenService = tokenService;

        _lobbyManager.PlayerLeftLobby += NotifyPlayerLeftAsync;
    }

    public async Task HandleMessageAsync(string connectionId, string messageType, string messageJson)
    {
        _logger.LogInformation("Handling message type '{MessageType}' from {ConnectionId}",
            messageType, connectionId);

        if (messageType != "echo")
        {
            try
            {
                var baseMessage = JsonSerializer.Deserialize<WebSocketMessage>(messageJson);
                if (baseMessage?.Token == null || !_tokenService.ValidateToken(baseMessage.Token))
                {
                    _logger.LogWarning("Invalid or missing auth token from {ConnectionId}", connectionId);
                    await SendErrorAsync(connectionId, "Authentication required. Please login first.");
                    return;
                }
            }
            catch (JsonException)
            {}
        }

        switch (messageType)
        {
            case "broadcast":
                await HandleBroadcastAsync(connectionId, messageJson);
                break;

            case "create_lobby":
                await HandleCreateLobbyAsync(connectionId, messageJson);
                break;

            case "join_lobby":
                await HandleJoinLobbyAsync(connectionId, messageJson);
                break;
            case "start_game":
                await StartGameAsync(connectionId, messageJson);
                break;
            case "story_points":
                await HandleStoryPointAsync(connectionId, messageJson);
                break;
            case "game_finished":
                await HandleGameFinishedAsync(connectionId, messageJson);
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

    private async Task HandleCreateLobbyAsync(string connectionId, string message)
    {
        LobbyType lobbyType;
        try
        {
            var createLobbyMessage = JsonSerializer.Deserialize<CreateLobbyMessage>(message);
            if (createLobbyMessage == null)
            {
                throw new JsonException("Deserialized CreateLobbyMessage is null");
            }

            if (!Enum.TryParse(createLobbyMessage.LobbyType, true, out lobbyType))
            {
                throw new JsonException($"Invalid lobby type: {createLobbyMessage.LobbyType}");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing create lobby message from {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "Invalid message format");
            return;
        }

        var lobbyCode = _lobbyManager.CreateLobby(connectionId, lobbyType);

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

            // Resolve userId from auth token
            int? userId = null;
            if (!string.IsNullOrEmpty(joinMessage.Token))
            {
                userId = _tokenService.GetUserIdFromToken(joinMessage.Token);
            }

            var joined = _lobbyManager.JoinLobby(
                connectionId,
                joinMessage.LobbyCode,
                joinMessage.DuckerName,
                userId
            );

            if (joined)
            {
                var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(joinMessage.LobbyCode);

                // Send confirmation to the joining player with all other players
                var joinedResponse = new JoinedLobbyResponse
                {
                    ConnectedPlayers = playersInLobby
                        .ToList()
                };

                await _connectionManager.SendAsync(
                    connectionId,
                    JsonSerializer.Serialize(joinedResponse)
                );

                // Notify other players in the lobby about the new player
                await NotifyLobbyPlayersAsync(connectionId, joinMessage.LobbyCode);
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

    private async Task NotifyLobbyPlayersAsync(string newPlayerConnectionId, int lobbyCode)
    {
        var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(lobbyCode);

        var newPlayer = playersInLobby.FirstOrDefault(p => p.ConnectionId == newPlayerConnectionId);
        if (newPlayer == null)
        {
            _logger.LogWarning("Could not find player with ConnectionId {ConnectionId} in lobby {LobbyCode}",
                newPlayerConnectionId, lobbyCode);
            return;
        }

        var playerJoinedResponse = new PlayerJoinedResponse
        {
            Player = newPlayer
        };

        var responseJson = JsonSerializer.Serialize(playerJoinedResponse);

        foreach (var player in playersInLobby)
        {
            if (player.ConnectionId != newPlayerConnectionId)
            {
                await _connectionManager.SendAsync(player.ConnectionId, responseJson);
            }
        }

        var hostId = _lobbyManager.GetLobbyHostId(lobbyCode);
        if (!string.IsNullOrEmpty(hostId) && hostId != newPlayerConnectionId)
        {
            await _connectionManager.SendAsync(hostId, responseJson);
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

    private async Task NotifyPlayerLeftAsync(int lobbyCode, string connectionId)
    {
        var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(lobbyCode);

        var playerLeftResponse = new PlayerLeftResponse
        {
            ConnectionId = connectionId
        };

        var responseJson = JsonSerializer.Serialize(playerLeftResponse);

        foreach (var player in playersInLobby)
        {
            await _connectionManager.SendAsync(player.ConnectionId, responseJson);
        }
    }

    private async Task StartGameAsync(string connectionId, string message)
    {
        StartGameMessage? startGameMessage;
        try
        {
            startGameMessage = JsonSerializer.Deserialize<StartGameMessage>(message);
            if (startGameMessage == null)
            {
                throw new JsonException("Deserialized StartGameMessage is null");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing start game message from {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "Invalid message format");
            return;
        }

        int lobbyCode = startGameMessage.LobbyCode;
        bool started = _lobbyManager.StartGame(connectionId, lobbyCode);

        if (started)
        {
            var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(lobbyCode);

            var randomDucker = playersInLobby[Random.Shared.Next(playersInLobby.Count)];
            if (randomDucker is RacerDucker racerDucker)
            {
                racerDucker.Speed = Constants.DuckerMinSpeed - 5;
            }

            var startGameResponse = new StartGameResponse
            {
                Players = playersInLobby
            };

            var responseJson = JsonSerializer.Serialize(startGameResponse);

            foreach (var player in playersInLobby)
            {
                await _connectionManager.SendAsync(player.ConnectionId, responseJson);
            }

            await _connectionManager.SendAsync(connectionId, responseJson);
        }
        else
        {
            var errorResponse = new ErrorResponse
            {
                Message = "Failed to start game. You may not be the lobby host."
            };

            var responseJson = JsonSerializer.Serialize(errorResponse);
            await _connectionManager.SendAsync(connectionId, responseJson);
        }
    }

    private async Task HandleStoryPointAsync(string connectionId, string message)
    {
        StoryPointsResponse storyPointsResponse;
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        try
        {
            storyPointsResponse = JsonSerializer.Deserialize<StoryPointsResponse>(message);
            if (storyPointsResponse == null)
            {
                throw new JsonException("Deserialized StoryPointsResponse is null");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing story points message from {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "Invalid message format");
            return;
        }

        var storyPointUpdate = new StoryPointsMessage
        {
            Type = "story_points",
            ConnectionId = connectionId,
            StoryPoints = storyPointsResponse.StoryPoints
        };

        var lobbyCode = _lobbyManager.GetLobbyCodeForConnection(connectionId);
        if (lobbyCode == null)
        {
            _logger.LogWarning("Could not find lobby for ConnectionId {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "You are not in a lobby");
            return;
        }

        var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(lobbyCode.Value);
        if (playersInLobby == null)
        {
            _logger.LogWarning("Could not find players for lobby code {LobbyCode}", lobbyCode);
            await SendErrorAsync(connectionId, "Lobby not found");
            return;
        }


        var responseJson = JsonSerializer.Serialize(storyPointUpdate);
        foreach (var player in playersInLobby)
        {
            await _connectionManager.SendAsync(player.ConnectionId, responseJson);
        }
        var host = _lobbyManager.GetLobbyHostId(lobbyCode.Value);
        await _connectionManager.SendAsync(host, responseJson);

    }

    private async Task HandleGameFinishedAsync(string connectionId, string message)
    {
        var lobbyCode = _lobbyManager.GetLobbyCodeForConnection(connectionId);
        if (lobbyCode == null)
        {
            _logger.LogWarning("Could not find lobby for ConnectionId {ConnectionId}", connectionId);
            await SendErrorAsync(connectionId, "You are not in a lobby");
            return;
        }

        var hostId = _lobbyManager.GetLobbyHostId(lobbyCode.Value);
        if (hostId != connectionId)
        {
            _logger.LogWarning("ConnectionId {ConnectionId} attempted to finish game but is not the host", connectionId);
            await SendErrorAsync(connectionId, "Only the host can finish the game");
            return;
        }

        // Get all players from the lobby
        var playersInLobby = _lobbyManager.GetDuckersFromLobbyCode(lobbyCode.Value);
        
        // Find DuckingUser entities for players with UserId
        var duckingUsers = new List<DuckingUser>();
        foreach (var player in playersInLobby)
        {
            if (player.UserId.HasValue)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == player.UserId.Value);
                if (user != null)
                {
                    duckingUsers.Add(user);
                }
            }
        }

        // Create and save the game record
        var game = new DuckingGame
        {
            Type = _lobbyManager.GetLobbyType(lobbyCode.Value),
            Players = duckingUsers
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();
    }
}