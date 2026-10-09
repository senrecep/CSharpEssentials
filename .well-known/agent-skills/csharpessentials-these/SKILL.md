---
name: csharpessentials-these
description: Use when modeling partial success. These<TError, TValue> holds Left (error only), Right (value only), or Both (error + value), enabling scenarios where a result can partially succeed while carrying warnings. Use FromResult to bridge from Result<T>, ToResult/ToResultLenient to bridge back, and Partition to split collections.
---

# CSharpEssentials.These

`These<TError, TValue>` is a three-state discriminated union: Left (failure), Right (success), or Both (partial success with warning). Unlike `Result<T>`, the Both state lets you carry a value AND an error simultaneously.

## Installation

```bash
dotnet add package CSharpEssentials.These
```

## Namespace

```csharp
using CSharpEssentials.These;
using CSharpEssentials.Maybe;         // for GetRight()/GetLeft()
using CSharpEssentials.Errors;        // for Error type in bridge methods
using CSharpEssentials.ResultPattern; // for Result<T> in bridge methods
```

## Creating These

```csharp
These<string, int> failure = These<string, int>.Left("error");
These<string, int> success = These<string, int>.Right(42);
These<string, int> partial = These<string, int>.Both("warning", 42);
```

## Checking State

```csharp
bool isErr  = these.IsLeft;
bool isOk   = these.IsRight;
bool isBoth = these.IsBoth;

// Safe access via Maybe<T>
Maybe<int>    value = these.GetRight(); // None when IsLeft
Maybe<string> error = these.GetLeft();  // None when IsRight
```

## Pattern Match

```csharp
string msg = these.Match(
    onLeft:  e      => $"Error: {e}",
    onRight: v      => $"Value: {v}",
    onBoth:  (e, v) => $"Partial: {v} (warning: {e})");
```

## Transforming

```csharp
// Map transforms the right value; Left passes through unchanged
These<string, int> doubled = these.Map(x => x * 2);

// MapLeft transforms the error; Right passes through unchanged
These<string, int> upper = these.MapLeft(e => e.ToUpper());

// FlatMap chains — Both keeps its original error when the mapper returns Right or Both
These<string, string> chained = these.FlatMap(x => These<string, string>.Right(x.ToString()));
```

## Side Effects

```csharp
// Tap fires on Right or Both
these.Tap(v => logger.LogInformation("Got {Value}", v));

// TapLeft fires on Left or Both
these.TapLeft(e => logger.LogWarning("Warning: {Warning}", e));
```

## Bridge to/from Result

```csharp
// Result<T> → These<Error, T>: success is Right, failure is Left with the first error only (never Both)
These<Error, int> these = TheseExtensions.FromResult(result);

// These<Error, T> → Result<T> (Both = failure)
Result<int> strict   = these.ToResult();

// These<Error, T> → Result<T> (Both = success, discards error side)
Result<int> lenient  = these.ToResultLenient();
```

## Partition Collections

```csharp
var (lefts, rights, boths) = items.Partition();
// lefts  → IReadOnlyList<TError>
// rights → IReadOnlyList<TValue>
// boths  → IReadOnlyList<(TError, TValue)>
```

## When to Use

| Scenario | Use |
|----------|-----|
| Operation must fully succeed or fail | `Result<T>` |
| Value may or may not exist | `Maybe<T>` |
| Partial success with a warning to propagate | `These<TError, TValue>` |
| Collecting errors while continuing | `These<TError, TValue>` with Both |

## Best Practices

- Prefer `Match()` over checking `IsLeft`/`IsRight`/`IsBoth` separately; it is exhaustive and compiler-safe
- `FlatMap` from Both keeps the original error: mapper `Right`/`Both` gives `Both(originalError, newValue)`, mapper `Left` gives that `Left`
- `ToResultLenient()` is the lenient bridge: Both → success (value wins, error side discarded)
- `ToResult()` is the strict bridge: Both → failure
- `default(These<TError, TValue>)` is none of Left, Right or Both; `ToResult()` on it throws `InvalidOperationException`
- A `These` serializes as `{"isLeft":...,"isRight":...,"left":...,"right":...}` (both flags are `true` for Both)
- Avoid using `These` as a general-purpose error type; use `Result<T>` for that; `These` is for partial-success semantics
