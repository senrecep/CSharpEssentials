---
name: csharpessentials-meta
description: Use when deciding which CSharpEssentials package to use. Gives an overview of all 23 packages organized by concern, what the CSharpEssentials meta-package bundles, and a quick-reference table mapping problems to packages.
---

# CSharpEssentials: Package Index

CSharpEssentials is a modular .NET NuGet ecosystem of 23 packages (22 focused packages plus the `CSharpEssentials` meta-package). Each package is independent; take only what you need.

## Meta-Package

```bash
dotnet add package CSharpEssentials
# Includes: Any, Clone, Core, Entity, Enums, Errors, Http, Json, Maybe, Results, Rules, These, Time
```

## All Packages

### Functional Core

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Results` | `dotnet add package CSharpEssentials.Results` | `csharpessentials-results` |
| `CSharpEssentials.Errors` | `dotnet add package CSharpEssentials.Errors` | `csharpessentials-errors` |
| `CSharpEssentials.Maybe` | `dotnet add package CSharpEssentials.Maybe` | `csharpessentials-maybe` |
| `CSharpEssentials.Any` | `dotnet add package CSharpEssentials.Any` | `csharpessentials-any` |
| `CSharpEssentials.These` | `dotnet add package CSharpEssentials.These` | `csharpessentials-these` |
| `CSharpEssentials.Core` | `dotnet add package CSharpEssentials.Core` | `csharpessentials-core` |
| `CSharpEssentials.Enums` | `dotnet add package CSharpEssentials.Enums` | `csharpessentials-enums` |

### Business Rules & Validation

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Rules` | `dotnet add package CSharpEssentials.Rules` | `csharpessentials-rules` |
| `CSharpEssentials.Validation` | `dotnet add package CSharpEssentials.Validation` | `csharpessentials-validation` |

### CQRS / Mediator

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Mediator` | `dotnet add package CSharpEssentials.Mediator` | `csharpessentials-mediator` |

### Domain Model / EF Core

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Entity` | `dotnet add package CSharpEssentials.Entity` | `csharpessentials-entity` |
| `CSharpEssentials.EntityFrameworkCore` | `dotnet add package CSharpEssentials.EntityFrameworkCore` | `csharpessentials-efcore` |

### Web / Infrastructure

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.AspNetCore` | `dotnet add package CSharpEssentials.AspNetCore` | `csharpessentials-aspnetcore` |
| `CSharpEssentials.Endpoints` | `dotnet add package CSharpEssentials.Endpoints` | `csharpessentials-endpoints` |
| `CSharpEssentials.DependencyInjection` | `dotnet add package CSharpEssentials.DependencyInjection` | `csharpessentials-dependencyinjection` |
| `CSharpEssentials.Http` | `dotnet add package CSharpEssentials.Http` | `csharpessentials-http` |
| `CSharpEssentials.Json` | `dotnet add package CSharpEssentials.Json` | `csharpessentials-json` |
| `CSharpEssentials.RequestResponseLogging` | `dotnet add package CSharpEssentials.RequestResponseLogging` | `csharpessentials-logging` |
| `CSharpEssentials.GcpSecretManager` | `dotnet add package CSharpEssentials.GcpSecretManager` | `csharpessentials-gcpsecretmanager` |

### Resilience

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Resilience` | `dotnet add package CSharpEssentials.Resilience` | `csharpessentials-resilience` |

