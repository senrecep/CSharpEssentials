# CSharpEssentials.Any

Discriminated Unions (Sum Types) for .NET. A type-safe way to represent a value that can be one of several fixed types.

## Features

- **Unions of 2 to 8 types**: `Any<T0, T1>` through `Any<T0, ..., T7>`.
- **Implicit Conversions**: Assign values directly to the union type.
- **Pattern Matching**: `Switch` and `Match` for exhaustive handling.
- **Type Helpers**: `Is<T>`, `As<T>`, `TryAs<T>`, `Deconstruct`, `ToTuple`.
- **JSON Support**: Built-in serialization.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Json` and `CSharpEssentials.Core`. The types live in the `CSharpEssentials.Any` namespace.

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

Any<string, int> first = Any<string, int>.First("text");   // explicit factories: First, Second, Third ... Eighth
Any<string, int> second = Any.Create<string, int>(5);      // or Any.Create with the type arguments spelled out
```

The union holds its active branch in `Index` (0 for the first type) and its value in `Value` (`object?`). Implicit conversion is ambiguous when two type arguments are the same type, for example `Any<int, int>`, and `default(Any<...>)` has `Index` 0 but a `null` `Value`, so `IsFirst` is `true` while `GetFirst()` returns `null` or `default`.

### Pattern Matching

```csharp
Any<string, int> result = ParseInput("123");

// Match returns AnyActionResult<TResult>: Status tells whether a handler ran, Result holds its value
AnyActionResult<string> output = result.Match(
    first: str => $"String: {str}",
    second: num => $"Number: {num}");
Console.WriteLine(output.Result);

AnyActionStatus status = result.Switch(
    first: str => Console.WriteLine("It's a string"),
    second: num => Console.WriteLine("It's a number"));
```

Every handler is optional. When the active branch has no handler, `Switch` returns `AnyActionStatus.NotExecuted` and `Match` returns an `AnyActionResult` whose `Status` is `NotExecuted` and whose `Result` is `default`; otherwise the status is `Executed`. Calling `Match` or `Switch` on `default(Any<...>)`, or on a union that holds `null`, throws `InvalidOperationException`.

### Type Checking

```csharp
if (result.IsSecond)
{
    int val = result.GetSecond();   // throws InvalidOperationException when another branch is active
}

// Generic helpers: the target type first, then the union's type arguments
bool isInt = result.Is<int, string, int>();
int? asInt = result.As<int, string, int>();
bool found = result.TryAs<int, string, int>(out int value);

// Deconstruct and ToTuple: the inactive branches are default
var (text, number) = result;
(string? First, int Second) tuple = result.ToTuple();
```

`IsFirst`, `IsSecond`, `IsThird` through `IsEighth` and `GetFirst()` through `GetEighth()` exist up to the arity of the union. `Is`, `As` and `TryAs` test the runtime type of the value, not the branch. For a value-type target, `As` returns `default` (so `0` for `int`) when the value is of another type, not `null`; use `Is` or `TryAs` to tell the cases apart.

A union serializes to JSON as `{"Index":1,"Value":123}` and deserializes back to the same branch. `ToString()` returns the JSON of the value.

### Multiple Types

```csharp
Any<string, int, Exception> response = GetData(5);

response.Switch(
    first: s => Console.WriteLine($"Success: {s}"),
    second: i => Console.WriteLine($"NotFound: {i}"),
    third: e => Console.WriteLine($"Error: {e.Message}"));
```

### Collection Extensions

Scatter a sequence of unions into typed arrays in one pass:

```csharp
// Classify API responses into successes and errors
var (users, errors) = responses
    .Traverse(r => ClassifyResponse(r));  // returns Any<UserDto, ApiError>

// Or partition an existing sequence of unions
var (drafts, published) = articles
    .Select(a => GetState(a))             // returns Any<Draft, Published>
    .Partition();
```

`Partition` and `Traverse` are available for all arities (`Any<T0,T1>` through `Any<T0,...,T7>`), returning a tuple with one typed array per variant.
