using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>An enabled logger that formats every message and counts its length in <see cref="LogSink"/>, so the formatting work cannot be skipped.</summary>
/// <typeparam name="T">The category.</typeparam>
public sealed class CountingLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);
        LogSink.Characters += message.Length;
        if (LogSink.Capture)
        {
            LogSink.Messages.Add(message);
        }
    }
}
