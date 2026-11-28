---
name: csharpessentials-results
description: Use when a method can fail in expected ways and should return the failure instead of throwing. Covers Result and Result<T> (implicit returns, Then/Map/Bind/Ensure chains, Match/Switch, Else/Compensate/Recover, Result.Try, Combine/CombineAll/Traverse), the *Async forms on Task and ValueTask sources, and typed Result<T, E1..E3> with Match, BindWiden, JSON and the CSE5001-CSE5006 analyzers.
---

# CSharpEssentials.Results

`Result` (no value) and `Result<T>` hold success or a failure with one or more `Error` values. `Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>` hold a value or exactly one typed error.

## Install

```bash
dotnet add package CSharpEssentials.Results
```

## Namespaces

```csharp
using CSharpEssentials.Errors;         // Error, ErrorType, NotFoundError, ConflictError, ...
using CSharpEssentials.ResultPattern;  // Result, Result<T>, Result<T, E1..E3>, extensions
```

`IResultBase`, `IResult<T>` and `ITypedResult` are in `CSharpEssentials.ResultPattern.Interfaces`.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

var service = new UserService();

Console.WriteLine(service.Rename(1, "Grace").Match(u => $"renamed to {u.Name}", e => e[0].Code)); // renamed to Grace
Console.WriteLine(service.Rename(1, "").Match(u => u.Name, e => e[0].Code));                     // User.NameEmpty
Console.WriteLine(service.Rename(9, "Bob").Match(u => u.Name, e => e[0].Code));                  // User.NotFound

Result<User> loaded = await service.FindAsync(1).EnsureAsync(u => u.Name.Length > 0, Error.Validation("User.NoName", "No name."));
Console.WriteLine(loaded.Value.Name);                                                            // Grace

public sealed record User(int Id, string Name);

public sealed class UserService
{
    private readonly Dictionary<int, User> _users = new() { [1] = new User(1, "Ada") };

    public Result<User> Find(int id)
    {
        if (!_users.TryGetValue(id, out User? user))
            return Error.NotFound("User.NotFound", $"User {id} was not found.");   // Error -> Result<User>
        return user;                                                               // User -> Result<User>
    }

    public Task<Result<User>> FindAsync(int id) => Task.FromResult(Find(id));

    public Result<User> Rename(int id, string name) =>
        Find(id)
            .Ensure(_ => name.Length > 0, Error.Validation("User.NameEmpty", "Name is required."))
            .Map(user => user with { Name = name })
            .Tap(user => _users[id] = user);
}
```

## Creating results

```csharp
Result ok = Result.Success();                                   // non-generic success: always the factory
Result<int> value = 42;                                         // T -> Result<T>
Result fail = Error.Validation("Input.Invalid", "Input was invalid.");
Result<int> many = new[] { Error.Validation("A", "a"), Error.Validation("B", "b") }; // Error[], List<Error>, HashSet<Error>
Result fromBool = true;                                         // false -> Error.False failure

// Only where the target type cannot be inferred (var, generic arguments, Task.FromResult):
var explicitFail = Result.Failure<int>(Error.NotFound("X.NotFound", "Missing."));
Task<Result<int>> task = Task.FromResult(Result.Success(42));
```

## Reading a result

```csharp
Result<int> result = 42;
if (result.IsSuccess) Console.WriteLine(result.Value);   // Value is default on failure
Error first = result.FirstError;                         // also LastError, Errors (Error[]), ErrorsOrEmptyArray
var (isSuccess, value, errors) = result;                 // Result<T>; Result gives (isSuccess, errors)
bool ok = result.TryGet(out int v, out Error[]? errs);
```

## Chaining (runs only on success)

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<string> r = Result.Success(" 42 ")
    .Map(s => s.Trim())                                                        // Func<T, U>
    .Bind(ParseInt)                                                            // Func<T, Result<U>>
    .Ensure(n => n > 0, Error.Validation("Range", "Must be positive."))        // fail when predicate is false
    .FailIf(n => n > 1000, Error.Validation("Range", "Too large."))            // fail when predicate is true
    .ThenEnsure(n => n % 2 == 0 ? Result.Success() : Error.Validation("Odd", "Must be even."))
    .Tap(n => Console.WriteLine($"got {n}"))                                   // side effect, result unchanged
    .Then(n => $"#{n}");                                                       // Then = Map or Bind by lambda shape

Console.WriteLine(r.Value);                                                    // #42

static Result<int> ParseInt(string s)
{
    if (int.TryParse(s, out int n))
        return n;
    return Error.Validation("Nan", "Not a number.");
}
```

