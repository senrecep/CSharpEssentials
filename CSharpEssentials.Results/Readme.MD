# CSharpEssentials.Results

`Result` and `Result<T>` return success or failure as a value, so expected failures do not need exceptions. Use them for code that can fail in known ways (validation, not found, conflict) and chain the steps with `Then`, `Map`, `Bind` and `Ensure`. `Result<T, E1..E3>` puts the error types in the signature when the caller must handle each failure on its own.

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. Depends on `CSharpEssentials.Errors` and `CSharpEssentials.Json`.

## Installation

```bash
dotnet add package CSharpEssentials.Results
```

```csharp
using CSharpEssentials.Errors;         // Error, ErrorType, typed error kinds
using CSharpEssentials.ResultPattern;  // Result, Result<T>, Result<T, E1..E3>
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

foreach (string input in new[] { "21", "abc", "500" })
{
    string message = Parse(input)
        .Ensure(n => n <= 100, Error.Validation("Input.TooLarge", "The number must be 100 or less."))
        .Map(n => n * 2)
        .Match(
            onSuccess: n => $"{input} -> {n}",
            onError: errors => $"{input} -> {errors[0].Code}: {errors[0].Description}");

    Console.WriteLine(message);
}

static Result<int> Parse(string input)
{
    if (int.TryParse(input, out int number))
        return number;                                                    // a value converts to Result<int>
    return Error.Validation("Input.NotANumber", $"'{input}' is not a number."); // so does an Error
}
```

Output:

```text
21 -> 42
abc -> Input.NotANumber: 'abc' is not a number.
500 -> Input.TooLarge: The number must be 100 or less.
```

## Usage

### Creating results

`Result` has no value. `Result<T>` carries a value on success. Both are `readonly record struct` types.

```csharp
Result ok = Result.Success();
Result<int> answer = 42;                                          // implicit from T
Result<int> same = Result.Success(42);                            // explicit factory
Result failed = Error.NotFound("User.NotFound", "User not found.");
```

### Returning failures

Return the error itself. `Error`, `Error[]`, `List<Error>` and `HashSet<Error>` convert implicitly to `Result` and `Result<T>`. A failure can hold several errors.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<User> found = Find(1);
Result<User> missing = Find(2);
Result invalid = Validate("", "not-an-email");

Console.WriteLine(found);                       // Success: User { Id = 1, Name = Ada }
Console.WriteLine(missing.FirstError.Code);     // User.NotFound
Console.WriteLine(invalid.Errors.Length);       // 2

// var, inferred lambdas and generic arguments need the explicit factories
var conflict = Result.Failure<string>(Error.Conflict("User.Duplicate", "User already exists."));
Task<Result<int>> pending = Task.FromResult(Result.Success(42));
Console.WriteLine($"{conflict.IsFailure} {(await pending).Value}"); // True 42

static Result<User> Find(int id)
{
    if (id != 1)
        return Error.NotFound("User.NotFound", $"User {id} was not found.");
    return new User(1, "Ada");
}

static Result Validate(string name, string email)
{
    var errors = new List<Error>();
    if (name.Length == 0)
        errors.Add(Error.Validation("Name.Empty", "Name is required."));
    if (!email.Contains('@'))
        errors.Add(Error.Validation("Email.Invalid", "Email is invalid."));
    return errors.Count == 0 ? Result.Success() : errors;
}

public sealed record User(int Id, string Name);
```

Rules for building results:

- `Result<T>` rejects `null`. `Result<string> r = (string?)null;` throws `ArgumentNullException`.
- A `bool` converts to `Result`: `true` is a success, `false` is the `Error.False` failure.
- A failure needs at least one error. `Result.From` and `Result.Failure` throw `ArgumentException` for an empty collection.
- Never use `default(Result)` or `default(Result<T>)` as a success. See [`default` is a failure](#default-is-a-failure).

### Inspecting a result

```csharp
Result<int> result = 42;

if (result.IsSuccess)
    Console.WriteLine(result.Value);
else
    Console.WriteLine($"{result.FirstError.Code}, {result.LastError.Code}, {result.Errors.Length}");

var (isSuccess, value, errors) = result;          // Result deconstructs to (isSuccess, errors)
bool ok = result.TryGet(out int v, out Error[]? e);
```

- `Value` is `default` on a failure. Check `IsSuccess` first.
- On a success, `Errors`, `FirstError` and `LastError` return the sentinels `Error.NoErrors`, `Error.NoFirstError` and `Error.NoLastError`. `ErrorsOrEmptyArray` returns an empty array.
- `Result<T>.ToResult()` drops the value.
- `ToString()` is for logs: `Success: ` plus the value's `ToString()` (invariant culture for `IFormattable` values), or the errors. For JSON, use `JsonSerializer` or `ConvertToJson()` from `CSharpEssentials.Json`.

### Chaining

Each step runs only on success. A failure skips the rest of the chain and keeps its errors.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<string> receipt = GetPrice("book")
    .Then(price => price * 2)                  // Then with a plain function works like Map
    .Bind(total => Charge(total))              // Bind takes a function that returns a Result
    .Tap(id => Console.WriteLine($"Charged, payment {id}"))
    .Map(id => $"Receipt for payment {id}");

Console.WriteLine(receipt.Value);              // Receipt for payment PAY-40

Result<string> failed = GetPrice("pen").Then(price => price * 2).Bind(Charge);
Console.WriteLine(failed.FirstError.Code);     // Product.NotFound

Result<int> count = Result.Success()
    .ThenDo(() => Console.WriteLine("side effect"))     // Result has no value, so handlers take no argument
    .Then(() => Result.Success())
    .Map(() => 3);
Console.WriteLine(count.Value);                // 3

static Result<decimal> GetPrice(string sku) =>
    sku == "book" ? 20m : Error.NotFound("Product.NotFound", $"No product {sku}.");

static Result<string> Charge(decimal amount) =>
    amount > 0 ? $"PAY-{amount}" : Error.Validation("Amount.Invalid", "Amount must be positive.");
```

| Member | Use it for |
|---|---|
| `Then` | A plain function (`Func<T, U>`) or one that returns a `Result<U>`. On `Result`, a `Func<Result>`. |
| `Map` | A plain function. A `Result`-returning overload exists too. |
| `Bind` | A function that returns a `Result` or `Result<U>`. |
| `BindIf(condition, func)` | `Bind` only when a `bool`, `Func<bool>` or `Func<T, bool>` holds. |
| `ThenDo`, `Tap` | A side effect. The result comes back unchanged. `Tap(condition, action)` takes a `bool` or `Func<bool>`. |
| `TapIf(predicate, action)` | A side effect when a `Func<T, bool>` holds (`Result<T>` only). |
| `Select`, `SelectMany` | LINQ query syntax on `Result<T>`. |

