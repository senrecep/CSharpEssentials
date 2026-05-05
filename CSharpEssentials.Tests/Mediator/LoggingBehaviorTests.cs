using Mediator;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

namespace CSharpEssentials.Tests.Mediator;

public sealed record TestLoggableCommand(string Name) : ICommand<Result>, IRequestLoggable;
public sealed record TestResponseLoggableCommand(string Name) : ICommand<Result>, IResponseLoggable;
public sealed record TestRequestResponseLoggableCommand(string Name) : ICommand<Result>, IRequestResponseLoggable;

public class LoggingBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestLoggableCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Handle_Should_Log_Basic_Request_When_Not_Loggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestLoggableCommand, Result>>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger.Object);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Log_Request_Payload_When_IRequestLoggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestLoggableCommand, Result>>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger.Object);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_Should_Log_Response_Payload_When_IResponseLoggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestResponseLoggableCommand, Result>>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var behavior = new LoggingBehavior<TestResponseLoggableCommand, Result>(logger.Object);
        var command = new TestResponseLoggableCommand("test");

        await behavior.Handle(command, (msg, ct) => new ValueTask<Result>(Result.Success()), default);

        // Must log "Handled {ResponseName} response: {ResponseJson}" — not just "Handled {ResponseName} response"
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled") && v.ToString()!.Contains("response:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Log_Both_Payloads_When_IRequestResponseLoggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestRequestResponseLoggableCommand, Result>>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var behavior = new LoggingBehavior<TestRequestResponseLoggableCommand, Result>(logger.Object);
        var command = new TestRequestResponseLoggableCommand("test");

        await behavior.Handle(command, (msg, ct) => new ValueTask<Result>(Result.Success()), default);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling") && v.ToString()!.Contains("request:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled") && v.ToString()!.Contains("response:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Log_Elapsed_Time()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestLoggableCommand, Result>>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var behavior = new LoggingBehavior<TestLoggableCommand, Result>(logger.Object);
        var command = new TestLoggableCommand("test");

        await behavior.Handle(command, SuccessNext, default);

        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("took")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
