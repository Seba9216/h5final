using Microsoft.EntityFrameworkCore.ChangeTracking;
using WebSocketServer.Server.Models;
using WebSocketServer.Server.Repositorys;

namespace WebSocketServer.Server.Services;

public class GameHistoryService : IGameHistoryService
{
    private IGameHistoryRepository _gameHistoryRepository;
    public GameHistoryService(IGameHistoryRepository gameHistoryRepository)
    {
        _gameHistoryRepository = gameHistoryRepository;
    }
    public async Task<DuckingGameHistory> GetDuckingGameHistoryFromUserId(int userId)
    {
        return await _gameHistoryRepository.GetDuckingGameHistoryFromUserId(userId);
    }
}
