using WebSocketServer.Core.context;
using WebSocketServer.Server.Repositorys;

namespace WebSocketServer.Server.Services;

public class DuckingLoginService : IDuckingLoginService
{
    IDuckingLoginRepository _duckingLoginRepository;
    public DuckingLoginService(IDuckingLoginRepository duckingLoginRepository)
    {
        _duckingLoginRepository = duckingLoginRepository;
    }

    async Task<List<DuckingLogins>> IDuckingLoginService.GetAllLoginsForUser(int userId)
    {
        return await _duckingLoginRepository.GetAllDuckingLoginsForUser(userId);
    }
}
