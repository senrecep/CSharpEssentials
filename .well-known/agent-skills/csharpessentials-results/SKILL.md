---
name: csharpessentials-results
description: Use when handling operation outcomes without exceptions. Covers Result and Result<T> for success/failure, railway-oriented chaining with Then/ThenAsync/Ensure, Match for consumption, and Result.And/Or for combining multiple results.
---

# CSharpEssentials.Results

`Result` and `Result<T>` model operation outcomes as values. No exceptions for control flow.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

## Namespace

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;
```

## Creating Results

```csharp
// Success
Result ok          = Result.Success();                              // non-generic: keep the factory
Result<User> found = user;                                          // T → Result<T>

// Failure — Error, Error[], List<Error>, HashSet<Error> convert implicitly
Result fail          = Error.Validation("Input.Invalid", "Input was invalid.");
Result<User> missing = Error.NotFound("User.NotFound", "Not found.");
Result<int> multi    = new[]
{
    Error.Validation("Name.Empty", "Name is required."),
    Error.Validation("Email.Invalid", "Email is invalid.")
};

// Explicit factories — only where the target type cannot be inferred (var, inferred lambdas, generic arguments)
var conflict = Result.Failure<string>(Error.Conflict("User.Duplicate", "Duplicate."));
Task<Result<int>> pending = Task.FromResult(Result.Success(42));
```

## Checking the Result

```csharp
if (result.IsFailure)
{
    Error first = result.FirstError; // first error in the list
    Error[] all = result.Errors;
}

if (result.IsSuccess)
{
    User value = result.Value;       // safe only after the IsSuccess check
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

// Ensure: guard condition — adds error if predicate fails
Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0,   Error.Validation("Range", "Must be positive."))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100."));

// EnsureAsync
Result<User> validated = await GetUserAsync(id)
    .EnsureAsync(u => IsActiveAsync(u), Error.Validation("User.Inactive", "Account is inactive."));
```

## Mapping Errors

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
// MapErrorAsync — instance: Task or ValueTask mappers, per error or per array
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

## Combining Results

```csharp
// And — all must pass (short-circuits on first failure)
Result combined = Result.And(r1, r2, r3);
Result<int[]> values = Result<int>.And(v1, v2); // generic form collects the values

// Or — first success wins
Result any = Result.Or(r1, r2, r3);
```

## Async Collections

```csharp
// Sequential (one item at a time), accumulates ALL errors in input order
Result<OrderDto[]> orders = await orderIds.TraverseAsync((id, ct) => GetOrderAsync(id, ct), ct);

// Already-started tasks: awaits every task, even after a failure; only the first exception is rethrown
Result<int[]> values = await tasks.SequenceAsync();
```

Cancellation throws `OperationCanceledException` (never an error `Result`). On cancellation `SequenceAsync` observes the remaining tasks of a materialized collection and does not enumerate lazy sources further. The `ValueTask` selector twins of `TraverseAsync` exist on .NET 9+ only; on C# 12 an untyped `async` lambda can then hit CS0121 (use LangVersion 13+ or a typed delegate).

## Safe Execution

```csharp
// Wrap exception → Result
Result<int> safe = Result.Try(() => int.Parse(input), ex => Error.Exception(ex));

// Async
Result<Data> data = await Result.TryAsync(() => _db.GetAsync(id), ex => Error.Exception(ex), cancellationToken);
// Caller cancellation (OperationCanceledException while cancellationToken is cancelled) propagates; it is not converted to an Error.
// Other OperationCanceledExceptions (e.g. HttpClient timeout) still go through the handler.
```

## Conditional Side Effects

```csharp
// TapIf — predicate-gated tap, only fires when Success AND predicate is true
result.TapIf(v => v > 0, v => Audit(v));

// Async — instance methods (the bool overload exists only on TapIfAsync)
await result.TapIfAsync(v => v > 0, async v => await AuditAsync(v));
await result.TapIfAsync(isEnabled,   async v => await TrackAsync(v));

// Extension variants — Task<Result<T>> and ValueTask<Result<T>>
await GetResultAsync().TapIfAsync(v => v > 0, v => Enqueue(v));
await GetValueTaskResultAsync().TapIfAsync(true, async v => await LogAsync(v));
```

## Async Naming

Every member that returns `Task` or `ValueTask` ends in `Async`: `BindAsync`, `ElseAsync`, `FailIfAsync`, `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `ThenAsync` and `ThenDoAsync`. The old unsuffixed names on `Task`/`ValueTask` sources (`task.Match(...)`, `task.Then(...)`, `result.Bind(async v => ...)`) still compile as `[Obsolete]` forwarders and will be removed in 7.0. An async lambda passed to `BindAsync` binds to the `Task` overload, and the `CancellationToken` is optional on every `Task` source.

## Best Practices

- Never access `.Value` without checking `.IsSuccess` first
- `onError` in `Match` receives `Error[]` (array), not a single `Error`
- `Then()` short-circuits: once a failure occurs, subsequent `Then()` calls are skipped
- Prefer implicit conversions (`return value;`, `return Error.NotFound(...);`); use `Result.Success(value)`/`Result.Failure<T>(error)` only where the target type cannot be inferred. Keep `Result.Success()` for non-generic success
- Use `Ensure()` to add guard conditions without breaking the chain
