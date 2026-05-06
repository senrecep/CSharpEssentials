<p align="center">
  <img src="icon.png" alt="CSharpEssentials" width="120" />
</p>

# CSharpEssentials

[![Build](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml/badge.svg)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)
[![Tests](https://img.shields.io/badge/tests-2239%20passing-brightgreen)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![Downloads](https://img.shields.io/nuget/dt/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/senrecep/CSharpEssentials/blob/main/LICENCE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010%20%7C%2011%20%7C%20ns2.0%20%7C%20ns2.1-512BD4)](https://dotnet.microsoft.com)
[![Packages](https://img.shields.io/badge/packages-18-blue)](https://www.nuget.org/profiles/senrecep)

Modular .NET NuGet ecosystem that bridges OOP and Functional Programming in C#.

## Packages

| Package | Description | Docs | NuGet |
|---------|-------------|------|-------|
| `CSharpEssentials` | Meta-package: Results + Errors + Maybe + Any + Core + Enums | [📖](examples/Examples.Main/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials) |
| `CSharpEssentials.Results` | `Result<T>` — railway-oriented error handling without exceptions | [📖](examples/Examples.Results/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Results.svg)](https://www.nuget.org/packages/CSharpEssentials.Results) |
| `CSharpEssentials.Errors` | Structured `Error` values with type, code, description, metadata | [📖](examples/Examples.Errors/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Errors.svg)](https://www.nuget.org/packages/CSharpEssentials.Errors) |
| `CSharpEssentials.Maybe` | `Maybe<T>` — explicit optionals, no null reference exceptions | [📖](examples/Examples.Maybe/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Maybe.svg)](https://www.nuget.org/packages/CSharpEssentials.Maybe) |
| `CSharpEssentials.Any` | `Any<T1,T2,...>` — type-safe discriminated unions | [📖](examples/Examples.Any/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Any.svg)](https://www.nuget.org/packages/CSharpEssentials.Any) |
| `CSharpEssentials.Core` | String/GUID/collection utilities | [📖](examples/Examples.Core/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Core.svg)](https://www.nuget.org/packages/CSharpEssentials.Core) |
| `CSharpEssentials.Enums` | `[StringEnum]` source generator — AOT-safe enum↔string | [📖](examples/Examples.Enums/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) |
| `CSharpEssentials.Rules` | Composable rule engine with `.And()/.Or()/.Linear()/.Next()` | [📖](examples/Examples.Rules/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Rules.svg)](https://www.nuget.org/packages/CSharpEssentials.Rules) |
| `CSharpEssentials.Mediator` | CQRS pipeline behaviors: validation, logging, caching, transactions | [📖](CSharpEssentials.Mediator/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Mediator.svg)](https://www.nuget.org/packages/CSharpEssentials.Mediator) |
| `CSharpEssentials.Entity` | `EntityBase<TId>`, soft deletion, domain events | [📖](examples/Examples.Entity/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Entity.svg)](https://www.nuget.org/packages/CSharpEssentials.Entity) |
| `CSharpEssentials.EntityFrameworkCore` | EF Core interceptors (audit, events, slow queries) + pagination | [📖](examples/Examples.EntityFrameworkCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.EntityFrameworkCore.svg)](https://www.nuget.org/packages/CSharpEssentials.EntityFrameworkCore) |
| `CSharpEssentials.AspNetCore` | `GlobalExceptionHandler`, `ResultEndpointFilter`, Swagger versioning | [📖](examples/Examples.AspNetCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) |
| `CSharpEssentials.Http` | `HttpClient` extensions returning `Result<T>` | [📖](CSharpEssentials.Http/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Http.svg)](https://www.nuget.org/packages/CSharpEssentials.Http) |
| `CSharpEssentials.Json` | Pre-configured `System.Text.Json` options and converters | [📖](examples/Examples.Json/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Json.svg)](https://www.nuget.org/packages/CSharpEssentials.Json) |
| `CSharpEssentials.RequestResponseLogging` | Request/response body logging middleware | [📖](examples/Examples.RequestResponseLogging/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.RequestResponseLogging.svg)](https://www.nuget.org/packages/CSharpEssentials.RequestResponseLogging) |
| `CSharpEssentials.GcpSecretManager` | GCP Secret Manager → `IConfiguration` provider | [📖](examples/Examples.GcpSecretManager/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.GcpSecretManager.svg)](https://www.nuget.org/packages/CSharpEssentials.GcpSecretManager) |
| `CSharpEssentials.Time` | Testable `IDateTimeProvider` wrapping `TimeProvider` | [📖](examples/Examples.Time/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Time.svg)](https://www.nuget.org/packages/CSharpEssentials.Time) |
| `CSharpEssentials.Clone` | `ICloneable<T>` — typed deep-copy for entity collections | [📖](examples/Examples.Clone/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Clone.svg)](https://www.nuget.org/packages/CSharpEssentials.Clone) |

## Installation

```bash
# Core functional modules
dotnet add package CSharpEssentials

# Individual packages
dotnet add package CSharpEssentials.Rules
dotnet add package CSharpEssentials.Mediator
```

## Core Concepts

### Result\<T\> — Railway-Oriented Error Handling

Stop throwing exceptions for expected failures. Chain operations that short-circuit on the first error.

```csharp
// ❌ Traditional: exceptions as control flow
public User GetUser(Guid id)
{
    var user = _db.Find(id);
    if (user == null) throw new NotFoundException("User not found");
    return user;
}

// ✅ Result<T>: explicit, composable error handling
public Result<User> GetUser(Guid id)
{
    var user = _db.Find(id);
    if (user is null) return Error.NotFound("User not found");
    return Result.Success(user);
}

// Chain operations — stops at first failure, no try/catch needed
Result<UserDto> result = GetUser(id)
    .Bind(user => ValidateAge(user))
    .Bind(user => CheckPermissions(user))
    .Map(user => new UserDto(user));
```

### Maybe\<T\> — Explicit Optionals

Model the absence of a value without null references.

```csharp
// ❌ Null checks everywhere, easy to miss
string display = user?.Profile?.DisplayName?.Trim()?.ToUpper() ?? "ANONYMOUS";

// ✅ Maybe<T>: pipeline that handles absence at every step
string display = Maybe.Return(user)
    .Bind(u  => Maybe.Return(u.Profile))
    .Bind(p  => Maybe.Return(p.DisplayName))
    .Map(n   => n.Trim().ToUpper())
    .Fallback("ANONYMOUS");
```

### Any\<T1,T2,...\> — Discriminated Unions

A value that is exactly one of several types — exhaustively matched at compile time.

```csharp
Any<Circle, Rectangle, Triangle> shape = new Circle(radius: 5);

double area = shape.Match(
    circle    => Math.PI * circle.Radius * circle.Radius,
    rectangle => rectangle.Width * rectangle.Height,
    triangle  => 0.5 * triangle.Base * triangle.Height
    // compiler error if any case is missing
);
```

### Rules — Composable Validation

Build complex validation logic from simple, reusable rules that return `Result<T>`.

```csharp
var minAge     = Rule.Create<User>(u => u.Age >= 18,          Error.Validation("Must be 18+"));
var validEmail = Rule.Create<User>(u => u.Email.Contains('@'), Error.Validation("Invalid email"));
var verified   = Rule.Create<User>(u => u.IsVerified,          Error.Validation("Account not verified"));

// AND — all rules must pass, collects every failure
Result<User> result = RuleEngine
    .Create(minAge, validEmail, verified)
    .Evaluate(user);

// OR — at least one rule must pass
Result<User> adminOrMod = RuleEngine
    .Create(isAdmin, isModerator)
    .Or()
    .Evaluate(user);
```

### AspNetCore — Result → HTTP, Automatically

Map `Result<T>` to correct HTTP status codes without writing any error-mapping boilerplate.

```csharp
// Program.cs
app.AddCSharpEssentials(); // registers GlobalExceptionHandler + ProblemDetails

// Controller — business logic returns Result<T>, HTTP mapping is automatic
[HttpGet("{id:guid}")]
public IActionResult GetProduct(Guid id)
    => _productService.GetProduct(id).Match(
        onSuccess: product => Ok(product),
        onFailure: errors  => errors.ToActionResult() // 404 / 400 / 409 based on ErrorType
    );

// Service — chain multiple steps, first failure short-circuits
public Result<Order> PlaceOrder(Guid productId, int quantity)
    => _productService.GetProduct(productId)
        .Then(p => ValidateStock(p, quantity))
        .Then(p => ReserveStock(p, quantity))
        .Then(p => CreateOrder(p, quantity));
```

## AI Skills

Install skills for Claude Code, Cursor, Codex, and 50+ other AI agents:

```bash
npx skills add senrecep/CSharpEssentials
```

Each package has a dedicated skill with accurate API examples, correct namespaces, and common pitfalls.
