# CSharpEssentials.Mediator

Pipeline behaviors for the [Mediator](https://github.com/martinothamar/Mediator) source-generator library:
validation, logging, exception handling, caching, transactions, resource locks and idempotency, built around
`Result` and `Result<T>`. Mediator itself provides `ICommand`, `IQuery`, the handlers and dispatch. This package adds
marker interfaces and the behaviors that act on them.

> Benchmarks are in [BENCHMARKS.md](./BENCHMARKS.md). This package is not a dispatcher, so its dispatch cost is
> Mediator's: on net10 a send (0 behaviors) takes 7.761 ns through Mediator and 68.270 ns through MediatR 12.5.0, but
> Mediator is 1.11× slower from a cold process (36.56 vs 33.06 ms). The validation and exception handling behaviors tie
> hand-written ones on Mediator for valid input (177.6 vs 179.3 ns) and cost more for invalid input (292.5 vs 282.1 ns,
> 752 vs 712 B), still faster than hand-written MediatR behaviors (396.0 ns). Under Native AOT (no
> `TrimmerRootAssembly`) Mediator runs and MediatR crashes; both CSE samples publish with 0 IL warnings and run, at 2.5 MB.

## Installation

```bash
dotnet add package CSharpEssentials.Mediator
dotnet add package Mediator.SourceGenerator --version 3.0.*
```

The entry project must reference `Mediator.SourceGenerator`. `CSharpEssentials.Validation` and
`CSharpEssentials.Idempotency` come with this package.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Mediator
// dotnet add package Mediator.SourceGenerator
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
// dotnet add package Microsoft.Extensions.Caching.Memory --version 10.0.1
// dotnet add package Microsoft.Extensions.Logging.Console --version 10.0.1
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();
services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddDistributedMemoryCache();
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();
services.AddValidator<CreateOrder, CreateOrderValidator>();

await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

Result<int> invalid = await sender.Send(new CreateOrder("", 0));
Console.WriteLine($"invalid: {string.Join(", ", invalid.Errors.Select(e => e.Code))}");

Result<int> created = await sender.Send(new CreateOrder("book", 2));
Console.WriteLine($"created: {created.Value}");

Result crashed = await sender.Send(new CancelOrder(42));
Console.WriteLine($"exception: {crashed.FirstError.Type} {crashed.FirstError.Code}");

await sender.Send(new GetPrice("book"));
await sender.Send(new GetPrice("book"));
Console.WriteLine($"price handler calls: {GetPriceHandler.Calls}");

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
        throw new InvalidOperationException($"Order {command.Id} is already shipped.");
}