### Ensuring

`Ensure` turns a success into a failure when a check fails. These members exist on `Result<T>`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> age = Result.Success(15)
    .Ensure(a => a >= 0, Error.Validation("Age.Negative", "Age cannot be negative."))
    .Ensure(a => a >= 18, a => Error.Validation("Age.Minor", $"{a} is under 18."));
Console.WriteLine(age.FirstError.Description);     // 15 is under 18.

Result<int> quantity = Result.Success(500)
    .FailIf(q => q > 100, Error.Validation("Quantity.TooHigh", "At most 100."));
Console.WriteLine(quantity.FirstError.Code);        // Quantity.TooHigh

Result<string> email = Result.Success("ada@example.com")
    .ThenEnsure(e => e.Contains('@') ? Result.Success() : Error.Validation("Email.Invalid", "No @."));
Console.WriteLine(email.Value);                     // ada@example.com
```

- `Ensure(predicate, error)` or `Ensure(predicate, errorFactory)` fails when the predicate is `false`.
- `FailIf` is the opposite: it fails when the predicate is `true`.
- `EnsureNotNull(error)` fails on a `null` value. A `Result<T>` built in code never holds `null`, but one read from JSON (`{"IsSuccess":true,"Value":null}`) can.
- `ThenEnsure(validator)` runs a validator that returns a `Result` or a `Result<T>`. Its failure becomes the result.

### Matching

`Match` returns a value from one of two handlers. `Switch` runs one of two actions.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> ok = 7;
Result<int> failed = new[]
{
    Error.Validation("A", "First problem."),
    Error.Validation("B", "Second problem."),
};

Console.WriteLine(ok.Match(onSuccess: v => $"value {v}", onError: errors => $"{errors.Length} errors"));
Console.WriteLine(failed.MatchFirst(onSuccess: v => "ok", onFirstError: e => e.Code)); // A
Console.WriteLine(failed.MatchLast(v => "ok", e => e.Code));                           // B

failed.Switch(
    onSuccess: v => Console.WriteLine(v),
    onError: errors => Console.WriteLine($"Failed: {errors[0].Description}"));

Result plain = Result.Success();
Console.WriteLine(plain.Match(onSuccess: () => "done", onFailure: errors => "failed")); // done
```

- `Match` and `Switch` give the failure handler the `Error[]`.
- `MatchFirst`, `MatchLast`, `SwitchFirst` and `SwitchLast` give it one `Error`.
- On a non-generic `Result` the success handler takes no argument and the failure parameter is named `onFailure`.

### Handling failures

These members run only on a failure. A success passes through unchanged.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> missing = Error.NotFound("Count.Missing", "No count stored.");

Console.WriteLine(missing.Else(0).Value);                                        // 0
Console.WriteLine(missing.Else(errors => errors.Length).Value);                   // 1
Console.WriteLine(missing.Else(Error.Failure("Count.Failed", "Lookup failed.")).FirstError.Code); // Count.Failed

Result<int> healed = missing.Compensate(errors => Result.Success(10));
Console.WriteLine(healed.Value);                                                 // 10

Result<int> recovered = missing.Recover(ErrorType.NotFound, error => -1);
Console.WriteLine(recovered.Value);                                              // -1

Result<int> stillFailed = missing.Recover(ErrorType.Conflict, error => -1);
Console.WriteLine(stillFailed.IsFailure);                                        // True

missing
    .TapError(errors => Console.WriteLine($"logged {errors.Length} error(s)"))
    .TapErrorFirst(error => Console.WriteLine($"first: {error.Code}"))
    .ElseDo(errors => Console.WriteLine("ElseDo is the same as TapError"));
```

| Member | What it does on a failure |
|---|---|
| `Else(...)` | Replaces the failure with an `Error`, or with what a function of the `Error[]` returns: an `Error`, an `Error[]` (`IEnumerable<Error>` on `Result`) or, on `Result<T>`, a `T`. `Result<T>` also takes a fallback `T` directly. |
| `Compensate(func)`, `CompensateFirst(func)` | Replaces the failure with the `Result` the function returns, so the fallback can fail too. |
| `Recover(errorType, recovery)`, `Recover(predicate, recovery)`, `RecoverFirst(errorType, recovery)` | `Result<T>` only. Turns the failure into a value or a `Result<T>` when an error has that `ErrorType` or matches the predicate. `RecoverFirst` looks at the first error only. |
| `TapError`, `TapErrorFirst`, `TapErrorIf(condition, action)`, `ElseDo`, `ElseDoFirst` | Runs a side effect with the `Error[]` or the first `Error`. Returns the failure unchanged. |

### Combining results

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result ok = Result.Success();
Result notFound = Error.NotFound("A.Missing", "A is missing.");
Result conflict = Error.Conflict("B.Taken", "B is taken.");

Console.WriteLine(Result.And(ok, notFound, conflict).Errors.Length);            // 1: stops at the first failure
Console.WriteLine(new[] { ok, notFound, conflict }.CombineAll().Errors.Length);  // 2: collects every error
Console.WriteLine(Result.Or(notFound, ok).IsSuccess);                            // True: one success is enough

Result<int> one = 1;
Result<string> name = "Ada";
Result<(int, string)> pair = Result<int>.Combine(one, name);                     // 2 to 8 results, collects every error
Console.WriteLine(pair.Value);                                                   // (1, Ada)

Result<int[]> both = Result<int>.And(1, 2);
Result<int[]> plus = one + Result.Success(2);                                    // same as And
Console.WriteLine($"{both.Value.Length} {plus.Value.Length}");                   // 2 2

Result<int> sum = from a in one
                  from b in Result.Success(41)
                  select a + b;
Console.WriteLine(sum.Value);                                                    // 42

Result<string> version = "v1.2".TrimStart("v");                                  // string extension from this package
Console.WriteLine(version.Value);                                                // 1.2

Result guard = Result.SuccessIf(DateTime.Now.Year > 2000, Error.Validation("Clock.Wrong", "Clock is wrong."));
Result<int> picked = Result.SuccessIf(true, 5, Error.Validation("X", "Unused."));
Result empty = Result.FailureIf(() => Array.Empty<int>().Length == 0, Error.Validation("Items.Empty", "No items."));
Console.WriteLine($"{guard.IsSuccess} {picked.Value} {empty.FirstError.Code}");  // True 5 Items.Empty
```

