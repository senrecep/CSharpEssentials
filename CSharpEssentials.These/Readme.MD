# CSharpEssentials.These

`These<TError, TValue>` holds an error, a value, or both at once. Use it when an operation can succeed with a warning, so the value and the problem travel together instead of the error turning the whole result into a failure.

```bash
dotnet add package CSharpEssentials.These
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Results`, `CSharpEssentials.Maybe` and `CSharpEssentials.Errors`. The type is a `readonly record struct` in the `CSharpEssentials.These` namespace.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.These
using CSharpEssentials.These;

string[] input = ["10", "abc", "250"];

foreach (string text in input)
{
    These<string, int> parsed = ParseQuantity(text);
    string line = parsed.Match(
        onLeft: error => $"rejected: {error}",
        onRight: value => $"accepted: {value}",
        onBoth: (warning, value) => $"accepted: {value} (warning: {warning})");
    Console.WriteLine(line);
}

static These<string, int> ParseQuantity(string text)
{
    if (!int.TryParse(text, out int n))
        return These<string, int>.Left($"'{text}' is not a number");
    if (n > 100)
        return These<string, int>.Both($"{n} capped at 100", 100);
    return These<string, int>.Right(n);
}
```

Output:

```text
accepted: 10
rejected: 'abc' is not a number
accepted: 100 (warning: 250 capped at 100)
```

## States

| Factory | `IsLeft` | `IsRight` | `IsBoth` | `HasLeft` | `HasRight` |
|---|---|---|---|---|---|
| `These<E, A>.Left(error)` | true | false | false | true | false |
| `These<E, A>.Right(value)` | false | true | false | false | true |
| `These<E, A>.Both(error, value)` | false | false | true | true | true |

`LeftOrDefault` and `RightOrDefault` return the raw payloads, or `default` when that side is missing.

Always create values through the three factories. `default(These<E, A>)` has both flags `false`, so it is none of the three states: `Match` sends it to `onBoth` with default values.

## Match

`Match` takes one handler per state and returns a value. All three handlers are required.

```csharp
These<string, int> both = These<string, int>.Both("rounded down", 7);

string text = both.Match(
    onLeft: e => $"Left({e})",
    onRight: v => $"Right({v})",
    onBoth: (e, v) => $"Both({e}, {v})");   // "Both(rounded down, 7)"
```

## Map and MapLeft

`Map` changes the value. A `Left` passes through and a `Both` keeps its error. `MapLeft` changes the error the same way and leaves a `Right` alone.

```csharp
These<string, int> right = These<string, int>.Right(42);
These<string, int> both = These<string, int>.Both("rounded down", 7);
These<string, int> left = These<string, int>.Left("not a number");

These<string, int> doubled = right.Map(n => n * 2);        // Right(84)
These<string, int> stillWarned = both.Map(n => n * 2);     // Both("rounded down", 14)
These<int, int> errorLength = left.MapLeft(e => e.Length); // Left(12)
```

## FlatMap

`FlatMap` chains a function that returns another `These`.

| Source | Mapper returns | Result |
|---|---|---|
| `Left(e)` | not called | `Left(e)` |
| `Right(v)` | anything | what the mapper returned |
| `Both(e, v)` | `Right(x)` or `Both(e2, x)` | `Both(e, x)` |
| `Both(e, v)` | `Left(e2)` | `Left(e2)` |

From a `Both`, the original error wins and the mapper's error inside a `Both` is dropped, because `TError` has no way to combine two errors.

```csharp
These<string, int> Half(int n) => n % 2 == 0
    ? These<string, int>.Right(n / 2)
    : These<string, int>.Both("odd value halved", n / 2);

These<string, int> a = These<string, int>.Right(42).FlatMap(Half);              // Right(21)
These<string, int> b = These<string, int>.Both("rounded down", 7).FlatMap(Half); // Both("rounded down", 3)
```

## Tap and TapLeft

Side effects that return the same `These`. `Tap` runs on `Right` and `Both`. `TapLeft` runs on `Left` and `Both`.

```csharp
These<string, int> both = These<string, int>.Both("rounded down", 7);

both
    .Tap(v => Console.WriteLine($"value: {v}"))
    .TapLeft(e => Console.WriteLine($"warning: {e}"));
```

## GetRight and GetLeft

Both return a `Maybe<T>` (from `CSharpEssentials.Maybe`) and never throw. `GetRight()` has a value for `Right` and `Both`. `GetLeft()` has a value for `Left` and `Both`. A `null` payload gives `None`.

```csharp
using CSharpEssentials.Maybe;

These<string, int> both = These<string, int>.Both("rounded down", 7);

Maybe<int> value = both.GetRight();    // Some(7)
Maybe<string> error = both.GetLeft();  // Some(rounded down)
```

## Converting to and from Result

The bridge works with `These<Error, TValue>` and `Result<TValue>` from `CSharpEssentials.Results`.

```csharp Program.cs
// dotnet add package CSharpEssentials.These
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.These;

Result<int> ok = 5;
Result<int> failed = Error.Validation("Qty.Invalid", "Quantity is invalid.");

These<Error, int> fromOk = TheseExtensions.FromResult(ok);
These<Error, int> fromFailed = TheseExtensions.FromResult(failed);
Console.WriteLine($"{fromOk.IsRight} {fromFailed.IsLeft}");   // True True

These<Error, int> partial = These<Error, int>.Both(
    Error.Validation("Qty.Capped", "Quantity capped at 10."), 10);

Result<int> strict = partial.ToResult();          // Both counts as failure
Result<int> lenient = partial.ToResultLenient();  // Both counts as success
Console.WriteLine($"{strict.IsFailure} {strict.FirstError.Code}");  // True Qty.Capped
Console.WriteLine($"{lenient.IsSuccess} {lenient.Value}");          // True 10
```

| Method | Left | Right | Both |
|---|---|---|---|
| `ToResult()` | failure | success | failure with the error |
| `ToResultLenient()` | failure | success | success with the value, error dropped |

- `FromResult` is a static method on `TheseExtensions`, not an extension method. Call it as `TheseExtensions.FromResult(result)`.
- `FromResult` never returns `Both`. A failed result becomes `Left` with its first error only.
- `ToResult()` on `default(These<Error, T>)` throws `InvalidOperationException`.

## Partition

`Partition` splits a sequence into its three buckets in one pass.

```csharp
These<string, int>[] items =
[
    These<string, int>.Right(10),
    These<string, int>.Left("bad row"),
    These<string, int>.Both("capped", 100),
];

var (lefts, rights, boths) = items.Partition();
// lefts:  IReadOnlyList<string>        ["bad row"]
// rights: IReadOnlyList<int>           [10]
// boths:  IReadOnlyList<(string, int)> [("capped", 100)]
```

## JSON

`System.Text.Json` writes the four stored properties and reads them back to the same state:

```json
{"isLeft":true,"isRight":true,"left":"rounded down","right":7}
```

`isLeft` holds `HasLeft` and `isRight` holds `HasRight`, so a `Both` has both set to `true`.

## These, Result or Maybe

| Situation | Type |
|---|---|
| The operation either works or fails | `Result<T>` |
| The value may be missing, with no reason attached | `Maybe<T>` |
| A value was produced but something needs reporting | `These<TError, TValue>` |

## Links

- Agent skill: [csharpessentials-these](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-these/SKILL.md)
- Example project: [examples/Examples.These](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.These)
- Benchmarks: [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.These/BENCHMARKS.md)
- Related: [CSharpEssentials.Results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD), [CSharpEssentials.Maybe](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Maybe/Readme.MD)