public sealed record GetPrice(string Product) : IQuery<Result<decimal>>, ICacheable
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
```

Output:

```text
invalid: Product.NotEmpty, Quantity.GreaterThan
created: 1001
exception: Unexpected InvalidOperationException
price handler calls: 1
```

## Registration

```csharp
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();                // validation, logging, exception handling, caching, transaction scope
services.AddMediatorLockBehavior();             // optional, after AddMediatorBehaviors
services.AddMediatorIdempotencyBehavior();      // optional, after AddMediatorBehaviors
```

Behaviors run in registration order. To pick them one by one:

| Method | Registers | Lifetime |
|--------|-----------|----------|
| `AddMediatorValidationBehavior()`, `AddMediatorValidationBehavior(configure)` | `ValidationBehavior`, its options and the default failure observer | Scoped |
| `AddMediatorLoggingBehavior()` | `LoggingBehavior` | Singleton |
| `AddMediatorExceptionHandlingBehavior()` | `ExceptionHandlingBehavior` | Singleton |
| `AddMediatorCachingBehavior()` | `CachingBehavior` | Singleton |
| `AddMediatorTransactionBehavior()` | `TransactionScopeBehavior`, replacing `TransactionBehavior` | Singleton |
| `AddMediatorTransactionRunnerBehavior()` | `TransactionBehavior`, replacing `TransactionScopeBehavior` | Scoped |
| `AddMediatorLockBehavior(placement)` | `LockBehavior`, and `InProcessResourceLock` unless an `IResourceLock` is registered | Scoped |
| `AddMediatorIdempotencyBehavior(configure)` | `IdempotencyBehavior`, its options and the idempotency executor | Scoped |
| `AddMediatorValidationOptions(configure)` | Only the validation options and the default observer (for Native AOT) | |

Register Mediator with `ServiceLifetime.Scoped`. Four behaviors are scoped, and the generated mediator is a singleton
by default: it then resolves them from the root provider. With scope validation on (the ASP.NET Core default in
Development) the first send throws `Cannot resolve scoped service ... from root provider`; with it off, one instance of
each scoped behavior, and the `DbContext` behind it, is shared by every request.

With a scoped mediator, a hosted service or another singleton cannot inject `ISender`. Create a scope with
`IServiceScopeFactory.CreateAsyncScope()` and resolve `ISender` from it for each unit of work.

### Native AOT

On Native AOT pass the behaviors to the generator instead of registering open generics in DI:

```csharp
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors; // namespace Microsoft.Extensions.DependencyInjection
});
services.AddMediatorValidationOptions();
```

`DefaultPipelineBehaviors` holds `ValidationBehavior<,>`, `LoggingBehavior<,>`, `ExceptionHandlingBehavior<,>`,
`CachingBehavior<,>` and `TransactionScopeBehavior<,>` in that order. For the others, build your own array:

- Transaction runner: put `TransactionBehavior<,>` where `TransactionScopeBehavior<,>` is.
- Lock: insert `LockBehavior<,>` right before (outside) or right after (inside) the transaction behavior, and register an `IResourceLock`.
- Idempotency: insert `IdempotencyBehavior<,>`, register an `IdempotencyBehaviorOptions` instance and call `AddIdempotencyExecutor()`.

The generator registers behaviors with its `ServiceLifetime`, so observers and other dependencies must live at least as
long. This package is not marked `IsAotCompatible`: `ValidationBehavior`, `ExceptionHandlingBehavior` and
`IdempotencyBehavior` build `Result` failures without reflection, but `CachingBehavior`, `LoggingBehavior` and
`IdempotencyBehavior` serialize with reflection-based `System.Text.Json`. Test the behaviors you use before you publish
trimmed. See [Mediator: use pipeline behaviors](https://github.com/martinothamar/Mediator#44-use-pipeline-behaviors).

## Requests and handlers

Handlers implement Mediator's interfaces. The method is `Handle` and returns a `ValueTask`.

```csharp
public sealed record DeleteUser(Guid Id) : ICommand<Result>;
public sealed record CreateUser(string Email) : ICommand<Result<Guid>>;
public sealed record GetUser(Guid Id) : IQuery<Result<UserDto>>;

public sealed class GetUserHandler(IUserRepository users) : IQueryHandler<GetUser, Result<UserDto>>
{
    public async ValueTask<Result<UserDto>> Handle(GetUser query, CancellationToken cancellationToken)
    {
        UserDto? user = await users.FindAsync(query.Id, cancellationToken);
        return user is null ? Error.NotFound("User.NotFound", "User not found.") : user;
    }
}
```

The behaviors understand `Result` and `Result<T>` best. Other response types are covered in
[Typed responses](#typed-responses) and in each section below.

## Validation

`ValidationBehavior` runs every `IValidator<TRequest>` from `CSharpEssentials.Validation` before the handler. It applies
to every request; a request with no validator passes straight through.

```csharp
services.AddMediatorValidationBehavior();
services.AddValidator<CreateUser, CreateUserValidator>(); // or AddValidatorsFromAssembly(typeof(Program).Assembly)

public sealed class CreateUserValidator : Validator<CreateUser>
{
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        return ValueTask.CompletedTask;
    }
}
```

On failure the handler does not run. `Result` and `Result<T>` responses get a failed result with the errors. Any other
response type gets an `EnhancedValidationException` (namespace `CSharpEssentials.Exceptions`), which the
`GlobalExceptionHandler` of `CSharpEssentials.AspNetCore` turns into a 400.

- Error codes use the property path, for example `Email.NotEmpty`.
- Validators are grouped by `Order` and the groups run in ascending order. Errors from all validators are combined in validator order without duplicates.
- A validator that throws becomes an error with the code `Validator.Exception`. `OperationCanceledException` still propagates.

### Validation modes

| Mode | Validators run | Observers called | Handler runs after a failure |
|------|---------------|------------------|------------------------------|
| `Enforce` (default) | Yes | Yes | No |
| `LogOnly` | Yes | Yes | Yes |
| `Off` | No | No | Yes |

`LogOnly` lets you watch new rules fail on real traffic before they reject anything. Set the default with
`ValidationBehaviorOptions.DefaultMode`, and let one request choose its own with `IValidationModeOverride`:

```csharp
services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.LogOnly);

