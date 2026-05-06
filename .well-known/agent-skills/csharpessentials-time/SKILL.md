---
name: csharpessentials-time
description: Use when you need testable time — IDateTimeProvider wraps .NET's TimeProvider so production code uses TimeProvider.System while tests use FakeTimeProvider to freeze/advance the clock; also provides .ToDateOnly() and .ToTimeOnly() DateTime extension methods.
---

# CSharpEssentials.Time

Testable time abstraction built on .NET's `TimeProvider`. Never call `DateTime.UtcNow` directly in domain or service code.

## Installation

```bash
dotnet add package CSharpEssentials.Time
```

## Namespace

```csharp
using CSharpEssentials.Time;
```

---

## IDateTimeProvider

```csharp
public interface IDateTimeProvider
{
    TimeZoneInfo TimeZone    { get; }   // TimeZoneInfo.Local
    TimeZoneInfo TimeZoneUtc { get; }   // TimeZoneInfo.Utc

    DateTime       UtcNowDateTime { get; }
    DateTimeOffset UtcNow         { get; }

    // NET6+ only:
    DateOnly UtcNowDate { get; }
    TimeOnly UtcNowTime { get; }
}
```

---

## Register in DI

```csharp
// Program.cs
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

---

## Use in Services

```csharp
public class OrderService
{
    private readonly IDateTimeProvider _time;

    public OrderService(IDateTimeProvider time) => _time = time;

    public Order Create(Cart cart) => new Order
    {
        CreatedAt = _time.UtcNow,
        DueDate   = _time.UtcNowDate.AddDays(7)   // DateOnly — NET6+
    };
}
```

---

## Test with FakeTimeProvider

```csharp
// Install: dotnet add package Microsoft.Extensions.TimeProvider.Testing
using Microsoft.Extensions.Time.Testing;

var fake = new FakeTimeProvider();
fake.SetUtcNow(new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));

var provider = new DateTimeProvider(fake);
var svc      = new OrderService(provider);

var order = svc.Create(cart);
Assert.Equal(new DateOnly(2025, 1, 15), order.DueDate.AddDays(-7));

// Advance the clock
fake.Advance(TimeSpan.FromHours(2));
Assert.Equal(new TimeOnly(12, 0, 0), provider.UtcNowTime);
```

---

## DateTime Extensions (NET6+)

```csharp
DateTime dt = DateTime.UtcNow;

DateOnly date = dt.ToDateOnly();   // DateOnly.FromDateTime(dt)
TimeOnly time = dt.ToTimeOnly();   // TimeOnly.FromDateTime(dt)
```

---

## Best Practices

- Inject `IDateTimeProvider` — never call `DateTime.UtcNow` directly in domain/service code
- `TimeProvider.System` is the production singleton — register once, reuse everywhere
- `DateOnly` / `TimeOnly` properties are `#if NET6_0_OR_GREATER` — guard usage in `netstandard2.x` targets
- `FakeTimeProvider.Advance()` simulates elapsed time without `Thread.Sleep` in tests
