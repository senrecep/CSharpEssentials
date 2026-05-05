# CSharpEssentials.Mediator

Pipeline behaviors for the [Mediator](https://github.com/martinothamar/Mediator) source-generator library, with built-in support for `Result<T>`, validation, caching, logging, and transactions. Native AOT compatible — zero reflection at runtime.

> **Note:** `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` are provided by the `Mediator` package itself. This package adds marker interfaces and pipeline behaviors that integrate with `Result<T>`.

## Features

- **Validation Behavior** — FluentValidation integration; validation failures map to `Result.Failure` automatically.
- **Logging Behavior** — Opt-in request/response payload logging with elapsed-time tracking.
- **Caching Behavior** — `IDistributedCache` integration with bypass and failure-cache control.
- **Transaction Behavior** — Wraps handlers in `TransactionScope` with async flow enabled.

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
    options.PipelineBehaviors = CSharpEssentials.Mediator.MediatorExtensions.DefaultPipelineBehaviors;
});
```

Pipeline order: **Validation → Logging → Caching → Transaction**

> See [Mediator §4.4 — Use pipeline behaviors](https://github.com/martinothamar/Mediator#44-use-pipeline-behaviors) for the full Native AOT pipeline behavior documentation.

### Commands & Queries

Use `ICommand`, `IQuery`, `ICommandHandler`, and `IQueryHandler` directly from the `Mediator` package. Return `Result<T>` from handlers naturally.

```csharp
using Mediator;
using CSharpEssentials.ResultPattern;

// Unit command
public record DeleteUserCommand(Guid Id) : ICommand<Result>;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        await _repo.DeleteAsync(command.Id, ct);
        return Result.Success();
    }
}

// Command with response
public record CreateUserCommand(string Email, string Name) : ICommand<Result<Guid>>;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var user = new User { Email = command.Email, Name = command.Name };
        await _repo.AddAsync(user, ct);
        return user.Id;
    }
}

// Query
public record GetUserQuery(Guid Id) : IQuery<Result<User>>;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, Result<User>>
{
    public async ValueTask<Result<User>> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await _repo.FindAsync(query.Id, ct);
        return user ?? Result.Failure<User>(Error.NotFound("User.NotFound", "User not found"));
    }
}
```

### Validation Behavior

Add FluentValidation validators to DI. The behavior intercepts requests before the handler runs.

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

Validation errors are converted to `Error.Validation` with `PropertyName` preserved in metadata. Multiple validators are aggregated.

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
| `ICacheable` | — | Enables `CachingBehavior` |
| `ITransactionalRequest` | — | Enables `TransactionScopeBehavior` |
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

> Behaviors are registered as singletons for `IPipelineBehavior<,>`. The Mediator source-generator automatically includes them in the pipeline for matching request types.

## Dependencies

- `CSharpEssentials.ResultPattern`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Json`
- `FluentValidation`
- `Mediator.Abstractions`
