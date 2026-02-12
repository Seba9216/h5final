using WebSocketServer.Server.Models;

namespace WebSocketServer.Server.Repositorys;

public interface IGameHistoryRepository
{
    public Task<DuckingGameHistory> GetDuckingGameHistoryFromUserId(int userId);
}