- `Result.And` returns the errors of the first failure. `CombineAll` collects all of them.
- `Result<T>.And` and `r1 + r2` give a `Result<T[]>` with every value or every error.
- `Result.Or` succeeds as soon as one result succeeds, otherwise it returns every error.
- `Result<T>.Combine` builds a tuple from 2 to 8 results of different types and collects every error.
- `SuccessIf` and `FailureIf` build a result from a `bool` or a `Func<bool>`.

### Safe execution

`Result.Try` runs code that can throw and turns the exception into an error. `TryCatch` does the same for the next step of a chain.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> parsed = Result.Try(() => int.Parse("not a number"), ex => Error.Exception(ex));
Console.WriteLine(parsed.FirstError.Code);   // FormatException
Console.WriteLine(parsed.FirstError.Type);   // Unexpected

Result<string> loaded = await Result.TryAsync(
    async () => { await Task.Delay(1); return "data"; },
    ex => Error.Exception(ex),
    CancellationToken.None);
Console.WriteLine(loaded.Value);             // data

Result<int> divided = Result.Success(0)
    .TryCatch(n => Result.Success(10 / n));  // the exception becomes Error.Exception(ex)
Console.WriteLine(divided.FirstError.Code);  // DivideByZeroException

Result<int> custom = Result.Success(0)
    .TryCatch(n => Result.Success(10 / n), Error.Validation("Math.DivByZero", "Cannot divide by zero."));
Console.WriteLine(custom.FirstError.Code);   // Math.DivByZero
```

- `Result.TryAsync` lets an `OperationCanceledException` through when the token you passed is cancelled. Any other `OperationCanceledException`, such as an `HttpClient` timeout, goes to the error handler.
- `TryCatch(func, error)` uses `error` for any exception, or `Error.Exception(ex)` when no error is given.

### Mapping errors

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> failed = new[] { Error.Validation("Name", "Bad name."), Error.Validation("Age", "Bad age.") };

// Per-error mapper: runs once for every error, in order
Result<int> prefixed = failed.MapError(e => Error.Validation($"User.{e.Code}", e.Description));
Console.WriteLine(string.Join(", ", prefixed.Errors.Select(e => e.Code)));   // User.Name, User.Age

// Array mapper: replaces the whole error array
Result<int> collapsed = failed.MapError(errors => new[] { Error.Failure("User.Invalid", $"{errors.Length} errors") });
Console.WriteLine(collapsed.FirstError.Description);                         // 2 errors

// Async mapper
Result<int> localized = await failed.MapErrorAsync(
    async e => { await Task.Yield(); return Error.Validation(e.Code, e.Description.ToUpperInvariant()); },
    CancellationToken.None);
Console.WriteLine(localized.FirstError.Description);                         // BAD NAME.
```

`MapError` never runs on a success. Async per-error mappers run one at a time. The token is checked before each call, but a mapper that is already running is not stopped, so pass the token into the mapper if it must stop early.

### Getting the value out

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> failed = Error.NotFound("Count.Missing", "No count.");

Console.WriteLine(failed.GetValueOrDefault());          // 0
Console.WriteLine(failed.GetValueOrDefault(5));         // 5
Console.WriteLine(failed.GetValueOrDefault(() => 6));   // 6
Console.WriteLine(failed.UnwrapOrDefault(7));           // 7
Console.WriteLine(failed.Finally(r => r.IsSuccess ? "ok" : "failed")); // failed

try { failed.Unwrap(); }
catch (ResultUnwrapException ex) { Console.WriteLine(ex.Errors[0].Code); }      // Count.Missing

try { failed.GetValueOrThrow("no count"); }
catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }         // no count
```

- `GetValueOrThrow(message)` throws `InvalidOperationException`. `GetValueOrThrow(exception)` throws your exception.
- `Unwrap()` throws `ResultUnwrapException` (an `InvalidOperationException`) whose `Errors` holds the errors.
- `Finally(func)` hands the whole result to the function and returns what it returns. `Finally(action)` runs the action and returns the result.
- On .NET 10 and later, `result.ValueOrDefault` and `left | right` (the left result when it succeeded, otherwise the right) are extension members.

### Other helpers

- `ToResult()` extensions turn an `Error`, an `Error[]`, an `IEnumerable<Error>` or a value into a `Result` or `Result<T>`.
- `TrimStart(prefix)` and `TrimEnd(suffix)` on `string` return a `Result<string>` (see [Combining results](#combining-results)). They fail with `InputIsEmpty`, `PrefixIsEmpty` or `SuffixIsEmpty` when a string is null or empty, and take an optional `StringComparison` (default `CurrentCulture`).
- `ResultComparer`, `ResultComparer<T>` and `ResultBaseComparer` in `CSharpEssentials.ResultPattern.Comparers` are `IEqualityComparer` implementations.
- `IResultBase`, `IResult` and `IResult<T>` in `CSharpEssentials.ResultPattern.Interfaces` let code take any result.
- Results serialize with `System.Text.Json` as an `errors` array plus, for `Result<T>`, a `Value`, and read back to the same state. A failure in JSON must have at least one error, and a `Result<T>` success must include `"IsSuccess": true`.

### `default` is a failure

`default(Result)` and `default(Result<T>)` (an unassigned field, an array slot) are failures. `IsFailure` is `true`, and `Errors` holds one `Error.Unexpected` with the code `Result.Default` (HTTP 500).

- Members that hand the errors to your delegate or build a new failure from them throw `InvalidOperationException`: `Match`, `MatchFirst`, `Switch`, `Map`, `Bind`, generic `Then`, `MapError`, `Else`, `TapError`, `ElseDo`, `Compensate`, `Recover`, sync and async.
- `Result.Then`, `ThenDo`, `Tap` and `Ensure` pass the `default` on unchanged.
- `TryGet`, `ToString`, equality, JSON, `Result.And`, `Partition` and `Combine` report the `Result.Default` error.

Build results with `Result.Success()`, a value or an error.

### Operations reference

| Operation | Runs on | Notes |
|---|---|---|
| `Then`, `Bind`, `Map`, `BindIf`, `Select`, `SelectMany` | success | Transform or chain the value. |
| `ThenDo`, `Tap`, `TapIf` | success | Side effect, result unchanged. |
| `Ensure`, `EnsureNotNull`, `FailIf`, `ThenEnsure` | success | `Result<T>` only. Turn a success into a failure. |
| `TryCatch` | success | Exceptions become errors. |
| `MapError` | failure | Rewrite the errors. |
| `Else`, `Compensate`, `CompensateFirst`, `Recover`, `RecoverFirst` | failure | Replace the failure. `Recover` is `Result<T>` only. |
| `TapError`, `TapErrorFirst`, `TapErrorIf`, `ElseDo`, `ElseDoFirst` | failure | Side effect, failure unchanged. |
| `Match`, `MatchFirst`, `MatchLast`, `Switch`, `SwitchFirst`, `SwitchLast`, `Finally` | both | Consume the result. |
| `GetValueOrDefault`, `UnwrapOrDefault`, `GetValueOrThrow`, `Unwrap` | both | `Result<T>` only. Read the value. |

### Async

Every operation has an async form whose name ends in `Async`. It works on an instance (with an async handler) and on a `Task<Result..>` or `ValueTask<Result..>` source, so a chain of async calls reads like a sync one.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

var audit = new List<string>();

Result<string> order = await LoadOrderAsync(1)
    .EnsureAsync(o => o.Lines > 0, Error.Validation("Order.Empty", "Order has no lines."))
    .TapAsync(async o => { await Task.Yield(); audit.Add($"loaded {o.Id}"); })   // async lambda: awaited
    .MapAsync(o => $"Order {o.Id} with {o.Lines} lines")
    .TapErrorAsync(async errors => { await Task.Yield(); audit.Add(errors[0].Code); });

Console.WriteLine(order.Value);                // Order 1 with 3 lines

Result<string> missing = await LoadOrderAsync(2)
    .BindAsync(o => Task.FromResult(Result.Success($"{o.Id}")))
    .ElseAsync(errors => "fallback");
Console.WriteLine(missing.Value);              // fallback

string text = await LoadOrderAsync(1).MatchAsync(o => $"ok {o.Id}", errors => "failed");
Console.WriteLine(text);                       // ok 1
Console.WriteLine(string.Join(", ", audit));   // loaded 1

static async Task<Result<Order>> LoadOrderAsync(int id)
{
    await Task.Yield();
    if (id != 1)
        return Error.NotFound("Order.NotFound", $"Order {id} not found.");
    return new Order(id, 3);
}

public sealed record Order(int Id, int Lines);
```

