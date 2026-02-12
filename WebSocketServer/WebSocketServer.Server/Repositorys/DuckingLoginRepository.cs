using Microsoft.EntityFrameworkCore;
using WebSocketServer.Core.context;

namespace WebSocketServer.Server.Repositorys;

public class DuckingLoginRepository : IDuckingLoginRepository
{
    private DuckingContext _context;
    public DuckingLoginRepository(DuckingContext dbContext)
    {
        _context = dbContext;
    }

    public async Task<List<DuckingLogins>> GetAllDuckingLoginsForUser(int userId)
    {
        return _context.Logins.Where(x => x.UserId == userId).AsNoTracking().ToList();
    }
}
