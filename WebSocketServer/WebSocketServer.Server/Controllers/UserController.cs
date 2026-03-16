using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using WebSocketServer.Core.Auth;
using WebSocketServer.Core.context;
using WebSocketServer.Models;

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
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, workFactor: 12);
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
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
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

    [HttpGet("validate-token")]
    public IActionResult ValidateToken([FromQuery] string? token = null)
    {
        var providedToken = token;

        if (string.IsNullOrWhiteSpace(providedToken) &&
            Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var rawHeader = authorizationHeader.ToString();
            if (rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                providedToken = rawHeader["Bearer ".Length..].Trim();
            }
            else
            {
                providedToken = rawHeader.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(providedToken))
        {
            return BadRequest(new { valid = false, message = "Token is required" });
        }

        var isValid = _tokenService.ValidateToken(providedToken);
        if (!isValid)
        {
            return Unauthorized(new { valid = false, message = "Invalid token" });
        }

        return Ok(new
        {
            valid = true,
            userId = _tokenService.GetUserIdFromToken(providedToken),
            userName = _tokenService.GetUserNameFromToken(providedToken)
        });
    }
}
