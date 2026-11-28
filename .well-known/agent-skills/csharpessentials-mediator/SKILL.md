---
name: csharpessentials-mediator
description: Use when adding pipeline behaviors to Mediator (martinothamar, source generator, not MediatR) CQRS handlers that return Result or Result<T>. Covers AddMediatorBehaviors, ValidationBehavior (Enforce/LogOnly/Off, IValidationFailureObserver), LoggingBehavior (IRequestLoggable), ExceptionHandlingBehavior, CachingBehavior (ICacheable), TransactionScopeBehavior and TransactionBehavior (ITransactionalRequest, ITransactionRunner), LockBehavior (ILockedRequest, IResourceLock) and IdempotencyBehavior (IIdempotentRequest, AddMediatorIdempotencyBehavior).
---

# CSharpEssentials.Mediator

Pipeline behaviors for the `Mediator` source-generator package. Each behavior runs for every request that has its
marker interface. Handlers come from Mediator: `ICommandHandler<TCommand, TResponse>` and
`IQueryHandler<TQuery, TResponse>` with a `Handle` method returning `ValueTask<TResponse>`.

## Install

```bash
dotnet add package CSharpEssentials.Mediator
dotnet add package Mediator.SourceGenerator --version 3.0.*   # in the entry project
```

## Namespaces

```csharp
using CSharpEssentials.Mediator;               // markers, behaviors, ValidationMode, LockPlacement, IdempotencyPlacement
using CSharpEssentials.Locking;                // IResourceLock, InProcessResourceLock
using CSharpEssentials.Idempotency;            // IdempotencyScope, IdempotencyScopes, IdempotencyKeys
using CSharpEssentials.Transactions;           // ITransactionRunner
using CSharpEssentials.ResultPattern;          // Result, Result<T>
using CSharpEssentials.Errors;                 // Error
using Mediator;                                // ICommand, IQuery, handlers, ISender
using Microsoft.Extensions.DependencyInjection; // AddMediator, AddMediator* behavior methods
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Mediator
// dotnet add package Mediator.SourceGenerator
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
// dotnet add package Microsoft.Extensions.Caching.Memory --version 10.0.1
// dotnet add package Microsoft.Extensions.Logging.Console --version 10.0.1
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();
services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Error));
services.AddDistributedMemoryCache();
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();
services.AddMediatorLockBehavior();
services.AddMediatorIdempotencyBehavior();
services.AddValidator<CreateOrder, CreateOrderValidator>();

await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

Result<int> invalid = await sender.Send(new CreateOrder("", 0));
Console.WriteLine($"validation: {string.Join(", ", invalid.Errors.Select(e => e.Code))}");

Result crashed = await sender.Send(new CancelOrder(42));
Console.WriteLine($"exception: {crashed.FirstError.Type} {crashed.FirstError.Code}");

await sender.Send(new GetPrice("book"));
await sender.Send(new GetPrice("book"));
Console.WriteLine($"cached, handler calls: {GetPriceHandler.Calls}");

Result<Guid> pay = await sender.Send(new Pay("key-1", "acme", 10m));
Result<Guid> payAgain = await sender.Send(new Pay("key-1", "acme", 10m));
Result<Guid> reused = await sender.Send(new Pay("key-1", "acme", 99m));
Console.WriteLine($"idempotent: replayed {pay.Value == payAgain.Value}, other amount {reused.FirstError.Code}");

public sealed record CreateOrder(string? Product, int Quantity) : ICommand<Result<int>>, ITransactionalRequest;

public sealed class CreateOrderValidator : Validator<CreateOrder>
{
    protected override ValueTask Configure(CreateOrder model, RuleContext<CreateOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Product).NotEmpty();
        rules.For(() => model.Quantity).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}

public sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Result<int>>
{
    public ValueTask<Result<int>> Handle(CreateOrder command, CancellationToken cancellationToken) =>
        new(Result<int>.Success(1001));
}

public sealed record CancelOrder(int Id) : ICommand<Result>;

public sealed class CancelOrderHandler : ICommandHandler<CancelOrder, Result>
{
    public ValueTask<Result> Handle(CancelOrder command, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Already shipped.");
}

public sealed record GetPrice(string Product) : IQuery<Result<decimal>>, ICacheable, IRequestResponseLoggable
{
    public string CacheKey => $"price:{Product}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}

public sealed class GetPriceHandler : IQueryHandler<GetPrice, Result<decimal>>
{
    public static int Calls;

    public ValueTask<Result<decimal>> Handle(GetPrice query, CancellationToken cancellationToken)
    {
        Calls++;
        return new(Result<decimal>.Success(12.50m));
    }
}

public sealed record Pay(string IdempotencyKey, string TenantId, decimal Amount)
    : ICommand<Result<Guid>>, IIdempotentRequest, ILockedRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Tenant(TenantId);
    public string LockKey => $"tenant:{TenantId}";
}

public sealed class PayHandler : ICommandHandler<Pay, Result<Guid>>
{
    public ValueTask<Result<Guid>> Handle(Pay command, CancellationToken cancellationToken) =>
        new(Result<Guid>.Success(Guid.NewGuid()));
}
```

