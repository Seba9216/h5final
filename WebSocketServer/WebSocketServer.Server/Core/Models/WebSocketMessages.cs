using WebSocketServer.Core.LobbyManager;

namespace WebSocketServer.Core.Models;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
}

public class BroadcastMessage : WebSocketMessage
{
    public string Content { get; set; } = string.Empty;
}

public class CreateLobbyMessage : WebSocketMessage
{
}

public class StartGameMessage : WebSocketMessage
{
    public int LobbyCode { get; set; }
}

public class JoinLobbyMessage : WebSocketMessage
{
    public string DuckerName { get; set; } = string.Empty;
    public int LobbyCode { get; set; }
}

public class LobbyCreatedResponse
{
    public string Type => "lobby_created";
    public int LobbyCode { get; set; }
}

public class JoinedLobbyResponse
{
    public string Type => "joined_lobby";
    public List<Ducker> ConnectedPlayers { get; set; } = new();
}

public class PlayerJoinedResponse
{
    public string Type => "player_joined";
    public Ducker Player { get; set; } = null!;
}

public class PlayerLeftResponse
{
    public string Type => "player_left";
    public string ConnectionId { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Type => "error";
    public string Message { get; set; } = string.Empty;
}

public class StartGameResponse
{
    public string Type => "start_game";
    public List<Ducker> Players { get; set; } = new();
}