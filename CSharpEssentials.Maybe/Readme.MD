# Maybe Monad in C#

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Maybe.svg)](https://www.nuget.org/packages/CSharpEssentials.Maybe) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Maybe.svg)](https://www.nuget.org/packages/CSharpEssentials.Maybe) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

The Maybe monad is a functional programming concept that provides a safe way to handle nullable values and avoid null reference exceptions. It represents a value that may or may not exist.

## Core Features

- **Type-Safe**: Provides compile-time safety for handling nullable values
- **Immutable**: Implemented as a readonly struct
- **JSON Serialization Support**: Built-in support for System.Text.Json
- **LINQ-Compatible**: Supports functional composition through LINQ methods
- **Async Support**: Built-in support for async operations

## Basic Usage

```csharp
// Creating Maybe instances
Maybe<string> someValue = Maybe.From("Hello");
Maybe<string> noValue = Maybe<string>.None;

// Implicit conversion
Maybe<int> number = 42;
Maybe<string> emptyString = null; // Will be converted to None

// Checking for values
if (someValue.HasValue)
{
    string value = someValue.Value;
}

if (noValue.HasNoValue)
{
    // Handle empty case
}
```

## Safe Value Access

```csharp
// Safe ways to access values
string value1 = someValue.GetValueOrDefault("default");
string value2 = someValue.GetValueOrThrow("Custom error message");

// Pattern matching
if (someValue.TryGetValue(out string? value))
{
    // Use value safely
}

// Deconstruction
var (hasValue, value) = someValue;
```

## Async Operations

```csharp
// Creating Maybe from async operations
Task<string?> asyncOperation = GetDataAsync();
Maybe<string> result = await Maybe.From(asyncOperation);

// Using with async functions
Maybe<string> result2 = await Maybe.From(async () => await GetDataAsync());
```

## Best Practices

1. Use Maybe instead of null when:

   - A value might not exist
   - You want to force null-check at compile time
   - You need to chain operations safely

2. Combine with LINQ for functional composition:

   ```csharp
   Maybe<int> result = Maybe.From("42")
       .Select(str => int.Parse(str))
       .Where(num => num > 0);
   ```

3. Use pattern matching for elegant handling:
   ```csharp
   string message = someValue.Match(
       some: value => $"Value is {value}",
       none: () => "No value present"
   );
   ```

## Benefits

- Eliminates null reference exceptions
- Makes code more expressive and intention-clear
- Enforces handling of missing values at compile-time
- Provides a functional approach to handling optional values

## Implementation Details

The Maybe monad is implemented as a readonly struct for performance and implements:

- `IMaybe<T>` for basic Maybe operations
- `IEquatable<T>` for value comparison
- JSON serialization support through System.Text.Json attributes
