namespace WebSocketServer.Core.LobbyManager;
using WebSocketServer.Core.Configuration;

public class RacerDucker : Ducker
{
    public int Speed { get; set; }

    public RacerDucker(string connectionId, string duckerName) 
        : base(connectionId, duckerName)
    {
        Speed = Random.Shared.Next(Constants.DuckerMinSpeed, Constants.DuckerMaxSpeed);
    }
}
