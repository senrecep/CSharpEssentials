# CSharpEssentials.Idempotency.Testing

xUnit contract tests for `IIdempotencyRecordStore` implementations from `CSharpEssentials.Idempotency`. Derive one
class, return your store, and every scenario the built-in stores pass runs against it.

## Installation

```bash
dotnet add package CSharpEssentials.Idempotency.Testing
```

Add it to an xUnit test project. Targets `net11.0`, `net10.0`, `net9.0` and `net8.0`.

## Usage

```csharp
using CSharpEssentials.Idempotency;
using CSharpEssentials.Idempotency.Testing;

public sealed class RedisIdempotencyRecordStoreTests : IdempotencyRecordStoreContractTests
{
    protected override bool UseRealClock => true;

    protected override IIdempotencyRecordStore CreateStore(TimeProvider timeProvider) =>
        new RedisIdempotencyRecordStore(Connection);
}
```

The scenarios cover:

- reserve, in flight, completed replay and fingerprint mismatch (in flight and completed);
- release of an in-flight key, of a completed key, of an unknown key and with another token;
- lease expiry, retention of completed payloads, and completion of an expired reservation that nobody took over;
- a stale token fenced out by the next reservation, and a second completion keeping the first payload;
- independent keys, namespaces and scopes, parts that differ only in case or by trailing spaces, global keys that never
  collide with scoped ones, and parts that contain separators;
- empty and binary payloads, and a stored payload that does not change with the caller's buffer;
- parallel reservations of a free key and of an expired key;
- argument validation.

## Options

| Member | Default | Use |
|---|---|---|
| `CreateStore(TimeProvider)` | abstract | Returns the store under test. Called once per test. |
| `UseRealClock` | `false` | `false` passes a `FakeTimeProvider` and advances it. Return `true` for a store whose expiry runs on its own clock (Redis TTL, a database `now()`); the tests then use `TimeProvider.System` and wait. |
| `IsReservationAtomic` | `true` | Return `false` for a best-effort store: the parallel tests then accept more than one reservation but still require that every call is either reserved or in flight. |
| `Lease`, `Retention`, `ExpirySlack` | 30 s, 1 h, 1 s (real clock: 1 s, 3 s, 500 ms) | Durations used by the expiry tests. |

Each test instance uses its own `Namespace`, so the scenarios can share one database or cache without cleanup.
Test helpers (`Key`, `ReserveAsync`, `AdvanceAsync`, `Payload`, `Store`, `Clock`) are protected, so store-specific
tests can live in the same class.
