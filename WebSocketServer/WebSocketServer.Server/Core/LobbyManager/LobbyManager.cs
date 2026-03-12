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

    public int CreateLobby(string connectionId, LobbyType lobbyType)
    {
        int lobbyCode;
        do
        {
            lobbyCode = Random.Shared.Next(100000, 999999);
        }
        while (_lobbies.ContainsKey(lobbyCode));

        _lobbies.TryAdd(lobbyCode, new Lobby(lobbyCode, connectionId, lobbyType));

        _logger.LogInformation("Lobby {LobbyCode} created by connection {ConnectionId}",
            lobbyCode, connectionId);

        return lobbyCode;
    }

    public bool JoinLobby(string connectionId, int lobbyCode, string duckerName, string lobbyType, int? userId = null)
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
            // Validate lobby type
            LobbyType parsedLobbyType;
            if(!Enum.TryParse(lobbyType, true, out parsedLobbyType))
             {
                _logger.LogWarning("Attempt to join lobby {LobbyCode} with invalid lobby type '{LobbyType}' by {ConnectionId}",
                    lobbyCode, lobbyType, connectionId);

                return false;
            }
            if (parsedLobbyType != lobby.LobbyType)
            {
                _logger.LogWarning("Attempt to join lobby {LobbyCode} with mismatched lobby type by {ConnectionId}",
                    lobbyCode, connectionId);

                return false;
            }

            bool added = lobby.AddPlayer(connectionId, duckerName, userId);

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
                .Select(p => CreateDuckerForLobbyType(lobby.LobbyType, p.Key, p.Value.Name, p.Value.UserId))
                .ToList();
        }

        return new List<Ducker>();
    }

    public LobbyType GetLobbyType(int lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            return lobby.LobbyType;
        }

        return LobbyType.Unknown;
    }

    private Ducker CreateDuckerForLobbyType(LobbyType lobbyType, string connectionId, string duckerName, int? userId)
    {
        switch (lobbyType)
        {
            case LobbyType.DuckRace:
                return new RacerDucker(connectionId, duckerName, userId);
            case LobbyType.PlanningPoker:
                return new PokerDucker(connectionId, duckerName, userId);
            default:
                return null;
        }
    }

    public int? GetLobbyCodeForConnection(string connectionId)
    {
        if (_connectionToLobbyMap.TryGetValue(connectionId, out var lobbyCode))
        {
            return lobbyCode;
        }

        var hostLobby = _lobbies.Values.FirstOrDefault(l => l.HostConnectionId == connectionId);
        if (hostLobby != null)
        {
            return hostLobby.Code;
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
            // Trigger event with connectionId instead of playerName
            PlayerLeftLobby?.Invoke(lobbyCode, connectionId);

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

    public string GetLobbyHostId(int lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            return lobby.HostConnectionId;
        }

        return string.Empty;
    }
}