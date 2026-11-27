using CSharpEssentials.EntityFrameworkCore.Hosting;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Outbox;

public sealed class OutboxProcessorHostedServiceTests
{
    private sealed class ScriptedProcessor(params Func<int>[] batches) : IOutboxProcessor
    {
        private int _calls;

        public int Calls => Volatile.Read(ref _calls);

        public Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
        {
            int call = Interlocked.Increment(ref _calls) - 1;
            return Task.FromResult(call < batches.Length ? batches[call]() : 0);
        }
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        while (!condition())
            await Task.Delay(10, timeout.Token);
    }

    [Fact]
    public async Task ExecuteAsync_Should_RunNextBatchAtOnce_When_BatchWasFull()
    {
        FakeTimeProvider clock = new();
        ScriptedProcessor processor = new(() => 2, () => 2, () => 0);
        OutboxProcessorHostedService service = new(processor, new OutboxProcessorOptions { BatchSize = 2 }, NullLogger<OutboxProcessorHostedService>.Instance, clock);

        await service.StartAsync(CancellationToken.None);
        await WaitForAsync(() => processor.Calls >= 3);
        await Task.Delay(50);
        int callsBeforeInterval = processor.Calls;
        clock.Advance(TimeSpan.FromSeconds(5));
        await WaitForAsync(() => processor.Calls >= 4);
        await service.StopAsync(CancellationToken.None);

        callsBeforeInterval.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_Should_WaitPollingInterval_When_BatchThrows()
    {
        FakeTimeProvider clock = new();
        ScriptedProcessor processor = new(() => throw new InvalidOperationException("db down"));
        OutboxProcessorHostedService service = new(processor, new OutboxProcessorOptions(), NullLogger<OutboxProcessorHostedService>.Instance, clock);

        await service.StartAsync(CancellationToken.None);
        await WaitForAsync(() => processor.Calls >= 1);
        await Task.Delay(50);
        int callsBeforeInterval = processor.Calls;
        clock.Advance(TimeSpan.FromSeconds(5));
        await WaitForAsync(() => processor.Calls >= 2);
        await service.StopAsync(CancellationToken.None);

        callsBeforeInterval.Should().Be(1);
    }

    [Fact]
    public void AddOutboxProcessorHostedService_Should_RegisterTheHostedService()
    {
        ServiceCollection services = new();

        services.AddOutboxProcessorHostedService();

        services.Should().ContainSingle(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(OutboxProcessorHostedService));
    }
}
