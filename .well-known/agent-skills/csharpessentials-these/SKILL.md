---
name: csharpessentials-these
description: Use when a result can succeed with a warning (partial success, capped values, batch imports). These<TError, TValue> is Left (error), Right (value) or Both. Covers the Left/Right/Both factories, IsLeft/IsRight/IsBoth, Match, Map, MapLeft, FlatMap, Tap, TapLeft, GetRight/GetLeft (Maybe<T>), TheseExtensions.FromResult, ToResult/ToResultLenient and Partition.
---

# CSharpEssentials.These

`These<TError, TValue>` is a `readonly record struct` with three states: `Left` (error only), `Right` (value only) and `Both` (value plus error). An error does not mean failure.

```bash
dotnet add package CSharpEssentials.These
```

```csharp
using CSharpEssentials.These;         // These<TError, TValue>, TheseExtensions, TheseCollectionExtensions
using CSharpEssentials.Maybe;         // Maybe<T> returned by GetRight / GetLeft
using CSharpEssentials.Errors;        // Error, for the Result bridge
using CSharpEssentials.ResultPattern; // Result<T>, for the Result bridge
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.These
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.These;

These<string, int>[] rows = [.. new[] { "10", "abc", "250" }.Select(ParseQuantity)];

foreach (These<string, int> row in rows)
{
    Console.WriteLine(row.Match(
        onLeft: e => $"rejected: {e}",
        onRight: v => $"accepted: {v}",
        onBoth: (e, v) => $"accepted: {v} (warning: {e})"));
}

var (errors, clean, warned) = rows.Partition();
Console.WriteLine($"{errors.Count} rejected, {clean.Count} clean, {warned.Count} with warnings");

These<Error, int> capped = These<Error, int>.Both(Error.Validation("Qty.Capped", "Capped at 100."), 100);
Result<int> strict = capped.ToResult();          // Both -> failure
Result<int> lenient = capped.ToResultLenient();  // Both -> success
Console.WriteLine($"strict failed: {strict.IsFailure}, lenient value: {lenient.Value}");

static These<string, int> ParseQuantity(string text)
{
    if (!int.TryParse(text, out int n))
        return These<string, int>.Left($"'{text}' is not a number");
    if (n > 100)
        return These<string, int>.Both($"{n} capped at 100", 100);
    return These<string, int>.Right(n);
}
```

## Creating values

```csharp
These<string, int> failure = These<string, int>.Left("error");
These<string, int> success = These<string, int>.Right(42);
These<string, int> partial = These<string, int>.Both("warning", 42);
```

There are no implicit conversions. Use the three static factories.

## Reading state

```csharp
These<string, int> these = These<string, int>.Both("warning", 42);

bool onlyError = these.IsLeft;      // Left only
bool onlyValue = these.IsRight;     // Right only
bool bothSides = these.IsBoth;      // Both
bool anyError = these.HasLeft;      // Left or Both
bool anyValue = these.HasRight;     // Right or Both
string? raw = these.LeftOrDefault;  // default when there is no error
int number = these.RightOrDefault;  // default when there is no value

Maybe<int> value = these.GetRight();    // Some for Right and Both
Maybe<string> error = these.GetLeft();  // Some for Left and Both
```

## Handling all states

```csharp
string message = these.Match(
    onLeft: e => $"Error: {e}",
    onRight: v => $"Value: {v}",
    onBoth: (e, v) => $"Value {v} with warning {e}");
```

## Transforming

```csharp
These<string, int> doubled = these.Map(x => x * 2);               // value side; Left unchanged
These<int, int> errorLength = these.MapLeft(e => e.Length);       // error side; Right unchanged
These<string, string> text = these.FlatMap(x => These<string, string>.Right(x.ToString()));

these.Tap(v => Console.WriteLine(v));       // runs on Right and Both
these.TapLeft(e => Console.WriteLine(e));   // runs on Left and Both
```

`FlatMap` from `Both(e, v)`: a mapper result of `Right(x)` or `Both(e2, x)` gives `Both(e, x)` (the mapper's `e2` is dropped). A mapper result of `Left(e2)` gives `Left(e2)`. From `Left`, the mapper is not called.

## Result bridge

```csharp
Result<int> result = 5;
These<Error, int> fromResult = TheseExtensions.FromResult(result); // success -> Right, failure -> Left(first error)

These<Error, int> partialResult = These<Error, int>.Both(Error.Validation("W", "warn"), 1);
Result<int> strict = partialResult.ToResult();          // Left and Both fail
Result<int> lenient = partialResult.ToResultLenient();  // only Left fails; Both keeps the value
```

## Splitting a batch

```csharp
IEnumerable<These<string, int>> items = [These<string, int>.Right(1), These<string, int>.Left("x")];
var (lefts, rights, boths) = items.Partition();
// IReadOnlyList<string>, IReadOnlyList<int>, IReadOnlyList<(string, int)>
```

## Rules

- `FromResult` is static: write `TheseExtensions.FromResult(result)`. `result.FromResult()` does not compile.
- `ToResult()` and `ToResultLenient()` exist only for `These<Error, TValue>`.
- `FromResult` never produces `Both` and keeps only the first error of a failed result.
- Never use `default(These<,>)`. It is none of the three states: `Match` sends it to `onBoth` with default values and `ToResult()` throws `InvalidOperationException`.
- The members are `LeftOrDefault` and `RightOrDefault`. There is no `Left` or `Right` property and no `Value` property.
- JSON shape is `{"isLeft":...,"isRight":...,"left":...,"right":...}`. Both flags are `true` for `Both`.
- Use `Result<T>` for plain success or failure and `Maybe<T>` for a missing value. Use `These` only when a value and an error must travel together.

## Diagnostics

This package reports no analyzer diagnostics.
