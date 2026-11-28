# CSharpEssentials.Results example

A console app that runs the `CSharpEssentials.Results` API section by section and prints what each call returns. It needs no database or network.

## Run

```bash
cd examples/Examples.Results
dotnet run
```

The project targets net10.0. It gets `LangVersion` 15 from the repository's `Directory.Build.props`, so the switch expressions over typed results compile. It references `CSharpEssentials.Results.Generators` as an analyzer, which registers the typed result JSON converter and runs the CSE5001-CSE5006 analyzers.

## What each section shows

| Section | Members |
|---|---|
| Creating results | `Result.Success()`, implicit conversions from a value, an `Error`, an `Error[]` and a `bool`, deconstruction, `ToString()` |
| Chaining | `Then`, `Bind`, `Tap`, `BindIf`, `Map`, and how a failure skips the rest |
| Ensuring | `Ensure` with an error and with an error factory, `FailIf`, `ThenEnsure` |
| Matching | `Match`, `MatchFirst`, `MatchLast`, `Switch` |
| Handling failures | `Else`, `Compensate`, `CompensateFirst`, `Recover`, `RecoverFirst`, `TapError`, `TapErrorFirst`, `TapErrorIf`, `ElseDo`, `ElseDoFirst`, `MapError` |
| Safe execution | `Result.Try`, `Result.TryAsync`, `TryCatch` |
| Getting the value out | `GetValueOrDefault`, `UnwrapOrDefault`, `Finally`, `Unwrap` (`ResultUnwrapException`), `GetValueOrThrow` |
| Combining | `Result.And`, `CombineAll`, `Result.Or`, `Result<T>.Combine`, `SuccessIf`, `FailureIf`, LINQ query syntax |
| Collections | `Sequence`, `Traverse`, `Partition`, `FirstFailureOrSuccesses`, `TraverseAsync`, `SequenceAsync` |
| Async chains | `EnsureAsync`, `TapAsync`, `MapAsync`, `BindAsync`, `MatchAsync` on a `Task<Result<T>>` |
| More members | `LastError`, `ErrorsOrEmptyArray`, `TryGet`, `ValueOrDefault` and the `\|` operator (.NET 10), `Result.From`, `ToResult` extensions, `SwitchFirst`, `SwitchLast`, `ThenDo`, `TapIf`, `Select`, `SelectMany`, `EnsureNotNull` on a null read from JSON, `IResultBase`, `ResultComparer<T>`, `TrimStart`, `TrimEnd` |
| More async members | `ElseAsync`, `CompensateAsync`, `FailIfAsync`, `ThenAsync`, `ThenEnsureAsync`, `MapErrorAsync`, `MatchFirstAsync`, `GetValueOrDefaultAsync`, `TapErrorAsync`, `SwitchAsync`, `FinallyAsync` |
| Typed results | `Result<T, E1, E2>` with an exhaustive switch expression, `Match`, `ToResult`, `BindWiden`, `Recover`, `TryCatch<TOut, TException>`, `TapError`, `Traverse`, `GetValueOr`, `HasValue`, `GetValueOrAsync`, `BindWidenAsync` |
| Typed result JSON | the `{"case": ..., "value": ...}` envelope through a source-generated `JsonSerializerContext`, and `TypedResultJsonConverterFactory.Register` |

See the [package Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD) for the full API.
