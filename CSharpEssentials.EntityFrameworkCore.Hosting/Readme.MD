# CSharpEssentials.EntityFrameworkCore.Hosting

A hosted service that drives the outbox processor of `CSharpEssentials.EntityFrameworkCore`. It calls `IOutboxProcessor.ProcessBatchAsync` in a loop while the host runs, so stored domain events reach your `IOutboxMessageHandler` without a scheduler of your own.

It is a separate package because it depends on `Microsoft.Extensions.Hosting.Abstractions`, which `CSharpEssentials.EntityFrameworkCore` does not. Without it, call `ProcessBatchAsync` from any loop or job.

## Install

```bash
dotnet add package CSharpEssentials.EntityFrameworkCore.Hosting
```

Targets net10.0, net9.0 and net8.0, each on the EF Core major of `CSharpEssentials.EntityFrameworkCore`.

## Usage

```csharp
using CSharpEssentials.EntityFrameworkCore.Hosting;
using CSharpEssentials.EntityFrameworkCore.Outbox;

builder.Services.AddEfCoreOutbox(types => types.Add("orders.placed", AppJsonContext.Default.OrderPlaced));
builder.Services.AddScoped<IOutboxMessageHandler, BrokerOutboxHandler>();
builder.Services.AddOutboxProcessor<AppDbContext>(options =>
{
    options.PollingInterval = TimeSpan.FromSeconds(2);
    options.Claimer = new SqlOutboxMessageClaimer(OutboxClaimDialect.PostgreSql);
});
builder.Services.AddOutboxProcessorHostedService();
```

| Member | What it does |
|--------|-------------|
| `AddOutboxProcessorHostedService()` | Registers `OutboxProcessorHostedService`. Needs `AddOutboxProcessor<TContext>()` |
| `OutboxProcessorHostedService` | After a full batch it runs the next one at once. After a smaller batch, or a batch that threw (logged as an error), it waits `OutboxProcessorOptions.PollingInterval` (5 seconds by default) on the registered `TimeProvider`. Stopping the host cancels the batch in progress; its claimed rows are claimed again when their lease ends |

Every application instance runs its own loop. The claimer decides which instance gets which row; see [Outbox](../CSharpEssentials.EntityFrameworkCore/Readme.MD#outbox).
