# CSharpEssentials.Time

Testable time abstraction built on .NET `TimeProvider`. Decouple your code from the system clock for reliable unit testing.

## Features

- **IDateTimeProvider** — Injectable interface for `UtcNow`, `UtcNowDateTime`, `UtcNowDate`, and `UtcNowTime`.
- **TimeProvider Integration** — Works with .NET's built-in `TimeProvider` and `FakeTimeProvider` for testing.
- **DateTime Extensions** — Convert `DateTime` to `DateOnly` and `TimeOnly`, next/previous weekday, age calculation.

## Installation

```bash
dotnet add package CSharpEssentials.Time
```

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

### Dependency Injection

```csharp
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

### Test with FakeDateTimeProvider

`CSharpEssentials.Time` ships a built-in `FakeDateTimeProvider` — no extra package needed.

```csharp
using CSharpEssentials.Time;

var fixed = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
var fake  = new FakeDateTimeProvider(fixed);

var svc = new OrderService(fake);
var order = svc.Create(cart);

// Advance the clock — no Thread.Sleep needed
fake.Advance(TimeSpan.FromHours(2));
fake.UtcNow.Should().Be(fixed.AddHours(2));

// Set to a specific time
fake.SetTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

// NET6+ only
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

`DateOnly` overloads and `GetAge` require .NET 6+; on netstandard2.1 only the `DateTime` weekday overloads are available.
