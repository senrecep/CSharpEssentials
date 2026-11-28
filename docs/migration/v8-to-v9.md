# Migrating from 8.x to 9.0

## Breaking changes

- `Error.Exception(...)` defaults to `ErrorType.Unexpected` instead of `ErrorType.Failure`, in all three overloads.
- `ErrorType` has a new member, `Domain` (numeric value 8), which maps to HTTP 422. The values of the existing members do not change.
- `CSharpEssentials.AspNetCore`: with `ExposeExceptionDetails` off (the default), a problem with status 500 or higher no longer shows error descriptions or exception metadata to the client. See [5xx problems hide server details](#5xx-problems-hide-server-details-csharpessentialsaspnetcore).
- `CSharpEssentials.Http` maps a 422 response to `ErrorType.Domain` instead of `ErrorType.Validation`.
- `ErrorType.Unknown` is the `[EnumFallback]` member: data reads map a type name or number they do not know to `Unknown`.
- `CSharpEssentials.Validation` stores the property path under the `field` metadata key on every error its built-in rules and `AddError(code, message)` record, so those errors no longer equal an `Error` built without metadata, and the AspNetCore `AllErrors` output (also `All` and `UseLegacyDefaults()`) writes that metadata.
- Request input bound through the AspNetCore enum conventions (`Input` mode) no longer accepts `unknown` or `7` for an `ErrorType` value and returns 400, and the OpenAPI description marks `Unknown` as response-only.
- `CSharpEssentials.AspNetCore`: `POST`, `PUT`, `PATCH` and `DELETE` operations with `WithIfMatch()` or `[IfMatch]` declare `400` and `412` `application/problem+json` responses in OpenAPI, plus `428` when the header is required. MVC actions get them with `AddConditionalRequests()`, from the attribute or from `MapControllers().WithIfMatch()`. A status the operation already declares is kept. Regenerate API clients and update OpenAPI snapshot tests.
- `CSharpEssentials.Core`: `Guider.NewGuid` has overloads that take a `DateTimeOffset` and, on .NET 8 and later, a `TimeProvider`. An XML doc reference `<see cref="Guider.NewGuid"/>` is now ambiguous (CS0419, an error with warnings as errors); write `Guider.NewGuid()`.
- `CSharpEssentials.Resilience`: `ResiliencePolicy.WithRetry` and `ResiliencePolicy<T>.WithRetry` take two more optional parameters, `useJitter` and `maxDelay`. Source that calls them compiles unchanged; an assembly compiled against 8.x must be recompiled (`MissingMethodException` otherwise).
- `CSharpEssentials.Resilience`: every `RetryIfFailed` overload (with and without `shouldRetry`, for `Result` and `Result<T>`) takes two more optional parameters, `useJitter` and `maxDelay`, before `cancellationToken`. A call that passes the token by name compiles unchanged; one that passes it positionally after `exponentialBackoff` must name it. An assembly compiled against 8.x must be recompiled (`MissingMethodException` otherwise).
- `CSharpEssentials.Json`: `EnhancedJsonSerializerOptions.StrictOptions` requires every constructor parameter that is a non-nullable value type and has no default (`JsonTypeInfoModifiers.RequireValueTypeConstructorParameters`). A payload that omits such a property now throws `JsonException` instead of binding `0`, the first enum member or `default`. Give the parameter a default value or make it nullable to keep it optional. A property without a setter (get-only, assigned in the constructor) stays optional.
- `CSharpEssentials.Time`: `DateTimeProvider.TimeZone` returns the `LocalTimeZone` of the wrapped `TimeProvider` instead of always `TimeZoneInfo.Local`. With `TimeProvider.System` nothing changes. Over `FakeTimeProvider` it is UTC unless the test calls `SetLocalTimeZone`, and `GetAge(dateTimeProvider)` follows it.
- `CSharpEssentials.Results`: `default(Result)` and `default(Result<T>)` are no longer successes. They report a failure with one `Error.Unexpected` coded `Result.Default`, and every member that passes the errors to a delegate or builds a new failure from them throws `InvalidOperationException` on them. See [Default results fail closed](#default-results-fail-closed-csharpessentialsresults-csharpessentialsrules).
- `CSharpEssentials.Rules`: a custom `IOrRule<TContext, TResult>` with an empty `Rules` array returns `EMPTY_RULE_ARRAY` like the generic And rule, instead of a wrapped `RuleEngine.Evaluate.OrRule` error. Calling `Evaluate` directly on a generic rule built by `.And()` or `.Or()` runs the whole rule instead of returning `default`.
- `CSharpEssentials.Entity`: the new public record `CSharpEssentials.Entity.DomainEvent` clashes with a `DomainEvent` type of your own when a file imports both namespaces (CS0104). Qualify the name or add an alias such as `using DomainEvent = MyApp.Domain.DomainEvent;`.
- `CSharpEssentials.EntityFrameworkCore`: `IDomainEventOutbox.StoreAsync` takes the saving `DbContext` as its first parameter: `StoreAsync(DbContext context, IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken)`. Add the parameter to your implementations. Rows you add to `context` are written by the save in progress, so remove any `SaveChanges` call from the outbox.
- `CSharpEssentials.EntityFrameworkCore`: with `DispatchDomainEventsOnSaveChanges` and a registered `IDomainEventOutbox`, `BaseDbContext` stores `AfterSave` events in the outbox before the write, in the same `SaveChanges`, instead of after the save. When `StoreAsync` throws, the entities are no longer saved. When the outbox or the save throws, the rows the outbox added are detached. `DispatchDomainEventsAsync` is no longer called with outbox-bound `AfterSave` events, so an override that stored them itself no longer sees them.
- `CSharpEssentials.Results`: on netstandard2.1 the `System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute` polyfill is internal and targets methods and properties, like the .NET type. A netstandard2.1 project that used the attribute through a reference to Results must declare its own polyfill or target .NET 5 or later.

## Default results fail closed (`CSharpEssentials.Results`, `CSharpEssentials.Rules`)

In 8.x `IsSuccess` was `_errors is null`, so an uninitialized `Result` or `Result<T>` (a field never assigned, an array slot, `default`) read as a success. For a value-type `T` its `Value` was `0`, and `EnsureNotNull` could not catch it. 9.0 stores success as an empty error array, so `default` is a third state that behaves like this:

| Member on `default(Result)` / `default(Result<T>)` | 8.x | 9.0 |
|---|---|---|
| `IsSuccess` / `IsFailure` | `true` / `false` | `false` / `true` |
| `Errors`, `ErrorsOrEmptyArray`, `FirstError`, `LastError` | the success placeholders (`Error.NoErrors`, `Error.NoFirstError`, `Error.NoLastError`) and `[]` | one `Error.Unexpected("Result.Default", ...)` |
| `Value` | `default(T)` | `default(T)` |
| `Match`, `MatchFirst`, `Switch`, `Map`, `Bind`, generic `Then`, `MapError`, `Else`, `TapError`, `ElseDo`, `Compensate`, `Recover`, sync and async, including the `Task`/`ValueTask` receiver overloads | ran the success path | throw `InvalidOperationException` |
| `Result.Then`, `ThenDo`, `Tap`, `Ensure` | ran the success path | return the `default` unchanged |
| `Result.And`, `Partition`, `Combine` | treated it as a success | collect the `Result.Default` error |
| `TryGet`, `ToString`, `Equals`, JSON | success | failure with `Result.Default` |
| `default(Result) == Result.Success()` | `true` | `false` |
| `ToProblemDetails()`, `ToProblemResult()` | threw (success) | 500 |

The struct size and the allocation-free success and failure paths do not change. `Result.Success()`, `(Result)true` and every `Result<T>` built from a value are successes as before.

What to change:

- Replace `return default;` or `default(Result<T>)` used as "success" with `Result.Success()` or a value. For a `Result<T>` with no meaningful value, return a real value or move to `Result`.
- Tests that pinned `default(Result<string>).ToString()` as `"Success: null"` now get the `Result.Default` failure text. A success with a `null` value is still possible through JSON (`{"IsSuccess":true,"Value":null}`) and prints `"Success: null"`.
- The Rules And and Or adapters used `default(Result<TResult>)` as their own success gate. They no longer have a gate: `RuleEngine` evaluates the children directly, and a direct `Evaluate` call on them returns the combined result.

## Exceptions become `Unexpected` (`CSharpEssentials.Errors`)

`Error.Exception(ex)`, `Error.Exception(code, ex)` and `Error.Exception(code, description, ex)` now give `ErrorType.Unexpected` when you do not pass a type. The status stays 500, so the HTTP response does not change. What changes is everything that reads the type:

- The problem title becomes `Unexpected Error` instead of `Server Failure`.
- `Result.TryCatch`, the Mediator `ExceptionHandlingBehavior`, a throwing validator in `ValidationBehavior` and a throwing rule in `CSharpEssentials.Rules` return `Unexpected` errors.
- The Mediator `IdempotencyBehavior` keeps a stored failure whose first error is `Unexpected` for the failure retention (`IdempotencyExecutorOptions.FailureRetention`), not for the success retention. This covers a handler that returns an `Unexpected` failure. A thrown exception is different: with the default order, `ExceptionHandlingBehavior` sits outside `IdempotencyBehavior`, so nothing is stored and a retry before the lease ends gets `Idempotency.InFlight` (see the Mediator Readme).
- Code that tested `error.Type == ErrorType.Failure` to find exceptions should test `ErrorType.Unexpected`, or check the `exceptionType` metadata key. That key is only there on the server: a 5xx problem response strips it (see the next section).

The default is a compile-time value. An assembly compiled against 8.x keeps passing `Failure` until it is recompiled. Pass the type explicitly to keep the old behavior:

```csharp
Error error = Error.Exception(ex, ErrorType.Failure);
```

## 5xx problems hide server details (`CSharpEssentials.AspNetCore`)

When the status of a problem is 500 or higher and `EnhancedProblemDetailsOptions.ExposeExceptionDetails` is `false` (the default), the problem details factory redacts it:

- `detail` becomes `An unexpected error occurred.`
- Every error in `errors` whose own status is 500 or higher gets that text as its description. 4xx errors in the same response keep theirs.
- The metadata keys `exception`, `exceptionType`, `exceptionMessage`, `exceptionStackTrace`, `innerException` and `stackTrace` are removed from each error and from the problem extensions.

This hits every error that maps to 500: `Error.Failure`, `Error.Unexpected`, `Error.Exception` and `ErrorType.Unknown`. A client that read the description of a `Failure` error gets the generic text now. Give business-rule errors a 4xx type the client is meant to read (`Domain`, `Validation`, `Conflict`, `Forbidden`). To keep the old output in development, turn the option on there:

```csharp
builder.Services.AddEnhancedProblemDetails(o => o.ExposeExceptionDetails = builder.Environment.IsDevelopment());
```

## `ErrorType.Domain` and HTTP 422

`Domain` means a business rule rejected a well-formed request: a declined payment, an order that can no longer be cancelled. Create it with `Error.Domain(code, description, metadata)`.

| Where | 8.x | 9.0 |
|---|---|---|
| `ErrorType.Domain.ToHttpStatusCode()` | (no member) | 422 |
| `422.ToErrorType()` | `Unexpected` | `Domain` |
| `HttpStatusCodeMapper.ToErrorType(HttpStatusCode.UnprocessableEntity)` | `Validation` | `Domain` |
| `HttpStatusCodeMapper.ToError(HttpStatusCode.UnprocessableEntity)` | `Error.Validation` | `Error.Domain` |
| Problem title | | `Domain Error` |
| Resilience retry | | not retried, like `Validation` |

A business-rule rejection that used `Failure` reaches the client as a 500 with the generic text (see above). Move it to `Error.Domain` if the client should read the description.

When a response carries several errors, the default `DefaultErrorStatusCodeMapper.SelectPrimaryError` picks the one with the highest status, so a `Domain` error (422) wins over `Conflict` (409) and the 4xx below it, and loses to any 5xx.

An HTTP client that handled a 422 as `ErrorType.Validation` must match `ErrorType.Domain` instead. `IdempotentRetry` only retries a 409 with the code `Idempotency.InFlight`, so it is not affected.

## Payloads written by another version

`ErrorType` is a `[StringEnum]`, and an `Error` carries both `Type` and `NumericType`.

- 8.x payloads read by 9.0: every 8.x name and number still exists, so nothing changes.
- 9.0 payloads read by 8.x: 8.x does not know `Domain` or the number 8. Readers that use the enum conventions of `CSharpEssentials.Json` in `Data` mode reject the unknown name, because 8.x has no fallback member. A reader that takes the plain number gets the undefined value 8, which the 8.x `ToHttpStatusCode` maps to 500. Roll the 9.0 packages out to every reader of cached, stored or idempotency payloads before a writer starts producing `Domain` errors.
- From 9.0, a name or number a reader does not know maps to `Unknown` (HTTP 500) in `Data` mode, so the next new member will not break readers.
- EF Core columns that store `ErrorType` through `EnumWireNameConverter` or `EnumIntegerConverter` also map an unknown stored value to `Unknown` instead of throwing. Saving the entity again writes `unknown` over the original value.

## Validation errors carry a `field` (`CSharpEssentials.Validation`, `CSharpEssentials.AspNetCore`)

A failed built-in rule records `Error.Validation("Name.NotEmpty", "...", new ErrorMetadata("field", "Name"))`. `AddError(code, message)` does the same. `AddError(Error)` and rules given an `Error` keep the error as passed. The AspNetCore JSON body mapper, `ConfigureInvalidModelStateResponse()` and `ValidateModelAttribute` set `field` too.

- `Error` equality and hashing compare metadata. An expected `Error.Validation("Name.NotEmpty", "...")` with no metadata is no longer equal to the recorded error. Compare `Code` and `Description`, or build the expected error with the same `field` metadata.
- With `ProblemErrorFields.AllErrors` (part of `All` and `UseLegacyDefaults()`) the `errors` array writes `"metadata": { "field": "Name" }` for these errors where 8.x wrote `"metadata": null`.
- `ValidationErrorsFormat.ByField` is new and opt-in. It groups `errors` by `field`, run through the HTTP JSON naming policy.

## Check the new defaults

This program prints the 9.0 behavior of the changes above that need no web host.

```csharp Program.cs
// dotnet add package CSharpEssentials.Results
// dotnet add package CSharpEssentials.Errors
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result uninitialized = default;
Console.WriteLine($"default(Result).IsFailure: {uninitialized.IsFailure}");
Console.WriteLine($"FirstError.Code: {uninitialized.FirstError.Code}");
Console.WriteLine($"default == Success(): {uninitialized == Result.Success()}");

try
{
    uninitialized.Match(onSuccess: () => "ok", onFailure: errors => errors[0].Code);
}
catch (InvalidOperationException)
{
    Console.WriteLine("Match on default throws InvalidOperationException");
}

Error fromException = Error.Exception(new TimeoutException("db timeout"));
Console.WriteLine($"Error.Exception type: {fromException.Type}");
Console.WriteLine($"Domain status: {ErrorType.Domain.ToHttpStatusCode()}");
Console.WriteLine($"422 maps to: {422.ToErrorType()}");
```

## New: typed results (`CSharpEssentials.Results`, `CSharpEssentials.Errors`)

`Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>` are new and opt-in; nothing above depends on them. Matching on them (any type pattern) needs C# 15; implicit returns, `Match`, `Switch`, `TryGetValue` and `ToResult` work at any language version. For C# 15, a net11.0 project has it by default, and a project on an older target framework needs the .NET 11 SDK and `<LangVersion>15</LangVersion>`. See [Typed errors](../../CSharpEssentials.Results/Readme.MD#typed-errors-resultt-e1-resultt-e1-e2-resultt-e1-e2-e3).

An existing catalog of `Error` fields does not convert into a typed slot. [Moving a catalog to typed kinds](../../CSharpEssentials.Errors/Readme.MD#moving-a-catalog-to-typed-kinds) shows the rewrite.