Prints `validation: Product.NotEmpty, Quantity.GreaterThan`, `exception: Unexpected InvalidOperationException`,
`cached, handler calls: 1` and `idempotent: replayed True, other amount Idempotency.KeyReused`.

## Registering behaviors

```csharp
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();            // Validation, Logging, ExceptionHandling, Caching, TransactionScope (this order)
services.AddMediatorTransactionRunnerBehavior(); // optional: TransactionBehavior replaces TransactionScopeBehavior
services.AddMediatorLockBehavior();         // optional: LockPlacement.OutsideTransaction by default
services.AddMediatorIdempotencyBehavior();  // optional: also registers AddIdempotencyExecutor()
```

Single behaviors: `AddMediatorValidationBehavior()`, `AddMediatorValidationBehavior(o => ...)`,
`AddMediatorLoggingBehavior()`, `AddMediatorExceptionHandlingBehavior()`, `AddMediatorCachingBehavior()`,
`AddMediatorTransactionBehavior()`. They run in registration order.

Native AOT: `AddMediator(o => { o.ServiceLifetime = ServiceLifetime.Scoped; o.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors; })`
plus `services.AddMediatorValidationOptions()`. `DefaultPipelineBehaviors` has no lock, idempotency or runner behavior;
add `LockBehavior<,>`, `IdempotencyBehavior<,>` or `TransactionBehavior<,>` to your own array.

## Validating requests

`ValidationBehavior` runs every `IValidator<TRequest>` (from `CSharpEssentials.Validation`) before the handler.

```csharp
services.AddValidator<CreateOrder, CreateOrderValidator>();          // or AddValidatorsFromAssembly(typeof(Program).Assembly)
services.AddMediatorValidationBehavior(o => o.DefaultMode = ValidationMode.LogOnly);

public sealed record ImportOrder(string Payload) : ICommand<Result>, IValidationModeOverride
{
    public ValidationMode ValidationMode => ValidationMode.Enforce;   // per-request override
}

public sealed class AuditObserver : IValidationFailureObserver
{
    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{failure.RequestType.Name} {failure.Mode}: {failure.Errors.Count} error(s)");
        return ValueTask.CompletedTask;
    }
}
services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationFailureObserver, AuditObserver>()); // Microsoft.Extensions.DependencyInjection.Extensions
```

- `Result`/`Result<T>` response: failed result with the errors, handler skipped. Other response types: `EnhancedValidationException` (namespace `CSharpEssentials.Exceptions`).
- Modes: `Enforce` (default) rejects; `LogOnly` notifies observers and runs the handler; `Off` skips validators.
- Error codes are `Property.Rule`, for example `Product.NotEmpty`. A throwing validator becomes `Validator.Exception`.
- `LoggingValidationFailureObserver` is registered for you: logs and counts `cse.mediator.validation.failures` on meter `CSharpEssentials.Mediator`. An observer that throws fails the request.

## Logging requests

Mark the request: `IRequestLoggable` (request JSON), `IResponseLoggable` (response JSON), `IRequestResponseLoggable`
(both), `ILoggableRequest` (names and elapsed time only). Unmarked requests are not logged. Logs at Information.

## Handling exceptions

No marker. A handler returning `Result`/`Result<T>` that throws returns `Error.Exception(ex)`: `ErrorType.Unexpected`,
code = exception type name. `OperationCanceledException` propagates. Other response types are not touched.

## Caching queries

```csharp
public sealed record GetUser(Guid Id) : IQuery<Result<UserDto>>, ICacheable
{
    public string CacheKey => $"user:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);   // <= 0: no expiration
    public bool BypassCache => false;                        // true: no lookup, no store
    public bool CacheFailures => false;
}
services.AddDistributedMemoryCache();                         // CachingBehavior needs IDistributedCache
```

JSON with default `System.Text.Json` settings; the response type must round-trip. An unreadable entry is removed and the
handler runs.

## Running in a transaction

```csharp
public sealed record PlaceOrder(OrderDto Order) : ICommand<Result<Guid>>, ITransactionalRequest; // no members
```

Commits unless the handler returns a failed `Result`/`Result<T>` or throws. Default: `TransactionScopeBehavior`
(`TransactionScope`, async flow enabled). For your own database transaction:

```csharp
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>();   // CSharpEssentials.EntityFrameworkCore
```

A custom runner implements `ITransactionRunner.ExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> work, Func<T, bool> shouldCommit, CancellationToken)`.

## Locking per key

```csharp
public sealed record CapturePayment(Guid OrderId) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"order:{OrderId}";
    public TimeSpan? LockTimeout => TimeSpan.FromSeconds(5);   // optional; null waits until cancelled
}
services.AddMediatorLockBehavior();                            // after AddMediatorBehaviors
```

