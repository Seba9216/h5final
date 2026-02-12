using WebSocketServer.Core.context;

namespace WebSocketServer.Server.Models;

public class DuckingGameHistory
{
    public int Id { get; set; }
    public List<DuckingGame> DuckingGames { get; set; }
}
