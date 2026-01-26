using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketServer.Core.LobbyManager;

public class DuckRacer {
    public string ConnectionID;
    public string RacerName;

    public DuckRacer(string connectionID, string racerName) {
        ConnectionID = connectionID;
        RacerName = racerName;
    }
}

public sealed class LobbyManager
{
    private static readonly Lazy<LobbyManager> _instance =
        new(() => new LobbyManager());

    public static LobbyManager Instance => _instance.Value;

    private LobbyManager() { }

    private readonly Dictionary<int, List<DuckRacer>> _lobbies = new();

    public int CreateLobby(string connectionId, string message)
    {
        int lobbyCode;
        do
        {
            lobbyCode = Random.Shared.Next(100000, 999999);
        }
        while (_lobbies.ContainsKey(lobbyCode));

        var json = JsonDocument.Parse(message);
        string racerName = json.RootElement.GetProperty("ducker_name").GetString()!;

        _lobbies.Add(
            lobbyCode,
            new List<DuckRacer> { new DuckRacer(connectionId, racerName) }
        );

        return lobbyCode;
    }

    public bool JoinLobby(string connectionId, string message)
    {
        var json = JsonDocument.Parse(message);
        int lobbyCode = json.RootElement.GetProperty("lobby_code").GetInt32();
        string racerName = json.RootElement.GetProperty("ducker_name").GetString()!;

        if (!_lobbies.ContainsKey(lobbyCode))
            return false;

        _lobbies[lobbyCode].Add(new DuckRacer(connectionId, racerName));
        return true;
    }
}