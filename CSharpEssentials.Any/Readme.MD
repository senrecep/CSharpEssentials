# CSharpEssentials.Any

`CSharpEssentials.Any` provides a robust implementation of **Discriminated Unions** (also known as Sum Types or Coproducts) for .NET. It allows you to define a type that can hold a value of one of several fixed types, providing type safety and exhaustive pattern matching.

## 🚀 Features

- **Discriminated Unions**: Support for unions of 2 up to 8 types (`Any<T0, T1>`, `Any<T0, T1, T2>`, etc.).
- **Type Safety**: Compiler-checked access to values.
- **Pattern Matching**: `Switch` and `Match` methods to handle each case.
- **Implicit Conversions**: Seamlessly convert values to their union type.
- **JSON Support**: Built-in serialization handling.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Any
```

## 🛠 Usage

### 1. Defining a Union

You can use `Any<T0, T1>` as a return type or property.

```csharp
using CSharpEssentials.Any;

public Any<string, int> ParseOrReturnOriginal(string input)
{
    if (int.TryParse(input, out int result))
    {
        return result; // Implicit conversion to Any<string, int>
    }
    return input; // Implicit conversion
}
```

### 2. Pattern Matching (Match)

Use `Match` to transform the value based on which type it holds.

```csharp
Any<string, int> result = ParseOrReturnOriginal("123");

string output = result.Match(
    first: str => $"It's a string: {str}",
    second: num => $"It's a number: {num}"
);
```

### 3. Executing Actions (Switch)

Use `Switch` to execute code based on the type.

```csharp
result.Switch(
    first: str => Console.WriteLine("String"),
    second: num => Console.WriteLine("Number")
);
```

### 4. Checking and Accessing

```csharp
if (result.IsSecond)
{
    int val = result.GetSecond();
}
```

### 5. Handling more types

You can handle up to 8 variations.

```csharp
// A result that can be Success (string), NotFound (int code), or Error (Exception)
public Any<string, int, Exception> GetData(int id)
{
    // ... logic
    return new Exception("Failed"); 
}

var data = GetData(5);

data.Switch(
    first: s => Console.WriteLine($"Success: {s}"),
    second: i => Console.WriteLine($"Not Found: {i}"),
    third: e => Console.WriteLine($"Error: {e.Message}")
);
```