- Timeout throws `TimeoutException`; a `Result` handler gets `Error.Exception` with code `TimeoutException`.
- Default lock is `InProcessResourceLock`: one process only, not reentrant. Several instances: `services.AddRelationalResourceLock<AppDbContext>(ResourceLockSqlDialect.PostgreSql)` (`CSharpEssentials.EntityFrameworkCore.Locking`) or your own `IResourceLock`.
- `AddMediatorLockBehavior(LockPlacement.InsideTransaction)` only for transaction-owned locks such as `pg_advisory_xact_lock`; it throws when no transaction behavior is registered.
- Test locks with `CSharpEssentials.Mediator.Testing` (namespace `CSharpEssentials.Locking.Testing`): `new ResourceLockContract(myLock).RunAsync(name)` over `ResourceLockContract.ScenarioNames`, and `RecordingResourceLock` (`Acquisitions`, `Releases`, `HeldKeys`, `FailWith`).

## Making a command idempotent

```csharp
public sealed record RefundPayment(string IdempotencyKey, string TenantId, Guid OrderId)
    : ICommand<Result<Guid>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Tenant(TenantId); // optional
}

services.AddIdempotency();   // ASP.NET Core (CSharpEssentials.AspNetCore): resolves the scope from HttpContext.User
services.AddMediatorIdempotencyBehavior(o =>
{
    o.ScopeResolver = IdempotencyScopes.Client;                       // default IdempotencyScopes.User
    o.ForRequest<RefundPayment>(p => p.Profile = "payments");
});
services.AddIdempotencyExecutor(o => o.AddProfile("payments", p => p.Retention = TimeSpan.FromDays(7)));
```

| Situation | Result error |
|-----------|--------------|
| Same key, same request, done | Stored response replayed, handler not run |
| Same key, still running | Conflict `Idempotency.InFlight` |
| Same key, different request (fingerprint) | Validation `Idempotency.KeyReused` |
| Missing / invalid key | Validation `Idempotency.KeyMissing` / `Idempotency.KeyInvalid` |
| No scope (request or caller) | Validation `Idempotency.ScopeRequired` (`UnscopedKeyBehavior.Reject`, default) |

- Optional members: `IdempotencyScope`, `IdempotencyFingerprint` (default SHA-256 of request JSON), `IdempotencyRetention`, `IdempotencyLease`.
- Stored: success for `Retention` (24 h). A failed `Result` is stored too, for `FailureRetention` (10 min) when its first error is `Unexpected`, else for `Retention`; `StoreFailures = false` releases every failure. A failed `ITransactionalRequest` always releases the key. A thrown exception keeps the key in flight until `Lease` (1 min), so a retry gets `Idempotency.InFlight`.
- Other `IdempotencyBehaviorOptions`: `Placement` (`OutsideTransaction` default), `UnscopedKeyBehavior` (`Reject` default, `Skip`, `Global`), `StoreFailures` (`true`), `SerializerOptions`.
- `o.Placement = IdempotencyPlacement.InsideTransaction` only with a store that joins the transaction (`AddEfCoreIdempotencyRecordStore<TContext>()`).
- Modes via `IdempotencyExecutorOptions.Mode` or `ForRequest`: `Enforce` (default), `Observe` (logs, never blocks), `Off`.
- Non-`Result` responses throw `EnhancedValidationException` instead of returning these errors.

## Rules

- Register Mediator with `options.ServiceLifetime = ServiceLifetime.Scoped`. Validation, transaction runner, lock and idempotency behaviors are scoped; a singleton mediator throws `Cannot resolve scoped service ... from root provider` under scope validation.
- With a scoped mediator, a `BackgroundService` or singleton must not inject `ISender`: create a scope (`IServiceScopeFactory.CreateAsyncScope()`) and resolve `ISender` from it.
- The handler method is `Handle`, not `HandleAsync`. Send with `ISender.Send` (or `IMediator.Send`).
- Call `AddMediatorLockBehavior()` and `AddMediatorIdempotencyBehavior()` after `AddMediatorBehaviors()`.
- `ICacheable` has no defaults: implement all four members. `ITransactionalRequest` has none.
- `CachingBehavior` needs an `IDistributedCache`; `LoggingBehavior`, `CachingBehavior` and `IdempotencyBehavior` need logging (`AddLogging`).
- An idempotent request in a console app, job or consumer has no caller: set `IdempotencyScope` on the request or it fails with `Idempotency.ScopeRequired`.
- Implement optional `IIdempotentRequest` members explicitly: `IdempotencyScope? IIdempotentRequest.IdempotencyScope => ...`.
- Do not catch exceptions inside handlers that return `Result` just to wrap them; `ExceptionHandlingBehavior` does it.
- `CachingBehavior` has no serializer options; `IdempotencyBehaviorOptions.SerializerOptions` exists for idempotency.
- Validators use `string?` properties with `NotEmpty()`; a non-nullable `string` gives a nullability warning.

## Diagnostics

This package reports no analyzer diagnostics.