Rules:

- Call the `*Async` name whenever the handler or the source is async. The unsuffixed names on `Task`/`ValueTask` sources (`task.Match(...)`, `task.Then(...)`) and the `Bind` overloads that take a `Task` function are `[Obsolete]` forwarders. See [the async naming migration](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v6-async-naming.md).
- `result.Bind(async v => ...)` is ambiguous (CS0121). Use `BindAsync`.
- `ThenAsync` takes the same handlers as `BindAsync` and, like `Then`, also a function that returns a plain value.
- The `CancellationToken` is optional on every `Task` source.
- When the token is cancelled while an operation waits on a source, it throws `TaskCanceledException` (a subclass of `OperationCanceledException`) on the .NET 9+ targets.

#### Async overload matrix

The source decides the handler flavour. A `Task` source takes sync and `Task` handlers. A `ValueTask` source takes sync and `ValueTask` handlers. An instance takes `Task` handlers and, on .NET 9+, `ValueTask` handlers.

| Operation | Instance | `Task<Result..>` source | `ValueTask<Result..>` source |
|---|---|---|---|
| `MapAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `ValueTask` |
| `BindAsync` | `Task`, `ValueTask` | sync, `Task` | sync, `ValueTask` |
| `TapAsync` | `Task`, `ValueTask`¹ | `Action`, `Task` | `Action`, `ValueTask` |
| `MapErrorAsync` | `Task`, `ValueTask` | sync, `Task` | sync, `ValueTask` |
| `ThenAsync`, `ThenDoAsync`, `EnsureAsync`, `FailIfAsync`, `ElseAsync`, `TapIfAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `MatchAsync`, `MatchFirstAsync`, `MatchLastAsync`, `SwitchAsync`, `SwitchFirstAsync`, `SwitchLastAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `TapErrorAsync`, `TapErrorFirstAsync`, `ElseDoAsync`, `ElseDoFirstAsync`, `CompensateAsync`, `CompensateFirstAsync`, `ThenEnsureAsync` | `Task`, `ValueTask`¹ | `Task` | `Task`, `ValueTask`¹ |
| `FinallyAsync` | none | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `Result.TryAsync` (static) | `Task`, `ValueTask`¹ | none | none |
| `TraverseAsync` | `Task` or `ValueTask`¹ selector | none | none |

¹ .NET 9+ only. The `netstandard2.1` asset (used by .NET 8) does not have these overloads.

`Task` and `ValueTask` sources also have `BindIfAsync`, `SelectAsync`, `TryCatchAsync` and `GetValueOrDefaultAsync`.

#### Lambda binding and C# 12

- On .NET 9+, an untyped `async` lambda picks the `Task` handler through `OverloadResolutionPriority`, which needs C# 13. With `LangVersion` 12 it is ambiguous (CS0121) on every operation marked ¹. Move to C# 13, or type the handler: `(Func<int, Task<string>>)(async v => ...)`.
- On C# 12, on every target, `MapErrorAsync(async e => ...)` and `Result<T>.BindAsync(async v => ...)` are ambiguous too. Cast the lambda to its `Func` type.
- On C# 13+, `MapErrorAsync` picks by what the lambda returns: an `Error` (including `return e;`) binds the per-error mapper, a new `Error[]` binds the array mapper. Cast to `Func<Error[], Task<Error[]>>` to be explicit.
- A handler of the wrong flavour gives a nested type, not an error: `vt.MapAsync(x => Task.FromResult(x))` returns `ValueTask<Result<Task<int>>>`.

#### Handlers that are not awaited

Never pass an `async` lambda to an `Action` overload. It runs as `async void`: nothing awaits it and its exceptions are lost.

- `TapAsync(async v => ...)` on a `Task` or `ValueTask` source binds the awaited `Func` overload. That is safe.
- A non-`async` lambda that returns a `Task` on a `ValueTask` source, or a `ValueTask` on a `Task` source, binds `Action<T>` and the task is dropped: `vt.TapAsync(v => SaveAsync(v))`. The same happens for a `ValueTask<T>` on a `ValueTask` source. Write `async v => await SaveAsync(v)`.
- The sync `Tap(condition, action)` takes only `Action`, so `result.Tap(isNew, async () => await SaveAsync())` is `async void`. Use `result.TapAsync(() => isNew ? SaveAsync() : Task.CompletedTask)`.

### Collection extensions

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int>[] results = [1, Error.Validation("Bad.2", "Two is bad."), 3, Error.Validation("Bad.4", "Four is bad.")];

Console.WriteLine(results.Sequence().Errors.Length);                       // 2: collects every error
Console.WriteLine(results.FirstFailureOrSuccesses().FirstError.Code);      // Bad.2: stops at the first failure

var (successes, errors) = results.Partition();
Console.WriteLine($"{successes.Length} ok, {errors.Length} failed");       // 2 ok, 2 failed

Result<int[]> squares = new[] { 1, 2, 3 }.Traverse(n => Result.Success(n * n));
Console.WriteLine(string.Join(",", squares.Value));                        // 1,4,9

Result<string[]> names = await new[] { 1, 2 }.TraverseAsync(
    async (id, ct) => { await Task.Delay(1, ct); return Result.Success($"user-{id}"); },
    CancellationToken.None);
Console.WriteLine(string.Join(",", names.Value));                          // user-1,user-2

Task<Result<int>>[] running = [Task.FromResult(Result.Success(1)), Task.FromResult(Result.Success(2))];
Result<int[]> all = await running.SequenceAsync();
Console.WriteLine(all.Value.Sum());                                        // 3
```

