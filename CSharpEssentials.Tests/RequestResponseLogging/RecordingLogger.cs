using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.RequestResponseLogging;

internal sealed class RecordingLogger(bool enabled) : ILogger
{
    public List<(string Message, IReadOnlyList<KeyValuePair<string, object?>> State)> Entries { get; } = [];

    public ILoggerFactory CreateFactory()
    {
        var factory = new Mock<ILoggerFactory>();
        factory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(this);
        return factory.Object;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => enabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((formatter(state, exception), ((IReadOnlyList<KeyValuePair<string, object?>>)state!).ToList()));
}
