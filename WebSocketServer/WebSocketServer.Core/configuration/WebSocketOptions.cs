using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace WebSocketGuide.Core.Configuration;

public class WebSocketOptions : IValidateOptions<WebSocketOptions>
{
    public const string SectionName = "WebSocket";

    [Range(1, 100000)]
    public int MaxConnections { get; set; } = 10000;

    [Range(1024, 1024 * 1024)]  // 1KB to 1MB
    public int ReceiveBufferSize { get; set; } = 4 * 1024;

    [Range(1, 300)]  // 1 to 300 seconds
    public int KeepAliveIntervalSeconds { get; set; } = 30;

    [Range(1, 3600)]  // 1 second to 1 hour
    public int CloseTimeoutSeconds { get; set; } = 30;

    [Range(1024, 10 * 1024 * 1024)]  // 1KB to 10MB
    public long MaxMessageSize { get; set; } = 1024 * 1024; // 1MB

    public bool EnableCompression { get; set; } = true;

    public string[] AllowedOrigins { get; set; } = { "*" };

    public bool RequireAuthentication { get; set; } = false;

    public string AuthenticationScheme { get; set; } = "Bearer";

    public RateLimitOptions RateLimit { get; set; } = new();


    public ValidateOptionsResult Validate(string? name, WebSocketOptions options)
    {
        var failures = new List<string>();

        if (options.MaxConnections <= 0)
            failures.Add("MaxConnections must be greater than 0");

        if (options.ReceiveBufferSize < 1024)
            failures.Add("ReceiveBufferSize must be at least 1024 bytes");

        if (options.MaxMessageSize <= 0)
            failures.Add("MaxMessageSize must be greater than 0");

        if (failures.Count > 0)
            return ValidateOptionsResult.Fail(failures);

        return ValidateOptionsResult.Success;
    }
}

public class RateLimitOptions
{
    public bool Enabled { get; set; } = true;

    [Range(1, 10000)]
    public int RequestsPerMinute { get; set; } = 60;

    [Range(1, 1000)]
    public int BurstSize { get; set; } = 10;

    public TimeSpan WindowSize { get; set; } = TimeSpan.FromMinutes(1);
}
