# CSharpEssentials.EntityFrameworkCore.Hosting

A hosted service that runs the outbox processor of `CSharpEssentials.EntityFrameworkCore` in the background. It calls `IOutboxProcessor.ProcessBatchAsync` in a loop while the host runs, so stored domain events reach your `IOutboxMessageHandler` without a scheduler of your own.

It is a separate package because it depends on `Microsoft.Extensions.Hosting.Abstractions`, which `CSharpEssentials.EntityFrameworkCore` does not. Without it, call `ProcessBatchAsync` from any loop or job.

## Install

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore.Hosting
```

Targets `net10.0`, `net9.0` and `net8.0`, each with the EF Core major of `CSharpEssentials.EntityFrameworkCore`.

## Quick start

The program saves an order, and the hosted service relays its `OrderPlaced` event to a handler. It starts the hosted services by hand so it can stop after one message; a real app lets the host do that.

```csharp Program.cs
// dotnet add package CSharpEssentials.EntityFrameworkCore.Hosting
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.1
using System.Text.Json.Serialization;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Hosting;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Shared in-memory database: the processor opens its own connections in its own scopes.
const string Db = "Data Source=hosting-demo;Mode=Memory;Cache=Shared";
using SqliteConnection keepAlive = new(Db);
keepAlive.Open();

ServiceCollection services = new();
services.AddLogging();
services.AddAuditInterceptor(() => "system");
services.AddDbContext<AppDb>(options => options.UseSqlite(Db));
services.AddSingleton<Inbox>();
services.AddEfCoreOutbox(types => types.Add("orders.placed", AppJson.Default.OrderPlaced));
services.AddScoped<IOutboxMessageHandler, InboxHandler>();
services.AddOutboxProcessor<AppDb>(options => options.PollingInterval = TimeSpan.FromMilliseconds(100));
services.AddOutboxProcessorHostedService();

await using ServiceProvider provider = services.BuildServiceProvider();
await using (AsyncServiceScope scope = provider.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreatedAsync();

IHostedService[] hosted = [.. provider.GetServices<IHostedService>()];
foreach (IHostedService service in hosted)
    await service.StartAsync(CancellationToken.None);

await using (AsyncServiceScope scope = provider.CreateAsyncScope())
{
    AppDb db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Orders.Add(Order.Place());
    await db.SaveChangesAsync();   // writes the order and one OutboxMessage row
}

IDomainEvent received = await provider.GetRequiredService<Inbox>().Received.Task.WaitAsync(TimeSpan.FromSeconds(10));
Console.WriteLine($"Relayed: {received}");   // Relayed: OrderPlaced { EventId = ..., OccurredAt = ..., OrderId = ... }

foreach (IHostedService service in hosted)
    await service.StopAsync(CancellationToken.None);

public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

[JsonSerializable(typeof(OrderPlaced))]
internal sealed partial class AppJson : JsonSerializerContext;

public sealed class Order : EntityBase<Guid>
{
    public static Order Place()
    {
        Order order = new() { Id = Guid.NewGuid() };
        order.Raise(new OrderPlaced(order.Id));
        return order;
    }
}

public sealed class AppDb(DbContextOptions<AppDb> options, IServiceScopeFactory scopes) : BaseDbContext<AppDb>(options, scopes)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.Audit;

    protected override bool DispatchDomainEventsOnSaveChanges => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().EntityBaseGuidIdMap();
        modelBuilder.AddOutboxMessages();
    }
}

public sealed class Inbox
{
    public TaskCompletionSource<IDomainEvent> Received { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class InboxHandler(Inbox inbox) : IOutboxMessageHandler
{
    public Task HandleAsync(OutboxMessage message, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        inbox.Received.TrySetResult(domainEvent);
        return Task.CompletedTask;
    }
}
```

## Registration

```csharp
services.AddEfCoreOutbox(types => types.Add("orders.placed", AppJson.Default.OrderPlaced));
services.AddScoped<IOutboxMessageHandler, BrokerHandler>();
services.AddOutboxProcessor<AppDb>(options =>
{
    options.PollingInterval = TimeSpan.FromSeconds(2);                         // default 5 s
    options.Claimer = new SqlOutboxMessageClaimer(OutboxClaimDialect.PostgreSql);
});
services.AddOutboxProcessorHostedService();
```

`AddOutboxProcessorHostedService()` registers `OutboxProcessorHostedService` with `AddHostedService`. Call `AddOutboxProcessor<TContext>()` too: it registers the `IOutboxProcessor` and the `OutboxProcessorOptions` the service reads. Without it the host fails to start.

## Loop behavior

- After a full batch (`BatchSize` rows claimed) the service runs the next batch at once.
- After a smaller batch, or a batch that threw, it waits `OutboxProcessorOptions.PollingInterval` (5 seconds by default) on the registered `TimeProvider`. A failed batch is logged as an error and the loop continues.
- Stopping the host cancels the batch in progress. Its claimed rows become due again when their lease ends.
- Every application instance runs its own loop. The claimer makes sure two instances never claim the same row.

Retries, dead letters, leases and the claimers are part of the outbox in the core package; see [Outbox](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#outbox).

## Links

- Agent skill: [csharpessentials-efcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-efcore/SKILL.md)
- Example project: [examples/Examples.EntityFrameworkCore.Operations](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.EntityFrameworkCore.Operations)
- Core package: [CSharpEssentials.EntityFrameworkCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD)
