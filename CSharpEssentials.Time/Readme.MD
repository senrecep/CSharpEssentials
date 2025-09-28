# DateTimeProvider

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Time.svg)](https://www.nuget.org/packages/CSharpEssentials.Time) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Time.svg)](https://www.nuget.org/packages/CSharpEssentials.Time) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

`DateTimeProvider` is a structure designed to facilitate time management and enhance testability in modern C# applications. This structure provides more functional and testable time management using .NET's new `TimeProvider` class.

## Key Features

- **Immutable Structure**: All properties are defined as readonly.
- **Thread-Safe**: No mutable state in its internal structure.
- **Testability**: `TimeProvider` can be easily mocked with dependency injection.
- **Time Components**: Provides UTC time information in different formats (DateTime, DateTimeOffset, DateOnly, TimeOnly).
- **Time Zone Support**: Easy access to local and UTC timezone information.

## Usage Examples

### 1. Using with Dependency Injection

```csharp
// Startup.cs or Program.cs
services.AddSingleton(TimeProvider.System);
services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// Usage
public class OrderService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public OrderService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Order CreateOrder()
    {
        return new Order
        {
            CreatedAt = _dateTimeProvider.UtcNow,
            OrderDate = _dateTimeProvider.UtcNowDate
        };
    }
}
```

### 2. Unit Test Example

```csharp
public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_ShouldSetCorrectTimestamp()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2023, 12, 25, 10, 30, 0, TimeSpan.Zero);
        var mockTimeProvider = TimeProvider.Create(fixedTime);
        var dateTimeProvider = new DateTimeProvider(mockTimeProvider);
        var orderService = new OrderService(dateTimeProvider);

        // Act
        var order = orderService.CreateOrder();

        // Assert
        Assert.Equal(fixedTime, order.CreatedAt);
        Assert.Equal(new DateOnly(2023, 12, 25), order.OrderDate);
    }
}
```

### 3. Using Different Time Formats

```csharp
public class ReportGenerator
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportGenerator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Report GenerateReport()
    {
        return new Report
        {
            GeneratedDate = _dateTimeProvider.UtcNowDate,      // Date only (DateOnly)
            GeneratedTime = _dateTimeProvider.UtcNowTime,      // Time only (TimeOnly)
            Timestamp = _dateTimeProvider.UtcNow,              // Full timestamp (DateTimeOffset)
            LocalTimeZone = _dateTimeProvider.TimeZone.Id      // Local timezone
        };
    }
}
```

### 4. Extensions

### DateTime Conversions

```csharp
DateTime dt = DateTime.Now;
TimeOnly time = dt.ToTimeOnly();    // Extracts time component only
DateOnly date = dt.ToDateOnly();    // Extracts date component only
```

## Best Practices

1. Always use with dependency injection.
2. Create fixed time using `TimeProvider.Create()` in unit tests.
3. Use a single instance for consistent time management across the application.
4. Prefer UTC for timestamp operations.
5. Use `TimeZone` property for local time conversions.

## Why Should We Use This Structure?

1. **Predictability for Tests**: Makes tests more reliable by using fixed time.
2. **Functional Programming**: Minimizes side effects through its immutable structure.
3. **Centralized Time Management**: Provides consistent time management across the application.
4. **Modern C# Features**: Leverages new .NET features (TimeProvider, DateOnly, TimeOnly).