Other success-side members: `BindIf(condition, func)`, `TapIf(predicate, action)`, `ThenDo(action)`, `EnsureNotNull(error)`, LINQ `from a in r1 from b in r2 select ...`. On non-generic `Result`, handlers take no argument (`Then(Func<Result>)`, `Map(Func<T>)`, `Bind(Func<Result<T>>)`, `ThenDo(Action)`).

## Consuming

```csharp
string text = result.Match(onSuccess: v => $"ok {v}", onError: errors => errors[0].Description); // Error[]
string first = result.MatchFirst(onSuccess: v => "ok", onFirstError: e => e.Code);              // one Error
result.Switch(onSuccess: v => Log(v), onError: errors => Log(errors));
string plain = Result.Success().Match(onSuccess: () => "ok", onFailure: errors => "failed");     // Result: onFailure
int n = result.GetValueOrDefault(0);           // also GetValueOrDefault(() => 0), UnwrapOrDefault(0)
int m = result.Unwrap();                       // ResultUnwrapException (.Errors) on failure
int k = result.GetValueOrThrow("message");     // InvalidOperationException on failure
string s = result.Finally(r => r.IsSuccess ? "ok" : "failed");
```

## Handling failures

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> failed = Error.NotFound("Count.Missing", "No count.");

Console.WriteLine(failed.Else(0).Value);                                           // fallback value: 0
Console.WriteLine(failed.Compensate(errors => Result.Success(1)).Value);            // fallback Result: 1
Console.WriteLine(failed.Recover(ErrorType.NotFound, e => 2).Value);                // only for that ErrorType: 2
Console.WriteLine(failed.MapError(e => Error.Failure($"Svc.{e.Code}", e.Description)).FirstError.Code); // Svc.Count.Missing
failed.TapError(errors => Console.WriteLine($"{errors.Length} error(s)"));          // side effect: 1 error(s)
```

Also: `Else(Error)`, `Else(Func<Error[], Error>)`, `CompensateFirst`, `RecoverFirst`, `Recover(Func<Error, bool>, ...)`, `TapErrorFirst`, `TapErrorIf(bool, ...)`, `ElseDo`, `ElseDoFirst`, `MapError(Func<Error[], Error[]>)`.

## Exceptions to errors

```csharp
Result<int> parsed = Result.Try(() => int.Parse(input), ex => Error.Exception(ex));
Result<Data> data = await Result.TryAsync(() => LoadAsync(ct), ex => Error.Exception(ex), ct); // caller cancellation still throws
Result<int> next = parsed.TryCatch(n => Result.Success(100 / n));   // func must return a Result; exception -> Error.Exception(ex)
```

## Combining

```csharp
Result first = Result.And(r1, r2, r3);                    // errors of the first failure
Result all = new[] { r1, r2, r3 }.CombineAll();           // every error
Result any = Result.Or(r1, r2);                           // success if one succeeds
Result<(int, string)> pair = Result<int>.Combine(a, b);   // 2..8 results of any types, every error
Result<int[]> values = Result<int>.And(a1, a2);           // same as a1 + a2
Result<Dto[]> mapped = ids.Traverse(id => Get(id));       // every error
var (ok, errors) = results.Partition();
Result<int[]> stop = results.FirstFailureOrSuccesses();   // stops at the first failure
Result guard = Result.SuccessIf(age >= 18, Error.Validation("Age.Minor", "Must be 18."));
Result empty = Result.FailureIf(items.Count == 0, Error.Validation("Items.Empty", "No items."));
```

## Async

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<string> r = await GetAsync(1)                                  // Task<Result<int>>
    .EnsureAsync(n => n > 0, Error.Validation("Range", "Positive."))
    .BindAsync(n => GetAsync(n + 1))                                  // Func<int, Task<Result<int>>>
    .TapAsync(async n => await Task.Delay(1))                         // async lambda: awaited
    .MapAsync(n => $"value {n}");

Console.WriteLine(r.Value);                                           // value 2

Result<string[]> names = await new[] { 1, 2 }.TraverseAsync(
    (id, ct) => Task.FromResult(Result.Success($"u{id}")), CancellationToken.None);   // sequential, every error
Console.WriteLine(string.Join(",", names.Value));                     // u1,u2

string text = await GetAsync(5).MatchAsync(n => $"ok {n}", errors => "failed");
Console.WriteLine(text);                                              // ok 5

static Task<Result<int>> GetAsync(int id) => Task.FromResult<Result<int>>(id);
```

