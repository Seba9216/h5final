using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSocketServer.Core.Auth;
using WebSocketServer.Core.context;
using WebSocketServer.Models;
using WebSocketServer.Server.Migrations;

namespace WebSocketServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class UserController : ControllerBase
{
    private readonly DuckingContext _dbContext;
    private readonly ITokenService _tokenService;

    public UserController(DuckingContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] DuckingUser user)
    {
        if (user == null)
        {
            return BadRequest("Invalid user data");
        }

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.UserName);

        _dbContext.Logins.Add(new DuckingLogins
        {
            UserId = user.Id,
            LoginTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { id = user.Id, token, message = "User created successfully" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Username and password are required");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _tokenService.GenerateToken(user.Id, user.UserName);

        _dbContext.Logins.Add(new DuckingLogins
        {
            UserId = user.Id,
            LoginTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return Ok(new { id = user.Id, userName = user.UserName, token, message = "Login successful" });
    }
}
