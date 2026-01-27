using System.Collections.Concurrent;

namespace WebSocketServer.Core.LobbyManager;

public class LobbyManager : ILobbyManager
{
    private readonly ConcurrentDictionary<int, List<Ducker>> _lobbies = new();
    private readonly ConcurrentDictionary<string, int> _connectionToLobbyMap = new();
    private readonly ILogger<LobbyManager> _logger;

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

        _lobbies.TryAdd(lobbyCode, new List<Ducker>());
        
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
            lock (lobby)
            {
                lobby.Add(new Ducker(connectionId, duckerName));
            }
            
            _connectionToLobbyMap.TryAdd(connectionId, lobbyCode);
            
            _logger.LogInformation("Player {DuckerName} ({ConnectionId}) joined lobby {LobbyCode}", 
                duckerName, connectionId, lobbyCode);
            
            return true;
        }

        return false;
    }

    public Dictionary<string, string> GetPlayersFromLobbyCode(int lobbyCode)
    {
        var connections = new Dictionary<string, string>();

        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            lock (lobby)
            {
                foreach (var ducker in lobby)
                {
                    connections[ducker.ConnectionId] = ducker.DuckerName;
                }
            }
        }

        return connections;
    }

    public List<Ducker> GetDuckersFromLobbyCode(int lobbyCode)
    {
        if (_lobbies.TryGetValue(lobbyCode, out var lobby))
        {
            lock (lobby)
            {
                return new List<Ducker>(lobby);
            }
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

        bool removed = false;
        lock (lobby)
        {
            var duckerToRemove = lobby.Find(d => d.ConnectionId == connectionId);
            if (duckerToRemove != null)
            {
                lobby.Remove(duckerToRemove);
                removed = true;
                
                _logger.LogInformation("Player {DuckerName} ({ConnectionId}) left lobby {LobbyCode}", 
                    duckerToRemove.DuckerName, connectionId, lobbyCode);
            }

            if (lobby.Count == 0)
            {
                _lobbies.TryRemove(lobbyCode, out _);
                _logger.LogInformation("Lobby {LobbyCode} removed (empty)", lobbyCode);
            }
        }

        if (removed)
        {
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
}