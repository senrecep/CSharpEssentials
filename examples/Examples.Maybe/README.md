# CSharpEssentials.Maybe example

A console app that walks through `Maybe<T>` with an in-memory user lookup. It needs no external services.

## Run

```bash
cd examples/Examples.Maybe
dotnet run
```

## What each part shows

1. Creating: implicit conversion, `Maybe.None`, `Maybe.From`, `AsMaybe`, `FromTry` and the lazy `From(Func<T?>)`.
2. Reading: `HasValue`, `Value`, the `GetValueOrDefault` overloads, `GetValueOrElse`, `TryGetValue`, `Deconstruct`, `AsNullable`, `ToList` and `GetValueOrThrow`.
3. Chaining: `Bind`, `Map`, `Where`, `Or`, `OrElse`, `MapIf` and `BindIf`.
4. LINQ query syntax over two lookups.
5. `Match` returning a value and `Match` with two actions.
6. Side effects: `Tap`, `TapIf`, `TapNone`, `Execute` and `ExecuteNoValue`.
7. The Result bridge: `ToResult(error)`, `ToMaybeResult()`, `ToUnitResult()`, `AsMaybe()` and a typed `Result<T, TError>`.
8. Collections: `TryFirst`, `TryLast`, `TryFind`, `Choose`, `Partition`, `Sequence`, `Traverse` and `Flatten`.
9. Async: `Maybe.FromAsync`, `MapAsync`, `OrAsync` on a `Task<Maybe<T>>`, `MatchAsync`, `ToResultAsync`, `BindAsync`, `WhereAsync`, `TapAsync`, `ExecuteAsync`, `GetValueOrDefaultAsync`, `AsMaybeAsync` and `ChooseAsync`.
10. Equality rules and the JSON shape.
