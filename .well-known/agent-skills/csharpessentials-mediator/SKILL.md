---
name: csharpessentials-mediator
description: Use when adding cross-cutting pipeline behaviors to CQRS handlers. Covers ValidationBehavior (CSharpEssentials.Validation; modes Enforce/LogOnly/Off, failure observers; Result failure or EnhancedValidationException), LoggingBehavior (ILoggableRequest), ExceptionHandlingBehavior (converts exceptions to Result.Failure for Result-returning handlers), CachingBehavior (ICacheable with IDistributedCache), TransactionScopeBehavior and TransactionBehavior (ITransactionalRequest, commits only on success) and LockBehavior (ILockedRequest).
---

# CSharpEssentials.Mediator

Pipeline behaviors for the Mediator source-generator library. Register cross-cutting concerns (validation, logging, caching, transactions) once; they run automatically for every matching handler.

> Built on the **Mediator** source-generator NuGet package, not MediatR.

## Installation

```bash
dotnet add package CSharpEssentials.Mediator
```

## Namespace

```csharp
using CSharpEssentials.Mediator;   // ICacheable, ILoggableRequest, ITransactionalRequest, ILockedRequest, ValidationMode, behaviors
using Microsoft.Extensions.DependencyInjection;   // AddMediatorBehaviors and the other AddMediator* methods
```

## Register Behaviors

```csharp
// Program.cs
builder.Services.AddMediator();           // Mediator source generator
builder.Services.AddMediatorBehaviors();  // all 5 behaviors: validation, logging, exception handling, caching, transaction scope

// Or selectively
builder.Services.AddMediatorValidationBehavior();
builder.Services.AddMediatorLoggingBehavior();
builder.Services.AddMediatorExceptionHandlingBehavior();
builder.Services.AddMediatorCachingBehavior();
builder.Services.AddMediatorTransactionBehavior();
```

Register `ValidationBehavior` first: invalid requests should never reach the handler. Selective registration runs behaviors in registration order. For Native AOT use `options.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors` in `AddMediator(...)` and `services.AddMediatorValidationOptions()` instead of the open-generic registrations.

---

## ValidationBehavior (CSharpEssentials.Validation)

Runs registered validators before the handler. On failure, the handler is never invoked. Errors are surfaced based on the handler return type:

| `TResponse` | Failure result |
|-------------|---------------|
| `Result` | `Result.Failure(errors)` returned directly |
| `Result<T>` | `Result<T>.Failure(errors)` returned directly |
| Any other type | `EnhancedValidationException` thrown, caught by `GlobalExceptionHandler` (400 ProblemDetails) |

```csharp
public class CreateOrderValidator : Validator<CreateOrderCommand>
{
    protected override ValueTask Configure(CreateOrderCommand model, RuleContext<CreateOrderCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.UserId).NotEqual(Guid.Empty);
        rules.For(() => model.Amount).GreaterThan(0m);
        return ValueTask.CompletedTask;
    }
}

// using CSharpEssentials.Validation; using CSharpEssentials.Validation.Validators; using CSharpEssentials.Validation.Extensions;
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

Validation modes (`ValidationBehaviorOptions.DefaultMode`, or per request with `IValidationModeOverride`): `Enforce` (default) rejects invalid requests, `LogOnly` runs the validators and notifies observers but lets the request continue, `Off` skips validation.

```csharp
builder.Services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.LogOnly);
```

Every `IValidationFailureObserver` in DI is called on a failure in `Enforce` and `LogOnly` mode; the built-in `LoggingValidationFailureObserver` logs it and increments the `cse.mediator.validation.failures` counter on the `CSharpEssentials.Mediator` meter. A validator that throws (other than `OperationCanceledException`) becomes a `Validator.Exception` error.

---

## LoggingBehavior (ILoggableRequest)

Interface hierarchy:

```csharp
// ILoggableRequest        — base marker
// IRequestLoggable        — logs request body only
// IResponseLoggable       — logs response body only
// IRequestResponseLoggable — logs both
```

```csharp
public record GetUserQuery(Guid UserId)
    : IQuery<Result<UserDto>>, IRequestResponseLoggable;

public record SendEmailCommand(string To, string Body)
    : ICommand<Result>, IRequestLoggable;  // response has no PII — log request only
