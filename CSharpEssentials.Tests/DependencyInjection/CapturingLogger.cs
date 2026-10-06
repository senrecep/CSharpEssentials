using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.DependencyInjection;

internal sealed class CapturingLogger(LogLevel minimumLevel = LogLevel.Trace) : ILogger
{
    public List<(LogLevel Level, EventId EventId, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, eventId, formatter(state, exception)));
}
