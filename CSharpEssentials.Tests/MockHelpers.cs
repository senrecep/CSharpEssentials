using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests;

internal static class MockHelpers
{
    public static Mock<T> CreateMock<T>() where T : class => new();

    public static Mock<ILogger<T>> CreateLoggerMock<T>() => new();

    public static Mock<ILoggerFactory> CreateLoggerFactoryMock()
    {
        Mock<ILoggerFactory> mock = new();
        Mock<ILogger> loggerMock = new();
        mock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        return mock;
    }

    public static DbContextOptions<TContext> CreateInMemoryOptions<TContext>(string? databaseName = null)
        where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
    }

    public static void VerifyLog<T>(Mock<ILogger<T>> loggerMock, LogLevel level, string? messageSubstring = null, Times? times = null)
    {
        times ??= Times.Once();
        loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => messageSubstring == null || v.ToString()!.Contains(messageSubstring)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times.Value);
    }

    public static void VerifyLogContains<T>(Mock<ILogger<T>> loggerMock, LogLevel level, string messageSubstring, Times? times = null)
    {
        VerifyLog(loggerMock, level, messageSubstring, times);
    }
}
