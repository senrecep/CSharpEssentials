# CSharpEssentials.Time

`CSharpEssentials.Time` provides a lightweight, testable abstraction over system time, built on top of the modern .NET `TimeProvider`. It simplifies date and time operations and makes unit testing time-dependent logic effortless.

## 🚀 Features

- **Testable Time Abstraction**: `IDateTimeProvider` interface to decouple your code from the system clock.
- **Modern .NET Support**: Uses `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- **Simple Extensions**: Helper methods to convert `DateTime` to `DateOnly` or `TimeOnly`.
- **TimeProvider Integration**: Designed to work seamlessly with the built-in .NET `TimeProvider`.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Time
```

## 🛠 Usage

### 1. Using the Date Time Provider

Inject `IDateTimeProvider` into your services to access current time properties. This abstraction allows you to control time during testing.

```csharp
public class TicketService(IDateTimeProvider dateTimeProvider)
{
    public void ProcessTicket(Ticket ticket)
    {
        // Get current UTC time
        DateTimeOffset now = dateTimeProvider.UtcNow;
        
        // Get just the date part
        DateOnly today = dateTimeProvider.UtcNowDate;

        ticket.ProcessedAt = now;
    }
}
```

### 2. Registration

Register the provider in your Dependency Injection container. Since `DateTimeProvider` depends on .NET's abstract `TimeProvider`, you should register that as well (usually `TimeProvider.System`).

```csharp
using CSharpEssentials.Time;

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
```

### 3. Extensions

Convenient extension methods for converting `DateTime`.

```csharp
using CSharpEssentials.Time;

DateTime now = DateTime.UtcNow;

DateOnly date = now.ToDateOnly();
TimeOnly time = now.ToTimeOnly();
```

## 🧪 Testing

The main benefit of this package is testability. You can mock `IDateTimeProvider` to return fixed times, ensuring your tests are deterministic.

```csharp
// Example using NSubstitute or Moq
var mockTime = Substitute.For<IDateTimeProvider>();
var fixedTime = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero);

mockTime.UtcNow.Returns(fixedTime);
mockTime.UtcNowDate.Returns(DateOnly.FromDateTime(fixedTime.DateTime));

var service = new TicketService(mockTime);
// ... run assertions ...
```
