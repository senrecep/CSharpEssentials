---
name: csharpessentials-meta
description: Use when deciding which CSharpEssentials package to use — overview of all 18 packages organized by concern, the meta-package that bundles core functional modules, and a quick-reference table mapping problems to packages.
---

# CSharpEssentials — Package Index

CSharpEssentials is a modular .NET NuGet ecosystem. Each package is independent — take only what you need.

## Meta-Package (core functional modules)

```bash
dotnet add package CSharpEssentials
# Includes: Results, Errors, Maybe, Any, Core, Enums
```

## All Packages

### Functional Core

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Results` | `dotnet add package CSharpEssentials.Results` | `csharpessentials-results` |
| `CSharpEssentials.Errors` | `dotnet add package CSharpEssentials.Errors` | `csharpessentials-errors` |
| `CSharpEssentials.Maybe` | `dotnet add package CSharpEssentials.Maybe` | `csharpessentials-maybe` |
| `CSharpEssentials.Any` | `dotnet add package CSharpEssentials.Any` | `csharpessentials-any` |
| `CSharpEssentials.Core` | `dotnet add package CSharpEssentials.Core` | `csharpessentials-core` |
| `CSharpEssentials.Enums` | `dotnet add package CSharpEssentials.Enums` | `csharpessentials-enums` |

### Business Rules

| Package | Install | Skill |
|---------|---------|-------|
| `CSharpEssentials.Rules` | `dotnet add package CSharpEssentials.Rules` | `csharpessentials-rules` |

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
| `CSharpEssentials.Http` | `dotnet add package CSharpEssentials.Http` | `csharpessentials-http` |
| `CSharpEssentials.Json` | `dotnet add package CSharpEssentials.Json` | `csharpessentials-json` |
| `CSharpEssentials.RequestResponseLogging` | `dotnet add package CSharpEssentials.RequestResponseLogging` | `csharpessentials-logging` |
| `CSharpEssentials.GcpSecretManager` | `dotnet add package CSharpEssentials.GcpSecretManager` | `csharpessentials-gcpsecretmanager` |

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
| Compose business validation rules | `CSharpEssentials.Rules` |
| CQRS pipeline behaviors (validate, log, cache, transact) | `CSharpEssentials.Mediator` |
| DDD aggregate base class + domain events | `CSharpEssentials.Entity` |
| EF Core audit, slow queries, pagination | `CSharpEssentials.EntityFrameworkCore` |
| Map errors to HTTP ProblemDetails | `CSharpEssentials.AspNetCore` |
| HttpClient that returns Result<T> | `CSharpEssentials.Http` |
| JSON serialization with string enums + polymorphism | `CSharpEssentials.Json` |
| Log request/response bodies | `CSharpEssentials.RequestResponseLogging` |
| Load secrets from GCP Secret Manager | `CSharpEssentials.GcpSecretManager` |
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
using CSharpEssentials.Core;                // string/GUID/collection helpers
using CSharpEssentials.Enums;              // [StringEnum]
using CSharpEssentials.Rules;              // IRule<T>, RuleEngine
using CSharpEssentials.Mediator;           // ICacheable, ILoggableRequest, ITransactionalRequest
using CSharpEssentials.Entity;             // EntityBase, SoftDeletableEntityBase
using CSharpEssentials.Entity.Interfaces;  // IDomainEvent
using CSharpEssentials.EntityFrameworkCore; // interceptors, pagination
using CSharpEssentials.AspNetCore;         // GlobalExceptionHandler, ResultEndpointFilter
using CSharpEssentials.Http;               // HttpClientResultExtensions, HttpRequestBuilder
using CSharpEssentials.Json;               // JsonOptions, converters
using CSharpEssentials.RequestResponseLogging; // LoggingOptions, SkipLoggingAttributes
using CSharpEssentials.GcpSecretManager;   // AddGcpSecretManager()
using CSharpEssentials.Time;               // IDateTimeProvider, DateTimeProvider
using CSharpEssentials.Clone;              // ICloneable<T>
```
