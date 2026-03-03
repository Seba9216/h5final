using Microsoft.EntityFrameworkCore;
using WebSocketServer.Core.context;
using WebSocketServer.Server.Models;

namespace WebSocketServer.Server.Repositorys;

public class GameHistoryRepository : IGameHistoryRepository
{
    DuckingContext _duckingContext;
    public GameHistoryRepository(DuckingContext duckingContext)
    {
        _duckingContext = duckingContext;
    }

  public async Task<DuckingGameHistory> GetDuckingGameHistoryFromUserId(int userId)
    {
        var games = await _duckingContext.Games
       .Where(g => g.Players.Any(p => p.Id == userId))
       .ToListAsync();
        return new DuckingGameHistory
        {
            Id = userId,
            DuckingGames = games
        }; 
    }
}
