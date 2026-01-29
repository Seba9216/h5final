using System.Collections.Concurrent;

namespace WebSocketServer.Core.LobbyManager;

public class LobbyManager : ILobbyManager
{
    private readonly ConcurrentDictionary<int, Lobby> _lobbies = new();
    private readonly ConcurrentDictionary<string, int> _connectionToLobbyMap = new();
    private readonly ILogger<LobbyManager> _logger;
    public event Func<int, string, Task>? PlayerLeftLobby;

    public LobbyManager(ILogger<LobbyManager> logger)
    {
        _logger = logger;
    }

    public int CreateLobby(string connectionId)
    {
        int lobbyCode;
        do
        {
            lobbyCode = Random.Shared.Next(100000, 999999);
        }
        while (_lobbies.ContainsKey(lobbyCode));

        _lobbies.TryAdd(lobbyCode, new Lobby(lobbyCode, connectionId));

        _logger.LogInformation("Lobby {LobbyCode} created by connection {ConnectionId}",
            lobbyCode, connectionId);

        return lobbyCode;
    }

    public bool JoinLobby(string connectionId, int lobbyCode, string duckerName)
    {
        if (!_lobbies.ContainsKey(lobbyCode))
        {
            _logger.LogWarning("Attempt to join non-existent lobby {LobbyCode} by {ConnectionId}",
                lobbyCode, connectionId);
            return false;
        }

        if (string.IsNullOrEmpty(duckerName))
        {
            _logger.LogWarning("Attempt to join lobby {LobbyCode} with empty name by {ConnectionId}",
                lobbyCode, connectionId);
            return false;
        }

        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            bool added = lobby.AddPlayer(connectionId, duckerName);

            if (added)
            {
                _connectionToLobbyMap.TryAdd(connectionId, lobbyCode);

                _logger.LogInformation("Player {DuckerName} ({ConnectionId}) joined lobby {LobbyCode}",
                    duckerName, connectionId, lobbyCode);

                return true;
            }
        }

        return false;
    }

    public List<Ducker> GetDuckersFromLobbyCode(int lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            return lobby.Players
                .Select(p => new Ducker(p.Key, p.Value))
                .ToList();
        }

        return new List<Ducker>();
    }

    public int? GetLobbyCodeForConnection(string connectionId)
    {
        if (_connectionToLobbyMap.TryGetValue(connectionId, out var lobbyCode))
        {
            return lobbyCode;
        }

        return null;
    }

    public bool LeaveLobby(string connectionId, int lobbyCode)
    {
        if (!_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            return false;
        }

        bool removed = lobby.RemovePlayer(connectionId);

        if (removed)
        {
            string? playerName = lobby.Players.ContainsKey(connectionId)
                ? lobby.Players[connectionId]
                : null;

            if (playerName != null)
            {
                PlayerLeftLobby?.Invoke(lobbyCode, playerName);
            }

            if (lobby.PlayerCount == 0)
            {
                _lobbies.TryRemove(lobbyCode, out _);
            }

            _connectionToLobbyMap.TryRemove(connectionId, out _);
        }

        return removed;
    }

    public void RemoveConnectionFromAllLobbies(string connectionId)
    {
        if (_connectionToLobbyMap.TryGetValue(connectionId, out var lobbyCode))
        {
            LeaveLobby(connectionId, lobbyCode);
        }
    }

    public bool StartGame(string connectionId, int lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            return lobby.StartGame(connectionId);
        }

        return false;
    }
}