### Utilities

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Time` | `dotnet add package CSharpEssentials.Time` | `csharpessentials-time` |
| `CSharpEssentials.Clone` | `dotnet add package CSharpEssentials.Clone` | `csharpessentials-clone` |

---

## Problem → Package Quick Reference

| Problem | Package |
|---------|---------|
| Return errors without exceptions | `CSharpEssentials.Results` + `CSharpEssentials.Errors` |
| Represent optional values (no null) | `CSharpEssentials.Maybe` |
| Return one of several distinct types | `CSharpEssentials.Any` |
| Partial success: errors and a value together | `CSharpEssentials.These` |
| Compose business validation rules | `CSharpEssentials.Rules` |
| Model-first validation returning `Result<T>` | `CSharpEssentials.Validation` |
| CQRS pipeline behaviors (validate, log, cache, transact) | `CSharpEssentials.Mediator` |
| DDD aggregate base class + domain events | `CSharpEssentials.Entity` |
| EF Core audit, slow queries, pagination | `CSharpEssentials.EntityFrameworkCore` |
| Map errors to HTTP ProblemDetails (privacy-first defaults in 4.0, `UseLegacyDefaults()` for 3.x output) | `CSharpEssentials.AspNetCore` |
| Organize Minimal API endpoints in classes (source-generated, AOT-safe) | `CSharpEssentials.Endpoints` |
| Register and decorate services with attributes (source-generated) | `CSharpEssentials.DependencyInjection` |
| HttpClient that returns Result<T> | `CSharpEssentials.Http` |
| JSON serialization with string enums + polymorphism (`StringEnumNaming` is the shared enum naming for JSON, EF Core, Swagger and binding) | `CSharpEssentials.Json` |
| Log request/response bodies | `CSharpEssentials.RequestResponseLogging` |
| Load secrets from GCP Secret Manager | `CSharpEssentials.GcpSecretManager` |
| Transient fault handling (retry, timeout, circuit breaker, fallback) | `CSharpEssentials.Resilience` |
| Testable time / freeze clock in tests | `CSharpEssentials.Time` |
| Deep-copy entity collections | `CSharpEssentials.Clone` |
| Fast enum-to-string (NativeAOT-safe) | `CSharpEssentials.Enums` |
| String case conversions, GUID utilities | `CSharpEssentials.Core` |

---

## Namespace Reference

```csharp
using CSharpEssentials.ResultPattern;       // Result, Result<T>
using CSharpEssentials.Errors;              // Error, ErrorType, ErrorMetadata
using CSharpEssentials.Maybe;               // Maybe<T>
using CSharpEssentials.Any;                 // Any<T1,T2,...>
using CSharpEssentials.These;               // These<TError,TValue>
using CSharpEssentials.Core;                // string/GUID/collection helpers
using CSharpEssentials.Enums;              // [StringEnum]
using CSharpEssentials.Rules;              // IRule<T>, RuleEngine
using CSharpEssentials.Validation;         // Validator<T>, RuleContext<T>, IValidator<T>
using CSharpEssentials.Mediator;           // ICacheable, ILoggableRequest, ITransactionalRequest
using CSharpEssentials.Entity;             // EntityBase, SoftDeletableEntityBase
using CSharpEssentials.Entity.Interfaces;  // IDomainEvent
using CSharpEssentials.EntityFrameworkCore; // interceptors, pagination
using CSharpEssentials.AspNetCore;         // GlobalExceptionHandler, ResultEndpointFilter, AddEnhancedProblemDetails
using CSharpEssentials.Endpoints;          // IEndpoint, IEndpointGroup, [EndpointGroup<T>]
using CSharpEssentials.DependencyInjection; // [RegisterScoped], [Decorates], RegistrationStrategy
using CSharpEssentials.Http;               // HttpClientResultExtensions, HttpRequestBuilder
using CSharpEssentials.Resilience;          // ResiliencePolicy, ResiliencePolicy<T>
using CSharpEssentials.Json;               // EnhancedJsonSerializerOptions, converters
using CSharpEssentials.RequestResponseLogging; // LoggingOptions, [SkipRequestLogging], [SkipResponseLogging]
using CSharpEssentials.GcpSecretManager;   // AddGcpSecretManager()
using CSharpEssentials.Time;               // IDateTimeProvider, DateTimeProvider
using CSharpEssentials.Clone;              // ICloneable<T>
```