Async names: `MapAsync`, `BindAsync`, `ThenAsync`, `TapAsync`, `TapIfAsync`, `EnsureAsync`, `FailIfAsync`, `ThenEnsureAsync`, `MatchAsync`, `MatchFirstAsync`, `SwitchAsync`, `ElseAsync`, `CompensateAsync`, `MapErrorAsync`, `TapErrorAsync`, `ElseDoAsync`, `FinallyAsync`, `BindIfAsync`, `SelectAsync`, `TryCatchAsync`, `GetValueOrDefaultAsync`, `TraverseAsync`, `SequenceAsync`, `Result.TryAsync`.

Handler flavour must match the source:

| Source | Handlers it takes |
|---|---|
| `Result` / `Result<T>` instance | `Task`; `ValueTask` on .NET 9+ |
| `Task<Result..>` | sync or `Task` |
| `ValueTask<Result..>` | sync or `ValueTask` (some members also `Task`) |

## Typed results

Use `Result<T, E1..E3>` when the caller must handle each failure differently and there are at most three. Error types come from `CSharpEssentials.Errors` (`NotFoundError`, `ConflictError`, `ValidationError`, ... or records derived from them).

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

foreach (int id in new[] { 1, 2, 3 })
{
    int status = Reserve(id).Match(
        onSuccess: _ => 200,
        onError1: _ => 404,                // NotFoundError
        onError2: _ => 409);               // OrderErrors.Locked
    Console.WriteLine($"{id}: {status}");  // 1: 200, 2: 404, 3: 409
}

Result<Order, NotFoundError, OrderErrors.Locked> r = Reserve(3);
Console.WriteLine(r.TryGetValue(out OrderErrors.Locked? locked) ? locked.OrderId : 0);     // 3
Console.WriteLine(r.ToResult().FirstError.Code);                                           // Order.Locked

Result<Order, NotFoundError, ConflictError> chained = Find(1).BindWiden(Lock);            // adds ConflictError slot
Result<Order, ConflictError> recovered = Find(5).BindWiden(Lock).Recover((NotFoundError _) => new Order(0)); // removes a slot
Console.WriteLine($"{chained.IsSuccess} {recovered.GetValueOrThrow().Id}");                // True 0

static Result<Order, NotFoundError> Find(int id) =>
    id is 1 or 3 ? new Order(id) : OrderErrors.NotFound;

static Result<Order, ConflictError> Lock(Order order) => order;

static Result<Order, NotFoundError, OrderErrors.Locked> Reserve(int id)
{
    if (id == 3) return new OrderErrors.Locked(id);   // instance, not the type name
    Result<Order, NotFoundError> found = Find(id);
    return found;                                     // widens implicitly
}

public sealed record Order(int Id);

