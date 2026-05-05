using Mediator;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Mediator;

internal sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly List<string> _messages = [];
    public IReadOnlyList<string> Messages => _messages;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel >= LogLevel.Information)
            _messages.Add(formatter(state, exception));
    }
}

internal sealed record TestLoggableCommand(string Name) : ICommand<Result>, IRequestLoggable;
internal sealed record TestResponseLoggableCommand(string Name) : ICommand<Result>, IResponseLoggable;
internal sealed record TestRequestResponseLoggableCommand(string Name) : ICommand<Result>, IRequestResponseLoggable;

public class LoggingBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestLoggableCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Handle_Should_Log_Basic_Request_When_Not_Loggable()
    {
        var logger = new CapturingLogger<LoggingBehavior<TestLoggableCommand, Result>>();
        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Messages.Should().Contain(m => m.Contains("Handling"));
    }

    [Fact]
    public async Task Handle_Should_Log_Request_Payload_When_IRequestLoggable()
    {
        var logger = new CapturingLogger<LoggingBehavior<TestLoggableCommand, Result>>();
        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Messages.Should().Contain(m => m.Contains("test"));
    }

    [Fact]
    public async Task Handle_Should_Log_Response_Payload_When_IResponseLoggable()
    {
        var logger = new CapturingLogger<LoggingBehavior<TestResponseLoggableCommand, Result>>();
        var behavior = new LoggingBehavior<TestResponseLoggableCommand, Result>(logger);
        var command = new TestResponseLoggableCommand("test");

        await behavior.Handle(command, (msg, ct) => new ValueTask<Result>(Result.Success()), default);

        logger.Messages.Should().Contain(m => m.Contains("Handled") && m.Contains("response:"));
    }

    [Fact]
    public async Task Handle_Should_Log_Both_Payloads_When_IRequestResponseLoggable()
    {
        var logger = new CapturingLogger<LoggingBehavior<TestRequestResponseLoggableCommand, Result>>();
        var behavior = new LoggingBehavior<TestRequestResponseLoggableCommand, Result>(logger);
        var command = new TestRequestResponseLoggableCommand("test");

        await behavior.Handle(command, (msg, ct) => new ValueTask<Result>(Result.Success()), default);

        logger.Messages.Should().Contain(m => m.Contains("Handling") && m.Contains("request:"));
        logger.Messages.Should().Contain(m => m.Contains("Handled") && m.Contains("response:"));
    }

    [Fact]
    public async Task Handle_Should_Log_Elapsed_Time()
    {
        var logger = new CapturingLogger<LoggingBehavior<TestLoggableCommand, Result>>();
        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Messages.Should().Contain(m => m.Contains("took"));
    }
}
