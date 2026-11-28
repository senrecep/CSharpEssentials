using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CSharpEssentials.Idempotency;
using Examples.Idempotency;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

ServiceCollection services = new();
services.AddLogging(logging => logging.AddSimpleConsole(o => o.SingleLine = true).SetMinimumLevel(LogLevel.Warning));
services.AddIdempotencyExecutor(options =>
{
    options.AddProfile("payments", profile =>
    {
        profile.Retention = TimeSpan.FromDays(7);
        profile.Lease = TimeSpan.FromMinutes(5);
    });
    options.AddProfile("reports", profile =>
    {
        profile.Mode = IdempotencyMode.Observe;
        profile.StoreKey = "cache";
    });
});
services.AddKeyedSingleton<IIdempotencyRecordStore>("cache",
    new DistributedCacheIdempotencyRecordStore(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();
IdempotencyInbox inbox = scope.ServiceProvider.GetRequiredService<IdempotencyInbox>();

using MeterListener listener = new();
Dictionary<string, int> decisions = [];
listener.InstrumentPublished = (instrument, l) =>
{
    if (instrument.Meter.Name == IdempotencyTelemetry.MeterName)
        l.EnableMeasurementEvents(instrument);
};
listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
{
    foreach (KeyValuePair<string, object?> tag in tags)
    {
        if (tag.Key == "decision")
            decisions[(string)tag.Value!] = decisions.GetValueOrDefault((string)tag.Value!) + (int)value;
    }
});
listener.Start();

IIdempotencyResultCodec<Receipt> receipts = IdempotencyResultCodec.Create<Receipt>(
    receipt => JsonSerializer.SerializeToUtf8Bytes(receipt),
    payload => JsonSerializer.Deserialize<Receipt>(payload.Span)!,
    receipt => receipt.Declined ? IdempotencyResultHandling.StoreAsFailure : IdempotencyResultHandling.Store);
IdempotencyExecutionOptions payments = new() { Profile = "payments" };
int charges = 0;

ValueTask<Receipt> Charge(decimal amount)
{
    charges++;
    return ValueTask.FromResult(new Receipt(Guid.NewGuid(), amount, Declined: false));
}

Section("Run an operation once");
IdempotencyRecordKey key = new("payments.capture", IdempotencyScope.Tenant("acme"), "order-42");
IdempotencyExecution<Receipt> first = await executor.ExecuteAsync(key, "amount=10", payments, _ => Charge(10m), receipts);
IdempotencyExecution<Receipt> retry = await executor.ExecuteAsync(key, "amount=10", payments, _ => Charge(10m), receipts);
IdempotencyExecution<Receipt> reused = await executor.ExecuteAsync(key, "amount=99", payments, _ => Charge(99m), receipts);
Console.WriteLine($"first: {first.Status}, retry: {retry.Status} (same receipt {first.Value!.Id == retry.Value!.Id}), other amount: {reused.Status}, charges: {charges}");

IdempotencyRecordKey otherTenant = new("payments.capture", IdempotencyScope.Tenant("globex"), "order-42");
IdempotencyExecution<Receipt> other = await executor.ExecuteAsync(otherTenant, "amount=10", payments, _ => Charge(10m), receipts);
Console.WriteLine($"same key, other tenant: {other.Status}");

Section("In flight and exceptions");
TaskCompletionSource release = new();
IdempotencyRecordKey slowKey = new("payments.capture", IdempotencyScope.Tenant("acme"), "order-43");
Task<IdempotencyExecution<Receipt>> running = executor.ExecuteAsync(slowKey, "amount=5", payments, async token =>
{
    await release.Task;
    return new Receipt(Guid.NewGuid(), 5m, Declined: false);
}, receipts).AsTask();
IdempotencyExecution<Receipt> concurrent = await executor.ExecuteAsync(slowKey, "amount=5", payments, _ => Charge(5m), receipts);
release.SetResult();
Console.WriteLine($"while the first run works: {concurrent.Status}, first run: {(await running).Status}");

IdempotencyRecordKey crashKey = new("payments.capture", IdempotencyScope.Tenant("acme"), "order-44");
try
{
    await executor.ExecuteAsync<Receipt>(crashKey, "amount=1", payments, _ => throw new HttpRequestException("provider timed out"), receipts);
}
catch (HttpRequestException)
{
    IdempotencyExecution<Receipt> afterCrash = await executor.ExecuteAsync(crashKey, "amount=1", payments, _ => Charge(1m), receipts);
    Console.WriteLine($"after the operation threw, a retry within the lease: {afterCrash.Status}");
}

Section("Process a message once");
int receiptsSent = 0;
for (int delivery = 1; delivery <= 2; delivery++)
{
    IdempotencyExecutionStatus status = await inbox.ProcessAsync(
        consumer: "billing.send-receipt",
        messageId: "msg-1001",
        scope: IdempotencyScope.Tenant("acme"),
        handler: _ =>
        {
            receiptsSent++;
            return ValueTask.CompletedTask;
        });
    Console.WriteLine($"delivery {delivery}: {status}");
}
Console.WriteLine($"receipts sent: {receiptsSent}");

Section("Observe mode on a keyed store");
IdempotencyExecutionOptions reports = new() { Profile = "reports" };
IdempotencyRecordKey reportKey = new("reports.build", IdempotencyScope.Global, "daily-2026-11-28");
IIdempotencyResultCodec<string> text = IdempotencyResultCodec.Create<string>(
    value => Encoding.UTF8.GetBytes(value),
    payload => Encoding.UTF8.GetString(payload.Span));
int builds = 0;
for (int run = 1; run <= 2; run++)
{
    IdempotencyExecution<string> report = await executor.ExecuteAsync(reportKey, "daily", reports, _ =>
    {
        builds++;
        return ValueTask.FromResult("report");
    }, text);
    Console.WriteLine($"run {run}: {report.Status}");
}
Console.WriteLine($"Observe never blocks, builds: {builds}");

Section("Telemetry");
Console.WriteLine(string.Join(", ", decisions.OrderBy(d => d.Key).Select(d => $"{d.Key}={d.Value}")));

Section("Deterministic keys");
Console.WriteLine($"For: {IdempotencyKeys.For("refund", "order-42", "refund-1")}");
Console.WriteLine($"FromMessage: {IdempotencyKeys.FromMessage("msg-1001", "billing.send-receipt")}");
Console.WriteLine($"ToUuidV5: {IdempotencyKeys.ToUuidV5("refund", "order-42", "refund-1")}");
Console.WriteLine($"non-ASCII part is hashed: {IdempotencyKeys.For("refund", "sipariş-42")}");
Console.WriteLine($"IsValid(\"a b\"): {IdempotencyKeys.IsValid("a b")}");

Section("Scopes");
ClaimsPrincipal user = new(new ClaimsIdentity([new Claim("sub", "u-7"), new Claim("tenant_id", "acme")], "jwt"));
ClaimsPrincipal machine = new(new ClaimsIdentity([new Claim("client_id", "billing-service")], "jwt"));
Console.WriteLine($"User: {IdempotencyScopes.User.Resolve(user)}");
Console.WriteLine($"UserAndTenant: {IdempotencyScopes.UserAndTenant("tenant_id").Resolve(user)}");
Console.WriteLine($"Client: {IdempotencyScopes.Client.Resolve(machine)}");
Console.WriteLine($"User for a machine caller: {IdempotencyScopes.User.Resolve(machine)?.ToString() ?? "null (unresolved)"}");
Console.WriteLine($"Custom: {IdempotencyScopes.Custom("account", p => p?.FindFirst("tenant_id")?.Value).Resolve(user)}");
Console.WriteLine($"storage key: {key.ToStorageKey()}");

Section("Use the store directly");
InMemoryIdempotencyRecordStore store = new();
IdempotencyRecordKey importKey = new("imports", IdempotencyScope.Global, IdempotencyKeys.For("import", "file-9"));
IdempotencyRecordReservation reservation = await store.TryReserveAsync(importKey, "sha256-abc", TimeSpan.FromMinutes(1));
Console.WriteLine($"reserve: {reservation.Status}");
Console.WriteLine($"reserve again: {(await store.TryReserveAsync(importKey, "sha256-abc", TimeSpan.FromMinutes(1))).Status}");
await store.CompleteAsync(importKey, reservation.Token!, "sha256-abc", "imported 12 rows"u8.ToArray(), TimeSpan.FromDays(1));
IdempotencyRecordReservation completed = await store.TryReserveAsync(importKey, "sha256-abc", TimeSpan.FromMinutes(1));
Console.WriteLine($"after complete: {completed.Status}, payload: {Encoding.UTF8.GetString(completed.Payload!.Value.Span)}");
Console.WriteLine($"other content: {(await store.TryReserveAsync(importKey, "sha256-def", TimeSpan.FromMinutes(1))).Status}");

return 0;

static void Section(string title) => Console.WriteLine($"{Environment.NewLine}--- {title} ---");