public sealed record ImportOrder(string Payload) : ICommand<Result>, IValidationModeOverride
{
    public ValidationMode ValidationMode => ValidationMode.Enforce;
}
```

### Failure observers

Every `IValidationFailureObserver` in DI gets each failure in `Enforce` and `LogOnly` mode, in registration order, as a
`ValidationFailureContext(RequestType, Request, Errors, Mode)`. The built-in `LoggingValidationFailureObserver` is
registered for you. It logs `LogOnly` failures at Warning and `Enforce` failures at Debug, and counts each one on the
`cse.mediator.validation.failures` counter of the `CSharpEssentials.Mediator` meter (`FailureCounterName` and
`MeterName`), tagged `request` and `mode`.

```csharp
public sealed class AuditValidationObserver(IAuditLog audit) : IValidationFailureObserver
{
    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken) =>
        audit.WriteAsync(failure.RequestType.Name, failure.Errors, cancellationToken);
}

services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationFailureObserver, AuditValidationObserver>());
```

- An observer that throws fails the request, in `LogOnly` mode too. Catch your own transport errors.
- Observers are resolved with the behavior. `AddMediatorValidationBehavior` registers it scoped, so scoped observers work. On Native AOT the generator's lifetime applies.

## Logging

`LoggingBehavior` runs only for requests that implement one of these interfaces. It logs at Information: the request
name, the response name and the elapsed time. The interface decides which payloads are added as JSON.

| Interface | Payload logged |
|-----------|----------------|
| `ILoggableRequest` | None |
| `IRequestLoggable` | Request |
| `IResponseLoggable` | Response |
| `IRequestResponseLoggable` | Both |

```csharp
public sealed record GetUser(Guid Id) : IQuery<Result<UserDto>>, IRequestResponseLoggable;
public sealed record SendEmail(string To, string Body) : ICommand<Result>, IRequestLoggable;
```

Use `IRequestLoggable` when the response holds personal data.

## Exception handling

When a handler that returns `Result` or `Result<T>` throws, `ExceptionHandlingBehavior` returns a failed result with
`Error.Exception(ex)`: type `Unexpected`, the exception type name as code and the message as description. No marker
interface is needed.

- `OperationCanceledException` always propagates.
- Other response types, typed `Result<T, E1..E3>` included, are not touched: the exception propagates.
- Behaviors registered after it (caching, transaction, lock, idempotency) run inside it, so their exceptions are converted too. Validation and logging run outside it.

## Caching

`CachingBehavior` stores the response of an `ICacheable` request in `IDistributedCache`. All four members are required:

```csharp
public sealed record GetUser(Guid Id) : IQuery<Result<UserDto>>, ICacheable
{
    public string CacheKey => $"user:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}

services.AddDistributedMemoryCache(); // or Redis, SQL Server, ...
```

| Member | Meaning |
|--------|---------|
| `CacheKey` | The cache entry key |
| `Expiration` | Absolute expiration from now. Zero or negative stores the entry without one |
| `BypassCache` | `true` skips the cache: no lookup and no store |
| `CacheFailures` | `true` caches failed results too |

- The response is stored as JSON with the default `System.Text.Json` settings, so it must round-trip through them. A response that cannot be serialized is returned without caching and logged.
- An entry that can no longer be read as the response type is logged, removed and the handler runs.
- Errors from the cache backend itself are not caught.

## Transactions

Mark a command with `ITransactionalRequest`. It has no members.

```csharp
public sealed record TransferMoney(Guid From, Guid To, decimal Amount) : ICommand<Result>, ITransactionalRequest;
```

The transaction commits unless the handler returns a failed `Result` or `Result<T>` or throws. A response that is not a
`Result` commits. One transaction behavior is active at a time:

- `TransactionScopeBehavior` (from `AddMediatorBehaviors` or `AddMediatorTransactionBehavior`) wraps the handler in a `TransactionScope` with `TransactionScopeAsyncFlowOption.Enabled`.
- `TransactionBehavior` (from `AddMediatorTransactionRunnerBehavior`) runs the handler through the registered `ITransactionRunner` (namespace `CSharpEssentials.Transactions`). It takes the position of `TransactionScopeBehavior`, and the reverse.

The EF Core runner comes from `CSharpEssentials.EntityFrameworkCore`:

```csharp
services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>();
```

Any other runner implements one method:

```csharp Program.cs
// dotnet add package CSharpEssentials.Mediator
// dotnet add package Mediator.SourceGenerator
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorTransactionRunnerBehavior();
services.AddSingleton<ITransactionRunner, ConsoleTransactionRunner>();

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

await sender.Send(new Rename(1, "Gadget"));
await sender.Send(new Rename(1, ""));

public sealed record Rename(int Id, string Name) : ICommand<Result>, ITransactionalRequest;

public sealed class RenameHandler : ICommandHandler<Rename, Result>
{
    public ValueTask<Result> Handle(Rename command, CancellationToken cancellationToken) =>
        new(command.Name.Length == 0 ? Result.Failure(Error.Validation("Name.Empty", "The name is empty.")) : Result.Success());
}

public sealed class ConsoleTransactionRunner : ITransactionRunner
{
    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work, Func<T, bool> shouldCommit, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("BEGIN");
        T result = await work(cancellationToken);
        Console.WriteLine(shouldCommit(result) ? "COMMIT" : "ROLLBACK");
        return result;
    }
}
```

On Native AOT the runner usually depends on a scoped `DbContext`, so set `ServiceLifetime.Scoped` in `AddMediator`.
A transaction behavior you register with a factory is not recognized by the replace logic and is left alone.

## Locks

Mark a request with `ILockedRequest` to run one handler at a time per `LockKey`:

```csharp Program.cs
// dotnet add package CSharpEssentials.Mediator
// dotnet add package Mediator.SourceGenerator
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
using CSharpEssentials.Locking;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();
services.AddMediatorLockBehavior();

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

