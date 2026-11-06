using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>A logging provider that renders every message it receives, as a real provider would, and only counts it.</summary>
public sealed class CountingLoggerProvider : ILoggerProvider
{
    /// <summary>The counts of this provider.</summary>
    public LogTally Tally { get; } = new();

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new CountingLogger(Tally);

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
