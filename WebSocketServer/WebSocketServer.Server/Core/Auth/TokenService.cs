using System.Collections.Concurrent;

namespace WebSocketServer.Core.Auth;

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, TokenInfo> _tokens = new();

    public string GenerateToken(int userId, string userName)
    {
        // Invalidate any existing token for this user
        var existingToken = _tokens.FirstOrDefault(t => t.Value.UserId == userId);
        if (existingToken.Key != null)
        {
            _tokens.TryRemove(existingToken.Key, out _);
        }

        var token = Guid.NewGuid().ToString("N");
        _tokens.TryAdd(token, new TokenInfo(userId, userName));
        return token;
    }

    public bool ValidateToken(string token)
    {
        return !string.IsNullOrEmpty(token) && _tokens.ContainsKey(token);
    }

    public int? GetUserIdFromToken(string token)
    {
        if (_tokens.TryGetValue(token, out var info))
        {
            return info.UserId;
        }
        return null;
    }

    public string? GetUserNameFromToken(string token)
    {
        if (_tokens.TryGetValue(token, out var info))
        {
            return info.UserName;
        }
        return null;
    }

    private record TokenInfo(int UserId, string UserName);
}