| Method | Strategy | Returns |
|---|---|---|
| `CombineAll()` | collect every error | `Result` or `Result<T[]>` |
| `Sequence()` | collect every error | `Result<T[]>` |
| `Traverse(selector)` | map, then `Sequence` | `Result<TOut[]>` |
| `Partition()` | split | `(T[] Successes, Error[] Errors)` |
| `FirstFailureOrSuccesses()` | stop at the first failure | `Result` or `Result<T[]>` |
| `TraverseAsync(selector, ct)` | async map, one item at a time, collect every error | `Task<Result<TOut[]>>` |
| `SequenceAsync(ct)` | await tasks that are already running, collect every error | `Task` or `ValueTask` of `Result<T[]>` |

- `TraverseAsync` awaits each item before it starts the next, so a `DbContext` is safe to use in the selector. Selectors take `(item)` or `(item, ct)` and return a `Task` (or a `ValueTask` on .NET 9+).
- Selector exceptions propagate. Cancellation throws `OperationCanceledException`, never an error.
- `SequenceAsync` awaits every task, even after a failure, and rethrows only the first exception, like `Task.WhenAll`. On cancellation it stops at once, observes the rest of a materialized collection and does not enumerate a lazy source further.

## Typed errors: `Result<T, E1>`, `Result<T, E1, E2>`, `Result<T, E1, E2, E3>`

A typed result holds the success value or exactly one error, and the error types are part of the signature. It is a `readonly struct` with one value field, one error field and a tag: `Result<int, NotFoundError, ConflictError>` is 16 bytes and the success path allocates nothing. Up to three error types are supported. The error kinds (`NotFoundError`, `ConflictError` and the rest) come from [CSharpEssentials.Errors](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#typed-error-kinds).

Matching with a switch expression needs C# 15. A net11.0 project has it by default with the .NET 11 SDK. On net10.0, net9.0, net8.0 or netstandard2.1, build with the .NET 11 SDK and set `<LangVersion>15</LangVersion>`; without it a type pattern over the result fails with CS9327 (C# 14), CS9260 (C# 13) or CS8400 (C# 8). Implicit returns, `Match`, `Switch`, `TryGetValue` and `ToResult` work at any language version, so the program below runs on C# 14.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

foreach (int id in new[] { 1, 2, 3, 4 })
{
    string response = Get(id).Match(
        onSuccess: user => $"200 {user.Name}",
        onError1: notFound => notFound.Code == UserErrors.Codes.Deleted ? "410 deleted" : "404 not found",
        onError2: locked => $"409 user {locked.Id} is locked");
    Console.WriteLine(response);
}

Result<UserDto, NotFoundError, UserErrors.Locked> result = Get(4);
if (result.TryGetValue(out UserErrors.Locked? locked))
    Console.WriteLine($"Locked id: {locked.Id}");           // Locked id: 4

Result<UserDto> classic = result.ToResult();                // back to Result<T>
Console.WriteLine(classic.FirstError.Type);                 // Conflict

static Result<UserDto, NotFoundError, UserErrors.Locked> Get(int id) => id switch
{
    1 => new UserDto(1, "Ada"),            // value
    2 => UserErrors.Deleted,               // predefined NotFoundError
    3 => UserErrors.NotFound,
    _ => new UserErrors.Locked(id),        // derived ConflictError in its own slot
};

public sealed record UserDto(int Id, string Name);

public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    public sealed record Locked(int Id) : ConflictError(Codes.Locked, "User is locked.");
}
```

Conversion rules:

- Values and errors convert implicitly. A predefined `static readonly` error keeps its instance.
- A derived error goes to the slot of its base kind when it has no slot of its own.
- An error type that is not in the set does not compile (CS0029). Two slots of the same type make every conversion ambiguous (CS0457).
- A nested error type is a type, not a value: `return UserErrors.Locked;` fails with CS0119. Create an instance.
- An `Error` value does not convert into a typed slot (CS0029). [Moving a catalog to typed kinds](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#moving-a-catalog-to-typed-kinds) shows the rewrite.

### Choosing between `Result<T>` and `Result<T, E..>`

Use a typed result when the caller acts differently on each failure and the failures form a short, closed set, such as a lookup that can be not found or locked. The compiler then checks that every caller handles every case.

Keep `Result<T>` when:

- a failure carries several errors at once, as in validation;
- a handler can fail in more than three ways, or the set is open;
- the caller only forwards the errors, for example to a problem details response;
- the consuming projects cannot use C# 15.

`ToResult()` turns a typed result into a `Result<T>`, and `ToError()` turns a typed error into an `Error`, so both models work together.

### Matching

With C# 15, use a switch expression. Once every case has an arm, no discard or `null` arm is needed. Drop a case and the compiler reports CS8509 naming it.

```csharp
// Needs <LangVersion>15</LangVersion> (default on net11.0). Uses Get and UserErrors from the program above.
string response = Get(id) switch
{
    UserDto user => $"200 {user.Name}",
    NotFoundError { Code: UserErrors.Codes.Deleted } => "410 deleted",
    NotFoundError => "404 not found",
    UserErrors.Locked locked => $"409 user {locked.Id} is locked",
};
```

Rules for the arms:

- A `static readonly` field is not a pattern (CS0150). Match predefined errors on a constant `Code`, which is why the catalog has a `Codes` class.
- Property arms alone do not cover a slot. Keep a bare kind arm after them (CS8509 otherwise).
- A `when` clause never counts toward exhaustiveness (CS8846 when it is the only arm of a slot).
- Put derived arms before the base kind. A base arm first makes the derived arm unreachable (CS8510).
- The success type and every slot type must be non-nullable. `Result<UserDto?, NotFoundError>` reports CS8714, and a switch over it or over `Result<UserDto, NotFoundError>?` needs a `null` arm (CS8655).
- An `Error` slot cannot be matched exhaustively by `Type`, because an enum can hold any number (CS8524). Use a bare `Error` arm or the typed kinds.
- Only switch expressions are checked. A switch statement, a `void` switch and an `if (r is A)` chain skip a missing case without a warning.

In statement code, or below C# 15, use `Match` and `Switch`. They take one delegate per case, in slot order (`onSuccess`, `onError1`, `onError2`, `onError3`). `TryGetValue(out T)` and `TryGetValue(out Ei)` read a case without throwing.

### Widening and reordering

A result converts implicitly into a result whose error set contains its own, in any order:

```csharp
Result<UserDto, NotFoundError> Find(int id) => new NotFoundError("User.NotFound", "Not found.");

