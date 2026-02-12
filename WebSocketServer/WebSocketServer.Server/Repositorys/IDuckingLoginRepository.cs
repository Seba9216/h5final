using WebSocketServer.Core.context;

namespace WebSocketServer.Server.Repositorys;

public interface IDuckingLoginRepository
{
    public Task<List<DuckingLogins>> GetAllDuckingLoginsForUser(int userId);
}
