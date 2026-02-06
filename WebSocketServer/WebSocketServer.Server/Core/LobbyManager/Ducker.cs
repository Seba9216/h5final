namespace WebSocketServer.Core.LobbyManager;
using System.Text.Json.Serialization;


[JsonDerivedType(typeof(RacerDucker), typeDiscriminator: "racer")]
[JsonDerivedType(typeof(PokerDucker), typeDiscriminator: "poker")]
public class Ducker
{
    public string ConnectionId { get; set; }
    public string DuckerName { get; set; }

    public Ducker(string connectionId, string duckerName)
    {
        ConnectionId = connectionId;
        DuckerName = duckerName;
    }
}