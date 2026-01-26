using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketServer.Core.LobbyManager;

public class Ducker {
    public string ConnectionID;
    public string RacerName;

    public Ducker(string connectionID, string racerName) {
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

    private readonly Dictionary<int, List<Ducker>> _lobbies = new();

    public int CreateLobby(string connectionId)
    {
        int lobbyCode;
        do
        {
            lobbyCode = Random.Shared.Next(100000, 999999);
        }
        while (_lobbies.ContainsKey(lobbyCode));

        _lobbies.Add(
            lobbyCode,
            new List<Ducker> {}
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

        _lobbies[lobbyCode].Add(new Ducker(connectionId, racerName));
        return true;
    }
}