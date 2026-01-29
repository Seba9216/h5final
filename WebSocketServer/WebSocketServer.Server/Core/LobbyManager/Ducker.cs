namespace WebSocketServer.Core.LobbyManager;
using WebSocketServer.Core.Configuration;

public class Ducker
{
    public string ConnectionId { get; set; }
    public string DuckerName { get; set; }
    public int Speed { get; set; } = 0;

    public Ducker(string connectionId, string duckerName)
    {
        ConnectionId = connectionId;
        DuckerName = duckerName;

        Speed = Random.Shared.Next(Constants.DuckerMinSpeed, Constants.DuckerMaxSpeed);
    }
}