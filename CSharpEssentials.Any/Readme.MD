# CSharpEssentials.Any

Discriminated unions (sum types) for .NET: a type-safe way to represent a value that can be one of several fixed types.

> Benchmarks against OneOf 3.0.271 and Dunet 1.16.2 (construct, match, try-pick, partition, JSON and ToString) are in
> [BENCHMARKS.md](./BENCHMARKS.md). That page is a draft: results are pending the full run.

## Features

- **Unions of 2 to 8 types**: `Any<T0, T1>` through `Any<T0, ..., T7>`.
- **Implicit conversions**: Assign values directly to the union type.
- **Pattern matching**: `Switch` and `Match` for exhaustive handling.
- **Type helpers**: `Is<T>`, `As<T>`, `TryAs<T>`, `Deconstruct`, `ToTuple`.
- **JSON**: Built-in serialization.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Json` and `CSharpEssentials.Core`. The types live in the `CSharpEssentials.Any` namespace.

## Installation

```bash
dotnet add package CSharpEssentials.Any
```

## Usage

### Creating unions

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

### Pattern matching

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

### Type checking

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

A union serializes to JSON as `{"Index":1,"Value":123}` and deserializes back to the same branch. The value is read with the `JsonSerializerOptions` you pass to `JsonSerializer`, so a naming policy or converter applies to it too. A `null` value deserializes when the branch type is a reference type or `Nullable<T>`, and throws `ArgumentNullException` for any other value-type branch. `ToString()` returns the value's own `ToString()`, or an empty string when the value is `null`. It does not use reflection, so it is safe under trimming and Native AOT.

### Multiple types

```csharp
Any<string, int, Exception> response = GetData(5);

response.Switch(
    first: s => Console.WriteLine($"Success: {s}"),
    second: i => Console.WriteLine($"NotFound: {i}"),
    third: e => Console.WriteLine($"Error: {e.Message}"));
```

### Collection extensions

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

## Upgrading to 8.0

- A `null` value is now accepted when its branch is a reference type or `Nullable<T>`. This applies to JSON deserialization (`{"Index":n,"Value":null}` now round-trips) and to the public `(int index, object? value)` constructor, so `new Any<int, string>(1, null)` no longer throws. Earlier versions threw `ArgumentNullException` for every `null` value. If you relied on that to reject such input, check `Value is null` after constructing or deserializing.
- JSON deserialization now reads the value with the caller's `JsonSerializerOptions`. Earlier versions ignored them and always used `EnhancedJsonSerializerOptions.DefaultOptions` (camelCase, case-insensitive, enum and date converters), so a snake_case payload lost its properties. If your payloads relied on those defaults, pass `EnhancedJsonSerializerOptions.DefaultOptions` (or options with the same settings) to `JsonSerializer.Deserialize`. With a source-generated `JsonSerializerContext`, register each branch type with `[JsonSerializable]`; an unregistered branch type now throws `NotSupportedException` on deserialization.
- `ToString()` now returns the value's own `ToString()` instead of its JSON, so a string value comes back without quotes (`text`, not `"text"`), an object returns its type's `ToString()` instead of a JSON object, and a `null` value returns an empty string instead of `null`. The old output came from reflection-based `System.Text.Json`, which is not safe under Native AOT. If you need the JSON, call `JsonSerializer.Serialize(any.Value, options)` with your own options or source-generated context.