await Task.WhenAll(sender.Send(new Restock(7)).AsTask(), sender.Send(new Restock(7)).AsTask());
Console.WriteLine($"most handlers at once: {RestockHandler.MaxRunning}");

IResourceLock locks = scope.ServiceProvider.GetRequiredService<IResourceLock>();
await using (await locks.AcquireAsync("product:7", timeout: null))
{
    Result result = await sender.Send(new Restock(7));
    Console.WriteLine($"while held elsewhere: {result.FirstError.Code}");
}

public sealed record Restock(int ProductId) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"product:{ProductId}";
    public TimeSpan? LockTimeout => TimeSpan.FromMilliseconds(200);
}

public sealed class RestockHandler : ICommandHandler<Restock, Result>
{
    private static int s_running;
    public static int MaxRunning;

    public async ValueTask<Result> Handle(Restock command, CancellationToken cancellationToken)
    {
        MaxRunning = Math.Max(MaxRunning, Interlocked.Increment(ref s_running));
        await Task.Delay(50, cancellationToken);
        Interlocked.Decrement(ref s_running);
        return Result.Success();
    }
}
```

Output:

```text
most handlers at once: 1
while held elsewhere: TimeoutException
```

- `LockTimeout` is optional. `null` (the default) waits until the request is cancelled.
- A lock not acquired in time throws `TimeoutException` and the handler does not run. Inside `ExceptionHandlingBehavior` (the default order) a `Result` handler gets it as an `Error.Exception` with the code `TimeoutException`.
- `AddMediatorLockBehavior` registers `InProcessResourceLock` as a singleton `IResourceLock` unless one is registered. It serializes within one process only, and it is not reentrant: a handler that sends another request with the same key waits for itself.

### Custom and distributed locks

With several instances, register a lock that all of them share:

```csharp
public interface IResourceLock // namespace CSharpEssentials.Locking
{
    ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default);
    ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default);
}
```

`CSharpEssentials.EntityFrameworkCore.Locking` has a database lock that needs no provider package:
`services.AddRelationalResourceLock<AppDbContext>(ResourceLockSqlDialect.PostgreSql)` (session advisory locks) or
`ResourceLockSqlDialect.SqlServer` (`sp_getapplock`). If your lock needs a numeric key, derive it with a stable hash
(FNV-1a 64 over the UTF-8 bytes, or PostgreSQL `hashtextextended(key, 0)`), never `string.GetHashCode()`, which differs
between processes. Test a custom lock with `ResourceLockContract` from
[CSharpEssentials.Mediator.Testing](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Mediator.Testing/Readme.MD).

### Lock placement

| `LockPlacement` | Order | Use with |
|-----------------|-------|----------|
| `OutsideTransaction` (default) | Lock, transaction, handler | Locks held until disposed: `InProcessResourceLock`, session advisory locks, Redis. The next waiter sees committed data |
| `InsideTransaction` | Transaction, lock, handler | Locks owned by the transaction, such as `pg_advisory_xact_lock`, whose handle dispose does nothing |

A lock that releases on dispose, placed inside, lets go before the commit, so the next request can read old data. A
transaction-scoped lock placed outside ends with its own short transaction and protects nothing.

```csharp
services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>();
services.AddScoped<IResourceLock, PostgresXactLock>();
services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);
```

- Call `AddMediatorLockBehavior` after the other behaviors. With no transaction behavior registered it is added at the end.
- `InsideTransaction` throws `InvalidOperationException` when no transaction behavior is registered.
- Calling it again moves the behavior instead of adding a second one.
- A transaction behavior registered with a factory is not recognized, so the lock is appended after it (inside).

## Idempotency

Mark a command with `IIdempotentRequest` to run it at most once per key and caller. A retry with the same key gets the
stored response and the handler does not run. The work goes through `IIdempotencyExecutor` from
[CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD).

```csharp Program.cs
// dotnet add package CSharpEssentials.Mediator
// dotnet add package Mediator.SourceGenerator
// dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.1
// dotnet add package Microsoft.Extensions.Logging.Console --version 10.0.1
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();
services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Error));
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services.AddMediatorBehaviors();
services.AddMediatorIdempotencyBehavior(options =>
    options.ForRequest<CapturePayment>(request => request.Profile = "payments"));
