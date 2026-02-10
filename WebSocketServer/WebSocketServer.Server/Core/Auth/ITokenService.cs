namespace WebSocketServer.Core.Auth;

public interface ITokenService
{
    string GenerateToken(int userId, string userName);
    bool ValidateToken(string token);
    int? GetUserIdFromToken(string token);
    string? GetUserNameFromToken(string token);
}