Result<UserDto, NotFoundError, UserErrors.Locked> wider = Find(1);
Result<UserDto, UserErrors.Locked, NotFoundError> reordered = wider;
```

### Combinators

Each combinator passes an error through in its slot without calling the delegate.

| Member | Error set of the returned result |
|---|---|
| `Bind<TOut>(Func<T, Result<TOut, E..>>)` | the same |
| `BindWiden<TOut, F..>(Func<T, Result<TOut, F..>>)` | this result's slots, then the inner result's (three at most) |
| `Map<TOut>(Func<T, TOut>)` | the same |
| `Ensure(predicate, Ei)`, `FailIf(predicate, Ei)` | the same |
| `Ensure<F>(predicate, F)`, `FailIf<F>(predicate, F)` | `F` added as the last slot (arity 1 and 2) |
| `MapError<F>(Func<Ei, F>)` | slot `Ei` becomes `F` |
| `Recover(Func<Ei, T>)` | slot `Ei` removed; on `Result<T, E1>` it returns `T` |
| `Compensate(Func<Ei, Result<T, E..>>)` | slot `Ei` removed; on `Result<T, E1>` the same set |

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<Order, NotFoundError, ConflictError> locked = Find(1).BindWiden(Lock);
Console.WriteLine(locked.IsFailure);                                   // True: order 1 is locked

Result<Order, ConflictError> orNew = Find(9)
    .BindWiden(Lock)
    .Recover((NotFoundError _) => new Order(0, 0m));                   // type the parameter to pick the slot
Console.WriteLine(orNew.GetValueOrThrow().Id);                         // 0

Result<Order, NotFoundError, ValidationError> checkedOrder = Find(2)
    .Ensure(o => o.Total > 0, new ValidationError("Order.Empty", "Total is zero."));
Console.WriteLine(checkedOrder.IsSuccess);                             // True

Result<decimal, NotFoundError> total = Find(2).Map(o => o.Total);
Result<string, NotFoundError> label = total.Bind<string>(t => $"{t:0.00}");
Console.WriteLine(label.GetValueOrDefault());                          // 12.50

static Result<Order, NotFoundError> Find(int id) =>
    id is 1 or 2 ? new Order(id, 12.5m) : new NotFoundError("Order.NotFound", $"Order {id} not found.");

static Result<Order, ConflictError> Lock(Order order) =>
    order.Id == 1 ? new ConflictError("Order.Locked", "Order is locked.") : order;

public sealed record Order(int Id, decimal Total);
```

Overload pitfalls:

- `Bind` keeps the error set; `BindWiden` adds to it. When the inner result has fewer error types, call `Bind<TOut>` with the type argument and let the inner result widen. `BindWiden` with such an inner result repeats a slot and fails with CS0029 on assignment.
- A lambda that returns a value on one path and an error on another has no single return type (CS0411). Give the type arguments: `.BindWiden<string, ConflictError>(...)` or `.Bind<string>(...)`.
- `MapError`, `Recover`, `Compensate` and `TapError` have one overload per slot. Type the lambda parameter, `Recover((NotFoundError e) => ...)`. An untyped `e => ...` fails with CS0121.
- `Ensure` with an error whose static type is derived, such as `new UserErrors.Locked(id)` on a `ConflictError` slot, binds `Ensure<F>` and adds a slot. Cast it to the slot type to keep the set. Analyzer CSE5006 reports this.
- These sync overloads behave the same on C# 12, 13 and 14. All throw `InvalidOperationException` on `default`, and `Map`, `MapError` and `Recover` throw `ArgumentNullException` when the delegate returns `null`.

### Side effects, values, queries and collections

| Member | What it does |
|---|---|
| `Tap(Action<T>)`, `TapIf(predicate, Action<T>)` | Runs the action on a success value. |
| `TapError(Action<Ei>)` | One overload per slot. Type the lambda parameter. |
| `TapError(Action<Error>)` | Runs with any error converted to `Error`. Only when every slot implements `IError`; write `(Error e) => ...`. |
| `TryCatch<TOut, TException>(Func<T, TOut>)` | Like `Map`, but a `TException` becomes an error in a new last slot. Other exceptions propagate. Give both type arguments. Arity 1 and 2. |
| `Finally<TOut>(Func<Result<..>, TOut>)`, `Finally(Action<Result<..>>)` | Hands the whole result to the delegate. |
| `GetValueOrDefault()`, `GetValueOr(T)` | The value or a fallback. Never throw, not even on `default`. |
| `GetValueOrThrow(string?)` | The value or `InvalidOperationException`. |
| `GetValueOr(Func<Error, T>)` | The value or a fallback built from the error. Only when every slot implements `IError`. |
| `Select`, `SelectMany` | Query syntax. `SelectMany` keeps the error set or adds the inner result's slots. |
| `Result<T, E..>.Combine(first, second, ...)` | 2 to 8 results with the same error set into a tuple. Stops at the first error. |
| `Sequence()`, `Traverse(selector)` | A collection of results into one result of an array. Stop at the first error. |
| `Partition()` | The success values and the errors (as `Error`) of a collection. |

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int, NotFoundError, FormatException> quantity = Find(1).TryCatch<int, FormatException>(text => int.Parse(text));
quantity.TapError((FormatException ex) => Console.WriteLine($"bad number: {ex.Message}"));
Console.WriteLine(quantity.IsFailure);                                  // True

Result<string[], NotFoundError> all = new[] { 2, 3 }.Traverse(Find);
Console.WriteLine(string.Join(",", all.GetValueOrDefault() ?? []));     // 12,7

var (values, errors) = new[] { Find(2), Find(9) }.Partition();
Console.WriteLine($"{values.Length} found, {errors[0].Code}");          // 1 found, Item.NotFound

Result<(string, string), NotFoundError> pair = Result<string, NotFoundError>.Combine(Find(2), Find(3));
Console.WriteLine(pair.GetValueOrThrow());                              // (12, 7)

Result<int, NotFoundError> sum =
    from a in Find(2)
    from b in Find(3)
    select int.Parse(a) + int.Parse(b);
