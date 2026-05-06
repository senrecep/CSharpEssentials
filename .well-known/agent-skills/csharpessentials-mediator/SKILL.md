---
name: csharpessentials-mediator
description: Use when adding cross-cutting pipeline behaviors to CQRS handlers — ValidationBehavior (auto FluentValidation), LoggingBehavior (ILoggableRequest), CachingBehavior (ICacheable with IDistributedCache), and TransactionScopeBehavior (ITransactionalRequest).
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
builder.Services.AddMediatorBehaviors();  // all 4 behaviors

// Or selectively
builder.Services.AddMediatorValidationBehavior();
builder.Services.AddMediatorLoggingBehavior();
builder.Services.AddMediatorCachingBehavior();
builder.Services.AddMediatorTransactionBehavior();
```

Register `ValidationBehavior` first — invalid requests should never reach the handler.

---

## ValidationBehavior — auto FluentValidation

Runs registered validators before the handler. Returns `Result` with collected errors on failure.

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Total).GreaterThan(0);
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
- Set `CacheFailures = false` — transient failures should not be cached
- `ITransactionalRequest` only on commands writing to multiple tables in one operation
- Use `IRequestLoggable` (not `IRequestResponseLoggable`) when the response contains PII
