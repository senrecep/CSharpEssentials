---
name: csharpessentials-idempotency
description: Use when an operation must run once per key and caller, or a retry or message redelivery must not repeat a side effect, outside HTTP and Mediator or under them. Covers AddIdempotencyExecutor, IIdempotencyExecutor.ExecuteAsync with IdempotencyRecordKey, codecs and profiles, IdempotencyInbox.ProcessAsync, IIdempotencyRecordStore (in-memory, distributed cache, EF Core, custom), IdempotencyScope and IdempotencyScopes, IdempotencyKeys.For/FromMessage/ToUuidV5, Observe to Enforce rollout and the idempotency.decisions counter.
---

# CSharpEssentials.Idempotency

Executor, inbox and record stores shared by the `Idempotency-Key` middleware (`CSharpEssentials.AspNetCore`), the
Mediator `IdempotencyBehavior` and message consumers. No reflection.

## Install

```bash
dotnet add package CSharpEssentials.Idempotency
dotnet add package CSharpEssentials.Idempotency.Testing   # test project, for a custom store
```

## Namespaces

```csharp
using CSharpEssentials.Idempotency;              // executor, inbox, stores, keys, scopes, options, telemetry names
using CSharpEssentials.Idempotency.Testing;      // IdempotencyRecordStoreContractTests (xUnit)
using Microsoft.Extensions.DependencyInjection;  // AddIdempotencyExecutor
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Idempotency
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
using System.Text.Json;
using CSharpEssentials.Idempotency;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();
services.AddIdempotencyExecutor(options =>
    options.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(7)));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();
IdempotencyInbox inbox = scope.ServiceProvider.GetRequiredService<IdempotencyInbox>();

IIdempotencyResultCodec<Receipt> codec = IdempotencyResultCodec.Create<Receipt>(
    receipt => JsonSerializer.SerializeToUtf8Bytes(receipt),
    payload => JsonSerializer.Deserialize<Receipt>(payload.Span)!);
IdempotencyRecordKey key = new("payments.capture", IdempotencyScope.Tenant("acme"), IdempotencyKeys.For("capture", "order-42"));
IdempotencyExecutionOptions payments = new() { Profile = "payments" };

IdempotencyExecution<Receipt> first = await executor.ExecuteAsync(
    key, "amount=10", payments, _ => ValueTask.FromResult(new Receipt(Guid.NewGuid(), 10m)), codec);
IdempotencyExecution<Receipt> retry = await executor.ExecuteAsync(
    key, "amount=10", payments, _ => ValueTask.FromResult(new Receipt(Guid.NewGuid(), 10m)), codec);
Console.WriteLine($"{first.Status} then {retry.Status}, same receipt: {first.Value!.Id == retry.Value!.Id}");

for (int delivery = 1; delivery <= 2; delivery++)
{
    IdempotencyExecutionStatus status = await inbox.ProcessAsync(
        consumer: "billing.send-receipt",
        messageId: "msg-1001",
        scope: IdempotencyScope.Tenant("acme"),
        handler: _ => ValueTask.CompletedTask);
    Console.WriteLine($"delivery {delivery}: {status}");
}

public sealed record Receipt(Guid Id, decimal Amount);
```

Prints `Executed then Replayed, same receipt: True`, `delivery 1: Executed`, `delivery 2: Replayed`.

## Running an operation once

`executor.ExecuteAsync(IdempotencyRecordKey key, string fingerprint, IdempotencyExecutionOptions? options, Func<CancellationToken, ValueTask<T>> operation, IIdempotencyResultCodec<T> codec, CancellationToken ct = default)`

| `IdempotencyExecutionStatus` | Meaning | `Value` |
|------------------------------|---------|---------|
| `Executed` | Ran now | Result |
| `Replayed` | Same key and fingerprint completed earlier | Stored result |
| `InFlight` | Another run holds the key; retry later | `default` |
| `KeyReused` | Same key, other fingerprint; do not retry | `default` |

- Key: `new IdempotencyRecordKey(namespace, IdempotencyScope? scope, key)`. `null` or `IdempotencyScope.Global` scope gives a global key.
- Codec: `IdempotencyResultCodec.Create<T>(serialize, deserialize, classify?)`. `classify` returns `Store` (default, `Retention`), `StoreAsFailure` (`FailureRetention`) or `Release` (free the key, for undone work).
- An exception from the operation is rethrown and the key stays in flight until `Lease` ends. A cancellation before the operation starts releases the key.

## Configuring the executor

```csharp
services.AddIdempotencyExecutor(o =>
{
    o.Mode = IdempotencyMode.Enforce;              // Off | Observe | Enforce (default)
    o.Retention = TimeSpan.FromHours(24);          // default
    o.Lease = TimeSpan.FromMinutes(1);             // default; <= Retention and <= MaxLease (15 min)
    o.FailureRetention = TimeSpan.FromMinutes(10); // default
    o.AddProfile("payments", p => { p.Retention = TimeSpan.FromDays(7); p.StoreKey = "payments"; });
});
```

- Per call: `new IdempotencyExecutionOptions { Profile, Mode, Retention, Lease, FailureRetention, StoreKey }`. Null settings come from the profile, then the defaults.
- `StoreKey` selects a keyed store: `services.AddKeyedSingleton<IIdempotencyRecordStore>("payments", store)` or `AddEfCoreIdempotencyRecordStore<AppDbContext>("payments")`.
- `AddIdempotencyExecutor` registers scoped `IIdempotencyExecutor` and `IdempotencyInbox`, and `InMemoryIdempotencyRecordStore` unless a store is already registered. Invalid options throw at registration.
- Options are read through `IOptionsMonitor` per call; a configuration reload switches `Mode` without a restart.

