using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class BaseDbContextTests
{
    private sealed class ConcreteDbContext : BaseDbContext<ConcreteDbContext>
    {
        public ConcreteDbContext(DbContextOptions<ConcreteDbContext> options, IServiceScopeFactory serviceScopeFactory) : base(options, serviceScopeFactory) { }
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        public List<string> Logs { get; } = new();
        public ILogger CreateLogger(string categoryName) => new ListLogger(Logs);
        public void Dispose() { }
        private sealed class ListLogger : ILogger
        {
            private readonly List<string> _logs;
            public ListLogger(List<string> logs) => _logs = logs;
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _logs.Add(formatter(state, exception));
            }
        }
    }

    private static (ConcreteDbContext context, List<string> logs) CreateContext()
    {
        using var loggerProvider = new ListLoggerProvider();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerProvider>(loggerProvider);
        services.AddLogging();
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var options = new DbContextOptionsBuilder<ConcreteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ConcreteDbContext(options, scopeFactory);
        return (context, loggerProvider.Logs);
    }

    [Fact]
    public void Constructor_ShouldCreateContextSuccessfully()
    {
        var (context, logs) = CreateContext();
        logs.Should().ContainSingle(s => s.Contains("created"));
        context.Dispose();
    }

    [Fact]
    public void Dispose_ShouldLogDisposal()
    {
        var (context, logs) = CreateContext();
        context.Dispose();
        logs.Should().Contain(s => s.Contains("disposed"));
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var (context, _) = CreateContext();
        Action act = () => context.Dispose();
        act.Should().NotThrow();
    }
}
