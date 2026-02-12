using WebSocketServer.Server.Models;

namespace WebSocketServer.Server.Services;

public interface IGameHistoryService
{
    public Task<DuckingGameHistory> GetDuckingGameHistoryFromUserId(int userId);
}
