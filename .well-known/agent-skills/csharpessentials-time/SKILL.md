---
name: csharpessentials-time
description: Use when you need testable time. Inject IDateTimeProvider (DateTimeProvider over TimeProvider in production, FakeDateTimeProvider with Advance/SetTime in tests), plus DateTime/DateOnly extensions ToDateOnly/ToTimeOnly, NextDayOfWeek/PreviousDayOfWeek and GetAge.
---

# CSharpEssentials.Time

Testable time abstraction built on .NET's `TimeProvider`. Inject `IDateTimeProvider` instead of calling `DateTime.UtcNow` in domain or service code.

```bash
dotnet add package CSharpEssentials.Time
```

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

    DateTime       UtcNowDateTime { get; }   // Kind == Utc
    DateTimeOffset UtcNow         { get; }

    // not in the netstandard2.1 build:
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

## Use in services

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

`FakeDateTimeProvider` ships in this package. Its time zone is UTC, and it is thread-safe.

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

## Date extensions

```csharp
DateTime dt = DateTime.UtcNow;
DateOnly date = dt.ToDateOnly();
TimeOnly time = dt.ToTimeOnly();

// Next/previous weekday; the current day counts only with includeCurrent: true
DateTime nextMonday = dt.NextDayOfWeek(DayOfWeek.Monday);
DateOnly lastFriday = date.PreviousDayOfWeek(DayOfWeek.Friday, includeCurrent: true);

// Age in whole years
var birthDate = new DateOnly(1990, 5, 20);
int age      = birthDate.GetAge(new DateOnly(2025, 5, 19));   // 34
int ageToday = birthDate.GetAge(dateTimeProvider);            // "today" in the IDateTimeProvider's time zone
```

`GetAge` throws `ArgumentOutOfRangeException` when the birth date is after the reference date.

---

## Guidance

- Register `TimeProvider.System` and `DateTimeProvider` as singletons.
- The `DateOnly`/`TimeOnly` members (`UtcNowDate`, `UtcNowTime`, `ToDateOnly`, `ToTimeOnly`, the `DateOnly` weekday overloads, `GetAge`) are not in the `netstandard2.1` build.
- Use `GetAge(IDateTimeProvider)` instead of `DateTime.Today` so age checks are testable.
