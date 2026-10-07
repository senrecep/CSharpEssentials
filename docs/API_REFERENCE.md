# CSharpEssentials API Reference

A guide to every package, method, and pattern in the CSharpEssentials ecosystem.

> **Philosophy:** Values over exceptions. Explicit over implicit. Composable over monolithic.
> Every abstraction exists to make C# code safer, more composable, and more expressive, bridging OOP and Functional Programming without abandoning either.

---

## Table of Contents

- [Errors: The Foundation](#1-csharpessentialserrors-the-foundation)
- [Results: Railway-Oriented Programming](#2-csharpessentialsresults-railway-oriented-programming)
- [Maybe: Explicit Optionals](#3-csharpessentialsmaybe-explicit-optionals)
- [Any: Discriminated Unions](#4-csharpessentialsany-discriminated-unions)
- [Core: Utility Belt](#5-csharpessentialscore-utility-belt)
- [Rules: Composable Business Rules](#6-csharpessentialsrules-composable-business-rules)
- [Entity: DDD Building Blocks](#7-csharpessentialsentity-ddd-building-blocks)
- [Http: Result-Returning HTTP Client](#8-csharpessentialshttp-result-returning-http-client)
- [Resilience: Transient Fault Handling](#9-csharpessentialsresilience-transient-fault-handling)
- [EntityFrameworkCore: EF Core Integration](#10-csharpessentialsentityframeworkcore-ef-core-integration)
- [Json: Serialization Defaults](#11-csharpessentialsjson-serialization-defaults)
- [AspNetCore: API Layer](#12-csharpessentialsaspnetcore-api-layer)
- [Mediator: Pipeline Behaviors](#13-csharpessentialsmediator-pipeline-behaviors)
- [Enums: Source-Generated String Enums](#14-csharpessentialsenums-source-generated-string-enums)
- [Time: Testable Clock](#15-csharpessentialstime-testable-clock)
- [Clone: Deep Copy](#16-csharpessentialsclone-deep-copy)
- [RequestResponseLogging: HTTP Logging Middleware](#17-csharpessentialsrequestresponselogging-http-logging-middleware)
- [GcpSecretManager: Secret Configuration](#18-csharpessentialsgcpsecretmanager-secret-configuration)
- [Validation: Model-First Validation](#19-csharpessentialsvalidation-model-first-validation)
- [These: 3-State Union](#20-csharpessentialsthese-3-state-union)
- [Endpoints: Source-Generated Endpoint Mapping](#21-csharpessentialsendpoints-source-generated-endpoint-mapping)
- [DependencyInjection: Attribute-Based Registration](#22-csharpessentialsdependencyinjection-attribute-based-registration)
- [CSharpEssentials: Meta Package](#23-csharpessentials-meta-package)
- [Ecosystem Design Patterns](#ecosystem-design-patterns)

---

## 1. CSharpEssentials.Errors: The Foundation

**What it is:** A structured error value type that replaces exceptions for expected failures.

**Why it exists:** Exceptions are expensive, invisible in type signatures, and break composability. `Error` is a `readonly record struct`: immutable, value-semantic, and carries enough information (code, description, type, metadata) to flow through any layer of your application without losing context.

Every other package in the ecosystem builds on this type.

### Error Factory Methods

| Method | Creates | HTTP Mapping | When to Use |
|--------|---------|-------------|-------------|
| `Error.Failure(code, desc)` | General failure | 500 | Domain logic failures |
| `Error.Validation(code, desc)` | Validation error | 400 | Input validation failures |
| `Error.NotFound(code, desc)` | Not found error | 404 | Missing resources |
| `Error.Unauthorized(code, desc)` | Auth error | 401 | Authentication failures |
| `Error.Forbidden(code, desc)` | Permission error | 403 | Authorization failures |
| `Error.Conflict(code, desc)` | Conflict error | 409 | Resource conflicts (duplicate, version mismatch) |
| `Error.Unexpected(code, desc)` | System error | 500 | Unexpected/infrastructure failures |
| `Error.Exception(ex)` | From exception | 500 | Bridging exception-based code into the error world |

All factory methods accept an optional `ErrorMetadata? metadata` parameter for attaching arbitrary key-value data.

### Error Composition

| Operation | What It Does |
|-----------|-------------|
| `error1 + error2` | Combines two errors into an `Error[]` |
| `Error.CreateMany(e1, e2, e3)` | Creates an error array explicitly |
| `implicit operator Error[]` | A single `Error` auto-converts to `Error[]` where needed |

### Error Extensions

| Method | What It Does |
|--------|-------------|
| `errorType.ToHttpStatusCode()` | Maps `ErrorType` to HTTP status code |
| `intValue.ToErrorType()` | Maps integer back to `ErrorType` |
| `error.ToResult()` | Converts an `Error` to a failed `Result` |
| `error.ToResult<T>()` | Converts an `Error` to a failed `Result<T>` |

### Special Values

| Value | Purpose |
|-------|---------|
| `Error.NoFirstError` | Sentinel for when no first error exists |
| `Error.NoErrors` | Sentinel for empty error state |
| `Error.False` | Sentinel used by `bool` → `Result` implicit conversion |

```csharp
// Creating errors with metadata
var error = Error.NotFound("User.NotFound", "User does not exist",
    new ErrorMetadata { ["UserId"] = userId.ToString() });

// Bridging exceptions
Result outcome;
try { /* external call */ outcome = Result.Success(); }
catch (Exception ex) { outcome = Error.Exception(ex); }

// Composing multiple errors
Error[] allErrors = validationError + conflictError;
```

---

## 2. CSharpEssentials.Results: Railway-Oriented Programming

> **Note:** The NuGet package is named `CSharpEssentials.Results`, but the actual C# namespace is `CSharpEssentials.ResultPattern`. Add `using CSharpEssentials.ResultPattern;` in your code.

**What it is:** A Result monad that makes success and failure explicit in your type signatures.

**Why it exists:** Traditional C# uses exceptions for flow control and null for absence. Both are invisible in method signatures and break composability. `Result<T>` forces every caller to handle both paths, enables method chaining that short-circuits on failure, and makes error accumulation trivial.

Two core types: `Result` (no value, just success/failure) and `Result<T>` (carries a value on success).

### Creating Results

| Method | Returns | When to Use |
|--------|---------|-------------|
| `return value;` / `return error;` | `Result<T>` / `Result` | Recommended: implicit conversion from `T`, `Error`, `Error[]`, `List<Error>`, `HashSet<Error>` |
| `Result.Success()` | `Result` | Void operations that succeeded |
| `Result.Success(value)` | `Result<T>` | Where the target type cannot be inferred (`var`, inferred lambdas, generic arguments) |
| `Result.Failure(error)` | `Result` | Single error failure |
| `Result.Failure(errors)` | `Result` | Multiple errors failure |
| `Result<T>.Failure(error)` | `Result<T>` | Typed failure |
| `Result.SuccessIf(condition, error)` | `Result` | Guard clause: success if condition holds |
| `Result.SuccessIf(condition, value, error)` | `Result<T>` | Guard clause: success with value if condition holds |
| `Result.FailureIf(condition, error)` | `Result` | Guard clause: failure if condition holds |
| `Result.FailureIf<TValue>(condition, error)` | `Result<T>` | Guard clause: typed failure if condition holds |
| `Result.Try(action, handler)` | `Result` | Wraps try/catch around `Action`, converts exception to Error |
| `Result.Try(func, handler)` | `Result<T>` | Wraps try/catch around `Func<T>`, returns value on success |
| `Result.Try(func, handler)` | `Result<T>` | Wraps try/catch around `Func<Result<T>>`, propagates inner result |
| `Result.Try(func, handler)` | `Result` | Wraps try/catch around `Func<Result>`, propagates inner result |
| `Result.TryAsync(action, handler)` | `Task<Result>` | Async try/catch around `Func<Task>` |
| `Result.TryAsync(func, handler)` | `Task<Result<T>>` | Async try/catch around `Func<Task<T>>` |
| `Result.TryAsync(func, handler)` | `Task<Result<T>>` | Async try/catch around `Func<Task<Result<T>>>` |
| `Result.TryAsync(func, handler)` | `Task<Result>` | Async try/catch around `Func<Task<Result>>` |
| `Result.From(errors)` | `Result` | Success if errors empty, failure otherwise |
| `Result<int> r = 42;` | `Result<int>` | Implicit operator for ergonomic creation |

### Chaining: The Success Railway

These methods execute only when the result is successful. On failure, they pass the error through unchanged.

| Method | FP Pattern | What It Does | When to Use |
|--------|-----------|-------------|-------------|
| `Bind(func)` | Monadic bind | Chains a `Result`-returning operation | Dependent operations (DB lookup, then validate) |
| `Map(func)` | Functor map | Transforms the success value | Value transformation (entity to DTO) |
| `Then(func)` | Bind alias | Same as Bind, more readable in chains | Fluent pipeline style |
| `Ensure(pred, error)` | Guard | Validates the value; fails if predicate is false | Post-condition checks |
| `EnsureNotNull(error)` | Null guard | Fails if value is null | Null safety at boundaries |
| `BindIf(cond, func)` | Conditional bind | Only executes bind if condition is true | Optional pipeline steps |
| `TryCatch(func, err)` | Exception-safe bind | Bind with automatic exception catching | Calling unsafe external code |

```csharp
Result<OrderDto> result = GetUser(userId)
    .Ensure(u => u.IsActive, Error.Failure("User.Inactive", "Account is deactivated"))
    .Bind(u => GetOrder(u.LatestOrderId))
    .Ensure(o => o.Status != OrderStatus.Cancelled, Error.Failure("Order.Cancelled", "Order was cancelled"))
    .Map(o => new OrderDto(o.Id, o.Total));
```

### Side Effects: Observe Without Changing the Railway

| Method | Runs On | What It Does |
|--------|---------|-------------|
| `Tap(action)` | Success | Executes side effect, returns self unchanged |
| `Tap(condition, action)` | Success + condition | Conditional side effect |
| `TapError(action)` | Failure | Side effect with all errors |
| `TapErrorFirst(action)` | Failure | Side effect with first error only |
| `ThenDo(action)` | Success | Executes action, returns self |
| `ElseDo(action)` | Failure | Executes action on errors, returns self |

```csharp
result
    .Tap(_ => _logger.LogInformation("Order retrieved"))
    .TapError(errors => _logger.LogWarning("Failed: {Errors}", errors));
```

### Error Handling: Recovery and Transformation

| Method | What It Does | When to Use |
|--------|-------------|-------------|
| `Else(error)` | Replaces all errors with a new error | Error message normalization |
| `Else(func)` | Transforms errors into replacement | Dynamic error replacement |
| `MapError(func)` | Transforms each error individually | Error enrichment (add context) |
| `Compensate(func)` | Attempts recovery: can return Success | Retry, fallback strategies |
| `CompensateFirst(func)` | Recovery using first error only | Single-error recovery |
| `Recover(errorType, func)` | Recovers only from specific error types | Selective recovery (e.g., only NotFound) |
| `FailIf(pred, error)` | Converts success to failure if predicate matches | Post-validation |

```csharp
// Recover from NotFound by creating a default
Result<Config> config = LoadConfig(key)
    .Recover(ErrorType.NotFound, err => Config.Default);

// Replace all errors with a user-friendly message
Result result = InternalOperation()
    .Else(Error.Failure("Operation.Failed", "Something went wrong. Please try again."));
```

### Extracting Values: Leaving the Railway

| Method | Safety | What It Does |
|--------|--------|-------------|
| `Match(onSuccess, onFailure)` / `Result<T>.Match(onSuccess, onError)` | Safe | Exhaustive fold: handles both cases, returns a value |
| `MatchFirst(onSuccess, onFirstError)` | Safe | Match using only the first error |
| `Switch(onSuccess, onFailure)` | Safe | Imperative branching (void) |
| `Unwrap()` | Unsafe | Returns value or throws `ResultUnwrapException` |
| `UnwrapOrDefault(fallback)` | Safe | Returns value or specified default |
| `GetValueOrDefault()` | Safe | Returns value or `default(T)` |
| `GetValueOrThrow(message)` | Unsafe | Returns value or throws with message |
| `Finally(func)` | Always | Executes regardless of success/failure |

```csharp
// Exhaustive matching — compiler ensures both paths are handled
string message = result.Match(
    onSuccess: order => $"Order {order.Id} placed successfully",
    onError: errors => $"Failed: {errors.First().Description}"
);

// Finally — always runs (logging, cleanup)
result.Finally(r => _metrics.Record(r.IsSuccess ? "success" : "failure"));
```

### Combining Multiple Results

| Method | Strategy | What It Does |
|--------|----------|-------------|
| `Result.And(results)` | All must succeed | Collects ALL errors if any fail |
| `Result.Or(results)` / `Result<T>.Or(results)` | Any can succeed | Returns first success; errors only if all fail |
| `Result<T1>.Combine(r1, r2, ..., r8)` | Applicative product | Combines up to 8 results into a tuple `Result<(T1, ..., T8)>` |

```csharp
// Validate multiple fields independently, collect all errors
Result validation = Result.And(new[]
{
    ValidateName(input.Name),
    ValidateEmail(input.Email),
    ValidateAge(input.Age)
});

// Try multiple providers, use first that works
Result<Config> config = Result<Config>.Or(
    LoadFromEnvironment(),
    LoadFromFile(),
    LoadFromDefaults());
```

### LINQ Query Syntax

`Result<T>` implements `Select` and `SelectMany`, enabling LINQ comprehension syntax:

```csharp
var result =
    from user in GetUser(id)
    from order in GetLatestOrder(user.Id)
    from payment in GetPayment(order.PaymentId)
    select new InvoiceDto(user.Name, order.Total, payment.Method);
```

### Async Support

Every method has `Task<Result>` and `ValueTask<Result>` extension variants with `CancellationToken` support. On a `Task<Result<T>>` the chain methods take the `Async` suffix (`BindAsync`, `MapAsync`, `TapAsync`):

```csharp
Result<UserDto> result = await GetUserAsync(id)
    .BindAsync(user => ValidateAsync(user, ct))
    .MapAsync(user => new UserDto(user))
    .TapAsync(_ => _logger.LogInformation("User retrieved"));
```

### Collection Extensions

Batch operations on sequences of results, without manually looping.

| Method | Strategy | What It Does |
|--------|----------|-------------|
| `CombineAll(IEnumerable<Result>)` | Collect all errors | Success if all succeed; accumulates ALL errors if any fail |
| `CombineAll<T>(IEnumerable<Result<T>>)` | Collect all errors | Same as `Sequence`: success array or all errors |
| `Sequence<T>(IEnumerable<Result<T>>)` | Collect all | Returns `Result<T[]>` with all values, or all errors |
| `Traverse<TSource, TOut>(source, selector)` | Map + sequence | Applies selector to each element, then sequences |
| `Partition<T>(IEnumerable<Result<T>>)` | Split | Returns `(T[] Successes, Error[] Errors)`: never fails |
| `FirstFailureOrSuccesses(IEnumerable<Result>)` | Short-circuit | Returns first failure immediately; otherwise success |
| `FirstFailureOrSuccesses<T>(IEnumerable<Result<T>>)` | Short-circuit | Returns first failure or `Result<T[]>` of all values |

```csharp
// Validate a batch — collect ALL errors
Result validation = validationResults.CombineAll();

// Map each item and collect all successes, or all errors
Result<OrderDto[]> orders = orderIds
    .Traverse(id => GetOrder(id));

// Split a mixed batch without short-circuiting
var (succeeded, failed) = results.Partition();
Console.WriteLine($"{succeeded.Length} succeeded, {failed.Length} errors");

// Stop at first failure — useful for sequential pipeline steps
Result pipeline = steps.FirstFailureOrSuccesses();
```

---

## 3. CSharpEssentials.Maybe: Explicit Optionals

**What it is:** An Option/Maybe monad that explicitly represents the presence or absence of a value.

**Why it exists:** `null` is invisible in C# type signatures (even with nullable reference types, it's a warning, not an error). `Maybe<T>` makes optionality a first-class citizen: you cannot access the value without acknowledging it might not exist. Unlike `Result`, Maybe does not carry a reason for absence, it simply says "there is no value."

### Creating Maybe Values

| Method | Creates | When to Use |
|--------|---------|-------------|
| `Maybe<int> m = 42;` | Some(42) via implicit operator | Recommended; `T?` converts the same way (null → None) |
| `Maybe.None` / `Maybe<T>.None` | Absence | Explicit "no value"; `Maybe.None` converts implicitly to any `Maybe<T>` |
| `Maybe.From(value)` | Some if non-null, None if null | Where the target type is not known (`var`, start of a chain) |
| `value.AsMaybe()` | Extension on nullable | Converting any nullable |

### Transformations

| Method | FP Pattern | What It Does | When to Use |
|--------|-----------|-------------|-------------|
| `Map(func)` | Functor | Transforms value if present, None passes through | Value transformation |
| `Bind(func)` | Monad | Chains Maybe-returning operations | Dependent lookups |
| `Where(predicate)` | Filter | Returns None if predicate fails | Conditional filtering |
| `MapIf(cond, func)` | Conditional map | Only maps if condition holds | Optional transformation |
| `BindIf(cond, func)` | Conditional bind | Only binds if condition holds | Optional chaining |
| `Flatten(nested)` | Monad join | `Maybe<Maybe<T>>` to `Maybe<T>` | Removing nesting |

```csharp
Maybe<string> displayName = GetUser(id)
    .Bind(u => u.Profile.AsMaybe())
    .Map(p => p.DisplayName.Trim())
    .Where(name => name.Length > 0);
```

### Extracting Values

| Method | Safety | What It Does |
|--------|--------|-------------|
| `Match(some, none)` | Safe | Exhaustive fold over both cases |
| `Or(() => fallback)` | Safe | Returns self, or a `Maybe` of the fallback when empty |
| `Or(Maybe<T> fallback)` | Safe | Returns self or fallback Maybe |
| `GetValueOrDefault(value)` | Safe | Returns value or default |
| `GetValueOrDefault()` | Safe | Returns `default(T)` |
| `TryGetValue(out value)` | Safe | Try pattern for extraction |
| `AsNullable()` | Safe | Converts back to nullable `T?` |

```csharp
string name = GetUser(id)
    .Map(u => u.DisplayName)
    .GetValueOrDefault("Anonymous");
```

### Side Effects

| Method | Runs On | What It Does |
|--------|---------|-------------|
| `Execute(action)` | Has value | Runs action with value (`void`, or `Task` for async actions) |
| `ExecuteNoValue(action)` | No value | Runs action when empty (`void`, or `Task` for async actions) |
| `Tap(action)` | Has value | Side effect with value, returns self |
| `TapIf(cond, action)` | Has value + condition | Conditional side effect, returns self |

### Collection Helpers

| Method | What It Does | Replaces |
|--------|-------------|----------|
| `TryFirst()` | First element or None | `FirstOrDefault` (no null) |
| `TryFirst(predicate)` | First matching or None | `FirstOrDefault(pred)` |
| `TryLast()` | Last element or None | `LastOrDefault` |
| `TryFind(key)` | Dictionary lookup or None | `TryGetValue` boilerplate |
| `Choose(maybes)` | Extracts all Some values, drops None | Manual null filtering |
| `ToList()` | Single-element or empty list | Manual conditional list |

```csharp
// Safe dictionary lookup — no KeyNotFoundException
Maybe<User> user = _cache.TryFind(userId);

// Filter a collection of Maybes to only present values
IEnumerable<string> names = users
    .Select(u => u.MiddleName.AsMaybe())
    .Choose();
```

### Maybe-Result Bridge

| Method | Direction | What It Does |
|--------|-----------|-------------|
| `maybe.ToMaybeResult(error?)` | Maybe to Result | None becomes Failure, Some becomes Success |
| `maybe.ToMaybeUnitResult(error?)` | Maybe to Result (unit) | None becomes Failure (no value) |
| `result.AsMaybe()` | Result to Maybe | Failure becomes None, Success becomes Some |

```csharp
// Upgrade Maybe to Result when you need error information
Result<User> result = FindUser(id)   // returns Maybe<User>
    .ToMaybeResult(Error.NotFound("User.NotFound", "User does not exist"));
```

### Collection Extensions

| Method | What It Does |
|--------|-------------|
| `Sequence<T>(IEnumerable<Maybe<T>>)` | `Maybe<T[]>`: `None` if any element is `None` |
| `Traverse<TSource, TOut>(source, selector)` | Applies selector then sequences: `None` if any is `None` |
| `Partition<T>(IEnumerable<Maybe<T>>)` | Returns `(T[] Values, int NoneCount)`: never returns `None` |

```csharp
// Require ALL lookups to succeed
Maybe<User[]> allUsers = userIds
    .Traverse(id => _cache.TryFind(id));  // None if any id is missing

// Collect present values, count absences
var (values, missingCount) = maybes.Partition();
```

---

## 4. CSharpEssentials.Any: Discriminated Unions

**What it is:** Type-safe union types for C#. `Any<T0, T1>` through `Any<T0, ..., T7>`, a value that holds exactly one of N possible types.

**Why it exists:** C# has no native discriminated unions (until future language versions). When a method can return different types, developers resort to `object`, `dynamic`, marker interfaces, or separate result classes. `Any<T0, T1>` provides compile-time type safety with exhaustive matching: if you forget a case, the compiler tells you.

### Creating Unions

| Method | What It Does |
|--------|-------------|
| `Any<int, string>.First(42)` | Explicit construction (index 0) |
| `Any<int, string>.Second("hello")` | Explicit construction (index 1) |
| `Any<int, string> a = 42;` | Implicit operator from any variant type |

### Inspecting

| Property/Method | What It Does |
|----------------|-------------|
| `Index` | Which variant is active (0-based) |
| `Value` | The held value as `object` |
| `IsFirst` / `IsSecond` / ... | Boolean check for active variant |
| `GetFirst()` / `GetSecond()` / ... | Typed extraction (throws if wrong variant) |
| `Is<T>()` | Checks if held value is of type T |
| `TryAs<T>(out value)` | Safe typed extraction via try pattern |

### Pattern Matching

| Method | Returns | What It Does |
|--------|---------|-------------|
| `Match(first:, second:, ...)` | `AnyActionResult<T>` | Transforms the active variant: partial (delegates are optional) |
| `Switch(first:, second:, ...)` | `AnyActionStatus` | Executes action for active variant: partial (delegates are optional) |
| `Deconstruct(out first, out second, ...)` | void | C# deconstruction; the inactive slots are `default` |

```csharp
// API that returns either data or a structured error
Any<UserDto, ApiError> response = CallExternalApi(request);

string message = response.Match(
    first: user => $"Welcome, {user.Name}!",
    second: error => $"API error: {error.Message}"
).Result;

// Modeling domain states
Any<Draft, Published, Archived> articleState = GetArticleState(id);

articleState.Switch(
    first: draft => SendForReview(draft),
    second: published => UpdateSearchIndex(published),
    third: archived => LogAccess(archived)
);
```

### Collection Extensions

Scatter a sequence of unions into per-type arrays. Works for all arities (`Any<T0,T1>` through `Any<T0,...,T7>`).

| Method | What It Does |
|--------|-------------|
| `Partition<T0,T1>(IEnumerable<Any<T0,T1>>)` | Returns `(T0[] First, T1[] Second)` |
| `Traverse<TSource,T0,T1>(source, selector)` | Applies selector then partitions |
| *(up to 8-arity)* | `Partition` and `Traverse` overloads for `Any<T0,...,T7>` |

```csharp
// Classify API responses into successes and errors in one pass
var (users, errors) = responses
    .Traverse(r => ClassifyResponse(r));   // returns Any<UserDto, ApiError>

Console.WriteLine($"{users.Length} succeeded, {errors.Length} failed");
```

---

## 5. CSharpEssentials.Core: Utility Belt

**What it is:** Foundational extension methods used across every project: null checks, string conversions, collection helpers, and async utilities.

**Why it exists:** Every C# project reinvents `IsNullOrEmpty`, `string.ToPascalCase()`, `list.WhereIf(condition, ...)`. This package provides well-tested, consistent implementations.

### Null and Boolean Guards

| Method | What It Does | Returns |
|--------|-------------|---------|
| `value.IsNull()` | True if null (reference and nullable value types) | `bool` |
| `value.IsNotNull()` | True if not null | `bool` |
| `str.IsEmpty()` | True if null, empty, or whitespace | `bool` |
| `str.IsNotEmpty()` | Negation of IsEmpty | `bool` |
| `bool.IsTrue()` | Identity (fluent readability) | `bool` |
| `bool.IsFalse()` | Negation (fluent readability) | `bool` |

### Conditional Execution

| Method | What It Does |
|--------|-------------|
| `bool.IfTrue(action)` | Executes action if true, returns the bool |
| `bool.IfFalse(action)` | Executes action if false, returns the bool |
| `value.IfNotNull(action)` | Executes action if non-null (with optional else branch) |
| `value.IfNull(action)` | Executes action if null (with optional else branch) |

### String Case Conversions

All methods accept an optional `CultureInfo` parameter.

| Method | Input | Output |
|--------|-------|--------|
| `ToPascalCase()` | `"hello world"` | `"HelloWorld"` |
| `ToCamelCase()` | `"hello world"` | `"helloWorld"` |
| `ToKebabCase()` | `"HelloWorld"` | `"hello-world"` |
| `ToSnakeCase()` | `"HelloWorld"` | `"hello_world"` |
| `ToMacroCase()` | `"HelloWorld"` | `"HELLO_WORLD"` |
| `ToTrainCase()` | `"helloWorld"` | `"Hello-World"` |
| `ToTitleCase()` | `"hello world"` | `"Hello World"` |
| `ToUnderscoreCamelCase()` | `"HelloWorld"` | `"_helloWorld"` |

### Collection Extensions

| Method | What It Does |
|--------|-------------|
| `WhereIf(condition, predicate)` | Applies filter only if condition is true; otherwise returns unfiltered |
| `WithoutNulls()` | Removes null entries from a collection |
| `HasSameElements(other)` | Order-independent element equality |
| `IfAdd(condition, item)` | Conditionally adds item to collection |
| `ForEach(action)` | Lazy: yields each item and runs the action while the sequence is enumerated |
| `AllTrue()` / `AllFalse()` | Checks bool collections |

### Comparison Helpers

| Method | What It Does |
|--------|-------------|
| `value.IsBetween(min, max)` | `true` when `min <= value <= max` (any `IComparable<T>`) |
| `value.IsBetweenExclusive(min, max)` | `true` when `min < value < max` |

### Async Helpers

| Method | What It Does |
|--------|-------------|
| `value.AsTask()` | Wraps value in `Task.FromResult` |
| `value.AsValueTask()` | Wraps value in completed `ValueTask` |
| `task.WithCancellation(ct)` | Adds `CancellationToken` support to any Task/ValueTask |

### Guid Utilities

| Method | What It Does |
|--------|-------------|
| `guid.ToStringFromGuid()` | URL-safe Base64-encoded short GUID string |
| `str.ToGuidFromString()` | Reverse: decodes back to `Guid` |

---

## 6. CSharpEssentials.Rules: Composable Business Rules

**What it is:** A rules engine where each rule is an independent, testable unit that returns `Result`. Rules compose into trees via AND, OR, Linear (sequential), and Conditional (if/else) strategies.

**Why it exists:** Complex business validation often ends up as deeply nested if/else blocks or procedural validators that are hard to test, reuse, or compose. The rules engine treats each rule as a first-class object that can be combined declaratively.

### Rule Types

| Interface | Strategy | Behavior |
|-----------|----------|----------|
| `IRule<TContext>` | Single rule | Evaluates and returns `Result` |
| `ILinearRule<TContext>` | Sequential chain | Evaluates in order; stops at first failure |
| `IAndRule<TContext>` | All must pass | Evaluates ALL rules; collects all errors |
| `IOrRule<TContext>` | Any can pass | Returns first success; fails only if all fail |
| `IConditionalRule<TContext>` | If/else | Branches based on a condition rule |

Each type has `IAsyncRule` variants and `TResult`-returning variants.

### RuleEngine API

| Method | What It Does |
|--------|-------------|
| `RuleEngine.Evaluate(rule, context, ct)` | Dispatches any rule type via pattern matching |
| `RuleEngine.Linear(rules, context, ct)` | Sequential: stops at first failure |
| `RuleEngine.And(rules, context, ct)` | All must pass: accumulates all errors |
| `RuleEngine.Or(rules, context, ct)` | First success wins |
| `RuleEngine.If(condition, success, failure, ctx)` | Conditional branching |
| `func.ToRule()` | Adapts a `Func<TContext, Result>` to `IRule` |

```csharp
// Define rules as simple classes
public sealed class MinimumAgeRule(int minAge) : IRule<UserContext>
{
    public Result Evaluate(UserContext ctx, CancellationToken ct)
        => Result.SuccessIf(ctx.User.Age >= minAge,
            Error.Validation("User.TooYoung", $"Must be at least {minAge}"));
}

public sealed class EmailVerifiedRule : IRule<UserContext>
{
    public Result Evaluate(UserContext ctx, CancellationToken ct)
        => Result.SuccessIf(ctx.User.EmailVerified,
            Error.Validation("User.EmailNotVerified", "Email must be verified"));
}

// Compose: all must pass, collect all validation errors
var rules = new IRule<UserContext>[] { new MinimumAgeRule(18), new EmailVerifiedRule() };
Result result = RuleEngine.And(rules, context, ct);

// Or use lambdas
Func<OrderContext, Result> stockCheck = ctx =>
    Result.SuccessIf(ctx.Product.Stock >= ctx.Quantity,
        Error.Conflict("Product.OutOfStock", "Insufficient stock"));

Result orderResult = RuleEngine.Evaluate(stockCheck.ToRule(), orderContext, ct);
```

---

## 7. CSharpEssentials.Entity: DDD Building Blocks

**What it is:** Base classes for Domain-Driven Design entities with audit fields, domain events, and soft deletion.

**Why it exists:** Every DDD entity needs audit trails (`CreatedAt`, `UpdatedBy`), domain event dispatch, and often soft-delete support. These base classes provide this infrastructure so domain models focus on business logic.

### EntityBase

| Member | Type | What It Does |
|--------|------|-------------|
| `CreatedAt` | `DateTimeOffset` | When the entity was created |
| `CreatedBy` | `string` | Who created it |
| `UpdatedAt` | `DateTimeOffset?` | When last updated |
| `UpdatedBy` | `string?` | Who last updated it |
| `DomainEvents` | `IReadOnlyList<IDomainEvent>` | Pending domain events |
| `Raise(event)` | method | Queues a domain event for later dispatch |
| `ClearDomainEvents()` | method | Clears the event queue (call after publishing) |
| `SetCreatedInfo(at, by)` | method | Sets creation audit fields |
| `SetUpdatedInfo(at, by)` | method | Sets update audit fields |

### EntityBase\<TId\>

Extends `EntityBase` with a strongly-typed `Id` property where `TId : IEquatable<TId>`.

### SoftDeletableEntityBase

Extends `EntityBase` with:

| Member | Type | What It Does |
|--------|------|-------------|
| `IsDeleted` | `bool` | Soft-delete flag |
| `DeletedAt` | `DateTimeOffset?` | When deleted |
| `DeletedBy` | `string?` | Who deleted it |

### Extensions

| Method | What It Does |
|--------|-------------|
| `entities.HardDelete()` | Physically removes soft-deleted entities from a collection |

---

## 8. CSharpEssentials.Http: Result-Returning HTTP Client

**What it is:** Extension methods and a fluent builder that wrap `HttpClient` calls to return `Result<T>` instead of throwing exceptions or requiring manual status code checks.

**Why it exists:** Raw `HttpClient` usage involves checking `IsSuccessStatusCode`, handling `HttpRequestException`, deserializing manually, and mapping status codes to domain errors. This package does all of that and returns `Result<T>`.

### Fluent Request Builder

```csharp
Result<UserDto> result = await HttpRequestBuilder
    .Get("https://api.example.com/users")
    .WithQuery("page", "1")
    .WithQuery("limit", "10")
    .WithHeader("Authorization", $"Bearer {token}")
    .WithHeader("X-Request-Id", correlationId)
    .AsResultAsync<UserDto>(httpClient);
```

| Method | What It Does |
|--------|-------------|
| `HttpRequestBuilder.Get(url)` | Creates GET builder |
| `.Post(url)` / `.Put(url)` / `.Patch(url)` / `.Delete(url)` | Other HTTP methods |
| `.WithHeader(name, value)` / `.WithHeaders(dictionary)` | Adds request headers |
| `.WithQuery(key, value)` / `.WithQuery(dictionary)` | Adds query parameters. A repeated key is kept. |
| `.WithQuery(key, object? value)` | Adds a query parameter. Enums are formatted by the enum conventions; flags and enum collections become repeated keys. |
| `.WithRoute(name, object? value)` | Replaces the `{name}` placeholder of the URI with the escaped value; enums are formatted by the enum conventions |
| `.WithEnumConventions(conventions, writeAs?)` | Sets the conventions and the output format (`Number` for legacy servers) for this request's route, query and default JSON body |
| `.WithJsonContent(body, options?)` | Sets JSON request body |
| `.WithContent(httpContent)` | Sets any `HttpContent` body |
| `.WithMethod(method)` / `.WithUri(uri)` | Changes the HTTP method or target URI |
| `.FollowRedirects(maxRedirects = 5)` | Follows redirect responses |
| `.Build()` | Returns `Result<HttpRequestMessage>` |
| `.AsResultAsync(client)` | Builds, sends, returns `Result` |
| `.AsResultAsync<T>(client, jsonOptions?, ct)` | Builds, sends, deserializes to `Result<T>` |

There is no bearer-token shortcut; set the header with `.WithHeader("Authorization", $"Bearer {token}")`.

### Enums in route, query and body

Enums with generated metadata (`[StringEnum]`) are written as wire names, or as numbers with `EnumWireFormat.Number`. Plain enums keep the framework behavior: `ToString()` in route and query, a number in JSON bodies. An undefined enum value is never sent; `Build()` returns a validation error and a JSON body fails to serialize.

```csharp
Result<OrderDto> order = await HttpRequestBuilder
    .Get("https://api.example.com/orders/{status}")
    .WithEnumConventions(EnumConventions.Default, EnumWireFormat.Number)  // a legacy server that accepts only numbers
    .WithRoute("status", OrderStatus.PendingApproval)                     // /orders/1
    .WithQuery("permissions", Permissions.Read | Permissions.Write)       // ?permissions=3
    .AsResultAsync<OrderDto>(httpClient, jsonOptions);
```

Pass `jsonOptions` built once with `new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(conventions, EnumReadMode.Data, writeAs)` so responses are read tolerantly (names, numbers and the fallback member). `QueryStringExtensions` has the same enum handling: `ToQueryString(source, conventions, format)` and `WithQueryString(name, value, conventions, format)`.

### HttpClient Extensions

| Method | What It Does |
|--------|-------------|
| `GetFromJsonAsResultAsync<T>` | GET + deserialize to `Result<T>` |
| `PostAsJsonAsResultAsync<T>` | POST JSON + deserialize response |
| `PutAsJsonAsResultAsync<T>` | PUT JSON + deserialize response |
| `PatchAsJsonAsResultAsync<T>` | PATCH JSON + deserialize response |
| `DeleteAsResultAsync` | DELETE to `Result` |
| `SendAsResultAsync` | Send any request to `Result` |

### Status Code Mapping

HTTP status codes are automatically mapped to `ErrorType`:

| HTTP Status | ErrorType |
|------------|-----------|
| 400 | `Validation` |
| 401 | `Unauthorized` |
| 403 | `Forbidden` |
| 404 | `NotFound` |
| 409 | `Conflict` |
| 5xx | `Unexpected` |

### Resilience (Polly Integration)

> **Moved:** These methods have been replaced by the dedicated `CSharpEssentials.Resilience` package (Section 9). Prefer `ResiliencePolicy` / `ResiliencePolicy<T>` for new code.

| Method | What It Does |
|--------|-------------|
| `CreateRetryPipeline` | Polly retry policy |
| `CreateCircuitBreakerPipeline` | Circuit breaker policy |
| `CreateTimeoutPipeline` | Timeout policy |
| `CreateResiliencePipeline` | Combines all resilience policies |
| `ExecuteAsResultAsync` | Executes through resilience pipeline, returns `Result` |

### SSRF Guard (.NET 9+)

Protects `IHttpClientFactory` clients that call URLs supplied by users. Not available on `netstandard2.1`.

```csharp
services.AddHttpClient("webhooks").AddSsrfGuard(o => o.Timeout = TimeSpan.FromSeconds(10));
```

| Type / Member | What It Does |
|---------------|-------------|
| `AddSsrfGuard(this IHttpClientBuilder, Action<SsrfGuardOptions>?)` | Installs a `SocketsHttpHandler` that resolves DNS and checks addresses in `ConnectCallback`, with no proxy, no cookie container and no automatic redirects. Adds a handler that follows redirects, checks every hop and caps the request version at HTTP/2 (QUIC skips `ConnectCallback`). The primary handler repeats the version cap and request policy on the final request, so handlers added after the guard cannot bypass them |
| `SsrfGuardOptions` | `AllowHttp`, `AllowedPorts`, `MaxRedirects` (3), `MaxResponseContentLength`, `Timeout`, `AllowedNetworks`/`BlockedNetworks` (`IPNetwork`), `AllowedHosts`/`BlockedHosts` (exact or `*.suffix`; `*.example.com` does not match `example.com`; case/trailing-dot/IDN normalized). `AllowedNetworks` matches only the raw address; `BlockedNetworks` matches the raw or embedded IPv4 address, `AddressPolicy`, `RequestPolicy` |
| `IOutboundAddressPolicy` | `bool IsAllowed(IPAddress address, Uri requestUri)`; receives the embedded IPv4 address for mapped/compatible/SIIT/NAT64/6to4 forms, then the raw address when it differs; both must be allowed. `::` and `::1` are not reduced to IPv4. Set `SsrfGuardOptions.AddressPolicy` for one client, or register one in DI for every guarded client |
| `IOutboundRequestPolicy` | `bool IsAllowed(Uri requestUri)`, checked on the request and every redirect. Set `SsrfGuardOptions.RequestPolicy` for one client, or register one in DI for every guarded client |
| `DefaultOutboundAddressPolicy.Instance` | Blocks private, loopback, link-local, site-local, CGNAT, Teredo, ORCHID, discard-only (`100::/64`), SIIT (`::ffff:0:0:0/96`), 6to4 relay anycast (`192.88.99.0/24`), local NAT64, documentation, benchmark, multicast and reserved ranges, plus IPv4-mapped, IPv4-compatible, SIIT, NAT64 and 6to4 forms of them |
| `DefaultOutboundRequestPolicy` | https only on port 443 by default; `AllowHttp` adds http and port 80 |
| `SsrfBlockedException : HttpRequestException` | Thrown when a request is blocked; `Reason` (`SsrfBlockReason`), `RequestUri` and `BlockedAddress`. The message never contains the resolved address, query string or user info (`RequestUri` keeps the full URI). `*AsResultAsync` maps every reason, including `Timeout` and `ResponseTooLarge`, to `ErrorType.Forbidden`, code `Http.SsrfBlocked`, `reason` metadata. A limit or timeout hit while buffering the body can surface as an `HttpRequestException` whose `InnerException` is the `SsrfBlockedException`; check the `InnerException` chain |
| `SsrfBlockReason` | `RequestNotAllowed`, `HostNotAllowed`, `AddressNotAllowed`, `RedirectLimitExceeded`, `ResponseTooLarge`, `Timeout` |

---

## 9. CSharpEssentials.Resilience: Transient Fault Handling

**What it is:** HTTP-agnostic resilience patterns (Retry, Timeout, Circuit Breaker, Fallback) with `Result<T>` integration. Composable `ResiliencePolicy` builder backed by Polly v8.

**Why it exists:** Transient faults are inevitable in distributed systems. This package provides a clean, composable API for handling retries, timeouts, circuit breakers, and fallbacks without coupling to any specific transport (HTTP, database, message queue, etc.).

### Quick Start

```csharp
using CSharpEssentials.Resilience;

// Simple retry
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .ExecuteAsync(_ => _db.GetUser(id));

// Retry + Timeout
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(_ => _orderService.GetOrder(id));

// Circuit Breaker + Fallback (fallback needs the typed policy)
Result<Product> product = await ResiliencePolicy<Product>
    .Create()
    .WithCircuitBreaker(minimumThroughput: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(_ => _productService.GetProduct(id));
```

### ResiliencePolicy

| Method | What It Does |
|--------|-------------|
| `ResiliencePolicy.Create()` | Creates an empty policy (`default(ResiliencePolicy)` behaves the same) |
| `ResiliencePolicy.Create(options)` | Builds from `ResiliencePolicyOptions` |
| `ResiliencePolicy.Create(Action<ResiliencePipelineBuilder>)` | Configures the Polly builder directly |
| `ResiliencePolicy.FromPipeline(pipeline)` / `.ToPipeline()` | Wraps or exposes a Polly `ResiliencePipeline` |
| `.WithRetry(maxAttempts = 3, delay, exponentialBackoff = true)` / `.WithRetry(RetryOptions)` | Adds retry strategy |
| `.WithTimeout(timeout)` / `.WithTimeout(TimeoutOptions)` | Adds timeout strategy |
| `.WithCircuitBreaker(minThroughput, samplingDuration, breakDuration, failureRatio)` / `.WithCircuitBreaker(CircuitBreakerOptions)` | Adds circuit breaker |
| `.ExecuteAsync(ct => Task)` / `.ExecuteAsync(ct => Task<Result>)` | Executes through the pipeline, returns `Result` |
| `.ExecuteAsync<T>(ct => Task<T>)` / `.ExecuteAsync<T>(ct => Task<Result<T>>)` | Executes typed action, returns `Result<T>` |

The non-generic policy has no `WithFallback`; use `ResiliencePolicy<T>`. Retry handles exceptions and failed `Result`s with the same rule as the typed policy (below), so a `Func<CancellationToken, Task<Result>>` that returns a retryable failure is retried. The package depends on `Polly.Core` `[8.0.0, 9.0.0)`.

### ResiliencePolicy\<T\> (Result-Aware)

The generic variant automatically filters retryable errors: `Unauthorized`, `Forbidden`, `NotFound`, and `Validation` errors are **not** retried.

| Method | What It Does |
|--------|-------------|
| `ResiliencePolicy<T>.Create()` / `Create(Action<ResiliencePipelineBuilder<Result<T>>>)` / `FromPipeline(pipeline)` | Creates a typed policy |
| `.WithRetry(...)` | Adds retry with Result error filtering |
| `.WithTimeout(...)` | Adds timeout |
| `.WithCircuitBreaker(...)` | Adds circuit breaker with Result error filtering |
| `.WithFallback(fallbackAsync)` | Adds fallback that returns `T` or `Result<T>` |
| `.ExecuteAsync(action)` | Executes through pipeline, returns `Result<T>` |

### Delegate Extensions

```csharp
// Direct execution — wraps any Func<Task<T>> in a Result
Func<Task<User>> load = () => _db.GetUser(id);
Result<User> user = await load.ExecuteAsync();

// With CancellationToken
Func<CancellationToken, Task<User>> loadWithToken = ct => _db.GetUser(id, ct);
Result<User> sameUser = await loadWithToken.ExecuteAsync(cancellationToken);
```

### Retry Extensions

```csharp
Func<CancellationToken, Task<Result<User>>> getUser = ct => _db.GetUser(id, ct);
Result<User> result = await getUser.RetryIfFailed(maxAttempts: 3);

// Custom retry predicate: retry only the errors you choose
Result<User> retried = await getUser.RetryIfFailed(
    shouldRetry: error => error.Type == ErrorType.Unexpected,
    maxAttempts: 5,
    delay: TimeSpan.FromMilliseconds(200));
```

| Method | What It Does |
|--------|-------------|
| `func.RetryIfFailed(maxAttempts = 3, delay, exponentialBackoff = true, ct)` | Retries a `Func<CancellationToken, Task<Result<T>>>` or `Task<Result>` while the result is a retryable failure; returns `ValueTask<Result<T>>` / `ValueTask<Result>` |
| `func.RetryIfFailed(shouldRetry, maxAttempts = 3, delay, exponentialBackoff = true, ct)` | Same, with a `Func<Error, bool>` deciding which failures are retried |
| `func.ExecuteAsync(ct)` | Runs a `Func<Task>`, `Func<Task<T>>`, `Func<Task<Result<T>>>` (or their `CancellationToken` forms) and returns `Result` / `Result<T>` |

### Error Handling

| Error Code | When |
|-----------|------|
| `Resilience.Timeout` | Operation exceeded timeout |
| `Resilience.CircuitBroken` | Circuit breaker is open |

When retries exhaust, the last exception is returned as `ErrorType.Unexpected`. Both are created with `Error.Failure`.

Cancellation: when the caller's `CancellationToken` is cancelled, `ExecuteAsync` and `RetryIfFailed` throw `OperationCanceledException` instead of returning a failed `Result`. This also applies when the cancellation lands during a retry delay or right after the last attempt.

### Configuration Options

```csharp
var options = new ResiliencePolicyOptions
{
    Retry = new RetryOptions { MaxAttempts = 3, Delay = TimeSpan.FromSeconds(1) },
    Timeout = new TimeoutOptions { Timeout = TimeSpan.FromSeconds(5) },
    CircuitBreaker = new CircuitBreakerOptions
    {
        MinimumThroughput = 10,
        FailureRatio = 0.5,
        BreakDuration = TimeSpan.FromSeconds(30)
    }
};

Result<User> user = await ResiliencePolicy
    .Create(options)
    .ExecuteAsync(_ => _db.GetUser(id));
```

---

## 10. CSharpEssentials.EntityFrameworkCore: EF Core Integration

**What it is:** EF Core extensions that bring the Result pattern to database operations, plus pagination, audit interceptors, and CQRS context separation.

**Why it exists:** EF Core returns `null` from queries and throws on save failures. This package wraps those operations in `Result<T>`, provides automatic audit field population, and supports separating read/write contexts for CQRS architectures.

### Result-Returning Query Extensions

| Method | What It Does | Replaces |
|--------|-------------|----------|
| `FirstOrDefaultAsResultAsync<T>` | Returns `Result<T>` (NotFound on null) | `FirstOrDefaultAsync` + null check |
| `SingleOrDefaultAsResultAsync<T>` | Returns `Result<T>` (NotFound on null) | `SingleOrDefaultAsync` + null check |
| `FindAsResultAsync<T>` | Returns `Result<T>` from `Find` | `FindAsync` + null check |
| `SaveChangesAsResultAsync` | Returns `Result` wrapping save | try/catch around `SaveChangesAsync` |
| `MigrateDataAsync<TEntity, TSeedData>(data, preCondition, converter)` | Seeds data rows when a precondition holds (returns `Task`) | Hand-written seeding code |

### Pagination

| Method | What It Does |
|--------|-------------|
| `PaginateAsync(query, request)` | Offset-based pagination → `PaginationResponse<T>` |
| `PaginateAsync(query, pageNumber, pageSize, includeTotalCount = true)` | Same, without building a `PaginationRequest` |
| `Paginate(query, request)` / `Paginate(query, pageNumber, pageSize)` | Synchronous offset pagination; also works on non-EF `IQueryable` (e.g. `list.AsQueryable()`) |
| `PaginateAsync(query, cursorRequest, cursorSelector, isAscending = true, search = null, thenBy = null)` | Single-column cursor pagination → `CursorPaginationResponse<T, TCursor>`; the cursor is sent as a SQL parameter and the column must be unique |
| `KeysetPaginateAsync(query, request, k => k.Descending(...).Ascending(...), ct)` | Composite keyset pagination → `Result<KeysetPaginationResponse<T>>`; reads `limit + 1` rows, no `COUNT`; bad cursors return `Error.Validation` |
| `KeysetPaginateAsync(query, request, keys, options, ct)` / `(query, request, ordering[, options], ct)` | Same, with `KeysetPaginationOptions` or a reusable `KeysetOrdering<T>` |

`Normalize()` on `IPaginationRequest` and `ICursorPaginationRequest<TCursor>` only enforces minimums; it does not cap the page size or limit. Opt in to a cap with `Normalize(int maxPageSize)` / `Normalize(int maxLimit)` (default interface methods): a larger value is lowered to the cap, like `KeysetPaginationOptions.MaxLimit`; a cap below 1 throws `ArgumentOutOfRangeException`. These are interface members, so call them through the interface type.

#### Keyset Pagination Types

Namespaces: `CSharpEssentials.EntityFrameworkCore.Pagination` (extensions), `.Pagination.Requests`, `.Pagination.Responses`, `.Pagination.Keyset`.

| Type | Members |
|------|---------|
| `KeysetPaginationRequest : IKeysetPaginationRequest` | `Limit` (default `10`), `After`, `Before` (both set → `KeysetPagination.AfterAndBefore` error) |
| `KeysetPaginationResponse<T>` | `Items`, `NextCursor`, `PreviousCursor`, `HasNext`, `HasPrevious` |
| `KeysetOrdering<T>` | Immutable; `Ascending(x => x.Key)`, `Descending(x => x.Key)`, `Count`. Keys must be non-nullable member accesses (no nullable navigation on the path) of a supported type, otherwise `ArgumentException`; enums stored as strings cannot be keys |
| `KeysetDirection` | `Ascending`, `Descending` |
| `KeysetPaginationOptions` (record) | `MaxLimit` (default `KeysetPaginationOptions.DefaultMaxLimit` = 100, `1..int.MaxValue - 1`), `MaxCursorLength` (default `DefaultMaxCursorLength` = 2048; longer cursors → `InvalidCode`), `Protector` (default `NoOpCursorProtector.Instance`), `PredicateBuilder` (default `KeysetPredicateBuilder.Instance`); `Default` |
| `ICursorProtector` | `string Protect(string cursor)`, `bool TryUnprotect(string protectedCursor, out string cursor)`; wrap `IDataProtector` to sign/encrypt cursors |
| `NoOpCursorProtector` | Default protector; returns the cursor unchanged |
| `IKeysetPredicateBuilder` | `Expression BuildPredicate(IReadOnlyList<KeysetColumn> columns)`; returns a `bool` expression |
| `KeysetPredicateBuilder` | Default; `a <= @a AND (a < @a OR (a = @a AND b > @b))` with a redundant leading-column condition |
| `KeysetColumn` | `Column`, `Value` (parameter expression), `Type`, `Direction` (effective, reversed for `Before` pages) |
| `KeysetCursorErrors` | Error codes `InvalidCode`, `UnsupportedVersionCode`, `DirectionMismatchCode`, `KeyMismatchCode`, `AfterAndBeforeCode`; metadata key `ParameterKey` (`"parameter"`, value `after`/`before`) |

Cursor format: base64url (no padding) of `{"v":1,"d":"a"|"b","k":"<key fingerprint>","p":[values]}`, then `ICursorProtector.Protect`. Supported key types: `int`, `long`, `short`, `byte`, `decimal`, `double`, `float`, `string`, `Guid`, `DateTime` (`Kind` preserved), `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly`, enums (not `ulong`-based). The provider must also translate ordering and comparison of the key type (SQLite does not for `decimal`, `DateTimeOffset`, `TimeSpan`).

### Batch Operations

| Method | What It Does |
|--------|-------------|
| `SoftDeleteAsync(query, deletedAt, deletedBy)` | Soft-deletes all matching `ISoftDeletable` rows in one `UPDATE` (`ExecuteUpdate`); skips rows already deleted; returns the affected row count |
| `SoftDeleteAsync(query, deletedBy, timeProvider = null)` | Same, taking the time from `TimeProvider` (default `TimeProvider.System`) |
| `HardDelete(entities)` | Marks tracked soft-deletable entities as hard-deleted and removes them on the next `SaveChanges` |

Batch updates bypass the change tracker and `SaveChanges` interceptors (audit, domain events), and require a relational provider.

### Database Error Translation

Namespace `CSharpEssentials.EntityFrameworkCore.DbErrors`.

| Member | What It Does |
|--------|-------------|
| `IDbErrorTranslator.TryTranslate(exception, out error)` | Provider hook: returns `true` and an `Error` when it recognizes the exception |
| `SqlStateErrorTranslator` | Default translator over `DbException.SqlState` (searches inner exceptions): `23505`/`23503` → `Conflict`, `23514`/`23502` → `Validation`, `40001`/`40P01` → `Conflict` with `retryable: true`. Metadata: `sqlState`, `entities` |
| `DbErrorTranslation.TryTranslate(exception, out error)` | Tries registered translators in registration order, then `SqlStateErrorTranslator`; first match wins |
| `DbErrorTranslation.SaveChangesAsync(context, ct)` | `SaveChangesAsync` returning `Result<int>`; recognized exceptions become the translated error, others are rethrown |
| `AddDbErrorTranslation()` | Registers `DbErrorTranslation` as a singleton |
| `AddDbErrorTranslator<TTranslator>()` | Adds a singleton `IDbErrorTranslator` (once per type) and `DbErrorTranslation` |

### Entity Configuration

| Method | What It Does |
|--------|-------------|
| `EntityBaseMap()` | Configures audit field mappings for `EntityBase` |
| `EntityBaseGuidIdMap()` | Configures `Guid` ID mapping |
| `SoftDeletableEntityBaseMap()` | Configures soft-delete field mappings |
| `ApplySoftDeleteQueryFilter()` | Adds global `IsDeleted == false` filter |
| `MaybeConversion<T>()` | EF value conversion for `Maybe<T>` properties |
| `HasJsonConversion<T>()` | Stores complex properties as JSON |

### Named Query Filters (net10.0, EF Core 10)

| Member | What It Does |
|--------|-------------|
| `QueryFilterNames.SoftDelete`, `QueryFilterNames.Tenant` | Filter name constants (`"SoftDelete"`, `"Tenant"`) |
| `HasSoftDeleteQueryFilter<TEntity>()` | `EntityTypeBuilder<TEntity>` extension (`TEntity : ISoftDeletableBase`): adds the `!IsDeleted` filter named `SoftDelete` |
| `ApplyNamedSoftDeleteQueryFilter()` | `ModelBuilder` extension: adds the named `SoftDelete` filter to every root entity type that implements `ISoftDeletableBase`; skips owned types; a soft-deletable type under a root that is not soft-deletable gets no filter |
| `IgnoreSoftDeleteQueryFilter<TEntity>()` | `IQueryable<TEntity>` extension: `IgnoreQueryFilters` with a cached `[QueryFilterNames.SoftDelete]` array, other named filters still apply |

EF Core rejects anonymous and named filters on the same entity type, so use these instead of `ApplySoftDeleteQueryFilter()`, not next to it; they throw `InvalidOperationException` when the entity type already has an anonymous filter. Named keys do not switch off the anonymous filter, so move the model to the named filter before the queries. Pass keys in a `static readonly` array or `new[] { ... }`: EF Core 10.0.x recompiles the query when they come from a collection expression or a `List`. Analyzer CSE3001 (Info; raise with `dotnet_diagnostic.CSE3001.severity = warning`) reports parameterless `IgnoreQueryFilters()` when the referenced EF Core has the named overload; its code fix passes `new[] { QueryFilterNames.SoftDelete }` for entities that implement `ISoftDeletableBase`.

### Enum Conventions

| Method | What It Does |
|--------|-------------|
| `ConfigureEnumConventions(EnumConventions? conventions = null, EnumStoredAs? existingStorage = null)` | Stores `[StringEnum]` enums by wire name (or integer per `EnumStorage`) and adds `ck_{table}_{column}_enum` check constraints. `existingStorage` keeps existing columns in their current format |
| `ConfigureEnumConventionsWithReflection(...)` | Same, and also handles enums without generated metadata through reflection (`[RequiresUnreferencedCode]`, `[RequiresDynamicCode]`) |
| `HasEnumStorage(EnumStorage)` | Per-property `String` or `Integer` storage; wins over `[StringEnum(Storage)]` and `EnumConventions.Storage` |
| `HasLegacyEnumStorage(EnumStoredAs)` | Keeps writing an old format (`Integer`, `MemberName`, `CamelCase`, `LegacySnakeCase`, `FlagsText`), reads every spelling, no check constraint |
| `HasEnumCheckConstraint(bool)` | Turns the property's check constraint off or on |
| `ConfigureEnumConventions(params Assembly[])`, `ConfigureEnumConventions(Action<EnumConventionOptions>, params Assembly[])` | Obsolete 4.x forwarders; assemblies are ignored |

| Type | Notes |
|---|---|
| `EnumWireNameConverter<TEnum>` | String storage: canonical writes, tolerant reads, `[EnumFallback]` for unknown values |
| `EnumIntegerConverter<TEnum, TNumber>` | Integer storage in the underlying type, with the defined check |
| `EnumJsonValueReaderWriter<TEnum>` | Enum properties inside `ToJson()` columns |
| `EnumStoredAs` | Format an existing column holds |

```csharp
configurationBuilder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer);
modelBuilder.Entity<Order>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);
```

Properties with your own `HasConversion` are skipped. See the EF Core package README for column types per provider.

### Enum Migration Helpers

| Method | What It Does |
|--------|-------------|
| `migrationBuilder.ConvertEnumColumn<TEnum>(table, column, EnumStoredAs from, EnumStorage to, schema?, type?)` | Replaces EF's generated `AlterColumn`: changes the column to text (wire names) or the integer type and converts the data in one step. Run between `DropCheckConstraint` and `AddCheckConstraint` |
| `migrationBuilder.ConvertEnumColumn<TEnum>(table, column, EnumStoredAs from, EnumStoredAs to, schema?, type?)` | Same, to a legacy text format (`MemberName`, `CamelCase`, `LegacySnakeCase`, `FlagsText`) or `Integer`; used in `Down()` |
| `migrationBuilder.ConvertEnumJsonPath<TEnum>(table, column, path, schema?, to = EnumStorage.String)` | Rewrites one path of a PostgreSQL `jsonb` column to wire names (or numbers); unknown values stay untouched |
| `EnumDataAudit.Sql<TEnum>(table, column, schema?, storedAs = EnumStoredAs.Text, provider?)` | Read-only `SELECT value, count(*)` of the values a conversion or check constraint would reject |

SQL is generated for PostgreSQL and SQLite per `MigrationBuilder.ActiveProvider`. No conversion writes `NULL`: text targets keep unknown values (the new constraint rejects them), integer targets keep integer text and abort on other text with a message that names the column, the enum and the value. Flags text is OR'ed through a temporary column on PostgreSQL. Analyzer CSE0014 (error) reports a generated `AlterColumn` left next to `ConvertEnumColumn` for the same column.

```csharp
migrationBuilder.DropCheckConstraint(name: "ck_orders_Status_enum", table: "orders");
migrationBuilder.ConvertEnumColumn<OrderStatus>("orders", "Status", from: EnumStoredAs.Integer, to: EnumStorage.String);
migrationBuilder.AddCheckConstraint(name: "ck_orders_Status_enum", table: "orders", sql: "...");
```

### Interceptors

| Method | What It Does |
|--------|-------------|
| `AddAuditInterceptor` | Auto-fills `CreatedAt/By`, `UpdatedAt/By` on SaveChanges |
| `AddSlowQueryInterceptor` | Logs queries exceeding a threshold |

### BaseDbContext\<TContext\>

Opt-in hooks; both are off by default.

| Member | Default | What It Does |
|--------|---------|-------------|
| `protected virtual DbContextInterceptors InterceptorsFromServices` | `None` | `OnConfiguring` attaches the selected interceptors (`Audit`, `DomainEvents`, `SlowQuery`, `All`) when registered in DI; skips ones already on the options |
| `protected virtual bool DispatchDomainEventsOnSaveChanges` | `false` | `SaveChanges`/`SaveChangesAsync` collect domain events, dispatch `BeforeSave` events before saving and `AfterSave` events after a successful save |
| `protected virtual Task DispatchDomainEventsAsync(IReadOnlyList<IDomainEvent>, DomainEventTiming, CancellationToken)` | outbox / publisher | Override to customize dispatch. Default: `AfterSave` → `IDomainEventOutbox` if registered, else `IDomainEventPublisher`; `BeforeSave` → `IDomainEventPublisher` |

Overriding `OnConfiguring` without calling `base.OnConfiguring` disables interceptor attachment. Use either `DispatchDomainEventsOnSaveChanges` or `DomainEventInterceptor`, not both; events collected by the context are cleared before the interceptor runs, so they are never dispatched twice.

### Transaction Runner

| Member | What It Does |
|--------|-------------|
| `ITransactionRunner` (`CSharpEssentials.Core`, namespace `CSharpEssentials.Transactions`) | `ExecuteAsync(work, shouldCommit, ct)`: runs work in a transaction, commits when `shouldCommit` is true, rolls back otherwise or on exception |
| `EfCoreTransactionRunner<TDbContext>` | Outermost call: execution strategy + `BeginTransactionAsync`, retry re-runs the whole unit. Nested call on the same context: joins `CurrentTransaction` behind a savepoint |
| `AddEfCoreTransactionRunner<TDbContext>()` | Registers the runner as the scoped `ITransactionRunner`, replacing an earlier one |

### CQRS Context Registration

| Method | What It Does |
|--------|-------------|
| `AddWriteDbContext` | Registers write-optimized context (tracking enabled) |
| `AddReadDbContext` | Registers read-optimized context (no tracking) |
| `AddCqrsDbContexts` | Registers both read and write contexts |
| `UseAsWriteContext()` | Configures context for write operations |
| `UseAsReadContext()` | Configures context for read operations (no tracking) |

---

## 11. CSharpEssentials.Json: Serialization Defaults

**What it is:** Pre-configured `System.Text.Json` options and custom converters.

**Why it exists:** Every project configures the same JSON settings: camelCase, lenient parsing, enum handling. This package provides sensible defaults and converters for polymorphic types and multi-format dates.

| Member | What It Does |
|--------|-------------|
| `EnhancedJsonSerializerOptions.DefaultOptions` | Pre-configured (camelCase, lenient) |
| `EnhancedJsonSerializerOptions.StrictOptions` | Strict mode options |
| `.DefaultOptionsWithDateTimeConverter` | Options with multi-format date parsing |
| `options.Create(configure)` | Copies options and applies a configuration delegate |
| `CreateOptionsWithConverters(params JsonConverter[])` | Default options plus the given converters |
| `ConvertToJson<T>()` | Extension: serialize to JSON string |
| `ConvertFromJson<T>()` | Extension: deserialize from JSON string |
| `jsonElement.ToClrObject()` | Converts a `JsonElement` to plain CLR values: objects to `Dictionary<string, object?>`, arrays to `List<object?>`, numbers to `int`/`long`/`decimal`/`double`, plus `string`, `bool` and `null` |
| `PolymorphicJsonConverterFactory` | Handles polymorphic serialization |
| `MultiFormatDateTimeConverterFactory` / `MultiFormatDateTimeConverter<T>` | Parses multiple date/time formats |
| `ConditionalStringEnumConverter` | `[Obsolete]` since 5.0; forwards to the enum conventions converter in `Input` mode (undefined numbers are rejected). Use `options.AddEnumConventions(...)` |
| `StringEnumNaming` | Obsolete facade over `EnumMetadata`; the naming source for enum strings, shared by JSON, EF Core, Swagger and query/route binding |
| `options.AddEnumConventions(conventions, mode, writeAs)` | Adds `EnumConverterFactory` for enums with generated metadata at position 0 and removes earlier convention factories. Other converters such as `JsonStringEnumConverter` stay, so plain enums keep the host's output. A `[StringEnum]` enum without generated metadata fails with `InvalidOperationException` instead of becoming a number |
| `EnumConverterFactory.CreateWithReflectionFallback(conventions, mode, writeAs)` | Opt-in factory that also converts enums without generated metadata that `EnumConventions.CanHandle` accepts, with metadata read by reflection. `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]`: not AOT safe |
| `options.AddEnumConventionsWithReflection(conventions, mode, writeAs)` | `AddEnumConventions` with the reflection fallback above. `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]`: opt-in, not AOT safe |
| `factory.UsesReflectionFallback` | `true` for a factory created with `CreateWithReflectionFallback` |

### StringEnumNaming

`[Obsolete]` since 5.0, a working facade over `EnumMetadata`. Use `EnumMetadata`, `EnumValueFormatter`, `EnumValueParser` or the generated `ToWireName()` and `TryParseWire` helpers instead.

Names resolve as `[JsonStringEnumMemberName]` when present, otherwise `JsonNamingPolicy.SnakeCaseLower` (the default policy).

| Member | What It Does |
|--------|-------------|
| `DefaultPolicy` | `JsonNamingPolicy.SnakeCaseLower` |
| `IsStringEnum(Type)` | `true` for enums marked with `[StringEnum]` |
| `GetName<TEnum>(value, policy?)` / `GetName(Type, object, policy?)` | String form of a value (flags joined with `", "`) |
| `GetNames<TEnum>(policy?)` / `GetNames(Type, policy?)` | String forms of all members, in declaration order |
| `TryParse<TEnum>(string?, out result, policy?, allowIntegerValues = true)` / `TryParse(Type, ...)` | Case-insensitive; accepts the policy name, the C# member name and (optionally) the number of a defined member |

```csharp
StringEnumNaming.GetName(HttpKind.HTTPStatus);                        // "http_status"
StringEnumNaming.TryParse<HttpKind>("HTTPStatus", out var kind);      // true
```

---

## 12. CSharpEssentials.AspNetCore: API Layer

**What it is:** ASP.NET Core integration that automatically maps `Result`/`Error` types to proper HTTP responses using ProblemDetails.

**Why it exists:** Without this, every controller action needs boilerplate to convert `ErrorType.NotFound` to `404`, `ErrorType.Validation` to `400`, etc. This package eliminates that mapping code entirely.

### Result to HTTP Mapping

| Method | What It Does |
|--------|-------------|
| `errors.ToProblemResult()` | Converts errors to Minimal API `IResult` |
| `errors.ToActionResult()` | Converts errors to MVC `IActionResult` |
| `errors.ToProblemDetails()` | Converts errors to `ProblemDetails` |
| `ResultEndpointFilter` | Minimal API filter that maps Result to HTTP automatically |
| `GlobalExceptionHandler` | Catches unhandled exceptions, returns ProblemDetails |

`ResultEndpointFilter` returns `200 OK` on success. On failure it returns a ProblemDetails response (3.x: `400` with the raw `Error[]`); a registered `IResultErrorMapper` takes precedence. `ToProblemResult` returns `EnhancedProblemHttpResult` and `ToActionResult` returns `EnhancedProblemObjectResult` (derives from `ObjectResult`); both read the registered options when they execute. `ToProblemDetails` uses default options because it has no request context.

> Upgrading from 3.x? Defaults changed (trace id, error fields, exception mapping, enum names). See [Migrating from 3.x to 4.0](migration/v3-to-v4.md).

### Configuration

| Method | What It Does |
|--------|-------------|
| `AddEnhancedProblemDetails()` | Configures ProblemDetails in DI |
| `AddEnhancedProblemDetails(Action<EnhancedProblemDetailsOptions>)` | Same, with options (below) |
| `AddEnhancedProblemDetails(Action<ProblemDetails, HttpContext>)` | Default options plus a delegate registered as an `IProblemDetailsEnricher` |
| `UseEnhancedProblemDetails()` | Runs `UseExceptionHandler()` and `UseStatusCodePages()` |
| `AddProblemDetailsEnricher<T>(lifetime = Singleton)` | Adds an `IProblemDetailsEnricher` |
| `AddErrorStatusCodeMapper<T>(lifetime = Singleton)` | Replaces the `IErrorStatusCodeMapper` |
| `AddExceptionProblemMapper<T>(lifetime = Singleton)` | Adds an `IExceptionProblemMapper` (tried before the default mapper) |
| `ConfigureModelValidatorResponse()` | Model validation errors as ProblemDetails |
| `ConfigureInvalidModelStateResponse(Func<string, ModelError, Error>? = null)` | `[ApiController]` automatic 400 as an enhanced problem (code = model state key) |
| `ConfigureSystemTextJson()` | Configures JSON serialization |

### EnhancedProblemDetailsOptions

All problem responses (Minimal API, MVC, `GlobalExceptionHandler`, status code pages, framework 404/405) go through `IProblemDetailsService` and share this configuration.

| Option | Default | Values / Notes |
|--------|---------|----------------|
| `TraceId` | `TraceIdFormat.W3CTraceId` | `TraceparentHeader` (3.x), `None` |
| `IncludeRequestId` | `false` | Writes `requestId` |
| `IncludeUser` | `false` | Writes `user` |
| `IncludeSpanIds` | `false` | Writes `spanId` / `parentSpanId` |
| `Instance` | `ProblemInstanceFormat.Path` | `MethodAndPath` (3.x), `None` |
| `ErrorFields` | `Codes \| ValidationErrors` | `ProblemErrorFields`: `None`, `Codes`, `ValidationErrors`, `Messages`, `AllErrors`, `All` |
| `ValidationErrorsFormat` | `List` | `Dictionary` groups descriptions by code |
| `TypeUriResolver` | `ProblemTypeUris.Rfc9110` | `Func<int, string?>`; `ProblemTypeUris.Rfc7231` restores 3.x |
| `ExposeExceptionDetails` | `false` | Writes an `exception` extension; enable only in Development |
| `UseLegacyDefaults()` | | Restores the 3.x output (everything above except `ExposeExceptionDetails`) |

```csharp
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.IncludeRequestId = true;
    o.ErrorFields |= ProblemErrorFields.Messages;
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

app.UseEnhancedProblemDetails();
```

### Extension Points

| Interface | Purpose | Register with |
|-----------|---------|---------------|
| `IProblemDetailsEnricher` | `void Enrich(ProblemDetailsContext)`: add or change fields on every problem response | `AddProblemDetailsEnricher<T>()` |
| `IErrorStatusCodeMapper` | `GetStatusCode(Error)`, `SelectPrimaryError(IReadOnlyList<Error>)`, `GetTitle(Error, int)`: status/title for error-based problems (derive from `DefaultErrorStatusCodeMapper` to override part) | `AddErrorStatusCodeMapper<T>()` |
| `IExceptionProblemMapper` | `bool TryMap(HttpContext, Exception, out ExceptionProblem?)`: first mapper returning `true` wins | `AddExceptionProblemMapper<T>()` |

`ExceptionProblem(int? StatusCode, string? Title, string? Detail, IReadOnlyList<Error>? Errors)` is the result of a mapper.

### GlobalExceptionHandler Mapping

`DefaultExceptionProblemMapper` runs after any registered mapper:

| Exception | Response |
|-----------|----------|
| `OperationCanceledException` | 499 |
| `BadHttpRequestException` | its own status code |
| `EnhancedValidationException` | status from its errors (validation: 400) |
| `DomainException` | status of its `ErrorType` (3.x: always 400) |
| Anything else | 500, message logged only |

`InvalidOperationException`, `ApplicationException` and `ValidationException` no longer return 400; register an `IExceptionProblemMapper` to restore that.

### Enum Conventions and Binding

| Method | What It Does |
|--------|-------------|
| `AddEnumConventions(Func<EnumConventions, EnumConventions>? = null)` | Registers `EnumConventions`, applies them to Minimal API and MVC `JsonOptions` (Input mode), maps JSON body enum errors to 400, adds the wire format filters. Returns `EnumConventionsBuilder` |
| `AddEnumConventionsWithReflection(...)` | Same, and handles enums without generated metadata that `CanHandle` selects, by reflection (`[RequiresUnreferencedCode]`, `[RequiresDynamicCode]`) |
| `EnumConventionsBuilder.ConfigureErrors(Func<EnumValueError, string, Error>)` | Creates the `Error` of a rejected value (the string is the key) for binding and JSON bodies |
| `UseEnumBinding()` | Middleware that normalizes enum route, query, header and form values before binding (Minimal API incl. `[AsParameters]`, and MVC). Throws when `AddEnumConventions` was not called |
| `WithEnumWireFormat(EnumWireFormat)` | Output format of an endpoint or group (`IEndpointConventionBuilder`) |
| `WithEnumWireFormat(string header, Func<HttpContext, EnumWireFormat>)` | Output format per request; adds `Vary: header` |
| `[EnumWireFormat(EnumWireFormat)]` | Output format of an MVC controller or action |
| `AddEnumBinding(Action<EnumBindingOptions>? = null)` | **Obsolete.** Forwards to `AddEnumConventions` (`CanBind` → `CanHandle`, `AllowIntegerValues` → `AcceptNumbers`, `ErrorFactory` → `ConfigureErrors`; a naming policy other than snake_case throws) |

`[StringEnum]` enums accept the same spellings as a JSON body (wire name, alias, C# member name, and with `AcceptNumbers` the number of a defined member; flags enums: comma-separated list; arrays: repeated keys or comma-separated values). Invalid values return a 400 ProblemDetails response listing the allowed values. Call `UseEnumBinding` after routing selected the endpoint. Output precedence: action attribute > controller attribute > endpoint/group > `EnumConventions.WriteAs`.

### Idempotency

| Member | What It Does |
|--------|-------------|
| `AddIdempotency(Action<IdempotencyOptions>? = null)` | Registers the options and a store (the in-memory store unless one is already registered) |
| `UseIdempotency()` | Middleware; call after `UseRouting`, authentication and authorization |
| `WithIdempotency()` | Adds `IdempotentAttribute` metadata to an endpoint or group (`IEndpointConventionBuilder`) |
| `[Idempotent]` | Same for an MVC controller or action |
| `IIdempotencyStore` | `TryReserveAsync(key, fingerprint, inFlightTimeout)`, `CompleteAsync(key, token, fingerprint, IdempotentResponse, retention)`, `ReleaseAsync(key, token)` (in-flight entries only). Complete and Release return `false` and change nothing when another reservation holds the key |
| `IdempotencyReservation` | `Status` (`Reserved`, `InFlight`, `Completed`, `FingerprintMismatch`), the `Token` when reserved, and the stored `Response` when completed |
| `IdempotentResponse(int StatusCode, IReadOnlyDictionary<string, string[]> Headers, ReadOnlyMemory<byte> Body)` | A stored response |
| `InMemoryIdempotencyStore` / `DistributedCacheIdempotencyStore` | Built-in stores; the in-memory one is unbounded, the distributed one is best effort (no atomic add or compare) |

| Option | Default | Notes |
|--------|---------|-------|
| `HeaderName` / `ReplayedHeaderName` | `Idempotency-Key` / `Idempotency-Replayed` | |
| `Methods` | POST, PATCH | Case-insensitive set |
| `InFlightTimeout` | 1 minute | Frees the key of a request that never finished; keep it longer than the request timeout |
| `RetentionPeriod` | 24 hours | How long a completed response is replayed |
| `RetryAfter` | 1 second | `Retry-After` on 409; `null` omits it |
| `MaxKeyLength` | 255 | Longer keys return 400 |
| `MaxResponseBodySize` | 1 MiB | Larger bodies are not stored (warning logged) |
| `RequireKey` | `false` | `true`: a missing key returns 400 |
| `ShouldStore` | 2xx | Other statuses release the key |
| `KeyScope` | `DefaultKeyScope` (`NameIdentifier`, then `sub`) | `null` scope passes through unless `AllowUnscopedKeys` |
| `AllowUnscopedKeys` | `false` | |
| `ReplayedHeaders` | `Content-Type`, `Content-Language`, `Location`, `ETag`, `Cache-Control` | `Set-Cookie` is never stored; `Content-Encoding`/`Vary` left out so response compression outside the middleware re-encodes replays |
| `UseInMemoryStore()` / `UseDistributedCacheStore()` / `UseStore<T>(lifetime = Scoped)` | | Replace the registered store |

Responses: replay of the stored response (`Idempotency-Replayed: true`), 409 while the first request runs, 422 when the key was used for a different method, path, query or body, 400 for an invalid key. Invalid options throw in `AddIdempotency`.

### Conditional requests

| Member | What It Does |
|--------|-------------|
| `AddConditionalRequests(Action<ConditionalRequestOptions>? = null)` | Registers the options, the default `IETagGenerator` (unless one is registered) and the MVC filters. Calling it again only replaces the options |
| `AddETagSource<T, TSource>()` | Registers `TSource : IETagSource<T>` (scoped). Used for values whose runtime type is `T` or derives from it; wins over `IVersioned` and `IETagGenerator`. Throws `ArgumentException` when `T` is an interface |
| `WithConditionalGet()` / `[ConditionalGet]` | `ETag`/`Last-Modified` and 304 for `GET`/`HEAD` on an endpoint, group, MVC controller or action |
| `WithIfMatch(bool required = false)` / `[IfMatch(Required = ...)]` | Parses `If-Match` on `POST`/`PUT`/`PATCH`/`DELETE` (safe methods are not evaluated; `If-Unmodified-Since` is not supported): 400 when malformed, 428 when missing and required. An endpoint overrides its group; action > controller > endpoint for MVC |
| `WithIfMatch<TBuilder, T>(Func<HttpContext, CancellationToken, ValueTask<T?>> loadCurrent, bool required = false)` | Also loads the current resource and returns 412 before the handler when it is missing or does not match. Not atomic. `T : class`. A weak ETag (body hash) never matches, so only `*` passes |
| `HttpContext.GetPreconditions()` | The parsed `Preconditions`; throws without `WithIfMatch`/`[IfMatch]` |
| `IVersioned` | `string Version`: strong ETag `"{Version}"`; invalid ETag characters are hashed (base64url SHA-256); empty: no ETag |
| `IVersioned.ToETag()` | The ETag the default generator sends for the resource (`EntityTagHeaderValue?`) |
| `IETagSource<in T>` | `ResourceValidators? GetValidators(T value)` |
| `IETagGenerator` | `ResourceValidators? GetValidators(object value)`: fallback for values without a source |
| `ResourceValidators(EntityTagHeaderValue? ETag, DateTimeOffset? LastModified)` | The validators of a resource |
| `ConditionalRequestErrors.PreconditionFailed` | Conflict `Error` with code `Http.PreconditionFailed`; `DefaultErrorStatusCodeMapper` maps it to 412 |

| `Preconditions` member | Notes |
|------------------------|-------|
| `HasIfMatch` / `IsWildcard` / `IfMatch` | The parsed header; empty for safe methods |
| `Matches(IVersioned?)` / `Matches(ResourceValidators?)` / `Matches(EntityTagHeaderValue?)` | `true` without `If-Match`; strong comparison, weak tags never match; `*` matches an existing resource |
| `TryGetIfMatchVersion(out string version)` | The opaque tag of a single strong ETag; pass it as the original concurrency token for an atomic update. Equals `IVersioned.Version` only when the version is a valid entity tag (`uint xmin`, `long` revision); otherwise use `Matches(IVersioned?)` |

| Option | Default | Notes |
|--------|---------|-------|
| `UseBodyHashFallback` | `false` | `true`: values without a source or `IVersioned` get a weak ETag from the SHA-256 of their JSON (host `JsonOptions`, also for MVC). Only buffered values: `IAsyncEnumerable<T>`, `Stream`, `PipeReader` and `IQueryable` get none; a lazy `IEnumerable<T>` is enumerated twice |

Reads: `If-None-Match` uses weak comparison and takes precedence over `If-Modified-Since`; an unparsable date is ignored. `Last-Modified` has second precision and is capped at now (`TimeProvider` from DI, else the system clock). A 304 has no body and keeps the headers already on the response. A response whose handler already set `ETag` is left untouched. Strings (also `Result<string>`), `ProblemDetails`, `null`, failed `Result<T>` and non-2xx results pass through. Works with `ResultEndpointFilter` in either order. A custom `IErrorStatusCodeMapper` that does not derive from `DefaultErrorStatusCodeMapper` must map `PreconditionFailedCode` to 412 itself; a registered `IResultErrorMapper` decides the `ResultEndpointFilter` response.

### API Versioning

| Method | What It Does |
|--------|-------------|
| `AddAndConfigureApiVersioning()` | Registers API versioning services |
| `CreateVersionSet(version = 1)` | Creates version set for Minimal APIs |
| `CreateVersionedGroup(route, version = 1)` | Creates versioned route group |
| `MapVersionedGroup(version)` | `MapGroup("v{version:apiVersion}")` with a version set for `version`; works for any endpoints, including a `CSharpEssentials.Endpoints` registry (`app.MapVersionedGroup(2).MapAppsEndpoints()`) |
| `AddSwagger()` / `UseVersionableSwagger()` | Swagger with version support (5.0: in `CSharpEssentials.AspNetCore.Swashbuckle`, same namespace) |

### OpenAPI Enum Schemas (5.0)

Two packages describe the enums the way the enum conventions write them; a host references one of them, never both (Microsoft.OpenApi 2.x would replace the 1.x that Swashbuckle 8/9 needs).

| Package | Method | Targets |
|---------|--------|---------|
| `CSharpEssentials.AspNetCore.OpenApi` | `services.AddOpenApi(o => o.AddEnumConventions())` (`OpenApiOptions`) | net10.0; `Microsoft.AspNetCore.OpenApi` 10.x, `Microsoft.OpenApi` 2.x (net11.0 with Microsoft.OpenApi 3.x later, non-breaking) |
| `CSharpEssentials.AspNetCore.Swashbuckle` | `AddSwaggerGen(o => o.AddEnumConventions())` (`SwaggerGenOptions`); `AddSwagger` calls it | net8.0 to net11.0; Swashbuckle 8.x/9.x, `Microsoft.OpenApi` 1.x |

Both produce the same enum schemas (shared golden files): one component per enum with the wire names in `enum`, `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` and a value table appended to the description (deprecated members marked, the fallback member marked `Response only`). `default` is the wire name. Flags are arrays with `uniqueItems`; nullable is written where the enum is used (`allOf` + `nullable` in OpenAPI 3.0, `oneOf` with `type: null` in 3.1). A document whose operations all write numbers describes integers; a mixed document describes strings and marks the number operations with `x-enum-wire-format: number`; header selected operations get `x-enum-wire-format-header` and a note. Enums without `[StringEnum]` or metadata keep the framework schema.

---

## 13. CSharpEssentials.Mediator: Pipeline Behaviors

**What it is:** Pipeline behaviors for the source-generated [Mediator](https://github.com/martinothamar/Mediator) library (`Mediator.Abstractions`) for cross-cutting concerns: validation, logging, exception handling, caching, and transactions.

**Why it exists:** CQRS handlers often need the same cross-cutting logic: validate input, log execution, convert exceptions to Result failures, cache results, wrap in a transaction. Pipeline behaviors apply these concerns declaratively via marker interfaces rather than repeating code in every handler.

### Behaviors

| Behavior | Marker Interface | What It Does |
|----------|-----------------|-------------|
| `ValidationBehavior` | None (auto for all); `IValidationModeOverride` to pick a mode per request | Runs CSharpEssentials.Validation before handler; returns `Result.Failure` with validation errors in `Enforce` mode, continues in `LogOnly`, skips in `Off`; notifies `IValidationFailureObserver`s |
| `LoggingBehavior` | `ILoggableRequest` | Logs request/response details |
| `ExceptionHandlingBehavior` | None (auto for `Result` / `Result<T>`) | Catches handler exceptions; converts to `Result.Failure(Error.Exception(ex))`; `OperationCanceledException` always propagates |
| `CachingBehavior` | `ICacheable` | Caches handler responses using `CacheKey` and `Expiration` (`BypassCache`, `CacheFailures` control the lookup) |
| `TransactionScopeBehavior` | `ITransactionalRequest` | Wraps handler execution in `TransactionScope`; completes only when the `Result` succeeds |
| `TransactionBehavior` | `ITransactionalRequest` | Runs the handler through the registered `ITransactionRunner`; commits only when the `Result` succeeds. Replaces `TransactionScopeBehavior` when registered |
| `LockBehavior` | `ILockedRequest` | Holds the `IResourceLock` lock for `LockKey` while the handler runs; waits up to `LockTimeout` (unbounded when `null`) and throws `TimeoutException` when it cannot acquire (a failed `Result` behind `ExceptionHandlingBehavior`). Registered only by `AddMediatorLockBehavior` |

### ExceptionHandlingBehavior

Singleton behavior that sits between `LoggingBehavior` and `CachingBehavior`. No interface or attribute needed. It activates automatically when `TResponse` is `Result` or `Result<T>`. Handlers returning other types pass through with zero overhead.

`Error.Exception(ex)` shape:

| Property | Value |
|----------|-------|
| `ErrorType` | `Failure` |
| `Code` | Exception type name (e.g. `"InvalidOperationException"`) |
| `Description` | Exception message |

```csharp
// Registration
builder.Services.AddMediatorExceptionHandlingBehavior(); // singleton

// Handler — no try/catch needed; exceptions become Result.Failure
public record ProcessPaymentCommand(Guid OrderId, decimal Amount)
    : ICommand<Result>;

public class ProcessPaymentHandler : ICommandHandler<ProcessPaymentCommand, Result>
{
    private readonly IPaymentGateway _paymentGateway;

    public ProcessPaymentHandler(IPaymentGateway paymentGateway)
        => _paymentGateway = paymentGateway;

    public async ValueTask<Result> Handle(ProcessPaymentCommand command, CancellationToken ct)
    {
        // Unhandled exceptions are caught by ExceptionHandlingBehavior
        // and returned as Result.Failure(Error.Exception(ex))
        await _paymentGateway.ChargeAsync(command.OrderId, command.Amount, ct);
        return Result.Success();
    }
}

// Caller — stays on the Result railway; no try/catch required
Result result = await mediator.Send(new ProcessPaymentCommand(orderId, 99.99m));
if (result.IsFailure)
{
    // result.Error.Code        => "HttpRequestException"
    // result.Error.Description => "Payment gateway timed out"
}
```

### Registration

| Method | What It Does |
|--------|-------------|
| `AddMediatorBehaviors()` | Registers all five behaviors |
| `AddMediatorValidationBehavior()` | Registers validation only |
| `AddMediatorValidationBehavior(configure)` | Registers validation and sets `ValidationBehaviorOptions.DefaultMode` |
| `AddMediatorValidationOptions(configure?)` | Registers `ValidationBehaviorOptions` and the default `LoggingValidationFailureObserver` (use with `DefaultPipelineBehaviors` under Native AOT) |
| `AddMediatorLoggingBehavior()` | Registers logging only |
| `AddMediatorExceptionHandlingBehavior()` | Registers exception handling only (singleton) |
| `AddMediatorCachingBehavior()` | Registers caching only |
| `AddMediatorTransactionBehavior()` | Registers `TransactionScopeBehavior`, replacing `TransactionBehavior` in place |
| `AddMediatorTransactionRunnerBehavior()` | Registers `TransactionBehavior` (scoped), replacing `TransactionScopeBehavior` in place; needs an `ITransactionRunner` |
| `AddMediatorLockBehavior(placement)` | Registers `LockBehavior` (scoped) just before (`LockPlacement.OutsideTransaction`, default) or just after (`InsideTransaction`) the transaction behavior, and `InProcessResourceLock` as the singleton `IResourceLock` unless one is registered |

### Resource Locks

| Type | Description |
|------|-------------|
| `IResourceLock` (namespace `CSharpEssentials.Locking`) | `AcquireAsync(key, timeout, ct)` waits for the lock and returns a handle that releases it on dispose (throws `TimeoutException`); `TryAcquireAsync(key, ct)` returns `Maybe<IAsyncDisposable>.None` when the key is held |
| `InProcessResourceLock` | Default: one `SemaphoreSlim` per key, removed when the last holder or waiter leaves. Serializes only within one process; register a distributed implementation for several instances |
| `ILockedRequest` | `LockKey` and optional `LockTimeout` (default `null`) |
| `LockPlacement` | `OutsideTransaction` (lock held until the transaction has committed) or `InsideTransaction` (lock taken inside the transaction, for transaction-scoped locks such as `pg_advisory_xact_lock`) |

---

## 14. CSharpEssentials.Enums: Source-Generated String Enums

**What it is:** A source generator, metadata and conventions that give every enum member one spelling (the wire name) across JSON, ASP.NET Core binding, OpenAPI, EF Core and outgoing HTTP.

**Why it exists:** `Enum.ToString()`, `Enum.Parse()` and `JsonStringEnumConverter` use reflection, disagree about naming between layers and are not NativeAOT friendly. `[StringEnum]` makes the generator write the metadata at compile time, and one `EnumConventions` instance decides how every layer reads and writes it.

The generator, analyzers and code fixes of `CSharpEssentials.Enums` flow through any CSharpEssentials package that depends on Enums, directly or through another CSharpEssentials package; reference `CSharpEssentials.Enums` directly only in a project that uses none of them (`.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on it).

```csharp
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

[StringEnum]                                     // optional: Naming = EnumNaming.KebabCaseLower, Storage = EnumStorage.Integer
public enum OrderStatus
{
    Pending,                                     // "pending"
    [EnumAlias("Approval")] PendingApproval,     // "pending_approval", also reads "Approval"
    [JsonStringEnumMemberName("sent")] Shipped,  // "sent"
    [EnumFallback] Unknown = 99,                 // data reads map unknown values here
}

string wire = OrderStatus.PendingApproval.ToWireName();                  // "pending_approval"
bool ok = OrderStatusExtensions.TryParseWire("Approval", out var status); // true, PendingApproval
OrderStatus parsed = OrderStatusExtensions.ParseWire("sent");             // Shipped
```

Wire name priority: `[JsonStringEnumMemberName]`, `[EnumMember(Value)]`, `[StringEnum(Naming)]`, the `CSharpEssentialsEnumNaming` MSBuild property, then `SnakeCaseLower`. `EnumNaming`: `Default`, `SnakeCaseLower`, `SnakeCaseUpper`, `KebabCaseLower`, `KebabCaseUpper`, `CamelCase`, `PascalCase`.

| Generated member (`{Enum}Extensions`) | What It Does |
|------------------|-------------|
| `value.ToWireName()` | Canonical wire name; throws `EnumValueException` for undefined values; flags are comma separated |
| `{Enum}Extensions.TryParseWire(text, out value)` / `ParseWire(text)` | Every known spelling (wire name, member name in any casing, alias, defined number), no fallback; `ParseWire` throws `EnumValueException` |
| `value.IsDefined()` | Defined member, or a combination of defined flags |
| `{Member}WireName` | Wire name constant |
| `value.ToOptimizedString()` / `ToKebabCase()` / `ToLowerCase()` / `ToUpperCase()` | Member name, 4.x kebab case, lower or upper case |
| `value.AsUnderlyingType()` | Underlying numeric value |
| `{Enum}Extensions.IsDefined(name)` / `GetNames()` / `GetValues()` | Member name check, member names, values |
| `{Member}SnakeCase` / `{Member}KebabCase` | 4.x constants, unchanged |
| `value.ToSnakeCase()`, `TryParse`, `Parse` | `[Obsolete]`: use `ToWireName()`, `TryParseWire`, `ParseWire`. `ToSnakeCase()` keeps its 4.x output (`HTTPStatus` → `httpstatus`, wire name `http_status`) |

Nested enums are supported (`Order.State` gets `Order_StateExtensions`).

| Runtime type | What It Does |
|------|-------------|
| `EnumConventions` | Record; `Default` has `AcceptNumbers`, `AcceptMemberNames`, `CaseInsensitive` = `true`, `UnknownValue = UseFallback`, `WriteAs = String`, `Storage = String`, `FlagsStorage = Integer`, `CheckConstraints = true`, `CanHandle = EnumMetadata.IsRegistered` |
| `EnumReadMode` | `Input` (strict, caller values, never the fallback) or `Data` (tolerant, stored or trusted values, applies `UnknownValue`) |
| `EnumValueParser.TryParse<TEnum>(text, mode, conventions, out value, out error)` / `TryParseNumber` | Parses one token |
| `EnumValueFormatter.Format<TEnum>(value, format)` / `FormatFlags` / `TryFormat` / `TryFormatMany` | Formats as wire name or number |
| `EnumValueError` / `EnumValueException` | Enum type, value, allowed values and path of a rejected value |
| `EnumMetadata.Get<TEnum>()` / `TryGet` / `IsRegistered(Type)` | Generated metadata (`EnumInfo<TEnum>`: `Members`, `WireNames`, `Fallback`, `IsFlags`, `Storage`) |
| `EnumMetadata.GetOrCreateWithReflection(Type)` | Opt-in reflection metadata; `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]` |

Analyzers CSE0002 to CSE0016 check duplicate wire names and aliases, fallback and flags rules, invalid names, migrations and enums the generator cannot reach; see the [Enums Readme](../CSharpEssentials.Enums/Readme.MD#diagnostics) for the table with fixes, and the [design document](design/CSharpEssentials.Enums-DESIGN.md) for the per-layer behavior. Upgrading: [Migrating from 4.x to 5.0](migration/v4-to-v5.md).

---

## 15. CSharpEssentials.Time: Testable Clock

**What it is:** An `IDateTimeProvider` interface that wraps the system clock for testability.

**Why it exists:** Code that calls `DateTime.UtcNow` directly is untestable for time-dependent logic. Injecting `IDateTimeProvider` lets tests control time.

| Type/Method | What It Does |
|-------------|-------------|
| `IDateTimeProvider` | Interface: `UtcNow` (`DateTimeOffset`), `UtcNowDateTime`, `UtcNowDate`, `UtcNowTime`, `TimeZone`, `TimeZoneUtc` |
| `DateTimeProvider` | Default implementation using system clock |
| `FakeDateTimeProvider(DateTimeOffset)` | Test clock with `Advance(TimeSpan)` and `SetTime(DateTimeOffset)` |
| `ToTimeOnly()` | Extension: `DateTime` to `TimeOnly` |
| `ToDateOnly()` | Extension: `DateTime` to `DateOnly` |
| `NextDayOfWeek(dayOfWeek, includeCurrent = false)` | Next given weekday for a `DateTime` or `DateOnly`; `includeCurrent` returns the date itself when it already matches |
| `PreviousDayOfWeek(dayOfWeek, includeCurrent = false)` | Previous given weekday, same rules |
| `birthDate.GetAge(today)` / `birthDate.GetAge(dateTimeProvider)` | Age in whole years for a `DateOnly` birth date; the provider form uses its `TimeZone`. Throws `ArgumentOutOfRangeException` when the birth date is after the reference date |

```csharp
var clock = new FakeDateTimeProvider(new DateTimeOffset(2026, 10, 6, 0, 0, 0, TimeSpan.Zero));
DateOnly nextMonday = clock.UtcNowDate.NextDayOfWeek(DayOfWeek.Monday);   // 2026-10-12
int age = new DateOnly(1990, 5, 1).GetAge(clock);                          // 36
```

---

## 16. CSharpEssentials.Clone: Deep Copy

**What it is:** Deep cloning via JSON serialization.

| Type/Method | What It Does |
|-------------|-------------|
| `ICloneable<T>` | Interface with `T Clone()` method |
| `collection.Clone<T>()` | Deep-clones `IEnumerable<T>` via JSON round-trip |
| `queryable.Clone<T>()` | Deep-clones `IQueryable<T>` results |

---

## 17. CSharpEssentials.RequestResponseLogging: HTTP Logging Middleware

**What it is:** ASP.NET Core middleware that logs HTTP request and response bodies.

| Member | What It Does |
|--------|-------------|
| `app.AddRequestResponseLogging(options => ...)` | Adds the middleware to the pipeline (`IApplicationBuilder`); options: `LoggingLevel`, `HeaderKeys`, `LoggingFields`, `UseSeparateContext`, `LoggerCategoryName` |
| `[SkipRequestLogging]` | Attribute to opt out of request body logging |
| `[SkipResponseLogging]` | Attribute to opt out of response body logging |
| `[SkipRequestResponseLogging]` | Attribute to opt out of both |

---

## 18. CSharpEssentials.GcpSecretManager: Secret Configuration

**What it is:** Plugs Google Cloud Secret Manager into the .NET `IConfiguration` system.

| Type | What It Does |
|------|-------------|
| `SecretManagerConfigurationSource` | `IConfigurationSource` for Secret Manager |
| `SecretManagerConfigurationProvider` | Loads secrets as configuration values |
| `SecretManagerConfigurationOptions` | `sealed record` options: `Projects` / `AddProject(...)` (fluent), `CredentialsPath`, `Loader`, `LoggerFactory` (optional `ILoggerFactory`; no console output), `LoadFromAppSettings`, `ConfigurationSectionName`, `BatchSize` (default 10), `PageSize` (default 300) |
| `ProjectSecretConfiguration` | `sealed record` per project: `ProjectId`, `Region` (null = global endpoint), `PrefixFilters`, `SecretIds`, `RawSecretIds`, `RawSecretPrefixes` (raw secrets are not parsed as JSON) |
| `configuration.AddGcpSecretManager(options => ...)` | Adds the source to an `IConfigurationManager` (for example `builder.Configuration`) |

```csharp
builder.Configuration.AddGcpSecretManager(options =>
{
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "my-gcp-project",
        PrefixFilters = ["MyApp__"]
    });
});
```

`AddProject(null)` and `Projects = null` throw `ArgumentNullException`. `Projects` is copy-on-write, so copies made with `with` never share changes. Listing and reading secrets retry through `CSharpEssentials.Resilience` (`RetryIfFailed` with a `shouldRetry` predicate): only `ResourceExhausted` and `Unavailable` are retried, 3 times with exponential backoff; any other status fails at once. A failed listing skips that project, a failed read skips that secret, and loading continues.

---

## 19. CSharpEssentials.Validation: Model-First Validation

**What it is:** A high-performance, model-first validation library that returns `Result<T>` natively.

**Why it exists:** FluentValidation uses expression trees and reflection at runtime. `CSharpEssentials.Validation` is zero-reflection: no expression tree evaluation, no deferred rule builds. Validators receive the model directly; property names are inferred at startup via `nameof`-equivalent extraction. Errors flow as `Result<T>` without exceptions or secondary return channels.

```bash
dotnet add package CSharpEssentials.Validation
```

### Defining a Validator

```csharp
public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Age).GreaterThan(0).LessThan(120);
        return ValueTask.CompletedTask;
    }
}

Result<CreateUserCommand> result = await new CreateUserCommandValidator().ValidateAsync(command);
// error codes: "Email.NotEmpty", "Name.MaxLength", "Age.GreaterThan"
```

**Inline (static) usage**, for one-off validations without a dedicated class:

```csharp
// Sync delegate — zero heap allocation
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate — when MustAsync or SetValidatorAsync is needed
Result<CreateUserCommand> checkedAsync = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", ct);
}, cancellationToken);
```

`Validator.ValidateAsync` (static utility class) and `Validator<T>` (abstract base class) are two independent types defined in the same file. The static form does not delegate to `Validator<T>` internally.

### String Validators

| Method | Fails When |
|--------|-----------|
| `NotEmpty()` | `null`, `""`, or whitespace |
| `NotNull()` | `null` only |
| `MinLength(n)` | fewer than `n` characters |
| `MaxLength(n)` | more than `n` characters |
| `Length(min, max)` | outside `[min, max]` characters |
| `EmailAddress()` | invalid email format |
| `Matches(pattern)` | regex mismatch |
| `Contains(sub)` | substring absent |
| `StartsWith(prefix)` | prefix mismatch |
| `EndsWith(suffix)` | suffix mismatch |

All string validators except `NotEmpty` / `NotNull` **skip** `null` values silently.

### Comparable Validators (`int`, `decimal`, `DateTime`, …)

```csharp
rules.For(() => model.Age)
    .GreaterThan(0)
    .GreaterThanOrEqualTo(18)
    .LessThan(150)
    .LessThanOrEqualTo(120)
    .InclusiveBetween(18, 65)
    .ExclusiveBetween(0, 100)
    .Equal(42)
    .NotEqual(0);
```

### Nullable Struct Validators (`int?`, `DateTime?`, …)

All comparable validators work on nullable value types: `null` is silently skipped.

```csharp
rules.For(() => model.ExpiresAt).GreaterThan(DateTime.UtcNow);
// null → no error   |   value < now → error
```

### Enum Validators

| Rule | Error code | Passes when |
|---|---|---|
| `IsDefinedEnum()` | `{Prop}.IsDefinedEnum` | The value is a defined member; a `[Flags]` enum also accepts any combination of defined flags |
| `IsOneOf(params TEnum[])` | `{Prop}.IsOneOf` | The value equals one of the given members |
| `HasOnlyDefinedFlags()` | `{Prop}.HasOnlyDefinedFlags` | Every set bit belongs to a defined member; zero passes |

A `null` nullable value passes. The message is the binding error text and lists the allowed values (`'42' is not a valid OrderStatus. Allowed values: pending, in_progress.`). Each rule also takes a custom `message` or an `Error`.

### Collection Validators

Works with any nullable collection: `List<T>?`, `IEnumerable<T>?`, `IList<T>?`, `IReadOnlyList<T>?`, `T[]?`, and any type implementing `IEnumerable`.

| Method | Fails When |
|--------|-----------|
| `NotEmpty()` | `null` or empty collection |
| `NotNull()` | `null` |
| `MinCount(n)` | fewer than `n` elements |
| `MaxCount(n)` | more than `n` elements |
| `CountBetween(min, max)` | count outside `[min, max]` |

`MinCount`, `MaxCount`, and `CountBetween` skip `null` collections. Use `NotNull()` or `NotEmpty()` first to enforce presence.

### CascadeMode

Default (`Stop`): first failure stops the chain. Switch to `Continue` to collect all errors for a field.

```csharp
rules.For(() => model.Password)
    .Cascade(CascadeMode.Continue)
    .MinLength(8)
    .Matches(@"[A-Z]", message: "Must contain an uppercase letter.")
    .Matches(@"[0-9]", message: "Must contain a digit.");
```

### Custom Predicates

```csharp
// Sync
rules.For(() => model.Username)
    .Must(name => name != "admin", "Username.Reserved", "The name 'admin' is reserved.");

// Async
await rules.For(() => model.Email)
           .MustAsync(async (email, ct) => await _db.IsUniqueAsync(email, ct),
                      "Email.NotUnique", "Email is already taken.");
```

### Nested Object Validation

`SetValidatorAsync` works with both non-nullable and nullable reference type properties, with no null-forgiving operator (`!`) required. `null` values are skipped automatically.

```csharp
// Non-nullable property
await rules.For(() => model.Address).SetValidatorAsync(new AddressValidator(), ct);

// Nullable reference type — Address? works directly, no ! needed
await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct);

// Error codes are prefixed: "Address.City.NotEmpty", "Address.ZipCode.Matches"
```

### Collection Item Validation

```csharp
// Sync — error codes: "Tags[0].NotEmpty", "Tags[1].MaxLength"
rules.ForEach(() => model.Tags, (tag, tagRules) =>
    tagRules.For(() => tag).NotEmpty().MaxLength(50));

// Async
await rules.ForEachAsync(() => model.Items, async (item, itemRules, ct) =>
{
    itemRules.For(() => item.Sku).NotEmpty();
    await itemRules.For(() => item.Sku)
                   .MustAsync(async (sku, c) => await _db.SkuExistsAsync(sku, c),
                              "Sku.NotFound", "SKU not found.");
}, ct);
```

### Native Conditional Rules

`Configure` receives the live model, so any C# control flow works directly. No `When()`/`Unless()` DSL needed.

```csharp
public class CheckoutValidator : Validator<Checkout>
{
    protected override ValueTask Configure(Checkout model, RuleContext<Checkout> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerId).NotEmpty();

        if (model.OrderType == OrderType.Business)
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);
        else
            rules.For(() => model.FirstName).NotEmpty().MaxLength(100);

        if (!model.AcceptsTerms) return ValueTask.CompletedTask;
        rules.For(() => model.Signature).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
```

### Validator Composition

```csharp
public class PaidCheckoutValidator : Validator<Checkout>
{
    protected override async ValueTask Configure(Checkout model, RuleContext<Checkout> rules, CancellationToken ct = default)
    {
        await Include(new CheckoutValidator(), model, rules, ct);   // merge base rules
        rules.For(() => model.PaymentReference).NotEmpty();
    }
}
```

### DI Registration

| Method | What It Does |
|--------|-------------|
| `AddValidator<TModel, TValidator>()` | Registers a single validator |
| `AddValidatorsFromAssembly(assembly)` | Registers all validators in an assembly |
| `AddValidatorsFromAssemblies(assemblies)` | Registers validators across multiple assemblies |

Default lifetime: `Scoped`. Pass a `lifetime` parameter to override. Registration uses `TryAddEnumerable`, so registering the same validator type twice (for example `AddValidator` and an assembly scan) adds it only once. Different validator types for the same `T` are all registered, and `ValidationBehavior` aggregates and deduplicates results from all of them.

### Validator Ordering

Override `Order` on `Validator<T>` to control execution sequence when multiple validators target the same model. Validators sharing the same `Order` run concurrently; groups with lower `Order` complete before higher-`Order` groups begin. All groups execute regardless of earlier failures. Errors are accumulated and deduplicated.

### Mediator Pipeline Integration

```csharp
services.AddMediatorValidationBehavior();
// or register all behaviors:
services.AddMediatorBehaviors();
```

Validation runs before the handler. On failure the handler is never invoked. `Result` / `Result<T>` handlers receive `Result.Failure` directly; all other handler return types trigger `EnhancedValidationException` (caught by `GlobalExceptionHandler`). Non-cancellation exceptions thrown by a validator are caught and converted to `Error.Exception("Validator.Exception", ex)` so validator bugs never rethrow through the pipeline.

### Railway Validation Bindings

`ValidateWith` / `ValidateWithAsync` plug validators directly into a `Result<T>` railway. If the result is already a failure, the validator is skipped entirely.

| Method | Input | Returns | When to Use |
|--------|-------|---------|-------------|
| `result.ValidateWith(configure)` | `Result<T>` | `Result<T>` | Inline sync validation in a pipeline |
| `result.ValidateWithAsync(validator, ct)` | `Result<T>` | `ValueTask<Result<T>>` | Named validator in a pipeline |
| `result.ValidateWithAsync(configure)` | `Result<T>` | `ValueTask<Result<T>>` | Inline sync delegate, async context |
| `result.ValidateWithAsync(asyncConfigure, ct)` | `Result<T>` | `ValueTask<Result<T>>` | Inline async delegate |
| `taskResult.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `Task<Result<T>>` | `ValueTask<Result<T>>` | Awaited task pipeline |
| `valueTaskResult.ValidateWithAsync(validator \| configure \| asyncConfigure, ct)` | `ValueTask<Result<T>>` | `ValueTask<Result<T>>` | ValueTask pipeline |

```csharp
// Named validator — plugs straight into a Result<T> chain
Result<CreateUserCommand> result = await ParseCommand(input)
    .ValidateWithAsync(new CreateUserCommandValidator(), ct);

// Inline validation — no dedicated class needed
Result<CreateUserCommand> inline = ParseCommand(input)
    .ValidateWith((m, rules) =>
    {
        rules.For(() => m.Email).NotEmpty().EmailAddress();
        rules.For(() => m.Name).NotEmpty().MaxLength(100);
    });

// Works on Task<Result<T>> — no intermediate await
Result<Checkout> checkout = await GetCheckoutAsync(id)  // Task<Result<Checkout>>
    .ValidateWithAsync(new CheckoutValidator(), ct);     // skips if already failed
```

Short-circuits immediately: if `result.IsFailure` before validation runs, the existing errors pass through and the validator is never invoked. This makes it safe to chain multiple `ValidateWithAsync` calls without nested null/failure checks.

---

## 20. CSharpEssentials.These: 3-State Union

**What it is:** A `readonly record struct` that holds Left (error only), Right (value only), or Both (error + value simultaneously), the only functional type in the ecosystem that can carry both sides at once.

**Why it exists:** `Result<T>` models binary outcomes: success or failure. `These<TError, TValue>` models partial success, scenarios where an operation produces a useful value *and* a warning/error simultaneously. Classic example: importing a CSV where valid rows succeed and invalid rows produce errors, but both results are needed by the caller.

### Creating

| Method | State | Meaning |
|--------|-------|---------|
| `These<TError, TValue>.Left(error)` | Left | Error only: no value |
| `These<TError, TValue>.Right(value)` | Right | Value only: no error |
| `These<TError, TValue>.Both(error, value)` | Both | Error + value simultaneously |

### Inspecting

| Property | What It Does |
|----------|-------------|
| `IsLeft` | True when error only (`HasLeft && !HasRight`) |
| `IsRight` | True when value only (`!HasLeft && HasRight`) |
| `IsBoth` | True when both present (`HasLeft && HasRight`) |
| `GetLeft()` | Returns `Maybe<TError>`: `None` if no error |
| `GetRight()` | Returns `Maybe<TValue>`: `None` if no value |

### Transforming

| Method | What It Does |
|--------|-------------|
| `Map(mapper)` | Transforms the value; passes Left through unchanged |
| `MapLeft(mapper)` | Transforms the error; passes Right through unchanged |
| `FlatMap(mapper)` | Chains into a new `These`: only if Right or Both |
| `Tap(action)` | Side-effect on value when Right or Both |
| `TapLeft(action)` | Side-effect on error when Left or Both |
| `Match(onLeft, onRight, onBoth)` | Exhaustive pattern match: all three branches required |

### Converting to Result

| Method | Both behavior | Use when |
|--------|--------------|----------|
| `ToResult()` | Both → **failure** (strict) | Warning = blocking |
| `ToResultLenient()` | Both → **success** (lenient) | Warning = non-blocking |

Both are extensions on `These<Error, TValue>`.

### Collection Extensions

| Method | What It Does |
|--------|-------------|
| `Partition(IEnumerable<These<TError,TValue>>)` | Returns `(Lefts, Rights, Boths)` as read-only lists |
| `TheseExtensions.FromResult(Result<TValue>)` | Wraps a `Result` into `These<Error, TValue>` |

```csharp
// Partial success: import CSV rows, collect errors without stopping
These<List<ImportError>, List<User>> result = ImportCsv(csv);

string summary = result.Match(
    onLeft:  errors          => $"All {errors.Count} rows failed",
    onRight: users           => $"{users.Count} rows imported",
    onBoth:  (errors, users) => $"{users.Count} rows imported, {errors.Count} skipped");

// Chain transformations
These<string, int> doubled = These<string, int>.Both("warn", 5)
    .Map(x => x * 2);          // Both("warn", 10)

// Partition a mixed sequence
var (lefts, rights, boths) = items.Partition();
```

### JSON Support

`These<TError, TValue>` is fully serializable via `System.Text.Json`. The `[JsonConstructor]` private constructor enables round-trip without a custom converter.

| JSON key | Maps to | Serialized |
|----------|---------|-----------|
| `isLeft` | `HasLeft` | Always |
| `isRight` | `HasRight` | Always |
| `left` | `LeftOrDefault` | When non-null |
| `right` | `RightOrDefault` | When non-null |
| `isBoth` | *(not present)* | `[JsonIgnore]`: derived from `isLeft && isRight` |

```csharp
// Both state round-trips cleanly
var these = These<string, int>.Both("warning", 42);
string json = JsonSerializer.Serialize(these);
// {"isLeft":true,"isRight":true,"left":"warning","right":42}

These<string, int> back = JsonSerializer.Deserialize<These<string, int>>(json);
bool isBoth = back.IsBoth;   // true
```

---

## 21. CSharpEssentials.Endpoints: Source-Generated Endpoint Mapping

**What it is:** Organizes ASP.NET Core Minimal API endpoints into types and groups. A bundled source generator writes a reflection-free registry per assembly. Route calls stay in your code, so binding, filters, `IResult`, OpenAPI and the Request Delegate Generator work unchanged. Targets `net11.0`, `net10.0`, `net9.0`, `net8.0`.

```csharp
public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps").RequireAuthorization();
}

[EndpointGroup<AppsGroup>]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request))
           .WithValidation<CreateAppRequest>();
}
```

### Declaring Endpoints

| Type/Attribute | What It Does |
|----------------|-------------|
| `IEndpoint` | `static void Map(IEndpointRouteBuilder app)`. Endpoint types are never instantiated; dependencies come from handler parameters |
| `IEndpointGroup` | `static string Prefix` (passed to `MapGroup`) and `static void Configure(RouteGroupBuilder group)` for group-wide conventions |
| `[EndpointGroup(typeof(TGroup))]` / `[EndpointGroup<TGroup>]` | Places an endpoint or a group under a group. Groups can nest |
| `[ExcludeFromMapping]` | Excludes an endpoint, a group (with everything under it) or an assembly |

### Mapping

| Member | What It Does |
|--------|-------------|
| `app.Map{Asm}Endpoints(options?)` | Generated per assembly in `Microsoft.AspNetCore.Builder`. `{Asm}` is the sanitized assembly name or the `[assembly: EndpointRegistryName("...")]` value |
| `app.MapAllEndpoints(options?)` | `internal`; maps the own registry, then every referenced registry once. Generated in `Exe`/`WinExe` projects that are not test projects |
| `[assembly: GenerateEndpointAggregate]` / `[assembly: DisableEndpointAggregate]` | Opt in to `MapAllEndpoints` from a library or test project / opt out in an application |
| `{Asm}EndpointRegistry.EndpointTypes` | Endpoint types in mapping order |
| `app.MapEndpointsFromAssemblies(assemblies)` / `(configure, assemblies)` | Reflection fallback for assemblies without a registry (plugins). Same rules and options; `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]` |
| `app.MapVersionedGroup(version).Map{Asm}Endpoints()` | Versioned routes (`/v2/...`) via `CSharpEssentials.AspNetCore` |

Each endpoint type is mapped inside its own `MapGroup("")`, so routes, metadata, filters and authorization match direct mapping.

```csharp
app.MapAppsEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Sample.Internal");
    options.OperationNaming = OperationNaming.TypeName;
    options.AutoTagFromGroup = true;
    options.LogDiscovered = true;
});
```

### `EndpointMappingOptions`

| Option | What It Does |
|--------|-------------|
| `Filter(predicate)` | Skips endpoint types for which the predicate returns `false`. Multiple predicates are AND-combined |
| `ConfigureEach((builder, type) => ...)` | Runs once per endpoint type after group `Configure`, in registration order |
| `OperationNaming` | `None` (default), `TypeName` or `Custom(...)`. `TypeName` builds unique names from the type and its containing types (`Orders_Endpoint`), adds the HTTP method when a type maps several routes (`Items_Get`, `Items_Post`) and qualifies colliding names with the namespace. An explicit `WithName(...)` always wins: constant explicit names anywhere in the project are reserved at build time, so a colliding generated name gets a numeric suffix (`Items_2`). Generated names can change when same-named types are added; use `WithName(...)` where client method names must stay stable |
| `AutoTagFromGroup` | Tags untagged endpoints with the innermost group name (`UsersGroup` → `Users`) |
| `LogDiscovered` | Logs mapped and filtered endpoint types at `Debug`, category `CSharpEssentials.Endpoints` |

### Routes and Authorization

| Member | What It Does |
|--------|-------------|
| `EndpointTypeMetadata` | Added to every endpoint; identifies the endpoint type at runtime |
| `app.RouteOf<TEndpoint>(values?)` / `RouteOf<TEndpoint>(nameOrMethod, values?)` | Builds a request path from the endpoint type (group prefixes included). Extra values become query string entries. Throws `InvalidOperationException` when no single route matches or a route value is missing |
| `RequireRoles(...)` | Any listed role |
| `RequirePolicies(...)` | Every listed policy |
| `RequireAuthSchemes(...)` | Any listed scheme |

```csharp
string path = app.RouteOf<GetApp>(new { id = 42 });   // "/apps/42"
group.RequireRoles("admin", "editor");
```

### Diagnostics

| ID | Severity | Rule |
|----|----------|------|
| CSE1001 | Error | Endpoint or group type is not accessible from generated code |
| CSE1002 | Error | Group nesting cycle |
| CSE1003 | Error | More than one group attribute on one type |
| CSE1004 | Warning | Endpoint declares instance state (code fix removes it) |
| CSE1005 | Warning | Two endpoints in one group map the same HTTP method and constant route |
| CSE1006 | Info | Abstract or open-generic endpoint or group type is skipped |
| CSE1007 | Error | `[EndpointGroup(typeof(X))]` target is not a concrete, non-ref struct `IEndpointGroup` |
| CSE1008 | Error | Endpoint or group type is a `ref struct` and is not mapped |
| CSE1009 | Warning | Registries (referenced or the project's own) share a sanitized name; `MapAllEndpoints` skips the referenced ones (use `[assembly: EndpointRegistryName]`) |
| CSE1010 | Warning | The same constant endpoint name is set at two call sites (`WithName`, or `EndpointNameAttribute`/`EndpointNameMetadata`/`RouteNameMetadata` in `WithMetadata`); endpoint and route names are compared separately |
| CSE1011 | Info | An explicit endpoint name outside an `IEndpoint`/`IEndpointGroup` type is not a constant, so it cannot be reserved at build time |

---

## 22. CSharpEssentials.DependencyInjection: Attribute-Based Registration

**What it is:** Attribute-based service registration and decoration for `Microsoft.Extensions.DependencyInjection`. A bundled source generator writes a reflection-free `Add{Assembly}Services` method; every strategy is key-aware. Targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.1` (generic attribute forms need `net7.0` or later).

```csharp
[RegisterScoped]                                     // IOrderService, by the I{TypeName} rule
public sealed class OrderService : IOrderService { }

[RegisterSingleton<IPaymentGateway>(Key = "stripe")] // keyed, generic form
public sealed class StripeGateway : IPaymentGateway { }

[RegisterTransient(typeof(IRepository<>))]           // open generic
public sealed class Repository<T> : IRepository<T> { }

[Decorates(typeof(IOrderService), Order = 1)]
public sealed class LoggingOrderService(IOrderService inner) : IOrderService { }
```

### Attributes

| Attribute | What It Does |
|-----------|-------------|
| `[RegisterScoped]` / `[RegisterSingleton]` / `[RegisterTransient]` | Registers the class. Optional service type: `(typeof(IFoo))` or `<IFoo>`. Without one (and without `As`), registers as the interface named `I{TypeName}`, otherwise as itself |
| `Key` | Registers a keyed service (`null` = non-keyed) |
| `As` | `ServiceAs.Self`, `ServiceAs.SelfWithInterfaces` (one shared instance), `ServiceAs.ImplementedInterfaces` |
| `Strategy` | `RegistrationStrategy.Add` (default), `TryAdd`, `TryAddEnumerable`, `Replace`, `Throw` |
| `[Decorates(typeof(TService), Order = n)]` / `[Decorates<TService>]` | Decorates every matching registration in ascending `Order`, keeping lifetime and key |
| `[ExcludeFromRegistration]` | Opts a class or an assembly out |

### Registration and Decoration

| Member | What It Does |
|--------|-------------|
| `services.Add{Asm}Services(logger?)` | Generated per assembly: registers services, then applies decorators; a repeated call does nothing. The logger reports duplicate (service, key) registrations at `Debug` |
| `services.AddAllServices()` | `internal`; registers every referenced registry and the application's own, then applies all decorators in ascending `Order` across assemblies (ties: assembly order, then type name). Skips assemblies already registered, like a repeated `Add{Assembly}Services`. Generated in applications |
| `[assembly: ServiceRegistryName("...")]` / `GenerateServiceAggregate` / `DisableServiceAggregate` | Rename the method / opt in to `AddAllServices` in a library or test project / opt out |
| `services.AddServicesFromAssemblies(assemblies)` / `(logger, assemblies)` | Reflection fallback; `[RequiresUnreferencedCode]`, `[RequiresDynamicCode]` |
| `services.Decorate<TService, TDecorator>(serviceKey?)` | Runtime decoration; throws `InvalidOperationException` when nothing matches |
| `services.Decorate<TService>((inner, sp) => ...)` | Factory decoration |
| `services.TryDecorate<TService, TDecorator>()` | Returns `false` when nothing matches |
| `services.Decorate(typeof(IRepository<>), typeof(CachedRepository<>))` | Decorates the closed registrations of an open generic |

```csharp
builder.Services.AddSampleBillingServices();
builder.Services.TryDecorate<IOrderService, LoggingOrderService>();
```

Decorated originals move to a hidden registration under a private key, so they never appear in `GetServices<T>()`, `GetKeyedServices<T>(KeyedService.AnyKey)` or `GetKeyedServices<object>(KeyedService.AnyKey)`. If decoration fails, the collection is left unchanged.

### Diagnostics

| ID | Severity | Rule |
|----|----------|------|
| CSE2001 | Error | The class does not implement the service type |
| CSE2002 | Error | `RegistrationStrategy.Throw` registration that follows an earlier registration of the same service type and key |
| CSE2003 | Info | No interface matches `I{TypeName}`, so the class registers as itself (code fix adds the service type) |
| CSE2004 | Error | Decorator has zero or several constructor parameters of the decorated type |
| CSE2005 | Error | Decorator does not have exactly one public constructor |
| CSE2006 | Info | Captive dependency between attribute registrations |
| CSE2007 | Error | Generated code cannot construct the class |
| CSE2008 | Warning | Open-generic decorators are not generated; call `Decorate(Type, Type)` |
| CSE2009 | Warning | Registries (referenced or the project's own) share a sanitized name; `AddAllServices` skips the referenced ones (use `[assembly: ServiceRegistryName]`) |

---

## 23. CSharpEssentials: Meta Package

**What it is:** One package reference that brings in the core libraries:

```bash
dotnet add package CSharpEssentials
```

It references `CSharpEssentials.Any`, `Clone`, `Core`, `Entity`, `Enums`, `Errors`, `Http`, `Json`, `Maybe`, `Results`, `Rules`, `These` and `Time`. Install `AspNetCore`, `DependencyInjection`, `Endpoints`, `EntityFrameworkCore`, `GcpSecretManager`, `Mediator`, `RequestResponseLogging`, `Resilience` and `Validation` separately when you need them.

---

## Ecosystem Design Patterns

### The Type Bridge System

The ecosystem provides natural transformations between its core types:

```
Exception world ──── Try / TryCatch ─────► Result world
Null world ──────── AsMaybe ──────────────► Maybe world
Maybe ◄──────── AsMaybe / ToMaybeResult ──► Result
Error ──────────── ToResult ───────────────► Result
HTTP response ──── StatusCodeMapper ───────► Error → Result
EF Core null ───── AsResultAsync ──────────► Result
```

### Pattern Summary

| Pattern | Where Used | Purpose |
|---------|-----------|---------|
| **Monadic bind** | `Result.Bind`, `Maybe.Bind` | Chain dependent operations; short-circuit on failure/absence |
| **Functor map** | `Result.Map`, `Maybe.Map` | Transform inner value without changing container |
| **Applicative** | `Result.And`, `Result.Combine` | Combine independent results; collect all errors |
| **Alternative** | `Result.Or`, `Maybe.Or` | First success wins; fallback chains |
| **Catamorphism** | `Result.Match`, `Maybe.Match`, `Any.Match` | Exhaustive decomposition |
| **Side-effect isolation** | `Tap`, `TapError`, `Execute` | Observe without altering the flow |
| **Error recovery** | `Compensate`, `Recover`, `Else` | Return to the success railway |
| **Exception bridging** | `Result.Try`, `Error.Exception` | Convert exceptions to typed errors |
| **Smart constructors** | `Error.NotFound`, `SuccessIf`, `FailureIf` | Validated construction with domain semantics |
| **Implicit lifting** | `Result<int> r = 42;` | Ergonomic construction |
| **LINQ integration** | `Select`, `SelectMany` | `from x in r from y in s select ...` |
| **Discriminated union** | `Any<T0,...,T7>` | Type-safe sum types |
| **Interpreter** | `RuleEngine.Evaluate` | Recursive rule tree evaluation |

### Choosing Between Types

| Scenario | Use |
|----------|-----|
| Operation can succeed or fail with a reason | `Result<T>` |
| Value may or may not exist (no reason needed) | `Maybe<T>` |
| Value is one of several known types | `Any<T0, T1, ...>` |
| Multiple validations, collect all errors | `Result.And(...)` or Rules engine |
| Complex business rules with branching | Rules engine |
| Need the absence reason from a Maybe | Bridge: `maybe.ToMaybeResult(error)` |
