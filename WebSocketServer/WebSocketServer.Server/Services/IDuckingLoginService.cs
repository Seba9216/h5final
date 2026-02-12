using WebSocketServer.Core.context;

namespace WebSocketServer.Server.Services;

public interface IDuckingLoginService
{
    public Task<List<DuckingLogins>> GetAllLoginsForUser(int userId);
}
