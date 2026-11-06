using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>Renders each message with its formatter and adds it to a <see cref="LogTally"/>; the factory's filters decide what reaches it.</summary>
public sealed class CountingLogger(LogTally tally) : ILogger
{
    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        tally.Record(formatter(state, exception));
    }
}
