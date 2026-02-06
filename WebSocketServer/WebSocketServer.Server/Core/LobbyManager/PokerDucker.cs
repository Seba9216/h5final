namespace WebSocketServer.Core.LobbyManager;

public class PokerDucker : Ducker
{
    public string? StoryPoints;

    public PokerDucker(string connectionId, string duckerName) 
        : base(connectionId, duckerName)
    {
    }
}
