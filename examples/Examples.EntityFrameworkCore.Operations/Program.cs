using System.Text.Json.Serialization;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.HealthChecks;
using CSharpEssentials.EntityFrameworkCore.Hosting;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Locking;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using CSharpEssentials.Locking;
using CSharpEssentials.Maybe;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

const string ConnectionString = "Data Source=operations-demo;Mode=Memory;Cache=Shared";
using SqliteConnection keepAlive = new(ConnectionString);
await keepAlive.OpenAsync();

ServiceCollection services = new();
services.AddLogging();
services.AddAuditInterceptor(() => "demo-user");
services.AddDbContext<OpsDb>(options => options.UseSqlite(ConnectionString));

services.AddHealthChecks().AddMigrationsCheck<OpsDb>();

services.AddSingleton<Delivered>();
services.AddEfCoreOutbox(types => types.Add("orders.placed", OpsJson.Default.OrderPlaced));
services.AddScoped<IOutboxMessageHandler, PrintingHandler>();
services.AddOutboxProcessor<OpsDb>(options => options.PollingInterval = TimeSpan.FromMilliseconds(100));
services.AddOutboxProcessorHostedService();

services.AddRelationalResourceLock<OpsDb>(OpsDb.TableLockDialect, options => options.PollInterval = TimeSpan.FromMilliseconds(20));

await using ServiceProvider provider = services.BuildServiceProvider();
await using (AsyncServiceScope scope = provider.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<OpsDb>().Database.EnsureCreatedAsync();

Console.WriteLine("1. Migrations health check");
HealthReport report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();
foreach ((string name, HealthReportEntry entry) in report.Entries)
    Console.WriteLine($"   {name} [{string.Join(",", entry.Tags)}]: {entry.Status}. {entry.Description}");
Console.WriteLine("   (This project has no migrations, so the model has changes no migration covers: Degraded.)");

Console.WriteLine("2. Outbox relayed by the hosted service");
IHostedService[] hosted = [.. provider.GetServices<IHostedService>()];
foreach (IHostedService service in hosted)
    await service.StartAsync(CancellationToken.None);

await using (AsyncServiceScope scope = provider.CreateAsyncScope())
{
    OpsDb db = scope.ServiceProvider.GetRequiredService<OpsDb>();
    db.Orders.Add(Order.Place("customer-1"));
    await db.SaveChangesAsync();
    Console.WriteLine($"   Saved 1 order and {await db.Set<OutboxMessage>().CountAsync()} outbox row");
}

OutboxMessage relayed = await provider.GetRequiredService<Delivered>().Task.WaitAsync(TimeSpan.FromSeconds(10));
Console.WriteLine($"   Handler received {relayed.Type} (message id {relayed.Id})");

foreach (IHostedService service in hosted)
    await service.StopAsync(CancellationToken.None);

Console.WriteLine("3. Relational resource lock");
IResourceLock resourceLock = provider.GetRequiredService<IResourceLock>();
await using (IAsyncDisposable held = await resourceLock.AcquireAsync("order:1", TimeSpan.FromSeconds(5)))
{
    Maybe<IAsyncDisposable> second = await resourceLock.TryAcquireAsync("order:1");
    Console.WriteLine($"   Second TryAcquireAsync while held: {(second.HasValue ? "acquired" : "busy")}");

    try
    {
        await resourceLock.AcquireAsync("order:1", TimeSpan.FromMilliseconds(100));
    }
    catch (TimeoutException ex)
    {
        Console.WriteLine($"   AcquireAsync with a 100 ms timeout: {ex.GetType().Name}");
    }
}

Maybe<IAsyncDisposable> afterRelease = await resourceLock.TryAcquireAsync("order:1");
Console.WriteLine($"   TryAcquireAsync after release: {(afterRelease.HasValue ? "acquired" : "busy")}");
if (afterRelease.HasValue)
    await afterRelease.Value.DisposeAsync();

public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

[JsonSerializable(typeof(OrderPlaced))]
internal sealed partial class OpsJson : JsonSerializerContext;

public sealed class Order : EntityBase<Guid>
{
    private Order() { }

    public string CustomerId { get; private set; } = "";

    public static Order Place(string customerId)
    {
        Order order = new() { Id = Guid.NewGuid(), CustomerId = customerId };
        order.Raise(new OrderPlaced(order.Id));
        return order;
    }
}

public sealed class LockRow
{
    public string Key { get; set; } = "";
}

public sealed class OpsDb(DbContextOptions<OpsDb> options, IServiceScopeFactory scopes) : BaseDbContext<OpsDb>(options, scopes)
{
    public static readonly ResourceLockSqlDialect TableLockDialect = new(
        "INSERT INTO \"locks\" (\"Key\") VALUES ({0}) ON CONFLICT DO NOTHING RETURNING 1 AS \"Value\"",
        "DELETE FROM \"locks\" WHERE \"Key\" = {0}");

    public DbSet<Order> Orders => Set<Order>();

    protected override DbContextInterceptors InterceptorsFromServices => DbContextInterceptors.Audit;

    protected override bool DispatchDomainEventsOnSaveChanges => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().EntityBaseGuidIdMap();
        modelBuilder.Entity<LockRow>(b =>
        {
            b.ToTable("locks");
            b.HasKey(x => x.Key);
        });
        modelBuilder.AddOutboxMessages();
    }
}

public sealed class Delivered
{
    private readonly TaskCompletionSource<OutboxMessage> _source = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<OutboxMessage> Task => _source.Task;

    public void Set(OutboxMessage message) => _source.TrySetResult(message);
}

public sealed class PrintingHandler(Delivered delivered) : IOutboxMessageHandler
{
    public Task HandleAsync(OutboxMessage message, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        delivered.Set(message);
        return Task.CompletedTask;
    }
}
