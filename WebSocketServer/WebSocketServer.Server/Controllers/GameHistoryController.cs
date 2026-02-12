using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Server.Services;

namespace WebSocketServer.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameHistoryController : ControllerBase
{
    IGameHistoryService _gameHistoryService;
    public GameHistoryController(IGameHistoryService gameHistoryService)
    {
        _gameHistoryService = gameHistoryService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserLoginsById(int userId)
    {
        var History = await _gameHistoryService.GetDuckingGameHistoryFromUserId(userId);

        if (History == null)
        {
            return NotFound();
        }

        return Ok(History);
    }

}