Console.WriteLine(sum.GetValueOr(e => -1));                             // 19

static Result<string, NotFoundError> Find(int id) => id switch
{
    1 => "twelve",
    2 => "12",
    3 => "7",
    _ => new NotFoundError("Item.NotFound", $"Item {id} not found."),
};
```

`Combine`, `Sequence` and `Traverse` return the first error, unlike the `Result<T>` versions that collect every error. To collect all errors, convert with `ToResult()` and use the `Result<T>` helpers.

Query syntax uses one name for both `SelectMany` forms. The inner result decides: the same error set keeps the set (on C# 12 this is CS1940, use `Bind`); other error types are added after this result's; a subset repeats a slot and fails with CS0029 on assignment, so use `Bind<TOut>` for that step.

### Async combinators

`Bind`, `BindWiden`, `Map`, `Ensure`, `FailIf`, `MapError`, `Compensate`, `Tap`, `TapIf`, `TapError`, `TryCatch`, `Finally` and `Select` have `*Async` forms: instance members that await a `Task` or `ValueTask` handler, and extensions on `Task<Result<T, E..>>` and `ValueTask<Result<T, E..>>` sources. `Recover`, `Combine` and `SelectMany` stay sync.

| Family | Instance handler | `Task` source handler | `ValueTask` source handler |
|---|---|---|---|
| `BindAsync`, `BindWidenAsync`, `MapErrorAsync` | `Task`, `ValueTask` | sync, `Task` | sync, `ValueTask` |
| `MapAsync`, `TapAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `ValueTask` |
| `EnsureAsync`, `FailIfAsync`, `TapIfAsync` | `Task`, `ValueTask`¹ | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `CompensateAsync`, `TapErrorAsync` | `Task`, `ValueTask`¹ | `Task` | `Task`, `ValueTask`¹ |
| `TryCatchAsync` | none | sync | sync |
| `FinallyAsync` (`Func`) | none | sync, `Task` | sync, `ValueTask`¹ |
| `FinallyAsync` (`Action`) | none | sync, `Task` | sync, `Task`, `ValueTask`¹ |
| `SelectAsync` | none | sync, `Task` | sync |

¹ .NET 9+ only.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<Order, NotFoundError, ConflictError> result = await FindAsync(2, CancellationToken.None)
    .BindWidenAsync(LockAsync)
    .MapAsync(o => o with { Locked = true });

Console.WriteLine(result.GetValueOrThrow());   // Order { Id = 2, Locked = True }

static async Task<Result<Order, NotFoundError>> FindAsync(int id, CancellationToken ct)
{
    await Task.Delay(1, ct);
    return id > 0 ? new Order(id, false) : new NotFoundError("Order.NotFound", "Not found.");
}

static Task<Result<Order, ConflictError>> LockAsync(Order order) =>
    Task.FromResult<Result<Order, ConflictError>>(order);

public sealed record Order(int Id, bool Locked);
```

- `GetValueOrDefaultAsync`, `GetValueOrAsync(T)` and, on `IError` slots, `GetValueOrAsync(Func<Error, T>)` exist on `Task` and `ValueTask` sources.
- `TraverseAsync` takes a `Task` selector, or a `ValueTask` one on .NET 9+, with or without a token, and awaits one item at a time. `SequenceAsync` takes `Task` or `ValueTask` results; when it stops at an error it does not await the rest, and it observes their faults.
- Every member takes a `CancellationToken`, observed while a pending task is awaited, except the instance `BindAsync` and `BindWidenAsync` (their `Task` and `ValueTask` source forms take one). `MapAsync` and `MapErrorAsync` also check it before they call the handler. Library awaits use `ConfigureAwait(false)`.
- An untyped `async` lambda picks the `Task` handler on C# 13+. On C# 12 it is ambiguous on an instance (CS0121); type the handler or give the type arguments, as in `source.EnsureAsync<int, NotFoundError>(v => v > 0, error)`.
- On a `Task` or `ValueTask` source, `EnsureAsync` with a derived error keeps the base slot. The sync `Ensure` and the instance `EnsureAsync` add a slot.

### `default`

`default(Result<T, E1, E2>)` holds no case. Analyzer [CSE5001](#diagnostics) reports where one is created in source, but uninitialized fields, `new Result<T, E1>()` and array elements are all `default`.

| Member | On `default` |
|---|---|
| `HasValue` | `false` |
| `TryGetValue(...)` | `false` for every case |
| `Value` | `null` |
| `IsSuccess` / `IsFailure` | `false` / `true` |
| switch expression without a `null` arm | throws `SwitchExpressionException` |
| `Match`, `Switch`, `ToResult`, `GetValueOrThrow`, every combinator | throw `InvalidOperationException` |
| `Finally` | passes the `default` to the delegate |
| `GetValueOrDefault`, `GetValueOr(T)` | return the fallback |
| `Errors`, `FirstError`, `LastError` | an `Unexpected` error with code `Result.Default` |
| `==` | equal only to another `default` |

### Equality

Two typed results are equal when they hold the same slot and an equal value (`EqualityComparer<T>.Default`) or an equal error (`Equals`). The same error instance in two different slots is not equal. Error kinds are records, so two errors with the same code, description and metadata are equal.

A typed result is a struct of three fields and can tear when one thread writes a shared field while another reads it. Guard a shared mutable field with a lock.

### Struct error types

Error types may be structs. A struct error is boxed when stored, because all slots share one `object` field. `default` of a struct error is a valid error with default fields, often a `null` code; the conversion to `Error` turns an `IError` with no code into an `Unexpected` error instead of throwing. Prefer the class kinds.

### JSON

A typed result serializes as an envelope with the case name and its value:

```text
{"case":"Success","value":{"Id":3}}
{"case":"NotFoundError","value":{"Code":"User.NotFound","Description":"User not found.", ...}}
```

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<UserDto, NotFoundError> found = new UserDto(3);
Result<UserDto, NotFoundError> missing = new NotFoundError("User.NotFound", "User not found.");

string json = JsonSerializer.Serialize(found, AppJsonContext.Default.Options);
Console.WriteLine(json);                                                   // {"case":"Success","value":{"Id":3}}

string errorJson = JsonSerializer.Serialize(missing, AppJsonContext.Default.Options);
var back = JsonSerializer.Deserialize<Result<UserDto, NotFoundError>>(errorJson, AppJsonContext.Default.Options);
Console.WriteLine(back.TryGetValue(out NotFoundError? error) ? error.Code : "?"); // User.NotFound

public sealed record UserDto(int Id);

// The bundled source generator registers the converter of every typed result listed here (Native AOT safe).
[JsonSerializable(typeof(Result<UserDto, NotFoundError>))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(NotFoundError))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
```

