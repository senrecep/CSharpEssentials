---
name: csharpessentials-time
description: Use when you need testable time. Inject IDateTimeProvider (DateTimeProvider over TimeProvider in production, FakeDateTimeProvider with Advance/SetTime in tests), plus DateTime/DateOnly extensions ToDateOnly/ToTimeOnly, NextDayOfWeek/PreviousDayOfWeek and GetAge.
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
    TimeZoneInfo TimeZone    { get; }   // DateTimeProvider: TimeZoneInfo.Local
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

`DateTimeProvider` wraps a `TimeProvider`:

```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

---

## Use in Services

```csharp
public class OrderService(IDateTimeProvider time)
{
    public Order Create(Cart cart) => new()
    {
        UserId    = cart.UserId,
        Total     = cart.Total,
        CreatedAt = time.UtcNow,
    };
}
```

---

## Test with FakeDateTimeProvider

`CSharpEssentials.Time` ships a `FakeDateTimeProvider`, so no extra NuGet package is needed. Its time zone is UTC.

```csharp
var start = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
var fake  = new FakeDateTimeProvider(start);

var svc = new OrderService(fake);   // inject as IDateTimeProvider

// Advance the clock without Thread.Sleep
fake.Advance(TimeSpan.FromHours(2));
DateTimeOffset now = fake.UtcNow;   // 2025-01-15 12:00:00 +00:00

// Jump to a specific instant
fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

DateOnly date = fake.UtcNowDate;
TimeOnly time = fake.UtcNowTime;
```

---

## Date Extensions

```csharp
DateTime dt = DateTime.UtcNow;
DateOnly date = dt.ToDateOnly();   // NET6+
TimeOnly time = dt.ToTimeOnly();   // NET6+

// Next/previous weekday; the current day counts only with includeCurrent: true
DateTime nextMonday = dt.NextDayOfWeek(DayOfWeek.Monday);
DateOnly lastFriday = date.PreviousDayOfWeek(DayOfWeek.Friday, includeCurrent: true);

// Age in whole years (DateOnly, NET6+)
var birthDate = new DateOnly(1990, 5, 20);
int age      = birthDate.GetAge(new DateOnly(2025, 5, 19));   // 34
int ageToday = birthDate.GetAge(timeProvider);                // "today" in the provider's time zone
```

`GetAge` throws `ArgumentOutOfRangeException` when the birth date is after the reference date.

---

## Best Practices

- Inject `IDateTimeProvider`; never call `DateTime.UtcNow` directly in domain/service code
- Register `TimeProvider.System` and `DateTimeProvider` as singletons
- `DateOnly` / `TimeOnly` members are `NET6_0_OR_GREATER` only, not available on `netstandard2.x`
- `FakeDateTimeProvider.Advance()` simulates elapsed time without `Thread.Sleep` in tests
- Use `GetAge(IDateTimeProvider)` instead of `DateTime.Today` so age checks are testable
