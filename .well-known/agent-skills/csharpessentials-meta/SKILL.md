---
name: csharpessentials-meta
description: Use when choosing which CSharpEssentials package solves a problem, or when a project references the CSharpEssentials meta-package. Maps all 32 packages to the problems they solve and their skills, lists what the meta-package bundles, and covers its own helpers JsonElement.TryGetProperty, JsonDocument.TryGetNestedProperty and long?.MsToDateTime.
---

# CSharpEssentials: package index

CSharpEssentials is 31 focused NuGet packages plus the `CSharpEssentials` meta-package. Reference only what you need; each package has its own skill.

## Meta-package

```bash
dotnet add package CSharpEssentials
# bundles: Any, Clone, Core, Entity, Enums, Errors, Http, Json, Maybe, Results, Rules, These, Time
```

```csharp Program.cs
// dotnet add package CSharpEssentials
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

var stock = new Dictionary<string, int> { ["pen"] = 3 };

Result<int> Reserve(string sku, int quantity)
{
    Maybe<int> available = stock.TryGetValue(sku, out int count) ? count : Maybe<int>.None;
    if (available.HasNoValue)
        return Error.NotFound("Stock.UnknownSku", $"Unknown SKU {sku}.");
    if (available.Value < quantity)
        return Error.Validation("Stock.Insufficient", $"Only {available.Value} left.");
    return available.Value - quantity;
}

foreach (var (sku, quantity) in new[] { ("pen", 2), ("pen", 5), ("ink", 1) })
    Reserve(sku, quantity).Switch(
        onSuccess: left => Console.WriteLine($"{sku}: reserved, {left} left"),
        onError: errors => Console.WriteLine($"{sku}: {errors[0].Code}"));
// pen: reserved, 1 left
// pen: Stock.Insufficient
// ink: Stock.UnknownSku
```

The meta-package assembly adds three helpers (they are not in any single package):

```csharp
using System.Text.Json;
using CSharpEssentials.Json;            // JsonExtensions
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;            // TimeExtensions

using JsonDocument doc = JsonDocument.Parse("""{"user":{"address":{"city":"Izmir"}},"userName":"ada1"}""");
long? nullableUnixMs = 1_700_000_000_000;

Result<JsonElement> el = doc.RootElement.TryGetProperty("user_name", "userName"); // first name that exists, same level
Result<JsonElement?> nested = doc.TryGetNestedProperty("user", "address", "city"); // path
Maybe<DateTime> when = nullableUnixMs.MsToDateTime();                             // long? -> Maybe<DateTime>, Kind = Utc
```

Failures are `Result` errors with codes `NoPropertyNames`, `PropertyNotFound` or `DocumentIsNull`; nothing throws.

## Which package

| Problem | Package | Skill |
|---|---|---|
| Return success or errors without exceptions | `CSharpEssentials.Results` + `CSharpEssentials.Errors` | `csharpessentials-results`, `csharpessentials-errors` |
| Optional value instead of `null` | `CSharpEssentials.Maybe` | `csharpessentials-maybe` |
| One of several types | `CSharpEssentials.Any` | `csharpessentials-any` |
| A value and errors together (partial success) | `CSharpEssentials.These` | `csharpessentials-these` |
| Assertions on `Result`, `Maybe` and problem details in tests | `CSharpEssentials.Results.Testing` | its Readme |
| Composable business rules | `CSharpEssentials.Rules` | `csharpessentials-rules` |
| Model validation that returns `Result<T>` | `CSharpEssentials.Validation` | `csharpessentials-validation` |
| Mediator with validation, logging, caching, transaction, idempotency and lock behaviors | `CSharpEssentials.Mediator` | `csharpessentials-mediator` |
| Contract tests for a custom `IResourceLock`, recording fake lock | `CSharpEssentials.Mediator.Testing` | its Readme |
| Entity base classes, soft delete, domain events | `CSharpEssentials.Entity` | `csharpessentials-entity` |
| EF Core: audit, soft delete, pagination, error translation, outbox, enum storage | `CSharpEssentials.EntityFrameworkCore` | `csharpessentials-efcore` |
| Health check that fails on pending migrations | `CSharpEssentials.EntityFrameworkCore.HealthChecks` | `csharpessentials-efcore` |
| Hosted service that drains the outbox | `CSharpEssentials.EntityFrameworkCore.Hosting` | `csharpessentials-efcore` |
| Database-backed lock for Mediator requests across instances | `CSharpEssentials.EntityFrameworkCore.Locking` | `csharpessentials-efcore` |
| Errors to ProblemDetails, enum binding, Idempotency-Key, ETag and If-Match | `CSharpEssentials.AspNetCore` | `csharpessentials-aspnetcore` |
| OpenAPI document with enum schemas (one of the two per host) | `CSharpEssentials.AspNetCore.OpenApi` (net10) or `CSharpEssentials.AspNetCore.Swashbuckle` | `csharpessentials-aspnetcore` |
| Minimal API endpoints in classes, source-generated | `CSharpEssentials.Endpoints` | `csharpessentials-endpoints` |
| Register and decorate services with attributes, source-generated | `CSharpEssentials.DependencyInjection` | `csharpessentials-dependencyinjection` |
| `HttpClient` calls that return `Result<T>` | `CSharpEssentials.Http` | `csharpessentials-http` |
| System.Text.Json presets, string enums, polymorphism across assemblies | `CSharpEssentials.Json` | `csharpessentials-json` |
| One string spelling per enum member everywhere, enum helpers | `CSharpEssentials.Enums` | `csharpessentials-enums` |
| Log request and response bodies | `CSharpEssentials.RequestResponseLogging` | `csharpessentials-logging` |
| Run an operation or message once (`IIdempotencyExecutor`, `IdempotencyInbox`) | `CSharpEssentials.Idempotency` | `csharpessentials-idempotency` |
| Contract tests for a custom `IIdempotencyRecordStore` | `CSharpEssentials.Idempotency.Testing` | its Readme |
| Retry, timeout, circuit breaker, fallback | `CSharpEssentials.Resilience` | `csharpessentials-resilience` |
| Secrets from GCP Secret Manager into configuration | `CSharpEssentials.GcpSecretManager` | `csharpessentials-gcpsecretmanager` |
| Testable clock, time zones, ages | `CSharpEssentials.Time` | `csharpessentials-time` |
| Typed `Clone()` for domain objects and collections | `CSharpEssentials.Clone` | `csharpessentials-clone` |
| Case conversion, version 7 GUIDs, collection and null helpers | `CSharpEssentials.Core` | `csharpessentials-core` |

