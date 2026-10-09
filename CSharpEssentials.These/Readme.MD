# CSharpEssentials.These

`These<TError, TValue>` is a three-state discriminated union: `Left` (error only), `Right` (value only), or `Both` (error and value together). Unlike `Result<T>`, it does not treat the presence of an error as an automatic failure. Use it when partial success carries a meaningful value alongside a warning or non-fatal error.

## Features

- **Three-state union**: `Left`, `Right`, `Both` with explicit state flags.
- **Functional API**: `Map`, `MapLeft`, `FlatMap`, `Tap`, `TapLeft`, `Match`.
- **Maybe bridges**: `GetRight()` and `GetLeft()` return `Maybe<T>` without throwing.
- **Result bridges**: Convert to/from `Result<T>` in strict or lenient mode.
- **Collection extensions**: Partition a sequence into its three states.
- **JSON round trip**: Serializes as `{"isLeft":true,"isRight":true,"left":...,"right":...}`.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Results`, `CSharpEssentials.Maybe` and `CSharpEssentials.Errors`. `These<TError, TValue>` is a `readonly record struct` in the `CSharpEssentials.These` namespace.

## Installation

```bash
dotnet add package CSharpEssentials.These
```

## Usage

### Creating These

```csharp
using CSharpEssentials.These;

// Error only — no value produced
These<string, int> left  = These<string, int>.Left("something went wrong");

// Value only — clean success
These<string, int> right = These<string, int>.Right(42);

// Both — value produced but a warning accompanies it
These<string, int> both  = These<string, int>.Both("partial failure", 42);
```

### State Flags

```csharp
bool isLeft  = these.IsLeft;  // true when Left only
bool isRight = these.IsRight; // true when Right only
bool isBoth  = these.IsBoth;  // true when Both
```

`HasLeft` is `true` for `Left` and `Both`, `HasRight` is `true` for `Right` and `Both`, and `LeftOrDefault` and `RightOrDefault` give the raw payloads (`default` when that side is absent). `default(These<TError, TValue>)` is none of the three states, and `Match` routes it to `onBoth` with default values, so create instances through `Left`, `Right` or `Both`.

### Map: Transform the Value

`Map` applies a function to the value. Left passes through unchanged; Both transforms the value while keeping the error.

```csharp
These<string, int> result = These<string, int>.Right(10)
    .Map(n => n * 2);   // Right(20)

These<string, int> both = These<string, int>.Both("warn", 10)
    .Map(n => n * 2);   // Both("warn", 20)
```

### MapLeft: Transform the Error

```csharp
These<int, string> result = These<string, string>.Left("err")
    .MapLeft(e => e.Length);  // Left(3)
```

### FlatMap: Chain These

`FlatMap` sequences operations that return `These`. When chaining from `Both`, the original error is kept: a mapper result of `Right` or `Both` yields `Both(originalError, newValue)` (the mapper's own error in a `Both` is dropped because `TError` has no combine operator), and a mapper result of `Left` yields that `Left`.

```csharp
These<string, int> result = These<string, int>.Right(5)
    .FlatMap(n => n > 0
        ? These<string, int>.Right(n * 10)
        : These<string, int>.Left("non-positive"));
```

### Tap / TapLeft: Side Effects

```csharp
these
    .Tap(v => logger.LogInformation("Value: {V}", v))      // fires on Right or Both
    .TapLeft(e => logger.LogWarning("Error: {E}", e));      // fires on Left or Both
```

### Match: Exhaustive Pattern Match

```csharp
string message = these.Match(
    onLeft:  error        => $"Failed: {error}",
    onRight: value        => $"OK: {value}",
    onBoth:  (error, val) => $"Partial: {val} (warning: {error})");
```

### GetRight / GetLeft

Both return `Maybe<T>` and do not throw on the wrong state.

```csharp
using CSharpEssentials.Maybe;

Maybe<int>    value = these.GetRight();  // Some(42) or None
Maybe<string> error = these.GetLeft();   // Some("msg") or None
```

`GetRight()` is `Some` for `Right` and `Both`, `GetLeft()` is `Some` for `Left` and `Both`. A `null` payload gives `None`.

## Result Bridge

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.These;

// From Result<T>: a success becomes Right, a failure becomes Left with its first error only
Result<int> result = GetSomeResult();
These<Error, int> these = TheseExtensions.FromResult(result);

// To Result<T> — strict: Both is treated as failure (Left wins)
Result<int> strict = these.ToResult();

// To Result<T> — lenient: Both is treated as success (value wins)
Result<int> lenient = these.ToResultLenient();
```

| Conversion | Left | Right | Both |
|------------|------|-------|------|
| `ToResult()` | Failure | Success | Failure |
| `ToResultLenient()` | Failure | Success | Success |

`FromResult` never produces `Both`, and it keeps only the first error of a failed result. Both `ToResult` methods are defined only for `These<Error, TValue>`.

## Collection Extensions

Partition a sequence into its three buckets in one pass.

```csharp
IEnumerable<These<string, int>> items = GetItems();

var (lefts, rights, boths) = items.Partition();

// lefts  : IReadOnlyList<string>
// rights : IReadOnlyList<int>
// boths  : IReadOnlyList<(string, int)>

foreach (string error in lefts)
    Console.WriteLine($"Error: {error}");

foreach ((string warning, int value) in boths)
    Console.WriteLine($"Value {value} with warning: {warning}");
```

## When to Use These vs Result

| Scenario | Use |
|----------|-----|
| Operation either succeeds or fails | `Result<T>` |
| Partial success with a non-fatal warning | `These<TError, TValue>` |
| Accumulating errors while still producing a value | `These<TError, TValue>` |
| Enrichment pipelines (value + audit log) | `These<TError, TValue>` |
