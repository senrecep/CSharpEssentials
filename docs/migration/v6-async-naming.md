# Async naming and the async overload matrix (6.4.0)

`CSharpEssentials.Results` and `CSharpEssentials.Maybe` give every member that returns `Task` or `ValueTask` an `Async` name, and fill the missing `Task`/`ValueTask` handler overloads (issues #108 and #121). Existing code keeps compiling; a few call sites bind to a different overload when recompiled, listed under [Behaviour changes](#behaviour-changes).

## Renamed members

The old names still compile as `[Obsolete]`, `[EditorBrowsable(Never)]` forwarders and will be removed in 7.0.

| Old name | New name | Receiver |
|---|---|---|
| `Bind(Func<.., Task<..>>)`, `Bind(Func<.., ValueTask<..>>)` | `BindAsync` | `Result`, `Result<T>` (also obsolete on `IResult`/`IResult<T>`) |
| `Else` | `ElseAsync` | `Task<Result..>`, `ValueTask<Result..>` |
| `FailIf` | `FailIfAsync` | `Task<Result<T>>`, `ValueTask<Result<T>>` |
| `Match`, `MatchFirst`, `MatchLast` | `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync` | `Task<Result..>`, `ValueTask<Result..>` |
| `Switch`, `SwitchFirst`, `SwitchLast` | `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync` | `Task<Result..>`, `ValueTask<Result..>` |
| `Then` | `ThenAsync` | `Task<Result..>`, `ValueTask<Result..>` |
| `ThenDo` | `ThenDoAsync` | `Task<Result..>`, `ValueTask<Result..>` |
| `Where` with a `Task`/`ValueTask` predicate or source | `WhereAsync` | `Maybe<T>`, `Task<Maybe<T>>`, `ValueTask<Maybe<T>>` |
| `Execute`, `ExecuteNoValue` with a `Task`/`ValueTask` handler or source | `ExecuteAsync`, `ExecuteNoValueAsync` | `Maybe<T>`, `Task<Maybe<T>>`, `ValueTask<Maybe<T>>` |
| `Or` with a `Task`/`ValueTask` fallback or source | `OrAsync` | `Maybe<T>`, `Task<Maybe<T>>`, `ValueTask<Maybe<T>>` |
| `Match` with a `Task`/`ValueTask` handler | `MatchAsync` | `Maybe<T>`, `Maybe<KeyValuePair<TKey, TValue>>` |
| `ToMaybeResult`, `ToMaybeUnitResult` on a `Task`/`ValueTask` source | `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` | `Task<Maybe<T>>`, `ValueTask<Maybe<T>>` |
| `Maybe.From(Task<T?>)`, `Maybe.From(Func<Task<T?>>)` | `Maybe.FromAsync` | static `Maybe` |

```csharp
// Before
string text = await LoadOrderAsync(id).Match(o => o.Number, errors => errors[0].Code);

// After
string text = await LoadOrderAsync(id).MatchAsync(o => o.Number, errors => errors[0].Code);
```

## New overloads

Handler flavours follow the source: a `Task<Result>` source takes sync and `Task` handlers, a `ValueTask<Result>` source takes sync and `ValueTask` handlers.

- Available on every target: instance `MapAsync` and `TapAsync` with `Task` handlers; `MapAsync` and `TapAsync` with `ValueTask` handlers on `ValueTask` sources; `TapAsync` with `Task` handlers on `Task` sources; `BindAsync(Func<Task<Result<TOut>>>)` on `Task<Result>`; `EnsureAsync` with a sync predicate on `Task<Result<T>>` and `ValueTask<Result<T>>`.
- .NET 9+ assets only: `ValueTask` twins of the instance `MapAsync`, `TapAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `EnsureAsync`, `TapIfAsync`, `ThenAsync` and `ThenDoAsync`, and the same `ValueTask` handlers on `ValueTask` sources.

The full table is in the [API reference](../API_REFERENCE.md#async-overload-matrix).

## Behaviour changes

1. `TapAsync` on `Task<Result>`, `Task<Result<T>>`, `ValueTask<Result>` and `ValueTask<Result<T>>` with an `async` lambda is now awaited. It used to bind `Action`/`Action<T>` and run as `async void`: the chain did not wait for the handler and its exceptions were lost. Handler exceptions now propagate to the caller.
2. `TapAsync` on `Task<Result>` and `Task<Result<T>>` with a non-`async` lambda that returns a `Task`, such as `taskResult.TapAsync(x => SaveAsync(x))`, now awaits that task. It used to bind `Action<T>` and discard it, so the chain did not wait and its exceptions were unobserved.
3. `MapAsync` on `ValueTask<Result<T>>` with an `async` lambda now returns `ValueTask<Result<U>>`. It used to return `ValueTask<Result<Task<U>>>`.
4. On .NET 9+, a lambda that returns `ValueTask` binds the new `ValueTask` handler instead of a sync or `Action` overload.
5. `ValueTask` handler twins exist only in the .NET 9+ assets. A library that targets `netstandard2.1` and calls them does not compile.
6. Projects that target .NET 9+ but set `LangVersion` 12 get CS0121 for untyped `async` lambdas on every operation that now has a `ValueTask` twin. See [Source compatibility](#source-compatibility).

Unchanged:

- A non-`async` lambda that returns a `Task` on a `ValueTask` source, such as `vt.TapAsync(v => SaveAsync(v))`, still binds `Action<T>`; the task is not awaited. Use `async v => await SaveAsync(v)` or return a `ValueTask`.
- A non-`async` lambda that returns a `ValueTask` on a `Task` source, such as `taskResult.TapAsync(x => SaveValueTaskAsync(x))`, still binds `Action<T>`; the `ValueTask` is discarded. Use `async x => await SaveValueTaskAsync(x)` or return a `Task`.
- When the handler flavour does not match the source, the result is a nested type, because there are no cross-flavour cells by design. For example `vt.MapAsync(x => Task.FromResult(x * 2))` on a `ValueTask<Result<int>>` binds the sync `Func<int, TOut>` overload and returns `ValueTask<Result<Task<int>>>`. Use a `ValueTask` handler on `ValueTask` sources and a `Task` handler on `Task` sources.
- The conditional `TapAsync(condition, action)` overloads on `Task`/`ValueTask` sources have no awaited twin yet, so an `async` lambda passed to them still runs as `async void`. Use `TapIfAsync` with a `Task` handler on `Result<T>` sources.
- `Result<T>.MapErrorAsync(async e => ...)` whose body fits both `Error` and `Error[]`, and `Result<T>.BindAsync(async v => ...)` returning `Result<T>`, stay ambiguous on every language version. Type the handler.

## Source compatibility

A project that targets .NET 9 or later and pins `LangVersion` 12 gets CS0121 for untyped `async` lambdas on `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `EnsureAsync`, `TapIfAsync`, `ThenAsync`, `ThenDoAsync` and `TraverseAsync`. These calls compiled before; they are now ambiguous because each operation gained a `ValueTask` twin and C# 12 ignores the `OverloadResolutionPriority` that picks the `Task` handler. The new instance `MapAsync` and `TapAsync` pairs behave the same way, and so do the `Maybe` instance and key/value members with a `Task` and a `ValueTask` twin (`ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `MatchAsync`), on every target including .NET 8. Projects on C# 13 or later, the default for .NET 9+, are not affected. Projects on .NET 8 (the `netstandard2.1` asset) are not affected by the `Result` twins, but are affected by the `Maybe` ones.

Fix it with C# 13 or later, or type the handler:

```csharp
// C# 12 on net9.0+
string text = await result.MatchAsync(
    (Func<int, Task<string>>)(async v => await FormatAsync(v)),
    async errors => errors[0].Code);
```

The first handler is enough to pick the overload. `tests/CSharpEssentials.Compat.CSharp12` lists every shape.
