# Any<T0, T1, ...> - Discriminated Union Type for Functional Programming in C#

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Any.svg)](https://www.nuget.org/packages/CSharpEssentials.Any) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Any.svg)](https://www.nuget.org/packages/CSharpEssentials.Any) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

`Any<T0, T1, ...>` is a discriminated union type developed to enhance functional programming capabilities in C#. It serves as a lightweight alternative to the OneOf library, allowing functions to return multiple different types in a type-safe manner.

## Features

- Support for 2 to 8 different types (from `Any<T0, T1>` to `Any<T0, T1, ..., T7>`)
- Implicit conversion support
- Pattern matching and switch expression support with execution status tracking
- Type safety at compile time
- Built-in JSON serialization support
- Null-safety by design
- Graceful handling of missing pattern handlers with `AnyActionStatus` and `AnyActionResult<T>`

## Usage Examples

### 1. Basic Usage

```csharp
public static Any<string, int> ParseOrKeepAsString(string input)
{
    return int.TryParse(input, out int number)
        ? number    // implicitly converts to Any<string, int>
        : input;   // implicitly converts to Any<string, int>
}
```

### 2. Collection Transformations

```csharp
public static Any<int[], List<int>, HashSet<int>, IEnumerable<int>> ToCollection(int type, int[] array)
{
    return type switch
    {
        0 => array,
        1 => array.ToList(),
        2 => array.ToHashSet(),
        3 => array.AsEnumerable(),
        _ => throw new InvalidOperationException()
    };
}
```

### 3. Domain Modeling Example

```csharp
public static Any<User, Company> GetEntityById(string id)
{
    if (id.StartsWith("USR"))
        return new User(id);  // implicitly converts
    return new Company(id);   // implicitly converts
}
```

### 4. Integration with Result Pattern

```csharp
public static Result<Any<string, int>> ValidateAndParse(string? input)
{
    if (input.IsEmpty())
        return Error.Validation("Input.Empty", "Input cannot be empty");

    Any<string, int> result = int.TryParse(input, out int number)
        ? number
        : input;

    return result;
}
```

### 5. Partial Pattern Matching

The new implementation allows for graceful handling of partial pattern matching:

```csharp
Any<string, int, bool> value = true;

// Only handle specific cases you care about
AnyActionStatus status = value.Switch(
    str => Console.WriteLine($"String: {str}"),
    // Note: no handler for int or bool
);

if (status == AnyActionStatus.NotExecuted)
{
    Console.WriteLine("No handler was provided for this type");
}

// Similarly with Match
AnyActionResult<string> result = value.Match(
    str => $"Processing string: {str}"
    // No handlers for int or bool
);

if (result.Status == AnyActionStatus.Executed)
{
    Console.WriteLine(result.Result);
}
```

## Pattern Matching

`Any` type provides two different approaches for pattern matching:

### 1. Switch Method

Use when you need to perform actions and get execution status. Returns `AnyActionStatus` indicating whether an action was executed:

```csharp
AnyActionStatus status = value.Switch(
    str => Console.WriteLine($"String: {str}"),
    num => Console.WriteLine($"Number: {num}")
);

// Check if an action was executed
if (status == AnyActionStatus.Executed)
{
    Console.WriteLine("Action was successfully executed");
}
else
{
    Console.WriteLine("No matching action was provided");
}
```

### 2. Match Method

Use when you need to transform the value into another type. Returns `AnyActionResult<TResult>` with both status and result:

```csharp
AnyActionResult<string> result = value.Match(
    str => $"String value: {str}",
    num => $"Number value: {num}"
);

// Check the execution status and get the result
if (result.Status == AnyActionStatus.Executed)
{
    string transformedValue = result.Result;
    Console.WriteLine(transformedValue);
}
else
{
    Console.WriteLine("No matching transformation function was provided");
}
```

## Action Result Types

The library provides specialized types for handling pattern matching results:

### AnyActionStatus

An enum indicating the execution status of pattern matching operations:

```csharp
public enum AnyActionStatus
{
    NotExecuted,  // No matching handler was provided
    Executed      // A handler was found and executed
}
```

### AnyActionResul<TResult>

A result type that combines execution status with the actual result:

```csharp
AnyActionResult<string> result = value.Match(
    str => $"Got string: {str}",
    num => $"Got number: {num}"
);

// Access both status and result
if (result.Status == AnyActionStatus.Executed)
{
    Console.WriteLine(result.Result);
}
```

## Type Checking and Value Retrieval

Each type position has dedicated methods for checking and retrieving values:

```csharp
Any<string, int> value = "test";

if (value.IsFirst)
{
    string str = value.GetFirst();
    // Use string value
}
else if (value.IsSecond)
{
    int number = value.GetSecond();
    // Use int value
}
```

## Static Factory Methods

The type provides a generic creation method using `Any.Create`:

```csharp
public static Any<int, string> CreateExample(bool condition)
{
    return condition
        ? Any.Create<int, string>("Good")  // Creates Any<int, string> containing string
        : Any.Create<int, string>(13);     // Creates Any<int, string> containing int
}
```

## Best Practices

1. Use as return types for functions that can return different types
2. Leverage in domain modeling for different entity types
3. Combine with pattern matching for elegant type handling
4. Always check `AnyActionStatus` when using `Switch` method to ensure actions were executed
5. Use `AnyActionResult<T>` when you need both execution status and transformation result
6. Provide handlers for all expected cases in `Switch` and `Match` methods
7. Keep type count reasonable (2-4 types is ideal)
8. Create meaningful type combinations that make sense in your domain

## Performance Characteristics

- Implemented as a struct to minimize heap allocations
- Minimized boxing/unboxing operations
- Designed for lightweight and fast execution
- Zero allocation in most common scenarios

## Limitations

- Maximum of 8 different types supported
- Cannot use the same type multiple times in type parameters
- Special handling might be needed for null values
- Type parameters must be distinct types
