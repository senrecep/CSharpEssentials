# CSharpEssentials.Compat.CSharp12

Compile-only guard for consumers on C# 12 (`LangVersion=12`). C# 12 ignores `[OverloadResolutionPriority]`, which the
Results package uses to prefer the `Task` handler over its `ValueTask` twin when an untyped `async` lambda fits both.

It builds for two targets:

- **net8.0** consumes the `netstandard2.1` asset. That asset has no `ValueTask` handler twins, so untyped async lambdas bind on their own.
- **net9.0** consumes the `net9.0` asset. That asset has the `ValueTask` twins, so some shapes need a typed handler under C# 12.

## Shapes that bind under C# 12 on every target

These are in `TaskSourceShapes` and `ValueTaskSourceShapes`. On `Task<Result>` and `Task<Result<T>>` sources the untyped async lambda shapes are:

- `MapAsync`, `BindAsync`, `TapAsync`, `EnsureAsync`, `MatchAsync`, `ThenAsync`.
- `MapErrorAsync`, when the lambda body only compiles for `Error`, for example because it reads `error.Description`.

On `ValueTask<Result>` and `ValueTask<Result<T>>` sources they are `MapAsync`, `BindAsync` and `TapAsync`. Sync lambdas bind on both kinds of source.

## Shapes that need C# 13, or a typed handler on net9.0+

These are in `UntypedTaskOnlyShapes` (net8.0) and `TypedTwinShapes` (net9.0).

On net9.0+ under C# 12, an untyped async lambda reports CS0121 for the following:

| Receiver | Methods |
|---|---|
| `Result<T>` / `Result` instance | `MapAsync`, `TapAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `EnsureAsync`, `TapIfAsync`, `ThenAsync`, `ThenDoAsync` |
| `ValueTask<Result<T>>` / `ValueTask<Result>` source | `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `EnsureAsync`, `TapIfAsync`, `ThenAsync`, `ThenDoAsync` |
| `IEnumerable<T>` | `TraverseAsync` |

Fix it in either of these ways:

- Move to C# 13 or later.
- Type the first handler, for example `(Func<int, Task<string>>)(async v => ...)` or a local function. A handler that returns `ValueTask` binds the `ValueTask` twin.

## Shapes that are ambiguous on every target and language version

These are in `TypedDelegateShapes`. In each case the lambda parameter or return type cannot choose between overloads, so the handler must be typed:

- `Result<T>.MapErrorAsync(async e => ...)` when the body compiles for both `Error` and `Error[]`, for example `return e;`.
- `Result<T>.BindAsync(async v => ...)` returning `Result<T>`: the `Result<TOut>` handler and the `Result` handler both fit, because `Result<T>` converts to `Result`.