Install any of them with `dotnet add package <name>`.

## Namespaces

| Namespace | Main types |
|---|---|
| `CSharpEssentials.ResultPattern` | `Result`, `Result<T>` |
| `CSharpEssentials.Errors` | `Error`, `ErrorType`, `ErrorMetadata` |
| `CSharpEssentials.Maybe` | `Maybe<T>` |
| `CSharpEssentials.Any` | `Any<T1, T2, ...>` |
| `CSharpEssentials.These` | `These<TError, TValue>` |
| `CSharpEssentials.Core` | case conversion, `Guider`, collection helpers, `HttpCodes` |
| `CSharpEssentials.Transactions` | `ITransactionRunner`, `ITransactionCallbacks` |
| `CSharpEssentials.Enums` | `[StringEnum]`, `EnumConventions`, `EnumMetadata` |
| `CSharpEssentials.Json` | `EnhancedJsonSerializerOptions`, `[JsonTypeDiscriminator]` |
| `CSharpEssentials.Rules` | rules and the rule engine |
| `CSharpEssentials.Validation` | `Validator<T>`, `IValidator<T>` |
| `CSharpEssentials.Mediator` | pipeline behaviors and marker interfaces |
| `CSharpEssentials.Entity`, `CSharpEssentials.Entity.Interfaces` | entity base classes, `IDomainEvent` |
| `CSharpEssentials.EntityFrameworkCore` (and `.Outbox`, `.Locking`, `.HealthChecks`, `.Hosting`) | EF Core integration |
| `CSharpEssentials.AspNetCore` | problem details, filters, enum binding |
| `CSharpEssentials.Endpoints` | `IEndpoint`, `IEndpointGroup` |
| `CSharpEssentials.DependencyInjection` | registration attributes |
| `CSharpEssentials.Http` | `HttpClient` result extensions, `HttpRequestBuilder` |
| `CSharpEssentials.Resilience` | `ResiliencePolicy`, `ResiliencePolicy<T>` |
| `CSharpEssentials.RequestResponseLogging` | logging middleware options |
| `CSharpEssentials.Idempotency` (and `.Testing`) | `IIdempotencyExecutor`, `IdempotencyKeys` |
| `CSharpEssentials.GcpSecretManager` | Secret Manager configuration |
| `CSharpEssentials.Time` | `IDateTimeProvider`, `DateTimeProvider`, `TimeZones` |
| `CSharpEssentials.Clone` | `ICloneable<T>` |
| `CSharpEssentials.ResultPattern.Testing` | `Result` assertions (Results.Testing) |
| `CSharpEssentials.Locking.Testing` | lock contract tests (Mediator.Testing) |

## Rules

- Prefer the focused package over the meta-package in libraries; the meta-package suits apps.
- `CSharpEssentials.AspNetCore.OpenApi` and `CSharpEssentials.AspNetCore.Swashbuckle` must not both be in one host.
- Packages that depend on Enums (Json, AspNetCore, EntityFrameworkCore, Http, the meta-package) bring the enum generator; no separate `CSharpEssentials.Enums` reference is needed.
- The `Result` types live in namespace `CSharpEssentials.ResultPattern`, not `CSharpEssentials.Results`.
