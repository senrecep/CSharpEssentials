# CSharpEssentials API Reference

A comprehensive guide to every package, method, and pattern in the CSharpEssentials ecosystem.

> **Philosophy:** Values over exceptions. Explicit over implicit. Composable over monolithic.
> Every abstraction exists to make C# code safer, more composable, and more expressive — bridging OOP and Functional Programming without abandoning either.

---

## Table of Contents

- [Errors — The Foundation](#1-csharpessentialserrors--the-foundation)
- [Results — Railway-Oriented Programming](#2-csharpessentialsresults--railway-oriented-programming)
- [Maybe — Explicit Optionals](#3-csharpessentialsmaybe--explicit-optionals)
- [Any — Discriminated Unions](#4-csharpessentialsany--discriminated-unions)
- [Core — Utility Belt](#5-csharpessentialscore--utility-belt)
- [Rules — Composable Business Rules](#6-csharpessentialsrules--composable-business-rules)
- [Entity — DDD Building Blocks](#7-csharpessentialsentity--ddd-building-blocks)
- [Http — Result-Returning HTTP Client](#8-csharpessentialshttp--result-returning-http-client)
- [Resilience — Transient Fault Handling](#9-csharpessentialsresilience--transient-fault-handling)
- [EntityFrameworkCore — EF Core Integration](#10-csharpessentialsentityframeworkcore--ef-core-integration)
- [Json — Serialization Defaults](#11-csharpessentialsjson--serialization-defaults)
- [AspNetCore — API Layer](#12-csharpessentialsaspnetcore--api-layer)
- [Mediator — Pipeline Behaviors](#13-csharpessentialsmediator--pipeline-behaviors)
- [Enums — Source-Generated String Enums](#14-csharpessentialsenums--source-generated-string-enums)
- [Time — Testable Clock](#15-csharpessentialstime--testable-clock)
- [Clone — Deep Copy](#16-csharpessentialsclone--deep-copy)
- [RequestResponseLogging — HTTP Logging Middleware](#17-csharpessentialsrequestresponselogging--http-logging-middleware)
- [GcpSecretManager — Secret Configuration](#18-csharpessentialsgcpsecretmanager--secret-configuration)
- [Validation — Model-First Validation](#19-csharpessentialsvalidation--model-first-validation)
- [Ecosystem Design Patterns](#ecosystem-design-patterns)

---

## 1. CSharpEssentials.Errors — The Foundation

**What it is:** A structured error value type that replaces exceptions for expected failures.

**Why it exists:** Exceptions are expensive, invisible in type signatures, and break composability. `Error` is a `readonly record struct` — immutable, value-semantic, and carries enough information (code, description, type, metadata) to flow through any layer of your application without losing context.

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
try { /* external call */ }
catch (Exception ex) { return Error.Exception(ex); }

// Composing multiple errors
Error[] allErrors = validationError + conflictError;
```

---

## 2. CSharpEssentials.Results — Railway-Oriented Programming

**What it is:** A Result monad that makes success and failure explicit in your type signatures.

**Why it exists:** Traditional C# uses exceptions for flow control and null for absence — both are invisible in method signatures and break composability. `Result<T>` forces every caller to handle both paths, enables method chaining that short-circuits on failure, and makes error accumulation trivial.

Two core types: `Result` (no value, just success/failure) and `Result<T>` (carries a value on success).

### Creating Results

| Method | Returns | When to Use |
|--------|---------|-------------|
| `Result.Success()` | `Result` | Void operations that succeeded |
| `Result.Success(value)` | `Result<T>` | Operations that return a value |
| `Result.Failure(error)` | `Result` | Single error failure |
| `Result.Failure(errors)` | `Result` | Multiple errors failure |
| `Result<T>.Failure(error)` | `Result<T>` | Typed failure |
| `Result.SuccessIf(condition, error)` | `Result` | Guard clause — success if condition holds |
| `Result.FailureIf(condition, error)` | `Result` | Guard clause — failure if condition holds |
| `Result.Try(action, handler)` | `Result` | Wraps try/catch, converts exception to Error |
| `Result.Try(func, handler)` | `Result<T>` | Typed try/catch wrapper |
| `Result.From(errors)` | `Result` | Success if errors empty, failure otherwise |
| `Result<int> r = 42;` | `Result<int>` | Implicit operator for ergonomic creation |

### Chaining — The Success Railway

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
    .Map(o => new OrderDto(o.Id, o.Total, o.Items.Count));
```

### Side Effects — Observe Without Changing the Railway

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

### Error Handling — Recovery and Transformation

| Method | What It Does | When to Use |
|--------|-------------|-------------|
| `Else(error)` | Replaces all errors with a new error | Error message normalization |
| `Else(func)` | Transforms errors into replacement | Dynamic error replacement |
| `MapError(func)` | Transforms each error individually | Error enrichment (add context) |
| `Compensate(func)` | Attempts recovery — can return Success | Retry, fallback strategies |
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

### Extracting Values — Leaving the Railway

| Method | Safety | What It Does |
|--------|--------|-------------|
| `Match(onSuccess, onFailure)` | Safe | Exhaustive fold — handles both cases, returns a value |
| `MatchFirst(onSuccess, onFailure)` | Safe | Match using only the first error |
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
    onFailure: errors => $"Failed: {errors.First().Description}"
);

// Finally — always runs (logging, cleanup)
result.Finally(r => _metrics.Record(r.IsSuccess ? "success" : "failure"));
```

### Combining Multiple Results

| Method | Strategy | What It Does |
|--------|----------|-------------|
| `Result.And(results)` | All must succeed | Collects ALL errors if any fail |
| `Result.Or(results)` | Any can succeed | Returns first success; errors only if all fail |
| `Result.Combine(r1, r2, ..., r8)` | Applicative product | Combines up to 8 results into a tuple |

```csharp
// Validate multiple fields independently, collect all errors
Result validation = Result.And(new[]
{
    ValidateName(input.Name),
    ValidateEmail(input.Email),
    ValidateAge(input.Age)
});

// Try multiple providers, use first that works
Result<Config> config = Result.Or(new[]
{
    LoadFromEnvironment(),
    LoadFromFile(),
    LoadFromDefaults()
});
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

Every method has `Task<Result>` and `ValueTask<Result>` extension variants with `CancellationToken` support. Async methods follow the naming pattern of their sync counterparts:

```csharp
Result<User> result = await GetUserAsync(id)
    .Bind(user => ValidateAsync(user, ct))
    .Map(user => new UserDto(user))
    .Tap(_ => _logger.LogInformation("User retrieved"));
```

### Collection Extensions

Batch operations on sequences of results — without manually looping.

| Method | Strategy | What It Does |
|--------|----------|-------------|
| `CombineAll(IEnumerable<Result>)` | Collect all errors | Success if all succeed; accumulates ALL errors if any fail |
| `CombineAll<T>(IEnumerable<Result<T>>)` | Collect all errors | Same as `Sequence` — success array or all errors |
| `Sequence<T>(IEnumerable<Result<T>>)` | Collect all | Returns `Result<T[]>` with all values, or all errors |
| `Traverse<TSource, TOut>(source, selector)` | Map + sequence | Applies selector to each element, then sequences |
| `Partition<T>(IEnumerable<Result<T>>)` | Split | Returns `(T[] Successes, Error[] Errors)` — never fails |
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

## 3. CSharpEssentials.Maybe — Explicit Optionals

**What it is:** An Option/Maybe monad that explicitly represents the presence or absence of a value.

**Why it exists:** `null` is invisible in C# type signatures (even with nullable reference types, it's a warning, not an error). `Maybe<T>` makes optionality a first-class citizen — you cannot access the value without acknowledging it might not exist. Unlike `Result`, Maybe does not carry a reason for absence — it simply says "there is no value."

### Creating Maybe Values

| Method | Creates | When to Use |
|--------|---------|-------------|
| `Maybe<T>.None` | Absence | Explicit "no value" |
| `Maybe<T>.From(value)` | Some if non-null, None if null | Converting nullable to Maybe |
| `Maybe<int> m = 42;` | Some(42) via implicit operator | Ergonomic creation |
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
| `Match(hasValue, hasNoValue)` | Safe | Exhaustive fold over both cases |
| `Or(fallback)` | Safe | Returns value or fallback |
| `Or(Maybe<T> fallback)` | Safe | Returns self or fallback Maybe |
| `GetValueOrDefault(value)` | Safe | Returns value or default |
| `GetValueOrDefault()` | Safe | Returns `default(T)` |
| `TryGetValue(out value)` | Safe | Try pattern for extraction |
| `AsNullable()` | Safe | Converts back to nullable `T?` |

```csharp
string name = GetUser(id)
    .Map(u => u.DisplayName)
    .Or("Anonymous");
```

### Side Effects

| Method | Runs On | What It Does |
|--------|---------|-------------|
| `Execute(action)` | Has value | Runs action with value, returns self |
| `ExecuteNoValue(action)` | No value | Runs action when empty |
| `Tap(action)` | Has value | Side effect with value |
| `TapIf(cond, action)` | Has value + condition | Conditional side effect |

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
| `Sequence<T>(IEnumerable<Maybe<T>>)` | `Maybe<T[]>` — `None` if any element is `None` |
| `Traverse<TSource, TOut>(source, selector)` | Applies selector then sequences — `None` if any is `None` |
| `Partition<T>(IEnumerable<Maybe<T>>)` | Returns `(T[] Values, int NoneCount)` — never returns `None` |

```csharp
// Require ALL lookups to succeed
Maybe<User[]> allUsers = userIds
    .Traverse(id => _cache.TryFind(id));  // None if any id is missing

// Collect present values, count absences
var (values, missingCount) = maybes.Partition();
```

---

## 4. CSharpEssentials.Any — Discriminated Unions

**What it is:** Type-safe union types for C#. `Any<T0, T1>` through `Any<T0, ..., T7>` — a value that holds exactly one of N possible types.

**Why it exists:** C# has no native discriminated unions (until future language versions). When a method can return different types, developers resort to `object`, `dynamic`, marker interfaces, or separate result classes. `Any<T0, T1>` provides compile-time type safety with exhaustive matching — if you forget a case, the compiler tells you.

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
| `Match(first:, second:, ...)` | `AnyActionResult<T>` | Transforms the active variant — exhaustive |
| `Switch(first:, second:, ...)` | `AnyActionStatus` | Executes action for active variant — exhaustive |
| `Deconstruct(out index, out value)` | void | C# deconstruction support |

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

## 5. CSharpEssentials.Core — Utility Belt

**What it is:** Foundational extension methods used across every project — null checks, string conversions, collection helpers, and async utilities.

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
| `ForEach(action)` | Eager foreach on `IEnumerable<T>` |
| `AllTrue()` / `AllFalse()` | Checks bool collections |

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
| `str.ToGuidFromString()` | Reverse — decodes back to `Guid` |

---

## 6. CSharpEssentials.Rules — Composable Business Rules

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
| `RuleEngine.Linear(rules, context, ct)` | Sequential — stops at first failure |
| `RuleEngine.And(rules, context, ct)` | All must pass — accumulates all errors |
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

## 7. CSharpEssentials.Entity — DDD Building Blocks

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

## 8. CSharpEssentials.Http — Result-Returning HTTP Client

**What it is:** Extension methods and a fluent builder that wrap `HttpClient` calls to return `Result<T>` instead of throwing exceptions or requiring manual status code checks.

**Why it exists:** Raw `HttpClient` usage involves checking `IsSuccessStatusCode`, handling `HttpRequestException`, deserializing manually, and mapping status codes to domain errors. This package does all of that and returns `Result<T>`.

### Fluent Request Builder

```csharp
Result<UserDto> result = await HttpRequestBuilder
    .Get("https://api.example.com/users")
    .WithQuery("page", "1")
    .WithQuery("limit", "10")
    .WithBearerToken(token)
    .WithHeader("X-Request-Id", correlationId)
    .AsResultAsync<UserDto>(httpClient);
```

| Method | What It Does |
|--------|-------------|
| `HttpRequestBuilder.Get(url)` | Creates GET builder |
| `.Post(url)` / `.Put(url)` / `.Patch(url)` / `.Delete(url)` | Other HTTP methods |
| `.WithHeader(name, value)` | Adds request header |
| `.WithQuery(key, value)` | Adds query parameter |
| `.WithJsonContent(body)` | Sets JSON request body |
| `.WithBearerToken(token)` | Sets Authorization: Bearer header |
| `.Build()` | Returns `Result<HttpRequestMessage>` |
| `.AsResultAsync(client)` | Builds, sends, returns `Result` |
| `.AsResultAsync<T>(client)` | Builds, sends, deserializes to `Result<T>` |

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

| Method | What It Does |
|--------|-------------|
| `CreateRetryPipeline` | Polly retry policy |
| `CreateCircuitBreakerPipeline` | Circuit breaker policy |
| `CreateTimeoutPipeline` | Timeout policy |
| `CreateResiliencePipeline` | Combines all resilience policies |
| `ExecuteAsResultAsync` | Executes through resilience pipeline, returns `Result` |

---

## 9. CSharpEssentials.Resilience — Transient Fault Handling

**What it is:** HTTP-agnostic resilience patterns (Retry, Timeout, Circuit Breaker, Fallback) with `Result<T>` integration. Composable `ResiliencePolicy` builder backed by Polly v8.

**Why it exists:** Transient faults are inevitable in distributed systems. This package provides a clean, composable API for handling retries, timeouts, circuit breakers, and fallbacks without coupling to any specific transport (HTTP, database, message queue, etc.).

### Quick Start

```csharp
using CSharpEssentials.Resilience;

// Simple retry
Result<User> user = await ResiliencePolicy
    .Create()
    .WithRetry(maxAttempts: 3, delay: TimeSpan.FromSeconds(1))
    .ExecuteAsync(() => _db.GetUser(id));

// Retry + Timeout
Result<Order> order = await ResiliencePolicy
    .Create()
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync(() => _orderService.GetOrder(id));

// Circuit Breaker + Fallback
Result<Product> product = await ResiliencePolicy
    .Create()
    .WithCircuitBreaker(minimumThroughput: 10, failureRatio: 0.5)
    .WithFallback(ct => _cache.GetAsync<Product>(id, ct))
    .ExecuteAsync(() => _productService.GetProduct(id));
```

### ResiliencePolicy

| Method | What It Does |
|--------|-------------|
| `ResiliencePolicy.Create()` | Creates an empty policy |
| `.WithRetry(maxAttempts, delay, exponentialBackoff)` | Adds retry strategy |
| `.WithTimeout(timeout)` | Adds timeout strategy |
| `.WithCircuitBreaker(minThroughput, samplingDuration, breakDuration, failureRatio)` | Adds circuit breaker |
| `.ExecuteAsync(action)` | Executes action through the pipeline, returns `Result` |
| `.ExecuteAsync<T>(action)` | Executes typed action, returns `Result<T>` |

### ResiliencePolicy\<T\> (Result-Aware)

The generic variant automatically filters retryable errors — `Unauthorized`, `Forbidden`, `NotFound`, and `Validation` errors are **not** retried.

| Method | What It Does |
|--------|-------------|
| `ResiliencePolicy<T>.Create()` | Creates an empty typed policy |
| `.WithRetry(...)` | Adds retry with Result error filtering |
| `.WithTimeout(...)` | Adds timeout |
| `.WithCircuitBreaker(...)` | Adds circuit breaker with Result error filtering |
| `.WithFallback(fallbackAsync)` | Adds fallback that returns `T` or `Result<T>` |
| `.ExecuteAsync(action)` | Executes through pipeline, returns `Result<T>` |

### Delegate Extensions

```csharp
Result<User> user = await (() => _db.GetUser(id))
    .WithRetry(3)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .ExecuteAsync();
```

### Retry Extensions

```csharp
Func<CancellationToken, Task<Result<User>>> getUser = ct => _db.GetUser(id, ct);
Result<User> result = await getUser.RetryIfFailed(maxAttempts: 3);
```

### Error Handling

| Error Code | When |
|-----------|------|
| `Resilience.Timeout` | Operation exceeded timeout |
| `Resilience.RetryExhausted` | All retry attempts failed |
| `Resilience.CircuitBroken` | Circuit breaker is open |

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
    .ExecuteAsync(() => _db.GetUser(id));
```

---

## 10. CSharpEssentials.EntityFrameworkCore — EF Core Integration

**What it is:** EF Core extensions that bring the Result pattern to database operations, plus pagination, audit interceptors, and CQRS context separation.

**Why it exists:** EF Core returns `null` from queries and throws on save failures. This package wraps those operations in `Result<T>`, provides automatic audit field population, and supports separating read/write contexts for CQRS architectures.

### Result-Returning Query Extensions

| Method | What It Does | Replaces |
|--------|-------------|----------|
| `FirstOrDefaultAsResultAsync<T>` | Returns `Result<T>` (NotFound on null) | `FirstOrDefaultAsync` + null check |
| `SingleOrDefaultAsResultAsync<T>` | Returns `Result<T>` (NotFound on null) | `SingleOrDefaultAsync` + null check |
| `FindAsResultAsync<T>` | Returns `Result<T>` from `Find` | `FindAsync` + null check |
| `SaveChangesAsResultAsync` | Returns `Result` wrapping save | try/catch around `SaveChangesAsync` |
| `MigrateDataAsync` | Runs migrations returning `Result` | Manual migration + exception handling |

### Pagination

| Method | What It Does |
|--------|-------------|
| `PaginateAsync(query, request)` | Offset-based pagination → `PaginationResponse<T>` |
| `PaginateAsync(query, cursorRequest)` | Cursor-based pagination → `CursorPaginationResponse<T>` |

### Entity Configuration

| Method | What It Does |
|--------|-------------|
| `EntityBaseMap()` | Configures audit field mappings for `EntityBase` |
| `EntityBaseGuidIdMap()` | Configures `Guid` ID mapping |
| `SoftDeletableEntityBaseMap()` | Configures soft-delete field mappings |
| `ApplySoftDeleteQueryFilter()` | Adds global `IsDeleted == false` filter |
| `MaybeConversion<T>()` | EF value conversion for `Maybe<T>` properties |
| `HasJsonConversion<T>()` | Stores complex properties as JSON |

### Interceptors

| Method | What It Does |
|--------|-------------|
| `AddAuditInterceptor` | Auto-fills `CreatedAt/By`, `UpdatedAt/By` on SaveChanges |
| `AddSlowQueryInterceptor` | Logs queries exceeding a threshold |

### CQRS Context Registration

| Method | What It Does |
|--------|-------------|
| `AddWriteDbContext` | Registers write-optimized context (tracking enabled) |
| `AddReadDbContext` | Registers read-optimized context (no tracking) |
| `AddCqrsDbContexts` | Registers both read and write contexts |
| `UseAsWriteContext()` | Configures context for write operations |
| `UseAsReadContext()` | Configures context for read operations (no tracking) |

---

## 11. CSharpEssentials.Json — Serialization Defaults

**What it is:** Pre-configured `System.Text.Json` options and custom converters.

**Why it exists:** Every project configures the same JSON settings: camelCase, lenient parsing, enum handling. This package provides sensible defaults and converters for polymorphic types and multi-format dates.

| Member | What It Does |
|--------|-------------|
| `EnhancedJsonSerializerOptions.DefaultOptions` | Pre-configured (camelCase, lenient) |
| `EnhancedJsonSerializerOptions.StrictOptions` | Strict mode options |
| `.DefaultOptionsWithDateTimeConverter` | Options with multi-format date parsing |
| `.Create(...)` | Factory with customization parameters |
| `ConvertToJson<T>()` | Extension — serialize to JSON string |
| `ConvertFromJson<T>()` | Extension — deserialize from JSON string |
| `PolymorphicJsonConverterFactory` | Handles polymorphic serialization |
| `MultiFormatDateTimeConverter` | Parses multiple date/time formats |
| `ConditionalStringEnumConverter` | Conditional enum to/from string |

---

## 12. CSharpEssentials.AspNetCore — API Layer

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

### Configuration

| Method | What It Does |
|--------|-------------|
| `AddEnhancedProblemDetails()` | Configures ProblemDetails in DI |
| `ConfigureModelValidatorResponse()` | Model validation errors as ProblemDetails |
| `ConfigureSystemTextJson()` | Configures JSON serialization |

### API Versioning

| Method | What It Does |
|--------|-------------|
| `AddAndConfigureApiVersioning()` | Registers API versioning services |
| `CreateVersionSet()` | Creates version set for Minimal APIs |
| `CreateVersionedGroup()` | Creates versioned route group |
| `AddSwagger()` / `UseVersionableSwagger()` | Swagger with version support |

---

## 13. CSharpEssentials.Mediator — Pipeline Behaviors

**What it is:** MediatR pipeline behaviors for cross-cutting concerns: validation, logging, caching, and transactions.

**Why it exists:** CQRS handlers often need the same cross-cutting logic — validate input, log execution, cache results, wrap in a transaction. Pipeline behaviors apply these concerns declaratively via marker interfaces rather than repeating code in every handler.

### Behaviors

| Behavior | Marker Interface | What It Does |
|----------|-----------------|-------------|
| `ValidationBehavior` | — (auto for all) | Runs FluentValidation before handler; returns `Result.Failure` with validation errors |
| `LoggingBehavior` | `ILoggableRequest` | Logs request/response details |
| `CachingBehavior` | `ICacheable` | Caches handler responses using `CacheKey` and `CacheDuration` |
| `TransactionScopeBehavior` | `ITransactionalRequest` | Wraps handler execution in `TransactionScope` |

### Registration

| Method | What It Does |
|--------|-------------|
| `AddMediatorBehaviors()` | Registers all four behaviors |
| `AddMediatorValidationBehavior()` | Registers validation only |
| `AddMediatorLoggingBehavior()` | Registers logging only |
| `AddMediatorCachingBehavior()` | Registers caching only |
| `AddMediatorTransactionBehavior()` | Registers transaction only |

---

## 14. CSharpEssentials.Enums — Source-Generated String Enums

**What it is:** A Roslyn source generator that produces fast, AOT-safe enum-to-string and string-to-enum methods.

**Why it exists:** `Enum.ToString()` and `Enum.Parse()` use reflection, which is slow and incompatible with NativeAOT trimming. The `[StringEnum]` attribute triggers compile-time generation of switch-based conversion methods.

```csharp
[StringEnum]
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered
}

// Generated methods (no reflection):
string str = OrderStatus.Pending.ToStringFast();     // "Pending"
bool ok = OrderStatusExtensions.TryParse("Shipped", out var status);
bool defined = OrderStatusExtensions.IsDefined("Processing");
IReadOnlyList<OrderStatus> all = OrderStatusExtensions.GetValues();
```

---

## 15. CSharpEssentials.Time — Testable Clock

**What it is:** An `IDateTimeProvider` interface that wraps the system clock for testability.

**Why it exists:** Code that calls `DateTime.UtcNow` directly is untestable for time-dependent logic. Injecting `IDateTimeProvider` lets tests control time.

| Type/Method | What It Does |
|-------------|-------------|
| `IDateTimeProvider` | Interface: `DateTimeOffset UtcNow { get; }` |
| `DateTimeProvider` | Default implementation using system clock |
| `ToTimeOnly()` | Extension: `DateTime`/`DateTimeOffset` to `TimeOnly` |
| `ToDateOnly()` | Extension: `DateTime`/`DateTimeOffset` to `DateOnly` |

---

## 16. CSharpEssentials.Clone — Deep Copy

**What it is:** Deep cloning via JSON serialization.

| Type/Method | What It Does |
|-------------|-------------|
| `ICloneable<T>` | Interface with `T Clone()` method |
| `collection.Clone<T>()` | Deep-clones `IEnumerable<T>` via JSON round-trip |
| `queryable.Clone<T>()` | Deep-clones `IQueryable<T>` results |

---

## 17. CSharpEssentials.RequestResponseLogging — HTTP Logging Middleware

**What it is:** ASP.NET Core middleware that logs HTTP request and response bodies.

| Member | What It Does |
|--------|-------------|
| `AddRequestResponseLogging()` | Registers the middleware in DI and pipeline |
| `[SkipRequestLogging]` | Attribute to opt out of request body logging |
| `[SkipResponseLogging]` | Attribute to opt out of response body logging |
| `[SkipRequestResponseLogging]` | Attribute to opt out of both |

---

## 18. CSharpEssentials.GcpSecretManager — Secret Configuration

**What it is:** Plugs Google Cloud Secret Manager into the .NET `IConfiguration` system.

| Type | What It Does |
|------|-------------|
| `SecretManagerConfigurationSource` | `IConfigurationSource` for Secret Manager |
| `SecretManagerConfigurationProvider` | Loads secrets as configuration values |
| `SecretManagerConfigurationOptions` | Options: project ID, filters, refresh interval |

---

## 19. CSharpEssentials.Validation — Model-First Validation

**What it is:** A high-performance, model-first validation library that returns `Result<T>` natively.

**Why it exists:** FluentValidation uses expression trees and reflection at runtime. `CSharpEssentials.Validation` is zero-reflection — no expression tree evaluation, no deferred rule builds. Validators receive the model directly; property names are inferred at startup via `nameof`-equivalent extraction. Errors flow as `Result<T>` without exceptions or secondary return channels.

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

Result<CreateUserCommand> result = await validator.ValidateAsync(command);
// error codes: "Email.NotEmpty", "Name.MaxLength", "Age.GreaterThan"
```

**Inline (static) usage** — for one-off validations without a dedicated class:

```csharp
// Sync delegate — zero heap allocation
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, (m, rules) =>
{
    rules.For(() => m.Email).NotEmpty().EmailAddress();
    rules.For(() => m.Name).NotEmpty().MaxLength(100);
});

// Async delegate — when MustAsync or SetValidatorAsync is needed
Result<CreateUserCommand> result = await Validator.ValidateAsync(command, async (m, rules, ct) =>
{
    rules.For(() => m.Name).NotEmpty();
    await rules.For(() => m.Email)
               .MustAsync(async (email, c) => await _db.IsUniqueAsync(email, c),
                          "Email.NotUnique", "Email is already taken.", c);
}, cancellationToken);
```

`Validator.ValidateAsync` (static utility class) and `Validator<T>` (abstract base class) are two independent types defined in the same file — the static form does not delegate to `Validator<T>` internally.

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

All comparable validators work on nullable value types — `null` is silently skipped.

```csharp
rules.For(() => model.ExpiresAt).GreaterThan(DateTime.UtcNow);
// null → no error   |   value < now → error
```

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

`SetValidatorAsync` works with both non-nullable and nullable reference type properties — no null-forgiving operator (`!`) required. `null` values are skipped automatically.

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

`Configure` receives the live model — any C# control flow works directly. No `When()`/`Unless()` DSL needed.

```csharp
protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
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
```

### Validator Composition

```csharp
public class PaidOrderValidator : Validator<Order>
{
    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        await Include(new BaseOrderValidator(), model, rules, ct);   // merge base rules
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

Default lifetime: `Scoped`. Pass a `lifetime` parameter to override. Multiple `IValidator<T>` registrations for the same `T` are supported — `ValidationBehavior` aggregates and deduplicates results from all of them.

### Validator Ordering

Override `Order` on `Validator<T>` to control execution sequence when multiple validators target the same model. Validators sharing the same `Order` run concurrently; groups with lower `Order` complete before higher-`Order` groups begin. All groups execute regardless of earlier failures — errors are accumulated and deduplicated.

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
| `taskResult.ValidateWithAsync(validator, ct)` | `Task<Result<T>>` | `ValueTask<Result<T>>` | Awaited task pipeline |
| `valueTaskResult.ValidateWithAsync(validator, ct)` | `ValueTask<Result<T>>` | `ValueTask<Result<T>>` | ValueTask pipeline |

```csharp
// Named validator — plugs straight into a Result<T> chain
Result<CreateUserCommand> result = await ParseCommand(input)
    .ValidateWithAsync(new CreateUserCommandValidator(), ct);

// Inline validation — no dedicated class needed
Result<CreateUserCommand> result = await ParseCommand(input)
    .ValidateWithAsync(command, (m, rules) =>
    {
        rules.For(() => m.Email).NotEmpty().EmailAddress();
        rules.For(() => m.Name).NotEmpty().MaxLength(100);
    });

// Works on Task<Result<T>> — no intermediate await
Result<Order> order = await GetOrderAsync(id)           // Task<Result<Order>>
    .ValidateWithAsync(new OrderValidator(), ct);        // skips if already failed
```

Short-circuits immediately: if `result.IsFailure` before validation runs, the existing errors pass through and the validator is never invoked. This makes it safe to chain multiple `ValidateWithAsync` calls without nested null/failure checks.

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
