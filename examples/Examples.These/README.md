# CSharpEssentials.These example

A console app that walks through every feature of `These<TError, TValue>`. It needs no external services.

## Run

```bash
cd examples/Examples.These
dotnet run
```

## What each part shows

1. The `Left`, `Right` and `Both` factories and the `IsLeft`, `IsRight`, `IsBoth`, `HasLeft` and `HasRight` flags.
2. `Match` with one handler per state.
3. `Map` on the value and `MapLeft` on the error.
4. `FlatMap`, including how a `Both` keeps its own error.
5. `Tap` and `TapLeft` for side effects.
6. `GetRight` and `GetLeft`, which return `Maybe<T>`.
7. `TheseExtensions.FromResult`, `ToResult` (a `Both` fails) and `ToResultLenient` (a `Both` succeeds).
8. `Partition` on a batch of parsed quantities.
9. A JSON round trip with `System.Text.Json`.
