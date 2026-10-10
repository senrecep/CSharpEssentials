# CSharpEssentials.Time

Testable time abstraction built on .NET `TimeProvider`. Decouple your code from the system clock for reliable unit testing.

## Features

- **IDateTimeProvider**: Injectable interface for `UtcNow`, `UtcNowDateTime` (always `DateTimeKind.Utc`), `UtcNowDate`, `UtcNowTime`, `TimeZone` and `TimeZoneUtc`.
- **TimeProvider Integration**: `DateTimeProvider` wraps .NET's `TimeProvider`, so `TimeProvider.System` or a test `TimeProvider` such as `FakeTimeProvider` both work.
- **FakeDateTimeProvider**: Built-in controllable clock with `Advance` and `SetTime` for tests.
- **DateTime Extensions**: Convert `DateTime` to `DateOnly` and `TimeOnly`, next/previous weekday, age calculation.

## Installation

```bash
dotnet add package CSharpEssentials.Time
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1` (which adds `Microsoft.Bcl.TimeProvider`).

## Usage

### DateTimeProvider

```csharp
using CSharpEssentials.Time;

public class OrderService(IDateTimeProvider time)
{
    public Order CreateOrder()
    {
        return new Order
        {
            CreatedAt = time.UtcNow,
            CreatedDate = time.UtcNowDate
        };
    }
}
```

`DateTimeProvider.UtcNow` reads `TimeProvider.GetUtcNow()`. `UtcNowDate` and `UtcNowTime` are the UTC date and time of day. `TimeZone` is `TimeZoneInfo.Local` and `TimeZoneUtc` is `TimeZoneInfo.Utc`.

### Dependency Injection

```csharp
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

### Test with FakeDateTimeProvider

`CSharpEssentials.Time` ships a built-in `FakeDateTimeProvider`, so no extra package is needed. Its `TimeZone` is UTC. The sample below asserts with FluentAssertions, which is not a dependency of this package.

```csharp
using CSharpEssentials.Time;
using FluentAssertions;

var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
var fake = new FakeDateTimeProvider(fixedTime);

var svc = new OrderService(fake);
Order order = svc.CreateOrder();
order.CreatedAt.Should().Be(fixedTime);

// Advance the clock — no Thread.Sleep needed
fake.Advance(TimeSpan.FromHours(2));
fake.UtcNow.Should().Be(fixedTime.AddHours(2));

// Set to a specific time
fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

// .NET 6+ targets only (not netstandard2.1)
fake.UtcNowDate.Should().Be(new DateOnly(2026, 1, 1));
fake.UtcNowTime.Should().Be(new TimeOnly(0, 0, 0));
```

### Extensions

```csharp
DateTime now = DateTime.UtcNow;

DateOnly date = now.ToDateOnly();
TimeOnly time = now.ToTimeOnly();

DateOnly today = new(2024, 6, 15);                     // Saturday
today.NextDayOfWeek(DayOfWeek.Monday);                 // 2024-06-17
today.PreviousDayOfWeek(DayOfWeek.Saturday);           // 2024-06-08
today.NextDayOfWeek(DayOfWeek.Saturday, includeCurrent: true); // 2024-06-15

new DateOnly(2000, 6, 16).GetAge(today);               // 23
birthDate.GetAge(dateTimeProvider);                    // uses the provider's time zone
```

`NextDayOfWeek` and `PreviousDayOfWeek` skip the current day unless `includeCurrent: true`. `GetAge` counts whole years and throws `ArgumentOutOfRangeException` when the birth date is after the reference date. `DateOnly` and `TimeOnly` members and `GetAge` require .NET 6+; on netstandard2.1 only the `DateTime` weekday overloads are available.
