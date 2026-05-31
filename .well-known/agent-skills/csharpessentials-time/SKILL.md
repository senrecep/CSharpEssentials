---
name: csharpessentials-time
description: Use when you need testable time — IDateTimeProvider wraps clock access so production code uses DateTimeProvider while tests use the built-in FakeDateTimeProvider to freeze/advance/set the clock; also provides .ToDateOnly() and .ToTimeOnly() DateTime extension methods.
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

## Test with FakeDateTimeProvider

`CSharpEssentials.Time` ships a built-in `FakeDateTimeProvider` — no extra NuGet package needed.

```csharp
var fixed = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
var fake  = new FakeDateTimeProvider(fixed);

// Inject as IDateTimeProvider
var svc   = new OrderService(fake);

// Advance the clock without Thread.Sleep
fake.Advance(TimeSpan.FromHours(2));
fake.UtcNow // → 2025-01-15 12:00:00

// Jump to a specific instant
fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

// NET6+ only
DateOnly date = fake.UtcNowDate;
TimeOnly time = fake.UtcNowTime;
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
