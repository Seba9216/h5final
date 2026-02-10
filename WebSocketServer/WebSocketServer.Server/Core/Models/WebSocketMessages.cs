using WebSocketServer.Core.LobbyManager;

namespace WebSocketServer.Core.Models;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
}

public class BroadcastMessage : WebSocketMessage
{
    public string? Token { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class CreateLobbyMessage : WebSocketMessage
{
    public string? Token { get; set; }
    public string LobbyType { get; set; } = string.Empty;
}

public class StartGameMessage : WebSocketMessage
{
    public string? Token { get; set; }
    public int LobbyCode { get; set; }
}

public class JoinLobbyMessage : WebSocketMessage
{
    public string? Token { get; set; }
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

public class StoryPointsMessage : WebSocketMessage
{
    public string? Token { get; set; }
    public string StoryPoints { get; set; } = string.Empty;
}

public class StoryPointsResponse
{
    public string Type => "story_points";
    public string ConnectionId { get; set; } = string.Empty;
    public string StoryPoints { get; set; } = string.Empty;
}

public class GameFinishedMessage : WebSocketMessage
{
    public string? Token { get; set; }
}

public class GameFinishedResponse
{
    public string Type => "finished_game";
}