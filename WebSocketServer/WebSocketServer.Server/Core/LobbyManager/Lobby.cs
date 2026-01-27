namespace WebSocketServer.Core.LobbyManager;

public class Lobby
{
    public int Code { get; set; }
    public string HostConnectionId { get; set; } = string.Empty;
    public Dictionary<string, string> Players { get; set; } = new(); // ConnectionId -> PlayerName
    public DateTime CreatedAt { get; set; }

    public Lobby(int code, string hostConnectionId)
    {
        Code = code;
        HostConnectionId = hostConnectionId;
        CreatedAt = DateTime.UtcNow;
    }

    public bool AddPlayer(string connectionId, string playerName)
    {
        return Players.TryAdd(connectionId, playerName);
    }

    public bool RemovePlayer(string connectionId)
    {
        return Players.Remove(connectionId);
    }

    public bool HasPlayer(string connectionId)
    {
        return Players.ContainsKey(connectionId);
    }

    public List<string> GetConnectionIds()
    {
        return Players.Keys.ToList();
    }

    public int PlayerCount => Players.Count;
}