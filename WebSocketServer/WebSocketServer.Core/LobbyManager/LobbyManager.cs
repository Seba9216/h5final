using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketServer.Core.LobbyManager
{
    public sealed class LobbyManager
    {
        private static readonly Lazy<LobbyManager> _instance =
            new(() => new LobbyManager());

        public static LobbyManager Instance => _instance.Value;

        private LobbyManager() { }

        private readonly Dictionary<int, List<string>> _lobbies = new();

        public Task CreateLobby(string connectionId)
        {
            int lobbyCode = Random.Shared.Next(100000, 999999);
            _lobbies.Add(lobbyCode, new List<string> { connectionId });
            return SendAsync(connectionId, lobbyCode);
        }

        public Task JoinLobby(string connectionId, string message)
        {
            var json = JsonDocument.Parse(message);
            int lobbyCode = json.RootElement.GetProperty("LobbyCode").GetInt32();

            _lobbies[lobbyCode].Add(connectionId);
            return Task.CompletedTask;
        }
    }
}
