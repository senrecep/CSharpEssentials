# CSharpEssentials.Any

Discriminated Unions (Sum Types) for .NET. A type-safe way to represent a value that can be one of several fixed types.

## Features

- **Unions of 2–8 types** — `Any<T0, T1>` through `Any<T0, ..., T7>`.
- **Implicit Conversions** — Assign values directly to the union type.
- **Pattern Matching** — `Switch` and `Match` for exhaustive handling.
- **Type Helpers** — `Is<T>`, `As<T>`, `TryAs<T>`, `Deconstruct`, `ToTuple`.
- **JSON Support** — Built-in serialization.

## Installation

```bash
dotnet add package CSharpEssentials.Any
```

## Usage

### Creating Unions

```csharp
using CSharpEssentials.Any;

Any<string, int> ParseInput(string input)
{
    if (int.TryParse(input, out int n))
        return n; // implicit to Any<string, int>
    return input;
}
```

### Pattern Matching

```csharp
Any<string, int> result = ParseInput("123");

string output = result.Match(
    first: str => $"String: {str}",
    second: num => $"Number: {num}");

result.Switch(
    first: str => Console.WriteLine("It's a string"),
    second: num => Console.WriteLine("It's a number"));
```

### Type Checking

```csharp
if (result.IsSecond)
{
    int val = result.GetSecond();
}

// Generic helpers
bool isInt = result.Is<int, string, int>();
int? asInt = result.As<int, string, int>();
bool found = result.TryAs<int, string, int>(out int value);
```

### Multiple Types

```csharp
Any<string, int, Exception> response = GetData(5);

response.Switch(
    first: s => Console.WriteLine($"Success: {s}"),
    second: i => Console.WriteLine($"NotFound: {i}"),
    third: e => Console.WriteLine($"Error: {e.Message}"));
```
