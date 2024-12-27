# CSharpEssentials

CSharpEssentials is a comprehensive library that enhances C#'s functional programming capabilities. It provides a robust set of tools and utilities designed to make your C# applications more maintainable, testable, and aligned with functional programming principles.

## 🌟 Package Information

- **NuGet Package**: [CSharpEssentials](https://www.nuget.org/packages/CSharpEssentials) [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)

## 🌟 Key Features

- **Functional Programming Support**: Enhanced functional programming capabilities for C#
- **Type Safety**: Strong type safety across all modules
- **Error Handling**: Comprehensive error handling with Result pattern
- **Domain-Driven Design**: Support for DDD patterns and practices
- **Performance**: Optimized for high performance with modern C# features
- **Testing**: Built with testability in mind

## 📦 Core Modules

### Maybe Monad

[`Maybe<T>`](Monad/Readme.MD) - Safe handling of nullable values:

```csharp
Maybe<string> someValue = Maybe.From("Hello");
Maybe<string> noValue = Maybe.None;

// LINQ support
var result = someValue
    .Select(str => str.ToUpper())
    .Where(str => str.Length > 5);
```

### Result Pattern

[`Result<T>`](Results/Readme.MD) - Functional approach to error handling:

```csharp
public Result<User> CreateUser(UserDto dto)
{
    if (string.IsNullOrEmpty(dto.Email))
        return Error.Validation("EMAIL_REQUIRED", "Email is required");

    var user = new User(dto);
    return Result<User>.Success(user);
}
```

### Rule Engine

[`RuleEngine`](Rules/Readme.MD) - Complex business rule management:

```csharp
public class EmailValidationRule : IRule<string>
{
    public Result Evaluate(string email, CancellationToken cancellationToken = default)
    {
        return email.Contains("@")
            ? Result.Success()
            : Error.Validation("INVALID_EMAIL", "Email must contain @");
    }
}
```

### Error Types

[`Error`](Errors/Readme.MD) - Structured error handling:

```csharp
var error = Error.Validation(
    code: "USER_INVALID_AGE",
    description: "User age must be greater than 18"
);
```

### Discriminated Unions

[`Any<T0,T1,...>`](Any/Readme.MD) - Type-safe union types:

```csharp
public Any<string, int> ParseOrKeepAsString(string input)
{
    return int.TryParse(input, out int number)
        ? number
        : input;
}
```

### Entity Framework Support

[`EntityBase`](Entity/Readme.MD) - DDD-friendly entity base classes:

```csharp
public class Product : EntityBase
{
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        Raise(new PriceUpdatedEvent(newPrice));
    }
}
```

### Time Management

[`DateTimeProvider`](Time/Readme.MD) - Testable datetime handling:

```csharp
public class OrderService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public Order CreateOrder()
    {
        return new Order { CreatedAt = _dateTimeProvider.UtcNow };
    }
}
```

### JSON Utilities

[`JSON`](Json/Readme.MD) - Enhanced JSON capabilities:

```csharp
var options = EnhancedJsonSerializerOptions.DefaultOptions;
string json = myObject.ConvertToJson(options);
```

### Extensions

[`Extensions`](Extensions/Readme.MD) - Useful extension methods:

```csharp
// String transformations
"helloWorld".ToPascalCase();  // => "HelloWorld"
"hello_world".ToCamelCase();  // => "helloWorld"

// Collection operations
var randomItem = list.GetRandomItem();
```

## 🚀 Getting Started

### Installation

```bash
dotnet add package CSharpEssentials
```

You can also install the package directly from the [NuGet Gallery](https://www.nuget.org/packages/CSharpEssentials).

### Basic Setup

Import the namespace:

```csharp
using CSharpEssentials;
```

## 📚 Documentation

Each module has its own detailed documentation:

- [Maybe Monad Documentation](Monad/Readme.MD)
- [Result Pattern Documentation](Results/Readme.MD)
- [Rule Engine Documentation](Rules/Readme.MD)
- [Error Types Documentation](Errors/Readme.MD)
- [Entity Base Documentation](Entity/Readme.MD)
- [JSON Utilities Documentation](Json/Readme.MD)
- [Extensions Documentation](Extensions/Readme.MD)
- [Time Management Documentation](Time/Readme.MD)
- [Any Type Documentation](Any/Readme.MD)

## 🔧 Best Practices

1. **Error Handling**

   - Use `Result<T>` for operations that can fail
   - Prefer `Maybe<T>` over null values
   - Use structured `Error` types for domain errors

2. **Domain Modeling**

   - Extend `EntityBase` for domain entities
   - Use `RuleEngine` for complex validations
   - Implement domain events for state changes

3. **Testing**
   - Use `DateTimeProvider` for time-dependent tests
   - Mock interfaces instead of concrete implementations
   - Utilize `Result<T>` for predictable error scenarios

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

This library was inspired by and builds upon the work of several excellent open-source projects:

- [ErrorOr](https://github.com/amantinband/error-or) - A simple, fluent discriminated union of an error or a result
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) - Functional extensions for C#
- [OneOf](https://github.com/mcintyre321/OneOf) - Easy to use F#-like discriminated unions for C#

Special thanks to all contributors who have helped shape this library and to the maintainers of these inspiring projects.

## 📬 Support

For support, please open an issue in the GitHub repository or contact the maintainers.
