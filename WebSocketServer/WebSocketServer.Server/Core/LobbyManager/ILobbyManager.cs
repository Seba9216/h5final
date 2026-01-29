namespace WebSocketServer.Core.LobbyManager;

public interface ILobbyManager
{
    int CreateLobby(string hostConnectionId);
    bool JoinLobby(string connectionId, int lobbyCode, string playerName);
    bool LeaveLobby(string connectionId, int lobbyCode);
    int? GetLobbyCodeForConnection(string connectionId);
    public List<Ducker> GetDuckersFromLobbyCode(int lobbyCode);
    void RemoveConnectionFromAllLobbies(string connectionId);
    public bool StartGame(string ConnectionId, int lobbyCode);

    event Func<int, string, Task>? PlayerLeftLobby;
}