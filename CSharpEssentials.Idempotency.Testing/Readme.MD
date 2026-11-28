# CSharpEssentials.Idempotency.Testing

xUnit contract tests for `IIdempotencyRecordStore` implementations of `CSharpEssentials.Idempotency`. Derive one class,
return your store, and every scenario the built-in stores pass runs against it. Use it when you write your own store
(Redis, DynamoDB, a SQL table).

## Installation

```bash
dotnet add package CSharpEssentials.Idempotency.Testing
```

Add it to an xUnit v2 test project. Targets `net11.0`, `net10.0`, `net9.0` and `net8.0`. Namespace:
`CSharpEssentials.Idempotency.Testing`.

## Quick start

```csharp
using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;

public sealed class MyStoreTests : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new InMemoryIdempotencyRecordStore(timeProvider); // your store here
}
```

`dotnet test` then runs every scenario as a `[Fact]`. The contract methods are public, so you can also call them
without a test runner, for example to check a store from a console app:

```csharp Program.cs
// dotnet add package CSharpEssentials.Idempotency.Testing
using System.Reflection;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;
using Xunit;

MethodInfo[] scenarios = [.. typeof(IdempotencyRecordStoreContractTests).GetMethods()
    .Where(method => method.GetCustomAttribute<FactAttribute>() is not null)];
int failed = 0;
foreach (MethodInfo scenario in scenarios)
{
    try
    {
        await (Task)scenario.Invoke(new InMemoryStoreContract(), null)!;
    }
    catch (Exception exception)
    {
        failed++;
        Console.WriteLine($"FAIL {scenario.Name}: {exception.InnerException?.Message ?? exception.Message}");
    }
}

Console.WriteLine($"{scenarios.Length - failed}/{scenarios.Length} scenarios passed");
return failed == 0 ? 0 : 1;

public sealed class InMemoryStoreContract : IdempotencyRecordStoreContractTests
{
    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new InMemoryIdempotencyRecordStore(timeProvider);
}
```

## What the scenarios check

- Reserve, in flight, replay of a completed payload, and fingerprint mismatch while in flight and after completion.
- Release of an in-flight key, of a completed key (kept), of an unknown key and with another token.
- Lease expiry, retention of completed payloads, and completion of an expired reservation that nobody took over.
- A stale token fenced out by the next reservation, and a second completion that keeps the first payload.
- Independent keys, namespaces and scopes; parts that differ only in case or by trailing spaces; global keys that never collide with scoped ones; parts that contain separators.
- Empty and binary payloads, and a stored payload that does not change with the caller's buffer.
- Distinct tokens, and parallel reservations of a free key and of an expired key.
- Argument validation.

The case and trailing-space scenarios catch a SQL store keyed on text columns with a case-insensitive or PAD SPACE
collation. Key such a store by a hash of `ToStorageKey()` in a binary column instead.

## Options

| Member | Default | Use |
|--------|---------|-----|
| `CreateStore(TimeProvider)` | abstract | Returns the store under test. Called once per test. Leases and retention must follow the given clock |
| `UseRealClock` | `false` | `false` passes a `FakeTimeProvider` and advances it. Return `true` for a store that keeps time on its own clock (Redis TTL, a database `now()`): the tests then use `TimeProvider.System` and real waits |
| `IsReservationAtomic` | `true` | Return `false` for a best-effort store. The parallel tests then accept more than one reservation but still require every result to be reserved or in flight |
| `Lease`, `Retention`, `ExpirySlack` | 30 s, 1 h, 1 s (real clock: 1 s, 3 s, 500 ms) | Durations of the expiry tests |

A store on its own clock:

```csharp
public sealed class RedisIdempotencyRecordStoreTests(RedisFixture redis)
    : IdempotencyRecordStoreContractTests, IClassFixture<RedisFixture>
{
    protected override bool UseRealClock => true;

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new RedisIdempotencyRecordStore(redis.Connection);
}
```

Each test instance uses its own `Namespace`, so the scenarios can share one database or cache without cleanup. The
helpers `Key(key)`, `Key(key, scope)`, `ReserveAsync`, `AdvanceAsync`, `Payload`, `Store` and `Clock` are protected,
so store-specific tests can live in the same class:

```csharp
[Fact]
public async Task Completed_Entry_Expires_After_Its_Retention()
{
    string token = await ReserveAsync(Key("order-1"));
    await Store.CompleteAsync(Key("order-1"), token, "fp", Payload(), Retention);

    await AdvanceAsync(Retention + ExpirySlack);
    IdempotencyRecordReservation again = await Store.TryReserveAsync(Key("order-1"), "fp", Lease);

    Assert.Equal(IdempotencyReservationStatus.Reserved, again.Status);
}
```

## Related

- Example: [examples/Examples.Idempotency.Testing](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Idempotency.Testing)
- Redis store under these tests: [examples/Examples.Idempotency.Redis](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Idempotency.Redis)
- Store requirements: [CSharpEssentials.Idempotency, External stores](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#external-stores)