services.AddIdempotencyExecutor(options =>
    options.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(7)));

await using ServiceProvider provider = services.BuildServiceProvider();
await using AsyncServiceScope scope = provider.CreateAsyncScope();
ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

Result<Guid> first = await sender.Send(new CapturePayment("pay-1", "acme", 10m));
Result<Guid> retry = await sender.Send(new CapturePayment("pay-1", "acme", 10m));
Console.WriteLine($"replayed: {first.Value == retry.Value}, handler calls: {CapturePaymentHandler.Calls}");

Result<Guid> reused = await sender.Send(new CapturePayment("pay-1", "acme", 99m));
Console.WriteLine($"same key, other amount: {reused.FirstError.Code}");

Result<Guid> missing = await sender.Send(new CapturePayment("", "acme", 10m));
Console.WriteLine($"no key: {missing.FirstError.Code}");

public sealed record CapturePayment(string IdempotencyKey, string TenantId, decimal Amount)
    : ICommand<Result<Guid>>, IIdempotentRequest
{
    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Tenant(TenantId);
}

public sealed class CapturePaymentHandler : ICommandHandler<CapturePayment, Result<Guid>>
{
    public static int Calls;

    public ValueTask<Result<Guid>> Handle(CapturePayment command, CancellationToken cancellationToken)
    {
        Calls++;
        return new(Result<Guid>.Success(Guid.NewGuid()));
    }
}
```

Output:

```text
replayed: True, handler calls: 1
same key, other amount: Idempotency.KeyReused
no key: Idempotency.KeyMissing
```

`AddMediatorIdempotencyBehavior` also registers the executor and, unless a store is registered, the in-memory record
store. Configure executor defaults, profiles and modes with `AddIdempotencyExecutor`, and register a shared
`IIdempotencyRecordStore` when you run several instances.

### Request members

| Member | Default |
|--------|---------|
| `IdempotencyKey` | Required. 1 to 255 printable ASCII characters other than space, `"` and `\`. Take it from the client or build it with `IdempotencyKeys.For(...)` or `IdempotencyKeys.FromMessage(...)` |
| `IdempotencyScope` | `null`: `ScopeResolver` resolves it from the current caller. Return a scope to set it yourself, for example `IdempotencyScope.Tenant(TenantId)` |
| `IdempotencyFingerprint` | `null`: SHA-256 of the request serialized as JSON. The same key with another fingerprint is `KeyReused` |
| `IdempotencyRetention`, `IdempotencyLease` | `null`: from `ForRequest<T>`, then its profile, then `IdempotencyExecutorOptions` |

