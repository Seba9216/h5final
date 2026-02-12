using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSocketServer.Server.Services;

namespace WebSocketServer.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginHistoryController : ControllerBase
{
    IDuckingLoginService _duckingLoginService;
    public LoginHistoryController(IDuckingLoginService duckingLoginService)
    {
        _duckingLoginService = duckingLoginService;
    }


    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserLoginsById(int userId)
    {
        var Logins= await _duckingLoginService.GetAllLoginsForUser(userId);

        if (Logins == null)
        {
            return NotFound();
        }

        return Ok(Logins);
    }
}
