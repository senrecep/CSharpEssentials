<p align="center">
  <img src="icon.png" alt="CSharpEssentials" width="120" />
</p>

# CSharpEssentials

[![Build](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml/badge.svg)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)

[![Tests](https://img.shields.io/badge/tests-3730%20passing-brightgreen)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![Downloads](https://img.shields.io/nuget/dt/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/senrecep/CSharpEssentials/blob/main/LICENCE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010%20%7C%2011%20%7C%20ns2.0%20%7C%20ns2.1-512BD4)](https://dotnet.microsoft.com)
[![Packages](https://img.shields.io/badge/packages-23-blue)](https://www.nuget.org/profiles/senrecep)

Modular .NET NuGet ecosystem that bridges OOP and Functional Programming in C#.

## Packages

| Package | Description | Docs | NuGet |
|---------|-------------|------|-------|
| `CSharpEssentials` | Meta-package: Results, Errors, Maybe, Any, These, Rules, Core, Enums, Entity, Clone, Json, Http, Time | [Docs](examples/Examples.Main/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials) |
| `CSharpEssentials.Results` | `Result<T>`: railway-oriented error handling without exceptions | [Docs](examples/Examples.Results/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Results.svg)](https://www.nuget.org/packages/CSharpEssentials.Results) |
| `CSharpEssentials.Errors` | Structured `Error` values with type, code, description, metadata | [Docs](examples/Examples.Errors/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Errors.svg)](https://www.nuget.org/packages/CSharpEssentials.Errors) |
| `CSharpEssentials.Maybe` | `Maybe<T>`: explicit optionals, no null reference exceptions | [Docs](examples/Examples.Maybe/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Maybe.svg)](https://www.nuget.org/packages/CSharpEssentials.Maybe) |
| `CSharpEssentials.Any` | `Any<T1,T2,...>`: type-safe discriminated unions | [Docs](examples/Examples.Any/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Any.svg)](https://www.nuget.org/packages/CSharpEssentials.Any) |
| `CSharpEssentials.These` | `These<TError,TValue>`: 3-state union with Left, Right, Both (error + value simultaneously) | [Docs](CSharpEssentials.These/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.These.svg)](https://www.nuget.org/packages/CSharpEssentials.These) |
| `CSharpEssentials.Core` | String/GUID/collection utilities | [Docs](examples/Examples.Core/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Core.svg)](https://www.nuget.org/packages/CSharpEssentials.Core) |
| `CSharpEssentials.Enums` | `[StringEnum]` source generator, AOT-safe enum↔string | [Docs](examples/Examples.Enums/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) |
| `CSharpEssentials.Rules` | Composable rule engine with `.And()/.Or()/.Linear()/.Next()` | [Docs](examples/Examples.Rules/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Rules.svg)](https://www.nuget.org/packages/CSharpEssentials.Rules) |
| `CSharpEssentials.Mediator` | CQRS pipeline behaviors: validation, logging, exception handling, caching, transactions | [Docs](CSharpEssentials.Mediator/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Mediator.svg)](https://www.nuget.org/packages/CSharpEssentials.Mediator) |
| `CSharpEssentials.Entity` | `EntityBase<TId>`, soft deletion, domain events | [Docs](examples/Examples.Entity/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Entity.svg)](https://www.nuget.org/packages/CSharpEssentials.Entity) |
| `CSharpEssentials.EntityFrameworkCore` | EF Core interceptors (audit, events, slow queries) + pagination | [Docs](examples/Examples.EntityFrameworkCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.EntityFrameworkCore.svg)](https://www.nuget.org/packages/CSharpEssentials.EntityFrameworkCore) |
| `CSharpEssentials.AspNetCore` | `GlobalExceptionHandler`, `ResultEndpointFilter`, Swagger versioning | [Docs](examples/Examples.AspNetCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) |
| `CSharpEssentials.Endpoints` | Source-generated Minimal API endpoint discovery (`IEndpoint`, `IEndpointGroup`), AOT-safe, analyzers CSE1001 to CSE1011 | [Docs](CSharpEssentials.Endpoints/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Endpoints.svg)](https://www.nuget.org/packages/CSharpEssentials.Endpoints) |
| `CSharpEssentials.DependencyInjection` | Attribute-based, source-generated service registration and decoration (`[RegisterScoped]`, `[Decorates]`), analyzers CSE2001 to CSE2009 | [Docs](CSharpEssentials.DependencyInjection/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.DependencyInjection.svg)](https://www.nuget.org/packages/CSharpEssentials.DependencyInjection) |
| `CSharpEssentials.Http` | `HttpClient` extensions returning `Result<T>` | [Docs](CSharpEssentials.Http/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Http.svg)](https://www.nuget.org/packages/CSharpEssentials.Http) |
| `CSharpEssentials.Resilience` | Transient fault handling: Retry, Timeout, Circuit Breaker, Fallback | [Docs](CSharpEssentials.Resilience/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Resilience.svg)](https://www.nuget.org/packages/CSharpEssentials.Resilience) |
| `CSharpEssentials.Json` | Pre-configured `System.Text.Json` options and converters | [Docs](examples/Examples.Json/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Json.svg)](https://www.nuget.org/packages/CSharpEssentials.Json) |
| `CSharpEssentials.RequestResponseLogging` | Request/response body logging middleware | [Docs](examples/Examples.RequestResponseLogging/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.RequestResponseLogging.svg)](https://www.nuget.org/packages/CSharpEssentials.RequestResponseLogging) |
| `CSharpEssentials.GcpSecretManager` | GCP Secret Manager → `IConfiguration` provider | [Docs](examples/Examples.GcpSecretManager/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.GcpSecretManager.svg)](https://www.nuget.org/packages/CSharpEssentials.GcpSecretManager) |
| `CSharpEssentials.Time` | Testable `IDateTimeProvider` wrapping `TimeProvider` | [Docs](examples/Examples.Time/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Time.svg)](https://www.nuget.org/packages/CSharpEssentials.Time) |
| `CSharpEssentials.Validation` | High-performance model-first validation with `Result<T>` integration | [Docs](CSharpEssentials.Validation/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Validation.svg)](https://www.nuget.org/packages/CSharpEssentials.Validation) |
| `CSharpEssentials.Clone` | `ICloneable<T>`: typed deep-copy for entity collections | [Docs](examples/Examples.Clone/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Clone.svg)](https://www.nuget.org/packages/CSharpEssentials.Clone) |

## Documentation

**[API Reference](docs/API_REFERENCE.md)**: guide to every package, method, and pattern in the ecosystem. It covers all 23 packages with method tables, philosophy, code examples, and cross-cutting FP patterns.

## Installation

```bash
# Core functional modules
dotnet add package CSharpEssentials

# Individual packages
dotnet add package CSharpEssentials.Rules
dotnet add package CSharpEssentials.Mediator
```

## Core Concepts

### Result\<T\>: Railway-Oriented Error Handling

Stop throwing exceptions for expected failures. Chain operations that short-circuit on the first error.

```csharp
// ❌ Traditional: exceptions as control flow
public User GetUserOrThrow(Guid id)
{
    var user = _db.Find(id);
    if (user == null) throw new NotFoundException("User not found");
    return user;
}

// ✅ Result<T>: explicit, composable error handling
public Result<User> GetUser(Guid id)
{
    var user = _db.Find(id);
    if (user is null) return Error.NotFound("User.NotFound", "User not found");
    return user;
}

// Chain operations — stops at first failure, no try/catch needed
Result<UserDto> result = GetUser(id)
    .Bind(user => ValidateAge(user))
    .Bind(user => CheckPermissions(user))
    .Map(user => new UserDto(user));
```

### Maybe\<T\>: Explicit Optionals

Model the absence of a value without null references.

```csharp
// ❌ Null checks everywhere, easy to miss
string display = user?.Profile?.DisplayName?.Trim()?.ToUpper() ?? "ANONYMOUS";

// ✅ Maybe<T>: pipeline that handles absence at every step
string displayName = Maybe.From(user)
    .Bind(u  => Maybe.From(u.Profile))
    .Bind(p  => Maybe.From(p.DisplayName))
    .Map(n   => n.Trim().ToUpper())
    .GetValueOrDefault("ANONYMOUS");
```

### Any\<T1,T2,...\>: Discriminated Unions

A value that is exactly one of several types, exhaustively matched at compile time.

```csharp
Any<Circle, Rectangle, Triangle> shape = new Circle(radius: 5);

AnyActionResult<double> area = shape.Match(
    circle    => Math.PI * circle.Radius * circle.Radius,
    rectangle => rectangle.Width * rectangle.Height,
    triangle  => 0.5 * triangle.Base * triangle.Height
    // compiler error if any case is missing
);
```

### Rules: Composable Validation

Build complex validation logic from simple, reusable rules that return `Result<T>`.

```csharp
Func<User, Result> minAge     = u => u.Age >= 18            ? Result.Success() : Error.Validation("Age.MinAge", "Must be 18+");
Func<User, Result> validEmail = u => u.Email.Contains('@') ? Result.Success() : Error.Validation("Email.Invalid", "Invalid email");
Func<User, Result> verified   = u => u.IsVerified          ? Result.Success() : Error.Validation("Account.NotVerified", "Account not verified");

// AND — all rules must pass, collects every failure
Result result = RuleEngine.Evaluate(new[] { minAge, validEmail, verified }.And(), user);

// OR — at least one rule must pass
Result adminOrMod = RuleEngine.Evaluate(new[] { isAdmin, isModerator }.Or(), user);
```

### AspNetCore: Result → HTTP, Automatically

Map `Result<T>` to correct HTTP status codes without writing any error-mapping boilerplate.

```csharp
// Program.cs
builder.Services.AddEnhancedProblemDetails(); // registers ProblemDetails (see "Migrating from 3.x to 4.0")

// Controller — business logic returns Result<T>, HTTP mapping is automatic
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public IActionResult GetProduct(Guid id)
        => productService.GetProduct(id).Match(
            onSuccess: product => Ok(product),
            onError: errors  => errors.ToActionResult() // 404 / 400 / 409 based on ErrorType
        );
}

// Service — chain multiple steps, first failure short-circuits
public Result<Order> PlaceOrder(Guid productId, int quantity)
    => _productService.GetProduct(productId)
        .Then(p => ValidateStock(p, quantity))
        .Then(p => ReserveStock(p, quantity))
        .Then(p => CreateOrder(p, quantity));
```

### Endpoints: Source-Generated Minimal APIs

Each endpoint is a type with a static `Map` method. A source generator collects them into one registry per assembly, so mapping uses no reflection and works with Native AOT.

```csharp
public sealed class ProductsGroup : IEndpointGroup
{
    public static string Prefix => "products";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Products");
}

[EndpointGroup<ProductsGroup>]
public sealed class GetProduct : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:guid}", (Guid id, IProductService service) =>
            service.GetProduct(id).Match(product => Results.Ok(product), errors => errors.ToProblemResult()));
}

// Program.cs
app.MapAllEndpoints(); // every generated registry of the app and its referenced assemblies
```

### DependencyInjection: Attribute-Based Registration

Mark a class with its lifetime. A source generator writes the registrations, and decorators are applied in order.

```csharp
[RegisterScoped] // registered as IOrderService, by the I{TypeName} rule
public sealed class OrderService : IOrderService { }

[Decorates(typeof(IOrderService), Order = 1)]
public sealed class LoggingOrderService(IOrderService inner) : IOrderService { }

// Program.cs
builder.Services.AddAllServices(); // every generated registry of the app and its referenced assemblies
```

## Upgrading

- [CHANGELOG.md](CHANGELOG.md) lists the changes in every release.
- Coming from 3.x? Read [Migrating from 3.x to 4.0](docs/migration/v3-to-v4.md). 4.0 changed ProblemDetails defaults, enum naming and some package dependencies.

## AI Skills

Install skills for Claude Code, Cursor, Codex, and 50+ other AI agents:

```bash
npx skills add senrecep/CSharpEssentials
```

Each package has a dedicated skill with accurate API examples, correct namespaces, and common pitfalls.
