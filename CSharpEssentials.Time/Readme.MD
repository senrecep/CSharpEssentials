# CSharpEssentials.Time

Testable time abstraction built on .NET `TimeProvider`. Decouple your code from the system clock for reliable unit testing.

## Features

- **IDateTimeProvider** — Injectable interface for `UtcNow`, `UtcNowDateTime`, `UtcNowDate`, and `UtcNowTime`.
- **TimeProvider Integration** — Works with .NET's built-in `TimeProvider` and `FakeTimeProvider` for testing.
- **DateTime Extensions** — Convert `DateTime` to `DateOnly` and `TimeOnly`.

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

### Testing with FakeTimeProvider

```csharp
using Microsoft.Extensions.Time.Testing;

FakeTimeProvider fakeTime = new();
fakeTime.SetUtcNow(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));

IDateTimeProvider provider = new DateTimeProvider(fakeTime);
provider.UtcNow.Year.Should().Be(2025);
```

### Extensions

```csharp
DateTime now = DateTime.UtcNow;

DateOnly date = now.ToDateOnly();
TimeOnly time = now.ToTimeOnly();
```
