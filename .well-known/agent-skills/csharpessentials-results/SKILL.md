---
name: csharpessentials-results
description: Use when handling operation outcomes without exceptions. Covers Result and Result<T> for success/failure, railway-oriented chaining with Then/ThenAsync/Ensure, Match for consumption, and Result.And/Or for combining multiple results.
---

# CSharpEssentials.Results

`Result` and `Result<T>` model operation outcomes as values instead of exceptions.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Namespace

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;
```

## Creating results

```csharp
// Success
Result ok          = Result.Success();                              // non-generic: keep the factory
Result<User> found = user;                                          // T → Result<T>

// Failure: Error, Error[], List<Error>, HashSet<Error> convert implicitly
Result fail          = Error.Validation("Input.Invalid", "Input was invalid.");
Result<User> missing = Error.NotFound("User.NotFound", "Not found.");
Result<int> multi    = new[]
{
    Error.Validation("Name.Empty", "Name is required."),
    Error.Validation("Email.Invalid", "Email is invalid.")
};

// Explicit factories: only where the target type cannot be inferred (var, inferred lambdas, generic arguments)
var conflict = Result.Failure<string>(Error.Conflict("User.Duplicate", "Duplicate."));
Task<Result<int>> pending = Task.FromResult(Result.Success(42));
```

## Checking the result

```csharp
if (result.IsFailure)
{
    Error first = result.FirstError; // first error in the list
    Error[] all = result.Errors;
}

if (result.IsSuccess)
{
    User value = result.Value;       // default on a failure
}
```

## Chaining: railway-oriented

```csharp
// Then: transform value, short-circuits on failure
Result<int> result = Parse("5")
    .Then(n => n * 2)
    .Then(n => n + 10);

// ThenAsync: async chain
Result<OrderConfirmation> placed = await GetUserAsync(id)
    .ThenAsync(user => ValidateOrderAsync(user, order))
    .ThenAsync(order => ChargePaymentAsync(order));

// Ensure: guard condition, adds error if predicate fails
Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0,   Error.Validation("Range", "Must be positive."))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100."));

// EnsureAsync
Result<User> validated = await GetUserAsync(id)
    .EnsureAsync(u => IsActiveAsync(u), Error.Validation("User.Inactive", "Account is inactive."));
```

## Mapping errors

```csharp
// Per-error mapper: called once for every error, in order; never on success
Result<User> renamed = GetUser(id)
    .MapError(e => Error.Failure($"Users.{e.Code}", e.Description));

// Array mapper: replaces the whole error array
Result<User> collapsed = GetUser(id)
    .MapError(errors => [Error.Failure("Users.Failed", $"{errors.Length} error(s)")]);
```

`MapError(Func<Error, Error>)` maps every error. Older versions mapped only `FirstError` and dropped the rest.

```csharp
// MapErrorAsync (instance): Task or ValueTask mappers, per error or per array
Result<User> localized = await result.MapErrorAsync(e => LocalizeAsync(e, ct), ct);

// Task<Result<T>> source: sync or Task mappers; ValueTask<Result<T>> source: sync or ValueTask mappers
Result<User> mapped = await GetUserAsync(id)
    .MapErrorAsync(e => Error.Failure($"Users.{e.Code}", e.Description));
```

Per-error async mappers run one at a time, in order. The `CancellationToken` is checked before each mapper call; a mapper that is already running is not abandoned, so pass the token into the mapper if it must stop early. An async lambda (`async e => ...`) binds to the `Task` overload.

## Consuming: Match

```csharp
string msg = result.Match(
    onSuccess: value  => $"OK: {value}",
    onError:   errors => $"Failed: {errors[0].Description}");  // errors is Error[]

// Async match: both handlers return Task<T>
bool sent = await result.MatchAsync(
    onSuccess: async value  => await SendConfirmationAsync(value),
    onError:   async errors => { await LogErrorsAsync(errors); return false; });
```

## Combining results

```csharp
// And: Result.And returns the errors of the first failure
Result combined = Result.And(r1, r2, r3);
Result<int[]> values = Result<int>.And(v1, v2); // collects every value, or every error

// Or: first success wins
Result any = Result.Or(r1, r2, r3);
```

## Recovery, side effects and extraction

```csharp
// Else: replace a failure (Error, Error[], fallback value or a function of the errors)
Result<int> fallback = GetCount().Else(0);

// Compensate / Recover: turn a failure back into a success
Result<int> healed  = GetCount().Compensate(errors => Result.Success(0));
Result<int> cached  = GetCount().Recover(ErrorType.NotFound, error => 0);

// TapError / ElseDo: side effect on failure, result unchanged
GetCount().TapError(errors => logger.LogWarning("{Count} errors", errors.Length));

// FailIf: the opposite of Ensure
Result<int> checkedCount = GetCount().FailIf(v => v > 100, Error.Validation("Count.TooHigh", "Too many."));

// Extraction
int a = GetCount().GetValueOrDefault(0);
int b = GetCount().Unwrap();                    // ResultUnwrapException (with .Errors) on failure
int c = GetCount().GetValueOrThrow("no count"); // InvalidOperationException on failure

// TryCatch: run a function on success, exception becomes an Error
Result<string> text = GetCount().TryCatch(v => Result.Success(v.ToString()));

// Conditional construction and tuple combine
Result guard = Result.SuccessIf(age >= 18, Error.Validation("Age.Minor", "Must be 18 or older."));
Result<(int, string)> pair = Result<int>.Combine(GetCount(), GetName());   // collects every error
```

## Async collections

```csharp
// Sequential (one item at a time), accumulates ALL errors in input order
Result<OrderDto[]> orders = await orderIds.TraverseAsync((id, ct) => GetOrderAsync(id, ct), ct);

