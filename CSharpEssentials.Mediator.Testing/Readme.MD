# CSharpEssentials.Mediator.Testing

Test helpers for the resource locks of `CSharpEssentials.Mediator`:

- `ResourceLockContractTests`: xUnit contract tests for any `IResourceLock`. Derive one class, return your lock, and the scenarios that the built-in locks pass run against it.
- `RecordingResourceLock`: a public in-process `IResourceLock` that records every acquire and release, for testing handlers behind `LockBehavior`.

## Installation

```bash
dotnet add package CSharpEssentials.Mediator.Testing
```

Add it to an xUnit (v2) test project. Targets `net11.0`, `net10.0`, `net9.0` and `net8.0`.

## Contract tests

```csharp
using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;

public sealed class RedisResourceLockTests : ResourceLockContractTests
{
    protected override IResourceLock CreateLock() => new RedisResourceLock(Connection);
}
```

The scenarios cover:

- try-acquire of a free key, of a held key, and after the holder disposed its handle;
- independent keys;
- `TimeoutException` when the key stays held, including a zero timeout;
- a waiting acquire that completes when the holder releases, and one cancelled while it waits;
- a second dispose of a handle that does not release the next holder;
- parallel acquirers that never overlap;
- empty and null keys.

Each test instance uses its own key prefix (`Key(name)`), so the scenarios can share one database or cache. The waits are real: `ShortWait` (300 ms) is how long a held key must keep a waiter out, and `LongWait` (10 seconds) bounds every wait that must succeed. Override them for a slow backend, and make a polling lock poll more often than `ShortWait`.

## Recording lock

```csharp
RecordingResourceLock locks = new();
services.AddSingleton<IResourceLock>(locks);

await mediator.Send(new CapturePaymentCommand(orderId));

locks.Acquisitions.Should().ContainSingle().Which.Key.Should().Be($"order:{orderId}");
locks.HeldKeys.Should().BeEmpty();
```

| Member | What it does |
|--------|-------------|
| `Acquisitions` | Every `AcquireAsync` and `TryAcquireAsync` call in order, as `LockAcquisition(Key, Timeout, TryOnly)`, including calls that failed |
| `Releases` | The key of every released handle, in order. A second dispose of a handle is not recorded |
| `HeldKeys` | The keys held right now |
| `FailWith` | When set, the next calls are recorded and then throw this exception |

It locks for real through `InProcessResourceLock`, so a test sees the same blocking and `TimeoutException` behavior as the default lock.