- The case is `Success` or the slot type's name. When two slots share a name, both use the full C# name (`MyApp.Invalid<MyApp.Email>`).
- The value is written and read as the slot type, so no `[JsonDerivedType]` is needed. A derived error in a base slot is written as the base type and comes back as the base kind.
- `default` is written as `null`, and `null` reads back as `default`. Reading throws `JsonException` for an unknown case, a missing or `null` `value`, or an extra property. `case` and `value` may come in either order, and their names are case-sensitive.
- Metadata comes back as `JsonElement` values, so an error with metadata keeps its code but is no longer equal to the original. Match on the code.
- The Mediator caching and idempotency behaviors replay a typed response through this envelope, with the same slot and error type.

Native AOT: the factory builds converters through reflection unless one is registered. The bundled generator registers every typed result named in a `[JsonSerializable]` attribute, including one used as a generic argument or array element (`List<Result<UserDto, NotFoundError>>`). It does not look inside properties: for a typed result reachable only through a property, list it in its own `[JsonSerializable]` or call `TypedResultJsonConverterFactory.Register<UserDto, NotFoundError>()` at startup. List the success type and every error type in the context too, or that case throws `NotSupportedException`. A converter you add to `Converters` still wins; `TypedResultJsonConverter<T, E1>`, `<T, E1, E2>` and `<T, E1, E2, E3>` are public for that. Under Native AOT, a typed result with no registered converter throws `NotSupportedException` naming the type to list. The generator needs C# 9 and skips types generated code cannot name, such as private nested types.

### HTTP responses

`CSharpEssentials.AspNetCore` turns a typed result into a response with `ToHttpResult()`, `ToOk()`, `ToCreated(...)` or `ToNoContent()`, and lists one problem response per error slot in OpenAPI. See [Typed results](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD#typed-results).

Do not return the raw result from a handler: its error would go out as a 200. `AddEnhancedProblemDetails` registers a converter that throws first, so the request ends as a 500, and analyzer [CSE1013](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#diagnostics) in `CSharpEssentials.Endpoints` reports it at build time.

### Converting to `Result<T>`

`ToResult()` returns a `Result<T>`. The typed result also implements `IResultBase` and `ITypedResult` (`HasValue`, `Value`), so Mediator behaviors, `ResultEndpointFilter` and other code written for `Result<T>` keep working. Each error becomes one `Error`:

- an `Error` stays as it is;
- any other `IError` keeps its code, description, type and metadata instance;
- an `Exception` becomes `Error.Exception(exception)`;
- anything else becomes an `Unexpected` error coded with the slot type name.

`IResultBase.Errors` never throws. When a handler that returns a typed result throws, the Mediator `ExceptionHandlingBehavior` lets the exception through unchanged; see Mediator [Typed responses](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Mediator/Readme.MD#typed-responses).

### Diagnostics

The package ships analyzers for typed results. They load in any SDK with Roslyn 4.8 or later. Every rule is a warning, so `TreatWarningsAsErrors` makes it a build error. Change a severity in `.editorconfig`: `dotnet_diagnostic.CSE5001.severity = none`.

| ID | Reports | Code fix |
|---|---|---|
| CSE5001 | A typed result created as `default`: `default`, `default(R)`, `new R()`, `new R[n]` without an initializer, and a private field or auto-property (no setter or a private one) without an initializer that no code of its type assigns. A member other types can set and `r == default` are not reported. | None: there is no value to put in its place. |
| CSE5002 | An error type repeated in one result, such as `Result<Dto, NotFoundError, NotFoundError>`. | Removes the repeated slots. A `Match` with one handler per slot then fails to compile, so nothing changes silently. |
| CSE5003 | Slots where one converts to another, such as `Result<Dto, TypedError, NotFoundError>`: a `NotFoundError` typed as `TypedError` goes to the `TypedError` slot. User-defined conversions are not reported. | None: pick error types that do not convert to each other. |
| CSE5004 | A success type that converts to or from an error slot, such as `Result<object, NotFoundError>`. | None. |
| CSE5005 | An `[ErrorKind]` that names another kind than the nearest base that declares one, or than a constant `Type` property on the same type. | Changes the attribute to the kind the errors already carry. Only when the attribute is on the type itself. |
| CSE5006 | `Ensure<F>`, `FailIf<F>`, `BindWiden`, `TryCatch`, a query `SelectMany` or their async forms adding a slot that repeats, converts to or accepts a slot the result already has. | `Ensure`/`FailIf`: casts the error to its slot type. `BindWiden`: becomes `Bind`. Not offered for `TryCatch`, `SelectMany`, or when a cast could throw. |

Every code fix supports Fix All. A fix either keeps the runtime behavior or leaves a compile error to resolve.

## Upgrading to 8.0

- Deserializing a failure without errors throws `JsonException`. JSON with `"IsFailure": true` (`Result`) or `"IsSuccess": false` (`Result<T>`) and a missing, `null` or empty `errors` array used to read as a success or a broken failure. A `Result<T>` success must include `"IsSuccess": true`.
- Async operations that wait on a task source with a `CancellationToken` throw `TaskCanceledException` instead of `OperationCanceledException` on the .NET 9, 10 and 11 targets. `catch (OperationCanceledException)` still catches it; an exact type check does not. The `netstandard2.1` asset is unchanged.

Upgrading to 9.0 (`default` results fail closed): see the [8.x to 9.0 migration guide](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v8-to-v9.md).

## Performance

Benchmarks against CSharpFunctionalExtensions, ErrorOr and FluentResults are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/BENCHMARKS.md). On net10, Map, Bind, Tap and Match allocate 0 B, like CSharpFunctionalExtensions and ErrorOr (FluentResults 88-344 B). A failure allocates 56 B (ErrorOr 88 B; CSharpFunctionalExtensions 0 B with its string error), and the async pipeline's failure path allocates 536 B vs 480 B for CSharpFunctionalExtensions. Error aggregation takes 99.09 ns / 336 B vs 42.79 ns / 184 B for ErrorOr's manual-list idiom. The package sets `IsAotCompatible` on its .NET 9+ targets and brings CSharpEssentials.Errors, Json, Core and Enums (CSharpFunctionalExtensions has no dependencies).

## See also

- [Agent skill](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-results/SKILL.md)
- [Example project](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Results)
- [CSharpEssentials.Errors](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD): `Error`, `ErrorType` and the typed error kinds
- [CSharpEssentials.Results.Testing](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results.Testing/Readme.MD): assertions for results
- [CSharpEssentials.AspNetCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD): results to problem details responses