```

---

## ExceptionHandlingBehavior: automatic for Result-returning handlers

Registered by `AddMediatorBehaviors()` or `AddMediatorExceptionHandlingBehavior()` as a singleton pipeline behavior between `LoggingBehavior` and `CachingBehavior`. When a handler throws, the behavior catches the exception and converts it to `Result.Failure(Error.Exception(ex))`. This keeps the caller on the Result railway instead of forcing a try/catch at every call site. `OperationCanceledException` always propagates and is never caught.

No marker interface is needed: once the behavior is registered it applies to any handler whose `TResponse` is `Result` or `Result<T>`. Handlers returning other types (plain DTOs, etc.) pass through with zero overhead.

### Pipeline execution order

| Position | Behavior | Activation |
|----------|----------|-----------|
| 1 | `ValidationBehavior` | Every request |
| 2 | `LoggingBehavior` | `ILoggableRequest` |
| 3 | `ExceptionHandlingBehavior` | `Result` / `Result<T>` return types |
| 4 | `CachingBehavior` | `ICacheable` |
| 5 | `TransactionScopeBehavior` | `ITransactionalRequest` |

### Error shape

`Error.Exception(ex)` produces an error with:
- `ErrorType`: `Failure`
- `Code`: exception type name (e.g. `"InvalidOperationException"`)
- `Description`: exception message

```csharp
// No interface needed — applied to all handlers returning Result or Result<T> once the behavior is registered
public record ProcessPaymentCommand(Guid OrderId, decimal Amount)
    : ICommand<Result>;

public class ProcessPaymentHandler : ICommandHandler<ProcessPaymentCommand, Result>
{
    private readonly IPaymentClient _paymentGateway;

    public ProcessPaymentHandler(IPaymentClient paymentGateway)
        => _paymentGateway = paymentGateway;

    public async ValueTask<Result> Handle(ProcessPaymentCommand command, CancellationToken ct)
    {
        // If this throws, ExceptionHandlingBehavior converts it to Result.Failure(Error.Exception(ex))
        // instead of letting the exception propagate to the caller.
        await _paymentGateway.ChargeAsync(command.OrderId, command.Amount, ct);
        return Result.Success();
    }
}

// Caller always receives a Result — no try/catch needed
Result result = await mediator.Send(new ProcessPaymentCommand(orderId, 99.99m));
if (result.IsFailure)
{
    // result.FirstError.Code        => "HttpRequestException"
    // result.FirstError.Description => "Payment gateway timed out"
}
```

---

## CachingBehavior (ICacheable)

Requires `IDistributedCache` registration.

```csharp
public record GetProductQuery(int ProductId)
    : IQuery<Result<ProductDto>>, ICacheable
{
    public bool BypassCache   => false;   // true: skip the cache entirely (no lookup, no store)
    public bool CacheFailures => false;   // never cache error results
    public string CacheKey    => $"product:{ProductId}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);   // zero or negative: no expiration
}

// Requires an IDistributedCache backend (in-memory here; Redis or SQL Server in production)
builder.Services.AddDistributedMemoryCache();
```

---

## TransactionScopeBehavior (ITransactionalRequest)

Wraps the handler in a `TransactionScope` created with `TransactionScopeAsyncFlowOption.Enabled` (default isolation level). The scope completes only when the handler returns a successful `Result` / `Result<T>` (or a response that is not a `Result`); a failed `Result` or an exception rolls it back.

```csharp
public record PlaceOrderCommand(OrderDto Order)
    : ICommand<Result<Guid>>, ITransactionalRequest;
// No members to implement on ITransactionalRequest
```

To run the handler through your own data access transaction instead (for example EF Core), register `TransactionBehavior` and an `ITransactionRunner` (namespace `CSharpEssentials.Transactions`). It replaces `TransactionScopeBehavior` in the pipeline and commits under the same rule:

```csharp
services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>(); // CSharpEssentials.EntityFrameworkCore
```

---

## LockBehavior (ILockedRequest)

Runs one handler at a time per key. The in-process default serializes within one process only; register a distributed `IResourceLock` for several instances.

```csharp
public record CapturePaymentCommand(Guid OrderId) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"order:{OrderId}";
    public TimeSpan? LockTimeout => TimeSpan.FromSeconds(5); // optional; null waits until cancelled
}

services.AddMediatorBehaviors();
services.AddMediatorLockBehavior(); // call after the other behaviors; LockPlacement.OutsideTransaction by default
```

On a lock timeout a `Result` handler gets a failed result (`Error.Exception(TimeoutException)`), because `LockBehavior` runs inside `ExceptionHandlingBehavior`. Use `LockPlacement.InsideTransaction` only with transaction-scoped locks such as `pg_advisory_xact_lock`.

---

## Best Practices

- Register `ValidationBehavior` first: invalid requests should never reach the handler
- Register `ExceptionHandlingBehavior` (included in `AddMediatorBehaviors()`); it then applies to `Result` / `Result<T>` handlers, so do not add try/catch inside handlers that already return `Result`
- Set `CacheFailures = false`; transient failures should not be cached
- `ITransactionalRequest` only on commands writing to multiple tables in one operation; return a failed `Result` or throw to roll back
- Use `IRequestLoggable` (not `IRequestResponseLoggable`) when the response contains PII
