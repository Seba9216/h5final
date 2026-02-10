namespace WebSocketServer.Core.LobbyManager;
using WebSocketServer.Core.Configuration;

public class RacerDucker : Ducker
{
    public int Speed { get; set; }

    public RacerDucker(string connectionId, string duckerName, int? userId = null) 
        : base(connectionId, duckerName, userId)
    {
        Speed = Random.Shared.Next(Constants.DuckerMinSpeed, Constants.DuckerMaxSpeed);
    }
}