Implement the optional members explicitly (`IdempotencyScope? IIdempotentRequest.IdempotencyScope => ...`), as above.
The record namespace is the full name of the request type, so two request types never share a record.

### Outcomes

| Situation | Response |
|-----------|----------|
| Same key, same request, completed | The stored response; the handler does not run |
| Same key, still running | `Error.Conflict("Idempotency.InFlight")`. Retry later |
| Same key, other request | `Error.Validation("Idempotency.KeyReused")`. Do not retry |
| Missing or invalid key | `Error.Validation("Idempotency.KeyMissing")` or `"Idempotency.KeyInvalid"` |
| No scope | Depends on `UnscopedKeyBehavior`, see below |

A response type that is not `Result` or `Result<T>` gets an `EnhancedValidationException` with the same error.

What is kept for a retry:

| Handler outcome | Record |
|-----------------|--------|
| Success | Stored for `Retention` (24 hours by default) |
| Failed `Result` of an `ITransactionalRequest` | Released: the transaction rolled back, so a retry runs again |
| Other failed `Result`, first error `Unexpected` | Stored for `FailureRetention` (10 minutes by default) |
| Other failed `Result` | Stored for `Retention`. `StoreFailures = false` releases every failure instead |
| Exception | The key stays in flight until `Lease` (1 minute by default) ends, because the work may have happened |

With the default order, `ExceptionHandlingBehavior` sits outside the idempotency behavior. The caller of a throwing
handler gets a failed `Result` with the exception, and a retry before the lease ends gets `Idempotency.InFlight`.

### Options

| `IdempotencyBehaviorOptions` | Default | Meaning |
|------------------------------|---------|---------|
| `Placement` | `OutsideTransaction` | Where the behavior runs relative to the transaction behavior |
| `ScopeResolver` | `IdempotencyScopes.User` | Finds the key owner from `IIdempotencyScopeAccessor.Principal`. Use `IdempotencyScopes.Client` for machine callers |
| `UnscopedKeyBehavior` | `Reject` | What happens when no scope resolves |
| `StoreFailures` | `true` | Store failed results of requests that are not `ITransactionalRequest` |
| `SerializerOptions` | `null` | `JsonSerializerOptions` for the fingerprint and the stored response |
| `ForRequest<TRequest>(configure)` | | Profile, mode, retention, lease, failure retention or store key for one request type |

### Scopes: HTTP and Mediator defaults

A key only means something to its owner. The behavior takes the request's own `IdempotencyScope` first, then asks
`ScopeResolver` with the principal from `IIdempotencyScopeAccessor`. When neither gives a scope,
`UnscopedKeyBehavior` decides, and its default differs from the HTTP middleware's:

| Layer | Default | Keyed request without a scope |
|-------|---------|-------------------------------|
| HTTP middleware (`AddIdempotency`) | `Skip` | Runs without idempotency and logs `Idempotency.UnscopedKeySkipped` |
| Mediator behavior | `Reject` | `Error.Validation("Idempotency.ScopeRequired")` |

`Global` stores the key under `IdempotencyScope.Global`, shared by every caller.

- In ASP.NET Core, call `AddIdempotency()` from `CSharpEssentials.AspNetCore`. It registers the accessor that reads `HttpContext.User`. Without an accessor and without a scope on the request, every keyed request fails with `Idempotency.ScopeRequired`.
- An anonymous or client-credentials request that the HTTP layer lets through is rejected once it reaches an idempotent command. Give both layers the same resolver, for example `IdempotencyScopes.Client`.
- A message consumer has no principal. Take the scope from the message (tenant, sending system), or return `IdempotencyScope.Global` on purpose.
- A failed `Result` with an idempotency code returned through `ToProblemResult()` or `ToActionResult()` gets the middleware's status and `code`: `InFlight` 409 with `Retry-After`, `KeyReused` 422, `KeyMissing`, `KeyInvalid` and `ScopeRequired` 400.

### Idempotency placement

