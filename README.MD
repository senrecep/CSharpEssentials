<p align="center">
  <img src="icon.png" alt="CSharpEssentials" width="120" />
</p>

# CSharpEssentials

[![Build](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml/badge.svg)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)

[![Tests](https://img.shields.io/badge/tests-4082%20passing-brightgreen)](https://github.com/senrecep/CSharpEssentials/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![Downloads](https://img.shields.io/nuget/dt/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/senrecep/CSharpEssentials/blob/main/LICENCE)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010%20%7C%2011%20%7C%20ns2.0%20%7C%20ns2.1-512BD4)](https://dotnet.microsoft.com)
[![Packages](https://img.shields.io/badge/packages-25-blue)](https://www.nuget.org/profiles/senrecep)

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
| `CSharpEssentials.Enums` | `[StringEnum]` source generator, AOT-safe enum↔string | [Docs](CSharpEssentials.Enums/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) |
| `CSharpEssentials.Rules` | Composable rule engine with `.And()/.Or()/.Linear()/.Next()` | [Docs](examples/Examples.Rules/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Rules.svg)](https://www.nuget.org/packages/CSharpEssentials.Rules) |
| `CSharpEssentials.Mediator` | CQRS pipeline behaviors: validation, logging, exception handling, caching, transactions | [Docs](CSharpEssentials.Mediator/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Mediator.svg)](https://www.nuget.org/packages/CSharpEssentials.Mediator) |
| `CSharpEssentials.Entity` | `EntityBase<TId>`, soft deletion, domain events | [Docs](examples/Examples.Entity/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Entity.svg)](https://www.nuget.org/packages/CSharpEssentials.Entity) |
| `CSharpEssentials.EntityFrameworkCore` | EF Core interceptors (audit, events, slow queries) + pagination | [Docs](examples/Examples.EntityFrameworkCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.EntityFrameworkCore.svg)](https://www.nuget.org/packages/CSharpEssentials.EntityFrameworkCore) |
| `CSharpEssentials.AspNetCore` | `GlobalExceptionHandler`, `ResultEndpointFilter`, enum binding, API versioning | [Docs](examples/Examples.AspNetCore/README.md) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) |
| `CSharpEssentials.AspNetCore.OpenApi` | Enum convention schemas for `Microsoft.AspNetCore.OpenApi` (net10.0+, Microsoft.OpenApi 2.x); one OpenAPI package per host | [Docs](CSharpEssentials.AspNetCore.OpenApi/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.OpenApi.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore.OpenApi) |
| `CSharpEssentials.AspNetCore.Swashbuckle` | Swagger versioning, schema ids, security schemes and enum convention schemas for Swashbuckle (net8.0+, Microsoft.OpenApi 1.x); one OpenAPI package per host | [Docs](CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD) | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.Swashbuckle.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore.Swashbuckle) |
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

**[API Reference](docs/API_REFERENCE.md)**: guide to every package, method, and pattern in the ecosystem. It covers all 25 packages with method tables, philosophy, code examples, and cross-cutting FP patterns.

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

## Enums end to end

Mark an enum with `[StringEnum]` and every layer reads and writes it the same way, driven by one `EnumConventions` instance (`CSharpEssentials.Enums`). The wire name is snake_case by default (`PendingApproval` → `pending_approval`); change it per enum (`[StringEnum(Naming = ...)]`), per member (`[JsonStringEnumMemberName]`) or per project (the `CSharpEssentialsEnumNaming` MSBuild property). Enums without `[StringEnum]` keep the framework defaults.

```csharp
[StringEnum]
public enum OrderStatus
{
    Pending,
    [EnumAlias("Approval")] PendingApproval,
    [EnumFallback] Unknown = 99, // what tolerant readers map unknown values to
}
```

| Layer | Package | Setup | Reads | Writes |
|---|---|---|---|---|
| JSON (System.Text.Json) | `CSharpEssentials.Json` | `options.AddEnumConventions(conventions)` | wire name, member name in any casing, alias, defined number; undefined numbers rejected | `"pending_approval"`, or `1` with `WriteAs = Number` |
| Route, query, header, form | `CSharpEssentials.AspNetCore` | `AddEnumConventions()` + `UseEnumBinding()` | same rules as a JSON body, trimmed; invalid → 400 problem listing the allowed values | |
| Responses | `CSharpEssentials.AspNetCore` | `WithEnumWireFormat(...)` per group, `[EnumWireFormat]` per controller | | string by default, number per group |
| OpenAPI | `CSharpEssentials.AspNetCore.OpenApi` or `.Swashbuckle` (one per host) | `o.AddEnumConventions()` | | string or integer schema, value table, `x-enum-*` |
| EF Core | `CSharpEssentials.EntityFrameworkCore` | `ConfigureEnumConventions(conventions)` | tolerant: any known spelling, unknown → fallback member | `text` column + `ck_{table}_{column}_enum`; `[Flags]` as an integer bitmask |
| Outgoing HTTP | `CSharpEssentials.Http` | `WithEnumConventions(conventions)` on `HttpRequestBuilder` | tolerant | wire names in route and query |
| Validation | `CSharpEssentials.Validation` | `IsDefinedEnum()`, `IsOneOf(...)`, `HasOnlyDefinedFlags()` | | |

`CSharpEssentials.Enums` carries the source generator, the analyzers and the code fixes. Any CSharpEssentials package brings the generator and analyzers to your project: every package that depends on Enums, directly or through another CSharpEssentials package, passes them on. A direct `CSharpEssentials.Enums` reference is not required; add it only when the project uses no other CSharpEssentials package that depends on it (`.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on it).

| Scenario | Packages |
|---|---|
| ASP.NET Core API | `CSharpEssentials.AspNetCore` |
| + OpenAPI | `CSharpEssentials.AspNetCore.OpenApi` (net10.0) or `CSharpEssentials.AspNetCore.Swashbuckle`, never both |
| + EF Core | `CSharpEssentials.EntityFrameworkCore` |
| Worker or console with EF Core | `CSharpEssentials.EntityFrameworkCore` |
| JSON only (message bus, client) | `CSharpEssentials.Json` |
| Outgoing HTTP | `CSharpEssentials.Http` |
| Generated helpers and `EnumMetadata` only | `CSharpEssentials.Enums` |

One-time setup per host:

```csharp
// Minimal API: JSON options, binding, body errors and output in one call
builder.Services.AddEnumConventions();
app.UseEnumBinding(); // after routing, UseAuthentication and UseAuthorization

// Body errors as the same 400 problem: Minimal APIs throw a body error only with ThrowOnBadRequest,
// and GlobalExceptionHandler maps it. Without these lines the framework writes its own 400 without a body.
// MVC reports body errors through model state instead.
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseEnhancedProblemDetails();
app.MapGet("/orders", (OrderStatus? status) => Results.Ok(status)); // ?status=pending_approval, approval or 1

// MVC: the same two calls; [FromQuery], [FromRoute], [FromHeader], [FromForm] and DTO properties bind
builder.Services.AddControllers();
builder.Services.AddEnumConventions();
app.UseEnumBinding();

// OpenAPI: one document per version, v1 keeps numbers for old clients
builder.Services.AddOpenApi("v1", o => o.AddEnumConventions());          // or AddSwaggerGen(o => o.AddEnumConventions())
builder.Services.AddOpenApi("v2", o => o.AddEnumConventions());
app.MapGroup("/api/v1").WithGroupName("v1").WithEnumWireFormat(EnumWireFormat.Number);
app.MapGroup("/api/v2").WithGroupName("v2");

// EF Core only (worker, console): no ASP.NET Core package needed
protected override void ConfigureConventions(ModelConfigurationBuilder builder) =>
    builder.ConfigureEnumConventions(EnumConventions.Default);
```

There is no message bus package. A bus that serializes with System.Text.Json takes the JSON conventions in tolerant `Data` mode, for example MassTransit: `cfg.ConfigureJsonSerializerOptions(o => o.AddEnumConventions(conventions, EnumReadMode.Data, writeAs: EnumWireFormat.Number))`. Upgrade consumers first, then switch producers to strings.

`[Flags]` enums are JSON arrays on the wire (`["read","write"]`), repeated or comma-separated query values, and an integer bitmask in the database. Give every member an explicit value and a `None = 0` member; the analyzers warn otherwise (CSE0005, CSE0006, and CSE0007 for a member that is neither a single bit nor a combination), and `[EnumFallback]` on flags is an error (CSE0008). Use them for a fixed, small set of independent on/off capabilities (permissions, feature toggles, days of week). Use `List<TEnum>` for a user-chosen multi-select, a set that grows often or past 64 members, or when a member needs data attached; use a single enum for one value at a time (statuses, types).

The `v2` document of [the end-to-end sample](examples/Examples.Enums.EndToEnd/README.md) describes the enum like this (the `v1` document has `"type": "integer"` and `"enum": [0, 1, 2, 99]`):

```json
"OrderStatus": {
  "enum": ["pending", "pending_approval", "shipped", "unknown"],
  "type": "string",
  "description": "Order lifecycle state.\n\n| value | number | description |\n|---|---|---|\n| `pending` | 0 | Created, waiting for payment. |\n| `pending_approval` | 1 | Paid, waiting for manual approval. |\n| `shipped` | 2 | Shipped to the customer. |\n| `unknown` | 99 | Response only: a value this API version does not know. |",
  "x-enum-varnames": ["Pending", "PendingApproval", "Shipped", "Unknown"],
  "x-enum-descriptions": ["Created, waiting for payment.", "Paid, waiting for manual approval.", "Shipped to the customer.", "Response only: a value this API version does not know."],
  "x-enum-numeric-values": [0, 1, 2, 99]
}
```

Details: [CSharpEssentials.Enums Readme](CSharpEssentials.Enums/Readme.MD) (generated API, diagnostics, renaming a member, AOT), [enum conventions design](docs/design/CSharpEssentials.Enums-DESIGN.md) (normative matrix, flags guidance, rollout) and [ADR-007](docs/adr/ADR-007-enum-conventions.md).

## Upgrading

- [CHANGELOG.md](CHANGELOG.md) lists the changes in every release.
- Coming from 4.x? Read [Migrating from 4.x to 5.0](docs/migration/v4-to-v5.md). 5.0 applies one set of enum conventions to JSON, binding, OpenAPI and EF Core, and moves the Swashbuckle support into its own package.
- Coming from 3.x? Read [Migrating from 3.x to 4.0](docs/migration/v3-to-v4.md). 4.0 changed ProblemDetails defaults, enum naming and some package dependencies.

## AI Skills

Install skills for Claude Code, Cursor, Codex, and 50+ other AI agents:

```bash
npx skills add senrecep/CSharpEssentials
```

Each package has a dedicated skill with accurate API examples, correct namespaces, and common pitfalls.