// Already-started tasks: awaits every task, even after a failure; only the first exception is rethrown
Result<int[]> values = await tasks.SequenceAsync();
```

Cancellation throws `OperationCanceledException` (never an error `Result`). On cancellation `SequenceAsync` observes the remaining tasks of a materialized collection and does not enumerate lazy sources further. The `ValueTask` selector twins of `TraverseAsync` exist on .NET 9+ only.

## Safe execution

```csharp
// Wrap exception → Result
Result<int> safe = Result.Try(() => int.Parse(input), ex => Error.Exception(ex));

// Async
Result<Data> data = await Result.TryAsync(() => _db.GetAsync(id), ex => Error.Exception(ex), cancellationToken);
// Caller cancellation (OperationCanceledException while cancellationToken is cancelled) propagates; it is not converted to an Error.
// Other OperationCanceledExceptions (e.g. HttpClient timeout) still go through the handler.
```

## Conditional side effects

```csharp
// TapIf: runs only on success when the predicate is true
result.TapIf(v => v > 0, v => Audit(v));

// Async: instance methods (the bool overload exists only on TapIfAsync)
await result.TapIfAsync(v => v > 0, async v => await AuditAsync(v));
await result.TapIfAsync(isEnabled,   async v => await TrackAsync(v));

// Extension variants: Task<Result<T>> and ValueTask<Result<T>>
await GetResultAsync().TapIfAsync(v => v > 0, v => Enqueue(v));
await GetValueTaskResultAsync().TapIfAsync(true, async v => await LogAsync(v));
```

## Async rules

- Every member that returns `Task` or `ValueTask` ends in `Async`. Always call the `*Async` name when a handler or source is async (`MapAsync`, `BindAsync`, `TapAsync`, `EnsureAsync`, `MatchAsync`, `ThenAsync`). The unsuffixed names on `Task`/`ValueTask` sources and the `Bind` overloads that take a `Task`/`ValueTask` function are `[Obsolete]` forwarders; never write them. The `CancellationToken` is optional on every `Task` source.
- Match the handler to the source: on `Task<Result..>` pass sync or `Task` handlers; on `ValueTask<Result..>` pass sync or `ValueTask` handlers. On an instance `Result`/`Result<T>`, `Task` handlers work everywhere; `ValueTask` handlers on instances and static `Result.TryAsync` (and on `ValueTask` sources for `Match*`, `Switch*`, `Ensure`, `TapIf`, `Then`, `ThenDo`, `Else`, `FailIf`, `TapError*`, `ElseDo*`, `Compensate*`, `ThenEnsure`, `Finally`) exist only on .NET 9+.
- Never pass an `async` lambda to an `Action` overload. It compiles as `async void`: nothing awaits it and its exceptions are lost. `TapAsync(async v => ...)` and the conditional `TapAsync(condition, async v => ...)` on a `Task`/`ValueTask` source bind the awaited `Func<T, Task>`/`Func<T, ValueTask>` overload. A non-`async` lambda whose flavour does not match the source also binds `Action` and its result is discarded: a `Task` on a `ValueTask` source (`vt.TapAsync(v => SaveAsync(v))`) or a `ValueTask` on a `Task` source (`taskResult.TapAsync(v => SaveValueTaskAsync(v))`). Write `async v => await SaveAsync(v)`. A non-`async` lambda returning a `Task` on a `Task` source is awaited.
- `MapAsync` on a `ValueTask<Result<T>>` with an `async` lambda returns `ValueTask<Result<U>>`.
- `TraverseAsync` is sequential and accumulates every error in input order (no short-circuit); cancellation throws. Use `SequenceAsync` for tasks that are already running.
- If the project targets .NET 9+ with `LangVersion` 12, untyped `async` lambdas are ambiguous (CS0121) on every operation with a `ValueTask` twin (the .NET 9+ list above, plus `TraverseAsync` and instance `MapAsync`/`TapAsync`). Use C# 13+ or type the first handler: `(Func<int, Task<string>>)(async v => ...)`.
- A mismatched handler flavour gives a nested type, not an error: `vt.MapAsync(x => Task.FromResult(x))` returns `ValueTask<Result<Task<int>>>`.
- On C# 12, `MapErrorAsync(async e => ...)` and `Result<T>.BindAsync(async v => ...)` are ambiguous on every target; type the handler with a cast (`(Func<int, Task<Result<int>>>)(async v => ...)`), because annotating the lambda parameter alone is not enough. On C# 13+ both compile and `MapErrorAsync` matches the lambda on what its body returns: an `Error` (including `return e;`) binds the per-error mapper, a new `Error[]` binds the array mapper; cast to `Func<Error[], Task<Error[]>>` to be explicit.
- `result.Bind(async v => ...)` (no `Async` suffix) is ambiguous on every language version; call `BindAsync`.

```csharp
Result<OrderDto> order = await LoadOrderAsync(id, ct)                    // Task<Result<Order>>
    .EnsureAsync(o => o.Lines.Count > 0, Error.Validation("Order.Empty", "Order has no lines."))
    .TapAsync(async o => await _audit.WriteAsync(o.Id, ct))             // awaited
    .MapAsync(o => new OrderDto(o));
```

## Rules

- Check `.IsSuccess` before reading `.Value`; it is `default` on a failure.
- `onError` in `Match` receives `Error[]`; `MatchFirst` passes a single `Error`.
- Prefer implicit conversions (`return value;`, `return Error.NotFound(...);`); use `Result.Success(value)`/`Result.Failure<T>(error)` only where the target type cannot be inferred. Keep `Result.Success()` for non-generic success.
