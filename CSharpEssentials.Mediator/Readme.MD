# CSharpEssentials.Mediator

Pipeline behaviors for the [Mediator](https://github.com/martinothamar/Mediator) source-generator library, with built-in support for `Result<T>`, validation, caching, logging, exception handling, transactions, resource locks, and idempotency. Request dispatch is generated at compile time by the Mediator source generator; see [Native AOT](#registration) for how the behaviors are registered in a Native AOT app.

> **Note:** `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` are provided by the `Mediator` package itself. This package adds marker interfaces and pipeline behaviors that integrate with `Result<T>`.

## Features

- **Validation Behavior**: CSharpEssentials.Validation integration; failures return `Result.Failure` for `Result`/`Result<T>` handlers, or throw `EnhancedValidationException` for any other return type. `Enforce`, `LogOnly` and `Off` modes, globally or per request, with pluggable failure observers.
- **Logging Behavior**: Opt-in request/response payload logging with elapsed-time tracking.
- **Exception Handling Behavior**: Converts an exception thrown by a `Result` / `Result<T>` handler into a failed result. `OperationCanceledException` always propagates.
- **Caching Behavior**: `IDistributedCache` integration with bypass and failure-cache control.
- **Transaction Behavior**: Wraps handlers in `TransactionScope` with async flow enabled, or runs them through a pluggable `ITransactionRunner` (EF Core implementation in `CSharpEssentials.EntityFrameworkCore`). Commits only when the `Result` succeeds.
- **Lock Behavior**: Serializes requests that share a resource key through a pluggable `IResourceLock`, placed outside or inside the transaction. Ships an in-process default.
- **Idempotency Behavior**: Runs an `IIdempotentRequest` at most once per key and caller through the `IIdempotencyExecutor` of `CSharpEssentials.Idempotency`, and replays the stored `Result` to a retry.

## Installation

```bash
dotnet add package CSharpEssentials.Mediator
dotnet add package Mediator.SourceGenerator --version 3.0.*
```

> `Mediator.SourceGenerator` must be referenced by your **entry project**.

## Usage

### Registration

**Standard (non-AOT)**

```csharp
services.AddMediatorBehaviors(); // Registers 5 behaviors: validation, logging, exception handling, caching, transaction scope

// Or individually (they run in registration order):
services.AddMediatorValidationBehavior();
services.AddMediatorLoggingBehavior();
services.AddMediatorExceptionHandlingBehavior();
services.AddMediatorCachingBehavior();
services.AddMediatorTransactionBehavior();
```

**Native AOT**

When targeting Native AOT, pass behaviors via `AddMediator` options instead of relying on open-generic DI registration. `DefaultPipelineBehaviors` provides the pre-ordered array (`ValidationBehavior<,>`, `LoggingBehavior<,>`, `ExceptionHandlingBehavior<,>`, `CachingBehavior<,>`, `TransactionScopeBehavior<,>`):

```csharp
services.AddMediator(options =>
{
    options.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors; // namespace Microsoft.Extensions.DependencyInjection
});
```

Pipeline order: **Validation → Logging → Exception handling → Caching → Transaction**

This package is not marked `IsAotCompatible`. The behaviors themselves are not reflection-free: `ValidationBehavior` and `ExceptionHandlingBehavior` build `Result<T>` failure responses with reflection and a compiled expression, and `CachingBehavior` and `LoggingBehavior` serialize requests and responses with reflection-based `System.Text.Json`. Publish with trimming or Native AOT only after testing the behaviors you use.

> See [Mediator §4.4: Use pipeline behaviors](https://github.com/martinothamar/Mediator#44-use-pipeline-behaviors) for the full Native AOT pipeline behavior documentation.

### Commands & Queries

Use `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` directly from the `Mediator` package. Return `Result<T>` from handlers naturally.

```csharp
using Mediator;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

// Unit command
public record DeleteUserCommand(Guid Id) : ICommand<Result>;

public class DeleteUserCommandHandler(IUserRepository repo) : ICommandHandler<DeleteUserCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        await repo.DeleteAsync(command.Id, ct);
        return Result.Success();
    }
}

// Command with response
public record CreateUserCommand(string Email, string Name) : ICommand<Result<Guid>>;

public class CreateUserCommandHandler(IUserRepository repo) : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var user = new User { Email = command.Email, Name = command.Name };
        await repo.AddAsync(user, ct);
        return user.Id;
    }
}

// Query
public record GetUserQuery(Guid Id) : IQuery<Result<User>>;

public class GetUserQueryHandler(IUserRepository repo) : IQueryHandler<GetUserQuery, Result<User>>
{
    public async ValueTask<Result<User>> Handle(GetUserQuery query, CancellationToken ct)
    {
        User? user = await repo.FindAsync(query.Id, ct);
        if (user is null)
            return Error.NotFound("User.NotFound", "User not found");
        return user;
    }
}
```

### Validation Behavior

Add `CSharpEssentials.Validation` validators to DI. The behavior intercepts requests before the handler runs.

```csharp
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;

services.AddMediatorValidationBehavior();
services.AddValidator<CreateUserCommand, CreateUserCommandValidator>(); // or AddValidatorsFromAssembly(...)

public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    protected override ValueTask Configure(CreateUserCommand model, RuleContext<CreateUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        return ValueTask.CompletedTask;
    }
}
```

On validation failure the handler is never invoked. Errors are surfaced based on the handler return type:

| `TResponse` | Failure result |
|-------------|---------------|
| `Result` | `Result.Failure(errors)` returned directly |
| `Result<T>` | `Result<T>.Failure(errors)` returned directly |
| Any other type | `EnhancedValidationException` thrown and caught by `GlobalExceptionHandler` |

Error codes use the property path (e.g. `"Email.NotEmpty"`). Multiple validators are aggregated and deduplicated.

#### Validation modes

| Mode | Validators run | Observers notified | Handler runs on failure |
|------|---------------|-------------------|------------------------|
| `Enforce` (default) | Yes | Yes | No, failure returned or thrown as above |
| `LogOnly` | Yes | Yes | Yes, the request continues |
| `Off` | No | No | Yes |

`LogOnly` is for rolling out new rules: watch real traffic fail validation before you start rejecting it. Set the default mode at registration, and override it per request with `IValidationModeOverride`:

```csharp
services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.LogOnly);

public record ImportLegacyOrderCommand(string Payload) : ICommand<Result>, IValidationModeOverride
{
    public ValidationMode ValidationMode => ValidationMode.Enforce;
}
```

With Native AOT, register the options and the default observer with `services.AddMediatorValidationOptions(options => ...)` next to `DefaultPipelineBehaviors`.

#### Failure observers

Every `IValidationFailureObserver` in DI is called, in registration order, when validation fails in `Enforce` or `LogOnly` mode. The built-in `LoggingValidationFailureObserver` is registered automatically. It logs a warning in `LogOnly` mode, debug in `Enforce` mode, and increments the `cse.mediator.validation.failures` counter on the `CSharpEssentials.Mediator` meter, tagged with `request` and `mode`. Add your own observer to send failures somewhere else:

```csharp
public sealed class AuditValidationObserver(IAuditLog audit) : IValidationFailureObserver
{
    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken) =>
        audit.WriteAsync(failure.RequestType.Name, failure.Errors, cancellationToken);
}

// using Microsoft.Extensions.DependencyInjection.Extensions;
services.TryAddEnumerable(ServiceDescriptor.Scoped<IValidationFailureObserver, AuditValidationObserver>());
```

Observers are resolved together with the behavior, so their lifetime must not be shorter than the behavior's. `AddMediatorValidationBehavior` registers the behavior as scoped, so scoped observers work. With Native AOT the source generator registers behaviors with its `ServiceLifetime` setting (singleton by default), so register observers as singletons there.

An exception thrown by an observer fails the request, in `LogOnly` mode too, so keep observers cheap and catch your own transport errors.

### Caching Behavior

Implement `ICacheable` on any request.

```csharp
public record GetUserQuery(Guid Id) : IQuery<Result<User>>, ICacheable
{
    public string CacheKey => $"user:{Id}";
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
    public bool CacheFailures => false;
}
```

| Property | Purpose |
|----------|---------|
| `CacheKey` | Unique key for the cached entry |
| `Expiration` | Absolute expiration relative to now; a zero or negative value stores the entry without an expiration |
| `BypassCache` | When `true`, the request skips the cache entirely: no lookup and no store |
| `CacheFailures` | When `true`, caches failed results too |

Requires `IDistributedCache` registered in DI. The response is stored and read as JSON (`System.Text.Json`), so it must round-trip through it.

### Transaction Scope Behavior

Implement `ITransactionalRequest` on any command.

```csharp
public record TransferMoneyCommand(Guid From, Guid To, decimal Amount)
    : ICommand<Result>, ITransactionalRequest;
```

The handler is wrapped in a `TransactionScope` with `TransactionScopeAsyncFlowOption.Enabled`. The scope completes only when the handler returns a successful `Result` / `Result<T>` (or a non-`Result` response); a failed `Result` or an exception rolls it back.

#### Transaction runner

To use the database transaction of your own data access instead of an ambient `TransactionScope`, register `TransactionBehavior` and an `ITransactionRunner` (namespace `CSharpEssentials.Transactions`). The same `ITransactionalRequest` marker applies. Only one transaction behavior is active at a time: `AddMediatorTransactionRunnerBehavior()` takes the pipeline position of a registered `TransactionScopeBehavior`, and `AddMediatorTransactionBehavior()` does the reverse.

```csharp
services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>(); // CSharpEssentials.EntityFrameworkCore
```

The runner commits when the `Result` succeeds and rolls back on a failed `Result` or an exception. Any other runner (Dapper, a message broker outbox, a test fake) implements the single method:

```csharp
public interface ITransactionRunner
{
    ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default);
}
```

With Native AOT, replace `TransactionScopeBehavior<,>` with `TransactionBehavior<,>` in your copy of `DefaultPipelineBehaviors` and set `options.ServiceLifetime = ServiceLifetime.Scoped` in `AddMediator`. With the generator's default singleton lifetime, the behavior would capture one runner, and with it one `DbContext`, for every request.

The replacement only matches behaviors registered by type through these methods; a transaction behavior you registered with a factory is left alone.

### Lock Behavior

Implement `ILockedRequest` to run only one handler at a time per key, for example one payment capture per order:

```csharp
public record CapturePaymentCommand(Guid OrderId) : ICommand<Result>, ILockedRequest
{
    public string LockKey => $"order:{OrderId}";
    public TimeSpan? LockTimeout => TimeSpan.FromSeconds(5); // optional; null (default) waits until cancelled
}

services.AddMediatorBehaviors();
services.AddMediatorLockBehavior(); // LockPlacement.OutsideTransaction
```

The behavior acquires the lock before the handler and releases it when the handler returns or throws. When the lock cannot be acquired within `LockTimeout`, the `IResourceLock` throws `TimeoutException` and the handler does not run. With the default registration `LockBehavior` sits inside `ExceptionHandlingBehavior`, so a `Result` or `Result<T>` handler gets a failed result carrying `Error.Exception(TimeoutException)` instead; cancellation still propagates as `OperationCanceledException`.

`AddMediatorLockBehavior` registers `InProcessResourceLock` as the singleton `IResourceLock` unless one is already registered. It serializes requests within one process only. With several instances, register a distributed implementation (a database advisory lock, Redis, ...) before or after the call:

```csharp
public interface IResourceLock // namespace CSharpEssentials.Locking
{
    ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default);
    ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default);
}

services.AddScoped<IResourceLock, PostgresAdvisoryLock>();
```

When the lock needs a numeric key, derive it with a stable hash (FNV-1a 64 over the UTF-8 bytes, or a server-side hash such as PostgreSQL `hashtextextended(key, 0)`), never `string.GetHashCode()`, which differs between processes.

#### Placement

The lock wraps the transaction behavior from outside by default. Pick the placement that matches the lock:

| Placement | Order | Use with | Caveat |
|-----------|-------|----------|--------|
| `OutsideTransaction` (default) | Lock → Transaction → Handler | Locks that live until disposed: `InProcessResourceLock`, session advisory locks, Redis | A transaction-scoped lock (`pg_advisory_xact_lock`) is released as soon as its own short transaction ends, so it protects nothing |
| `InsideTransaction` | Transaction → Lock → Handler | Transaction-scoped locks taken on the runner's connection, such as `pg_advisory_xact_lock`, whose handle disposes as a no-op and which the database releases at commit or rollback | A lock that is released on dispose (`InProcessResourceLock`) is let go before the commit, so the next request can read data from before the commit |

```csharp
services.AddMediatorBehaviors();
services.AddMediatorTransactionRunnerBehavior();
services.AddEfCoreTransactionRunner<AppDbContext>();
services.AddScoped<IResourceLock, PostgresXactLock>();
services.AddMediatorLockBehavior(LockPlacement.InsideTransaction);
```

Call `AddMediatorLockBehavior` after the other behaviors are registered. With no transaction behavior it is appended, so calling it first would put the lock outside validation, logging and exception handling. A transaction behavior you registered with a factory is not recognized either, and the lock is appended after it, which means inside.

`InsideTransaction` throws `InvalidOperationException` when no transaction behavior is registered. The placement is relative to the transaction behavior, so switching between `TransactionScopeBehavior` and `TransactionBehavior` later keeps it. Calling `AddMediatorLockBehavior` again moves the behavior instead of adding a second one. Only requests that implement both `ITransactionalRequest` and `ILockedRequest` are affected by the order.

With Native AOT, `DefaultPipelineBehaviors` does not include `LockBehavior<,>`. Insert it into your own array right before (outside) or right after (inside) the transaction behavior, and register the `IResourceLock` yourself.

### Idempotency Behavior

Implement `IIdempotentRequest` to run a command at most once per key and caller. A retry with the same key gets the stored response instead of running the handler again:

```csharp
public record CapturePaymentCommand(string IdempotencyKey, Guid OrderId, decimal Amount)
    : ICommand<Result<Receipt>>, IIdempotentRequest;

services.AddMediatorBehaviors();
services.AddMediatorIdempotencyBehavior(o =>
{
    o.ScopeResolver = IdempotencyScopes.Client;                         // machine callers; default: IdempotencyScopes.User
    o.ForRequest<CapturePaymentCommand>(p => p.Profile = "payments");
});
services.AddIdempotencyExecutor(o => o.AddProfile("payments", p =>
{
    p.Retention = TimeSpan.FromDays(7);
    p.Lease = TimeSpan.FromMinutes(5);
}));
```

`AddMediatorIdempotencyBehavior` also registers the executor and, unless one is registered, the in-memory record store. Configure the executor defaults and profiles with `AddIdempotencyExecutor`, and register your own `IIdempotencyRecordStore` for several instances.

The key is the only required member. The others have defaults:

| Member | Default |
|--------|---------|
| `IdempotencyKey` | Required: 1 to 255 printable ASCII characters other than space, `"` and `\`. Take it from the client or build it with `IdempotencyKeys.For(...)` or `IdempotencyKeys.FromMessage(...)` |
| `IdempotencyScope` | `null`: resolved by `ScopeResolver` from the current caller (`IIdempotencyScopeAccessor`). Return an `IdempotencyScope` factory value to scope the key yourself, for example `IdempotencyScope.Tenant(TenantId)` |
| `IdempotencyFingerprint` | `null`: the SHA-256 of the request serialized as JSON. The same key with another fingerprint is `KeyReused` |
| `IdempotencyRetention` / `IdempotencyLease` | `null`: from `ForRequest<T>`, then its profile, then `IdempotencyExecutorOptions` |

The record namespace is the full name of the request type, so two request types never share a record. Outcomes come back as a `Result`:

| Situation | Response |
|-----------|----------|
| Same key, same request, completed | The stored `Result`; the handler does not run |
| Same key, still running | `Error.Conflict("Idempotency.InFlight")`: retry later |
| Same key, different request | `Error.Validation("Idempotency.KeyReused")`: do not retry |
| Missing or invalid key | `Error.Validation("Idempotency.KeyMissing")` / `("Idempotency.KeyInvalid")` |
| No scope resolved | `UnscopedKeyBehavior` (see below) |

A response type that is not a `Result` gets an `EnhancedValidationException` with the same error instead.

What is stored:

| Outcome | Key |
|---------|-----|
| Success | Stored for `Retention` (24 hours) |
| Failed `Result` of an `ITransactionalRequest` | Released, because the transaction rolled back: a retry runs the handler again |
| Other failed `Result` whose first error is `ErrorType.Unexpected` | Stored for `FailureRetention` (10 minutes), then a retry runs again |
| Other failed `Result` | Stored for `Retention`, like a success: a business failure is deterministic. `StoreFailures = false` releases every failure instead |
| Exception thrown by the handler | Stays in flight until `Lease` (1 minute) expires, because the work may be done. A retry before that gets `Idempotency.InFlight`, even when `ExceptionHandlingBehavior` turned the exception into a failed `Result` |

#### Scopes: HTTP and Mediator defaults

A key is only meaningful for its owner, so the behavior scopes it: the request's own `IdempotencyScope` first, then `ScopeResolver` with the principal of `IIdempotencyScopeAccessor`. When neither gives a scope, `UnscopedKeyBehavior` decides. The default differs from the HTTP middleware on purpose:

| Layer | Default | Unscoped keyed request |
|-------|---------|------------------------|
| HTTP middleware (`AddIdempotency`) | `Skip` | Runs without idempotency and logs `Idempotency.UnscopedKeySkipped` |
| Mediator behavior | `Reject` | `Error.Validation("Idempotency.ScopeRequired")` |

So an anonymous or client-credentials request that the HTTP layer lets through is rejected when it reaches an idempotent command. Use `IdempotencyScopes.Client` (or `UserAndTenant`, `Custom`) for machine callers in both layers so both resolve the same scope. `Skip` runs the request without idempotency and logs a warning; `Global` shares one key space across callers. A message consumer has no principal: give the scope from the message (tenant, sending system) or return `IdempotencyScope.Global` deliberately.

In an ASP.NET Core app call `AddIdempotency()` from `CSharpEssentials.AspNetCore`: it registers the `IIdempotencyScopeAccessor` that reads `HttpContext.User`. Without an accessor the resolver gets no principal and, with `Reject`, every keyed request fails with `Idempotency.ScopeRequired`. A failed `Result` carrying an idempotency code that is returned with `ToProblemResult()` or `ToActionResult()` gets the middleware's status and top-level `code` member: `Idempotency.InFlight` → 409 with `Retry-After`, `Idempotency.KeyReused` → 422, `KeyMissing`, `KeyInvalid` and `ScopeRequired` → 400.

#### Placement

| Placement | Order | Use with |
|-----------|-------|----------|
| `OutsideTransaction` (default) | Idempotency → Lock → Transaction → Handler | Any store. The record is written after the commit or rollback. If the process stops between the commit and the write, the key stays in flight until `Lease` expires and a retry after that runs the handler again |
| `InsideTransaction` | Transaction → Idempotency → Lock → Handler | A record store that enlists in the same transaction, so the record commits or rolls back with the handler's data |

> **Warning:** with `InsideTransaction` and a store that does not enlist in the transaction (the in-memory or distributed cache store), a commit that fails after the record was written leaves a stored response for work that never committed, and a retry replays it.

Call `AddMediatorIdempotencyBehavior` after `AddMediatorBehaviors`, so validation runs first and a validation failure never reserves a key. With no transaction behavior it is appended. Calling it again configures the same options and moves the behavior instead of adding a second one. With Native AOT, `DefaultPipelineBehaviors` does not include `IdempotencyBehavior<,>`. Insert it into your own array, and register an `IdempotencyBehaviorOptions` instance and the executor (`AddIdempotencyExecutor`) yourself.

#### Modes

`IdempotencyExecutorOptions.Mode`, a profile or `ForRequest<T>(p => p.Mode = ...)` sets the mode. In `Off` the behavior only calls the handler. In `Observe` it never blocks: a request it would reject (missing or invalid key, unresolved scope) and a replay, in-flight or reused key are logged, and the request runs. A scope resolver or fingerprint that throws is logged as `Idempotency.Observe.ResolutionFailed` and the request runs once without idempotency; in `Enforce`, or when the request was cancelled, the exception propagates.

The mode is read per request from `IOptionsMonitor<IdempotencyExecutorOptions>`, so binding the options to a reloadable configuration section switches it without a restart. It is separate from the `Idempotency-Key` middleware's `IdempotencyOptions.Mode`. `Observe` measures duplicates; it does not prevent them, so keep it short for commands that move money. Decisions are counted on `idempotency.decisions` (meter `CSharpEssentials.Idempotency`) with `source` `mediator` and the request type as `target`; see the [Idempotency Readme](../CSharpEssentials.Idempotency/Readme.MD#telemetry).

### Logging Behavior

Implement marker interfaces to opt into payload logging.

```csharp
public record GetUserQuery(Guid Id) : IQuery<Result<User>>, IRequestResponseLoggable;
```

| Interface | Logs |
|-----------|------|
| `IRequestLoggable` | Request payload as JSON |
| `IResponseLoggable` | Response payload as JSON |
| `IRequestResponseLoggable` | Both request and response |

`LoggingBehavior` only runs for requests that implement one of these interfaces (it is constrained to `ILoggableRequest`). Such a request always logs the handling and handled messages and the elapsed time at `Information` level; the interfaces add the JSON payloads.

### Exception Handling Behavior

When a handler that returns `Result` or `Result<T>` throws, `ExceptionHandlingBehavior` returns `Result.Failure(Error.Exception(ex))` (`ErrorType.Failure`, the exception type name as code, the message as description) instead of letting the exception escape. No marker interface is needed, but the behavior has to be registered: `AddMediatorBehaviors()` or `AddMediatorExceptionHandlingBehavior()`. `OperationCanceledException` always propagates. Handlers with any other response type pass through untouched.

Behaviors placed after it in the pipeline (caching, transaction, lock) run inside it, so their exceptions are converted too. Validation and logging run outside it.

## Marker Interfaces

| Interface | Extends | Purpose |
|-----------|---------|---------|
| `ICacheable` | None | Enables `CachingBehavior` |
| `ITransactionalRequest` | None | Enables `TransactionScopeBehavior` or `TransactionBehavior`, whichever is registered |
| `ILockedRequest` | None | Enables `LockBehavior` with `LockKey` and optional `LockTimeout` |
| `IIdempotentRequest` | None | Enables `IdempotencyBehavior` with `IdempotencyKey` and optional `IdempotencyScope`, `IdempotencyFingerprint`, `IdempotencyRetention`, `IdempotencyLease` |
| `ILoggableRequest` | `IMessage` | Base for logging opt-in |
| `IRequestLoggable` | `ILoggableRequest` | Log request payload |
| `IResponseLoggable` | `ILoggableRequest` | Log response payload |
| `IRequestResponseLoggable` | `IRequestLoggable`, `IResponseLoggable` | Log both |

## Pipeline Behaviors

| Behavior | Registered by | Applies to |
|----------|---------------|------------|
| `ValidationBehavior<TRequest,TResponse>` | `AddMediatorBehaviors()`, `AddMediatorValidationBehavior()` | `TRequest : IMessage` (every request) |
| `LoggingBehavior<TRequest,TResponse>` | `AddMediatorBehaviors()`, `AddMediatorLoggingBehavior()` | `TRequest : ILoggableRequest, IMessage` |
| `ExceptionHandlingBehavior<TRequest,TResponse>` | `AddMediatorBehaviors()`, `AddMediatorExceptionHandlingBehavior()` | `TRequest : IMessage`; converts only for `Result` / `Result<T>` responses |
| `CachingBehavior<TRequest,TResponse>` | `AddMediatorBehaviors()`, `AddMediatorCachingBehavior()` | `TRequest : ICacheable, IMessage` |
| `TransactionScopeBehavior<TRequest,TResponse>` | `AddMediatorBehaviors()`, `AddMediatorTransactionBehavior()` | `TRequest : ITransactionalRequest, IMessage` |
| `TransactionBehavior<TRequest,TResponse>` | `AddMediatorTransactionRunnerBehavior()`, replaces `TransactionScopeBehavior` | `TRequest : ITransactionalRequest, IMessage` |
| `LockBehavior<TRequest,TResponse>` | `AddMediatorLockBehavior()` | `TRequest : ILockedRequest, IMessage` |
| `IdempotencyBehavior<TRequest,TResponse>` | `AddMediatorIdempotencyBehavior()` | `TRequest : IIdempotentRequest, IMessage`; replays and rejections are `Result` failures for `Result` / `Result<T>` responses |

> `ValidationBehavior` is registered as **scoped** (to avoid captive dependency with scoped validators). `TransactionBehavior`, `LockBehavior` and `IdempotencyBehavior` are scoped as well, because `ITransactionRunner` and database-backed `IResourceLock` implementations usually depend on a scoped `DbContext`. All other behaviors are singletons. The Mediator source-generator automatically includes them in the pipeline for matching request types.

## Dependencies

- `CSharpEssentials.Results`
- `CSharpEssentials.Maybe`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Json`
- `CSharpEssentials.Validation`
- `CSharpEssentials.Idempotency`
- `Mediator.Abstractions`
- `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.DependencyInjection.Abstractions`
