namespace WebSocketServer.Core.LobbyManager;

public abstract class Ducker
{
    public string ConnectionId { get; set; }
    public string DuckerName { get; set; }

    public Ducker(string connectionId, string duckerName)
    {
        ConnectionId = connectionId;
        DuckerName = duckerName;
    }
}