| `IdempotencyPlacement` | Order | Use with |
|------------------------|-------|----------|
| `OutsideTransaction` (default) | Idempotency, lock, transaction, handler | Any store. The record is written after the commit or rollback |
| `InsideTransaction` | Transaction, idempotency, lock, handler | A store that joins the same transaction, such as `EfCoreIdempotencyRecordStore<TContext>` from `CSharpEssentials.EntityFrameworkCore` |

- Outside: if the process stops between the commit and the record write, the key stays in flight until the lease ends, and a retry after that runs the handler again.
- Inside with a store that does not join the transaction (in-memory, distributed cache): a commit that fails after the record was written leaves a stored response for work that never committed, and a retry replays it.
- Call `AddMediatorIdempotencyBehavior` after `AddMediatorBehaviors`, so a validation failure never reserves a key. With no lock or transaction behavior it is added at the end.
- `InsideTransaction` without a transaction behavior throws `InvalidOperationException`. Calling the method again configures the same options and moves the behavior.

### Modes

The mode comes from `ForRequest<T>(p => p.Mode = ...)`, else its profile, else `IdempotencyExecutorOptions.Mode`. It is
read on every request through `IOptionsMonitor`, so a reloaded configuration switches it without a restart. It is
separate from the HTTP middleware's `IdempotencyOptions.Mode`.

- `Off`: the behavior only calls the next step.
- `Observe`: nothing is blocked. A request it would reject (missing or invalid key, no scope) and a replay, in-flight or reused key are logged, and the request runs. A scope resolver or fingerprint that throws is logged as `Idempotency.Observe.ResolutionFailed` and the request runs without idempotency. `Observe` measures duplicates; it does not prevent them, so keep it short for commands that move money.
- `Enforce` (default): as described above. A resolver or fingerprint that throws propagates.

Decisions are counted on `idempotency.decisions` (meter `CSharpEssentials.Idempotency`) with `source` `mediator` and
the request type as `target`. See [Telemetry](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#telemetry).

## Typed responses

A handler can return a typed `Result<T, E1>`, `Result<T, E1, E2>` or `Result<T, E1, E2, E3>` from
`CSharpEssentials.Results`:

| Behavior | With a typed response |
|----------|-----------------------|
| `ExceptionHandlingBehavior` | Does nothing. The original exception propagates with its stack trace |
| `ValidationBehavior` | A failed validation throws `EnhancedValidationException` and the handler does not run |
| `CachingBehavior` | A failure is not cached unless `CacheFailures` is `true`. A cached result comes back in the same slot |
| `IdempotencyBehavior` | Stores and replays the result in the same slot, failures too while `StoreFailures` is on. A rejection throws `EnhancedValidationException` with the idempotency code |

Caching and idempotency store the result as JSON (`{"case":"<slot>","value":...}`), so every slot type must round-trip
through `System.Text.Json`. An exception slot does not: `IdempotencyBehavior` releases the key and `CachingBehavior`
returns the result without caching it.

```csharp
public sealed record GetUser(int Id) : IQuery<Result<User, NotFoundError>>, ICacheable
{
    public string CacheKey => $"user:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}
```

## Reference

| Marker | Behavior | Applies to |
|--------|----------|------------|
| none | `ValidationBehavior` | Every request |
| `ILoggableRequest` and its children | `LoggingBehavior` | Marked requests |
| none | `ExceptionHandlingBehavior` | Every request; converts only for `Result` and `Result<T>` |
| `ICacheable` | `CachingBehavior` | Marked requests |
| `ITransactionalRequest` | `TransactionScopeBehavior` or `TransactionBehavior` | Marked requests |
| `ILockedRequest` | `LockBehavior` | Marked requests |
| `IIdempotentRequest` | `IdempotencyBehavior` | Marked requests |
| `IValidationModeOverride` | `ValidationBehavior` | Picks the validation mode per request |

This package reports no analyzer diagnostics.

## Related

- Agent skill: [csharpessentials-mediator](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-mediator/SKILL.md)
- Example: [examples/Examples.Mediator](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Mediator)
- Test helpers for locks: [CSharpEssentials.Mediator.Testing](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Mediator.Testing/Readme.MD)
- Idempotency: [CSharpEssentials.Idempotency](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD)
- Validators: [CSharpEssentials.Validation](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD)
