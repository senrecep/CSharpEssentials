# CSharpEssentials.Mediator

Pipeline behaviors for the [Mediator](https://github.com/martinothamar/Mediator) source-generator library, with built-in support for `Result<T>`, validation, caching, logging, and transactions. Native AOT compatible, with zero reflection at runtime.

> **Note:** `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` are provided by the `Mediator` package itself. This package adds marker interfaces and pipeline behaviors that integrate with `Result<T>`.

## Features

- **Validation Behavior**: CSharpEssentials.Validation integration; failures return `Result.Failure` for `Result`/`Result<T>` handlers, or throw `EnhancedValidationException` for any other return type. `Enforce`, `LogOnly` and `Off` modes, globally or per request, with pluggable failure observers.
- **Logging Behavior**: Opt-in request/response payload logging with elapsed-time tracking.
- **Caching Behavior**: `IDistributedCache` integration with bypass and failure-cache control.
- **Transaction Behavior**: Wraps handlers in `TransactionScope` with async flow enabled.

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
services.AddMediatorBehaviors(); // Registers all 4 behaviors

// Or individually:
services.AddMediatorValidationBehavior();
services.AddMediatorLoggingBehavior();
services.AddMediatorCachingBehavior();
services.AddMediatorTransactionBehavior();
```

**Native AOT**

When targeting Native AOT, pass behaviors via `AddMediator` options instead of relying on open-generic DI registration. `DefaultPipelineBehaviors` provides the pre-ordered array:

```csharp
services.AddMediator(options =>
{
    options.PipelineBehaviors = MediatorExtensions.DefaultPipelineBehaviors; // namespace Microsoft.Extensions.DependencyInjection
});
```

Pipeline order: **Validation → Logging → Caching → Transaction**

> See [Mediator §4.4: Use pipeline behaviors](https://github.com/martinothamar/Mediator#44-use-pipeline-behaviors) for the full Native AOT pipeline behavior documentation.

### Commands & Queries

Use `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` directly from the `Mediator` package. Return `Result<T>` from handlers naturally.

```csharp
using Mediator;
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
| `Expiration` | Absolute expiration relative to now |
| `BypassCache` | When `true`, skips cache lookup and stores fresh |
| `CacheFailures` | When `true`, caches failed results too |

Requires `IDistributedCache` registered in DI.

### Transaction Scope Behavior

Implement `ITransactionalRequest` on any command.

```csharp
public record TransferMoneyCommand(Guid From, Guid To, decimal Amount)
    : ICommand<Result>, ITransactionalRequest;
```

The handler is wrapped in a `TransactionScope` with `TransactionScopeAsyncFlowOption.Enabled`.

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

All requests log basic handling/handled messages and elapsed time regardless of marker interfaces.

## Marker Interfaces

| Interface | Extends | Purpose |
|-----------|---------|---------|
| `ICacheable` | None | Enables `CachingBehavior` |
| `ITransactionalRequest` | None | Enables `TransactionScopeBehavior` |
| `ILoggableRequest` | `IMessage` | Base for logging opt-in |
| `IRequestLoggable` | `ILoggableRequest` | Log request payload |
| `IResponseLoggable` | `ILoggableRequest` | Log response payload |
| `IRequestResponseLoggable` | `IRequestLoggable`, `IResponseLoggable` | Log both |

## Pipeline Behaviors

| Behavior | Trigger | Constraint |
|----------|---------|------------|
| `ValidationBehavior<TRequest,TResponse>` | Always registered | `TRequest : IMessage` |
| `LoggingBehavior<TRequest,TResponse>` | Always registered | `TRequest : ILoggableRequest, IMessage` |
| `CachingBehavior<TRequest,TResponse>` | Always registered | `TRequest : ICacheable, IMessage` |
| `TransactionScopeBehavior<TRequest,TResponse>` | Always registered | `TRequest : ITransactionalRequest, IMessage` |

> `ValidationBehavior` is registered as **scoped** (to avoid captive dependency with scoped validators). All other behaviors are singletons. The Mediator source-generator automatically includes them in the pipeline for matching request types.

## Dependencies

- `CSharpEssentials.ResultPattern`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Json`
- `CSharpEssentials.Validation`
- `Mediator.Abstractions`
