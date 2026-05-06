# CSharpEssentials

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)

Modular .NET NuGet ecosystem that bridges OOP and Functional Programming in C#.

## Packages

| Package | Description |
|---------|-------------|
| `CSharpEssentials` | Meta-package: Results + Errors + Maybe + Any + Core + Enums |
| `CSharpEssentials.Results` | `Result<T>` — railway-oriented error handling without exceptions |
| `CSharpEssentials.Errors` | Structured `Error` values with type, code, description, metadata |
| `CSharpEssentials.Maybe` | `Maybe<T>` — explicit optionals, no null reference exceptions |
| `CSharpEssentials.Any` | `Any<T1,T2,...>` — type-safe discriminated unions |
| `CSharpEssentials.Core` | String/GUID/collection utilities |
| `CSharpEssentials.Enums` | `[StringEnum]` source generator — AOT-safe enum↔string |
| `CSharpEssentials.Rules` | Composable rule engine with `.And()/.Or()/.Linear()/.Next()` |
| `CSharpEssentials.Mediator` | CQRS pipeline behaviors: validation, logging, caching, transactions |
| `CSharpEssentials.Entity` | `EntityBase<TId>`, soft deletion, domain events |
| `CSharpEssentials.EntityFrameworkCore` | EF Core interceptors (audit, events, slow queries) + pagination |
| `CSharpEssentials.AspNetCore` | `GlobalExceptionHandler`, `ResultEndpointFilter`, Swagger versioning |
| `CSharpEssentials.Http` | `HttpClient` extensions returning `Result<T>` |
| `CSharpEssentials.Json` | Pre-configured `System.Text.Json` options and converters |
| `CSharpEssentials.RequestResponseLogging` | Request/response body logging middleware |
| `CSharpEssentials.GcpSecretManager` | GCP Secret Manager → `IConfiguration` provider |
| `CSharpEssentials.Time` | Testable `IDateTimeProvider` wrapping `TimeProvider` |
| `CSharpEssentials.Clone` | `ICloneable<T>` — typed deep-copy for entity collections |

## Installation

```bash
# Core functional modules
dotnet add package CSharpEssentials

# Individual packages
dotnet add package CSharpEssentials.Rules
dotnet add package CSharpEssentials.Mediator
```

## AI Skills

Install skills for Claude Code, Cursor, Codex, and 50+ other AI agents:

```bash
npx skills add senrecep/CSharpEssentials
```

Each package has a dedicated skill with accurate API examples, correct namespaces, and common pitfalls.
