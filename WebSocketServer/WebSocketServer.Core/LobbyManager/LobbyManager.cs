using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketServer.Core.LobbyManager;

public class Ducker {
    public string ConnectionID;
    public string DuckerName;

    public Ducker(string connectionID, string duckerName) {
        ConnectionID = connectionID;
        DuckerName = duckerName;
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

    public bool JoinLobby(string connectionId, int lobbyCode, string duckerName)
    {
        if (!_lobbies.ContainsKey(lobbyCode))
            return false;

        if(string.IsNullOrEmpty(duckerName))
            return false;

        _lobbies[lobbyCode].Add(new Ducker(connectionId, duckerName));
        return true;
    }

    public List<string> GetConnectionsFromLobbyCode(int lobbyID)
    {
        List<string> connections = new();

        if (_lobbies.TryGetValue(lobbyID, out var lobby))
        {
            foreach (var ducker in lobby)
            {
                connections.Add(ducker.ConnectionID);
            }
        }

        return connections;
    }
}