using System.Text.Json;

namespace Examples.RequestResponseLogging.Infrastructure;

/// <summary>
/// Custom structured JSON logger for request/response information.
/// This can be plugged into the UseHandler delegate for complete control.
/// </summary>
public static class StructuredJsonLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false // Compact JSON for logging systems
    };

    public static Task WriteAsync(CSharpEssentials.RequestResponseLogging.RequestResponseContext context, ILogger logger)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Url = context.Url,
            Duration = context.ResponseTime,
            RequestLength = context.RequestLength,
            ResponseLength = context.ResponseLength,
            RequestBody = Truncate(context.RequestBody, 1000),
            ResponseBody = Truncate(context.ResponseBody, 1000)
        };

        string json = JsonSerializer.Serialize(logEntry, JsonOptions);

        logger.LogInformation("HTTP request completed in {Duration} | {Json}",
            context.ResponseTime,
            json);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Truncates long bodies to prevent huge log entries.
    /// </summary>
    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        return value[..maxLength] + $"... [truncated, total length: {value.Length}]";
    }
}
