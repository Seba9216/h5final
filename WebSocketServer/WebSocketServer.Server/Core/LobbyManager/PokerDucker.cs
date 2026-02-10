namespace WebSocketServer.Core.LobbyManager;

public class PokerDucker : Ducker
{
    public string? StoryPoints;

    public PokerDucker(string connectionId, string duckerName, int? userId = null) 
        : base(connectionId, duckerName, userId)
    {
    }
}
