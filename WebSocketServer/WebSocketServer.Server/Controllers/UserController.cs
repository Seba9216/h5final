using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSocketServer.Core.context;
using WebSocketServer.Models;

namespace WebSocketServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class UserController : ControllerBase
{
    private readonly DuckingContext _dbContext;

    public UserController(DuckingContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] DuckingUser user)
    {
        if (user == null)
        {
            return BadRequest("Invalid user data");
        }

        _dbContext.users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { id = user.Id, message = "User created successfully" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _dbContext.users.FindAsync(id);
        
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

        var user = await _dbContext.users
            .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        return Ok(new { id = user.Id, userName = user.UserName, message = "Login successful" });
    }
}