## Processing messages once

`inbox.ProcessAsync(consumer, messageId, scope, handler, options?, ct)` returns `IdempotencyExecutionStatus`; the
overload with a codec returns `IdempotencyExecution<T>`. Namespace `messaging:{consumer}`, key
`IdempotencyKeys.FromMessage(messageId, consumer)`, constant fingerprint, built-in `messaging` profile (7 days).
`Replayed`: duplicate, acknowledge. `InFlight`: redeliver later.

## Building keys

```csharp
string key = IdempotencyKeys.For("refund", orderId, refundId);          // "6:refund|8:order-42|8:refund-1"
string messageKey = IdempotencyKeys.FromMessage(messageId, consumer);  // messageId first
Guid uuid = IdempotencyKeys.ToUuidV5("refund", orderId, refundId);     // for providers that need a UUID
bool ok = IdempotencyKeys.IsValid(clientKey);                          // 1..255 printable ASCII, no space, " or \
```

Non-ASCII or long canonical strings become `h:` plus base64url SHA-256. Send the same key to downstream providers.

## Scoping keys

- Factories only: `IdempotencyScope.User(id)`, `Tenant(id)`, `Client(id)`, `UserAndTenant(userId, tenantId)`, `Custom(kind, value)`, `Global`. Kinds `user`, `tenant`, `client`, `global` are reserved for `Custom`.
- Resolvers from a `ClaimsPrincipal`: `IdempotencyScopes.User` (`NameIdentifier`, then `sub`), `Client` (`client_id`, then `azp`), `Tenant(claim)`, `UserAndTenant(tenantClaim)`, `Custom(kind, principal => value)`, `Global`. They return `null` when unresolved.
- Machine callers (client credentials) have no user claim: use `IdempotencyScopes.Client`.
- Unresolved scope: HTTP middleware defaults to `UnscopedKeyBehavior.Skip`, the Mediator behavior to `Reject` (`Idempotency.ScopeRequired`).

## Using a store directly

```csharp
InMemoryIdempotencyRecordStore store = new();
IdempotencyRecordReservation r = await store.TryReserveAsync(key, fingerprint, lease);   // Reserved | InFlight | Completed | FingerprintMismatch
if (r.Status == IdempotencyReservationStatus.Reserved)
    await store.CompleteAsync(key, r.Token!, fingerprint, payloadBytes, retention);    // or ReleaseAsync(key, r.Token!)
```

| Store | Use |
|-------|-----|
| `InMemoryIdempotencyRecordStore` | One process; atomic |
| `DistributedCacheIdempotencyRecordStore(IDistributedCache, keyPrefix?)` | One instance or dev; not atomic |
| `EfCoreIdempotencyRecordStore<TContext>` (`AddEfCoreIdempotencyRecordStore<TContext>()`, `CSharpEssentials.EntityFrameworkCore`) | Several instances; writes in your transaction |

## Writing a custom store

Implement `IIdempotencyRecordStore`: atomic `TryReserveAsync`, token-checked `CompleteAsync` and `ReleaseAsync`,
storage key from `key.ToStorageKey(prefix)` compared ordinally. Prove it:

```csharp
public sealed class MyStoreTests : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) => new MyStore(timeProvider);
    // protected override bool UseRealClock => true;         // store expires on its own clock (Redis TTL, db now())
    // protected override bool IsReservationAtomic => false; // best-effort store
}
```

On SQL Server key the table by a hash of `ToStorageKey()` in a binary column (trailing spaces are ignored by every
collation). Reference Redis store: `examples/Examples.Idempotency.Redis`.

## Rolling out

Start in `Observe` (stores records, never blocks, counts what `Enforce` would do), read the counter, then switch to
`Enforce`. Counter: meter `IdempotencyTelemetry.MeterName` (`CSharpEssentials.Idempotency`), instrument
`IdempotencyTelemetry.DecisionsInstrumentName` (`idempotency.decisions`), tags `decision`, `mode`, `source`, `target`.

## Guarantee modes

Lease mode (in-memory, cache, Redis, the HTTP middleware): a run that outlives its lease or a crash before the record
write can repeat after the lease. Transactional mode (EF Core store with Mediator `IdempotencyPlacement.InsideTransaction`):
the record commits with the data. Use it for money movement.

## Rules

- `IIdempotencyExecutor` and `IdempotencyInbox` are scoped: resolve them from a scope, not the root provider.
- Argument order: `IdempotencyKeys.FromMessage(messageId, consumer)`, `new IdempotencyRecordKey(namespace, scope, key)`.
- Never pass a raw string as a scope; use the `IdempotencyScope` factories.
- Keep `Lease` at least as long as the server timeout and the client's total retry budget; it must not exceed `Retention` or `MaxLease`.
- Register a custom store before `AddIdempotencyExecutor` (it uses `TryAdd`), or as a keyed store with `StoreKey`.
- `DistributedCacheIdempotencyRecordStore` is not atomic: never use it for payments across instances.
- A message handler has no caller: pass the scope from the message (tenant, account) or `IdempotencyScope.Global`.
- `Observe` does not prevent duplicates.

## Diagnostics

No analyzer diagnostics. Log events use EventIds 7001 to 7017 (`Idempotency.KeptInFlight` 7003, `Idempotency.StoreFailed`
7005, `Idempotency.Observe.*` 7010 to 7017). Error codes are in `IdempotencyErrorCodes`: `Idempotency.KeyMissing`,
`KeyInvalid`, `InFlight`, `KeyReused`, `ScopeRequired`, `UnscopedKeySkipped`.

Migrating from 6.x: `docs/migration/v6-to-v7-idempotency.md`.
