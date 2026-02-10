namespace WebSocketServer.Core.LobbyManager;
using System.Text.Json.Serialization;


[JsonDerivedType(typeof(RacerDucker), typeDiscriminator: "racer")]
[JsonDerivedType(typeof(PokerDucker), typeDiscriminator: "poker")]
public class Ducker
{
    public string ConnectionId { get; set; }
    public string DuckerName { get; set; }
    public int? UserId { get; set; }

    public Ducker(string connectionId, string duckerName, int? userId = null)
    {
        ConnectionId = connectionId;
        DuckerName = duckerName;
        UserId = userId;
    }
}