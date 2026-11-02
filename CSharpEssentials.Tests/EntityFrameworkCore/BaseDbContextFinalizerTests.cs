using System.Runtime.CompilerServices;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Tests.Core;
using CSharpEssentials.Tests.Endpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

[Collection(NonParallelGcCollection.Name)]
public class BaseDbContextFinalizerTests
{
    private sealed class FinalizerDbContext(
        DbContextOptions<FinalizerDbContext> options,
        IServiceScopeFactory serviceScopeFactory)
        : BaseDbContext<FinalizerDbContext>(options, serviceScopeFactory);

    private static int CountLogs(CapturingLoggerProvider logs, string suffix) =>
        logs.Entries.Count(e => e.Message.EndsWith(suffix, StringComparison.Ordinal));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateContexts(ServiceProvider provider, int count, bool dispose)
    {
        for (int i = 0; i < count; i++)
        {
            var context = new FinalizerDbContext(
                new DbContextOptionsBuilder<FinalizerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                provider.GetRequiredService<IServiceScopeFactory>());
            if (dispose)
                context.Dispose();
        }
    }

    private static void CollectAll()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    [Fact]
    public void DisposedContext_ShouldNotRunFinalizer()
    {
        var logs = new CapturingLoggerProvider();
        using ServiceProvider provider = new ServiceCollection().AddLogging(b => b.AddProvider(logs)).BuildServiceProvider();

        CreateContexts(provider, 10, dispose: true);
        CollectAll();

        CountLogs(logs, " disposed").Should().Be(10);
        CountLogs(logs, " destructed").Should().Be(0);
    }

    [Fact]
    public void UndisposedContext_ShouldStillLogFromFinalizer()
    {
        var logs = new CapturingLoggerProvider();
        using ServiceProvider provider = new ServiceCollection().AddLogging(b => b.AddProvider(logs)).BuildServiceProvider();

        CreateContexts(provider, 10, dispose: false);
        CollectAll();

        CountLogs(logs, " destructed").Should().BeGreaterThan(0);
    }
}