public static class OrderErrors
{
    public static class Codes { public const string NotFound = "Order.NotFound"; public const string Locked = "Order.Locked"; }
    public static readonly NotFoundError NotFound = new(Codes.NotFound, "Order not found.");
    public sealed record Locked(int OrderId) : ConflictError(Codes.Locked, "Order is locked.");
}
```

Switch expressions over a typed result need C# 15 (`<LangVersion>15</LangVersion>` on net10.0 and lower, default on net11.0). On C# 14 a type pattern fails with CS9327; use `Match`, `Switch` or `TryGetValue`.

```csharp
// C# 15 only
IResult http = Reserve(id) switch
{
    Order o => Results.Ok(o),
    NotFoundError => Results.NotFound(),
    OrderErrors.Locked l => Results.Conflict(l.OrderId),
};
```

Typed members: `Bind<TOut>` (same set), `BindWiden` (adds the inner slots), `Map`, `Ensure`/`FailIf` (same slot, or `Ensure<F>` adds a slot), `MapError((Ei e) => ...)`, `Recover((Ei e) => ...)`, `Compensate((Ei e) => ...)`, `Tap`, `TapIf`, `TapError((Ei e) => ...)`, `TapError((Error e) => ...)`, `TryCatch<TOut, TException>`, `Finally`, `GetValueOrDefault`, `GetValueOr(T)`, `GetValueOr(Func<Error, T>)`, `GetValueOrThrow`, `Select`/`SelectMany`, `Result<T, E..>.Combine`, `Sequence`, `Traverse`, `Partition`, `ToResult`, plus `*Async` forms (`BindAsync`, `BindWidenAsync`, `MapAsync`, `EnsureAsync`, ...). `Combine`, `Sequence` and `Traverse` stop at the first error.

JSON: a typed result serializes as `{"case":"Success","value":...}` or `{"case":"NotFoundError","value":{...}}`. For Native AOT, list the result, the success type and every error type in a `JsonSerializerContext`; the bundled generator registers the converter:

```csharp
[JsonSerializable(typeof(Result<Order, NotFoundError>))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(NotFoundError))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
// A typed result reachable only through a property: TypedResultJsonConverterFactory.Register<Order, NotFoundError>();
```

## Testing results

`dotnet add package CSharpEssentials.Results.Testing`, namespace `CSharpEssentials.ResultPattern.Testing`. Each helper throws `ResultAssertionException` on a mismatch and returns what it checked; no test framework is referenced.

```csharp
int value = Divide(10, 2).ShouldBeSuccess();                    // Result<T> and typed results return the value
Error[] errors = Divide(1, 0).ShouldBeFailure();
Error byCode = Divide(1, 0).ShouldHaveError("Math.DivideByZero"); // also (ErrorType) and (ErrorType, code)
ConflictError c = reserve.ShouldBeFailureOf<ConflictError>();      // typed results only
JsonElement body = await response.ShouldBeProblemAsync(404, "Order.NotFound");
await response.ShouldBeValidationProblemAsync("Name.NotEmpty");
```

`Maybe<T>` has `ShouldHaveValue()` and `ShouldBeNone()`. See the [Results.Testing Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results.Testing/Readme.MD).

## Rules

- Return values and errors directly (`return user;`, `return Error.NotFound(...);`). Use `Result.Success()` for non-generic success.
- Never `return default;` for a result. `default(Result)`/`default(Result<T>)` is a failure coded `Result.Default`, and `Match`, `Map`, `Bind`, `MapError`, `Else` and similar throw `InvalidOperationException` on it. `default` typed results hold no case (CSE5001).
- Check `IsSuccess` before `Value`. `Result<T>` cannot hold `null`: `Result.Success<string?>(null)` throws `ArgumentNullException`.
- `Match` `onError` receives `Error[]`; use `MatchFirst` for one `Error`. Non-generic `Result.Match` names it `onFailure`.
- Every Task/ValueTask-returning member ends in `Async`. Write `BindAsync`, never `Bind(async v => ...)` (CS0121). Unsuffixed names on `Task` sources are `[Obsolete]`.
- Never pass an `async` lambda where an `Action` is taken (`Tap(condition, async () => ...)`): it becomes `async void`. On a `ValueTask` source, `TapAsync(v => SaveAsync(v))` (non-async lambda returning `Task`) drops the task; write `async v => await SaveAsync(v)`.
- `TryCatch` on `Result<T>` takes a function that returns a `Result`, not a plain value.
- `Result.And` stops at the first failure; `CombineAll`, `Sequence`, `Traverse` and `Result<T>.Combine` collect every error.
- `TraverseAsync` is sequential and safe with one `DbContext`; cancellation throws, it is not an error.
- Typed results: `return OrderErrors.Locked;` is a type (CS0119), create an instance. An `Error` does not convert to a typed slot (CS0029); use typed kinds or `ToError()` the other way. Type the lambda parameter for per-slot members (`Recover((NotFoundError e) => ...)`), or CS0121. Use `Bind<TOut>` when the inner result has fewer error types; `BindWiden` would repeat a slot.
- A typed result returned raw from an ASP.NET Core handler is not an HTTP error; call `ToHttpResult()` from `CSharpEssentials.AspNetCore`.

## Diagnostics

| ID | Meaning | Fix |
|---|---|---|
| CSE5001 | Typed result created as `default` (`default`, `new R()`, `new R[n]`, unassigned private field) | Assign a value or an error |
| CSE5002 | Same error type twice in one result | Code fix removes the repeat |
| CSE5003 | One error slot converts to another (`TypedError` and `NotFoundError`) | Pick unrelated error types |
| CSE5004 | Success type converts to or from an error slot (`object`) | Use a specific success type |
| CSE5005 | `[ErrorKind]` disagrees with the base kind or the `Type` property | Code fix sets the right kind |
| CSE5006 | `Ensure<F>`, `FailIf<F>`, `BindWiden`, `TryCatch` or `SelectMany` adds a slot the result already covers | Code fix casts the error or uses `Bind` |

All are warnings (errors under `TreatWarningsAsErrors`). Full reference: [Results Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD).
