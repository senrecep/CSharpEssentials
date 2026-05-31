# CSharpEssentials.These

`These<TError, TValue>` is a three-state discriminated union — `Left` (error only), `Right` (value only), or `Both` (error and value together). Unlike `Result<T>`, it does not treat the presence of an error as an automatic failure. Use it when partial success carries a meaningful value alongside a warning or non-fatal error.

## Features

- **Three-state union** — `Left`, `Right`, `Both` with explicit state flags.
- **Functional API** — `Map`, `MapLeft`, `FlatMap`, `Tap`, `TapLeft`, `Match`.
- **Maybe bridges** — `GetRight()` and `GetLeft()` return `Maybe<T>` without throwing.
- **Result bridges** — Convert to/from `Result<T>` in strict or lenient mode.
- **Collection extensions** — Partition a sequence into its three states.

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
these.IsLeft  // true when Left only
these.IsRight // true when Right only
these.IsBoth  // true when Both
```

### Map — Transform the Value

`Map` applies a function to the value. Left passes through unchanged; Both transforms the value while keeping the error.

```csharp
These<string, int> result = These<string, int>.Right(10)
    .Map(n => n * 2);   // Right(20)

These<string, int> both = These<string, int>.Both("warn", 10)
    .Map(n => n * 2);   // Both("warn", 20)
```

### MapLeft — Transform the Error

```csharp
These<int, string> result = These<string, string>.Left("err")
    .MapLeft(e => e.Length);  // Left(3)
```

### FlatMap — Chain These

`FlatMap` sequences operations that return `These`. When chaining from `Both`, the original error is discarded; the new result determines state.

```csharp
These<string, int> result = These<string, int>.Right(5)
    .FlatMap(n => n > 0
        ? These<string, int>.Right(n * 10)
        : These<string, int>.Left("non-positive"));
```

### Tap / TapLeft — Side Effects

```csharp
these
    .Tap(v => logger.LogInformation("Value: {V}", v))      // fires on Right or Both
    .TapLeft(e => logger.LogWarning("Error: {E}", e));      // fires on Left or Both
```

### Match — Exhaustive Pattern Match

```csharp
string message = these.Match(
    onLeft:  error        => $"Failed: {error}",
    onRight: value        => $"OK: {value}",
    onBoth:  (error, val) => $"Partial: {val} (warning: {error})");
```

### GetRight / GetLeft

Both return `Maybe<T>` — no exceptions on the wrong state.

```csharp
Maybe<int>    value = these.GetRight();  // Some(42) or None
Maybe<string> error = these.GetLeft();   // Some("msg") or None
```

## Result Bridge

```csharp
using CSharpEssentials.These;

// From Result<T>
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
