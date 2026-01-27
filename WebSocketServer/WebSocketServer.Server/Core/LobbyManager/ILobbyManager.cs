namespace WebSocketServer.Core.LobbyManager;

public interface ILobbyManager
{
    int CreateLobby(string hostConnectionId);
    bool JoinLobby(string connectionId, int lobbyCode, string playerName);
    bool LeaveLobby(string connectionId, int lobbyCode);
    List<string> GetConnectionsFromLobbyCode(int lobbyCode);
    int? GetLobbyCodeForConnection(string connectionId);
    void RemoveConnectionFromAllLobbies(string connectionId);
}