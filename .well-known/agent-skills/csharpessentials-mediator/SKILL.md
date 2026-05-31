---
name: csharpessentials-mediator
description: Use when adding cross-cutting pipeline behaviors to CQRS handlers — ValidationBehavior (CSharpEssentials.Validation, throws EnhancedValidationException), LoggingBehavior (ILoggableRequest), ExceptionHandlingBehavior (auto-converts exceptions to Result.Failure for Result-returning handlers), CachingBehavior (ICacheable with IDistributedCache), and TransactionScopeBehavior (ITransactionalRequest).
---

# CSharpEssentials.Mediator

Pipeline behaviors for the Mediator source-generator library. Register cross-cutting concerns (validation, logging, caching, transactions) once — they run automatically for every matching handler.

> Built on the **Mediator** source-generator NuGet package — not MediatR.

## Installation

```bash
dotnet add package CSharpEssentials.Mediator
```

## Namespace

```csharp
using CSharpEssentials.Mediator;   // ICacheable, ILoggableRequest, ITransactionalRequest
using Microsoft.Extensions.DependencyInjection;
```

## Register Behaviors

```csharp
// Program.cs
builder.Services.AddMediator();           // Mediator source generator
builder.Services.AddMediatorBehaviors();  // all 5 behaviors

// Or selectively
builder.Services.AddMediatorValidationBehavior();
builder.Services.AddMediatorLoggingBehavior();
builder.Services.AddMediatorExceptionHandlingBehavior();
builder.Services.AddMediatorCachingBehavior();
builder.Services.AddMediatorTransactionBehavior();
```

Register `ValidationBehavior` first — invalid requests should never reach the handler.

---

## ValidationBehavior — CSharpEssentials.Validation

Runs registered validators before the handler. On failure, the handler is never invoked. Errors are surfaced based on the handler return type:

| `TResponse` | Failure result |
|-------------|---------------|
| `Result` | `Result.Failure(errors)` returned directly |
| `Result<T>` | `Result<T>.Failure(errors)` returned directly |
| Any other type | `EnhancedValidationException` thrown — caught by `GlobalExceptionHandler` |

```csharp
public class CreateOrderValidator : Validator<CreateOrderCommand>
{
    protected override ValueTask Configure(CreateOrderCommand model, RuleContext<CreateOrderCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerId).NotEmpty();
        rules.For(() => model.Total).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

---

## LoggingBehavior — ILoggableRequest

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

## ExceptionHandlingBehavior — automatic for Result-returning handlers

Registered as a singleton pipeline behavior between `LoggingBehavior` and `CachingBehavior`. When a handler throws, the behavior catches the exception and converts it to `Result.Failure(Error.Exception(ex))` — keeping the caller on the Result railway instead of forcing a try/catch at every call site. `OperationCanceledException` always propagates and is never caught.

No interface needed — the behavior activates automatically for any handler whose `TResponse` is `Result` or `Result<T>`. Handlers returning other types (plain DTOs, etc.) pass through with zero overhead.

### Pipeline execution order

| Position | Behavior | Activation |
|----------|----------|-----------|
| 1 | `ValidationBehavior` | Auto (all handlers) |
| 2 | `LoggingBehavior` | `ILoggableRequest` |
| 3 | `ExceptionHandlingBehavior` | Auto (`Result` / `Result<T>` return types) |
| 4 | `CachingBehavior` | `ICacheable` |
| 5 | `TransactionScopeBehavior` | `ITransactionalRequest` |

### Error shape

`Error.Exception(ex)` produces an error with:
- `ErrorType`: `Failure`
- `Code`: exception type name (e.g. `"InvalidOperationException"`)
- `Description`: exception message

```csharp
// No interface needed — automatically applied to all handlers returning Result or Result<T>
public record ProcessPaymentCommand(Guid OrderId, decimal Amount)
    : ICommand<Result>;

public class ProcessPaymentHandler : ICommandHandler<ProcessPaymentCommand, Result>
{
    private readonly IPaymentGateway _paymentGateway;

    public ProcessPaymentHandler(IPaymentGateway paymentGateway)
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
    // result.Error.Code        => "HttpRequestException"
    // result.Error.Description => "Payment gateway timed out"
}
```

---

## CachingBehavior — ICacheable

Requires `IDistributedCache` registration.

```csharp
public record GetProductQuery(int ProductId)
    : IQuery<Result<ProductDto>>, ICacheable
{
    public bool BypassCache   => false;
    public bool CacheFailures => false;   // never cache error results
    public string CacheKey    => $"product:{ProductId}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}

// Requires a cache backend
builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
// or:
builder.Services.AddDistributedMemoryCache();
```

---

## TransactionBehavior — ITransactionalRequest

Wraps the handler in `TransactionScope` (ReadCommitted + AsyncFlowEnabled). Commits on success, rolls back on failure or exception.

```csharp
public record PlaceOrderCommand(OrderDto Order)
    : ICommand<Result<Guid>>, ITransactionalRequest;
// No members to implement on ITransactionalRequest
```

---

## Best Practices

- Register `ValidationBehavior` first — invalid requests should never reach the handler
- `ExceptionHandlingBehavior` requires no setup; it activates automatically for `Result` / `Result<T>` handlers — do not add try/catch inside handlers that already return `Result`
- Set `CacheFailures = false` — transient failures should not be cached
- `ITransactionalRequest` only on commands writing to multiple tables in one operation
- Use `IRequestLoggable` (not `IRequestResponseLoggable`) when the response contains PII
