# CSharpEssentials

![CSharpEssentials Banner](icon.png)

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![GitHub Stars](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)
[![License](https://img.shields.io/github/license/senrecep/CSharpEssentials)](LICENSE)

**CSharpEssentials** is a modular .NET ecosystem that bridges Object-Oriented and Functional Programming. It provides type-safe tools for error handling, optional values, business rules, DDD, and web development.

Multi-targets: **.NET 9 / .NET 10 / .NET 11 / netstandard2.1 / netstandard2.0**.

## Ecosystem

| Package | Description |
|---------|-------------|
| **[CSharpEssentials](CSharpEssentials/Readme.MD)** | Meta-package with all core functional modules. |
| **[CSharpEssentials.Core](CSharpEssentials.Core/README.MD)** | String conversions, collection helpers, GUID utilities, task extensions. |
| **[CSharpEssentials.Errors](CSharpEssentials.Errors/Readme.MD)** | Structured errors with codes, types, and metadata. |
| **[CSharpEssentials.Results](CSharpEssentials.Results/Readme.MD)** | `Result` and `Result<T>` for explicit success/failure handling. |
| **[CSharpEssentials.Maybe](CSharpEssentials.Maybe/Readme.MD)** | `Maybe<T>` for null-safe optional values. |
| **[CSharpEssentials.Json](CSharpEssentials.Json/Readme.MD)** | Polymorphic serialization, multi-format dates, conditional string enums. |
| **[CSharpEssentials.Enums](CSharpEssentials.Enums/Readme.MD)** | Source generator for fast enum operations via `[StringEnum]`. |
| **[CSharpEssentials.Any](CSharpEssentials.Any/Readme.MD)** | Discriminated Unions (`Any<T1, T2, ...>`). |
| **[CSharpEssentials.Rules](CSharpEssentials.Rules/Readme.MD)** | Composable business rule engine. |
| **[CSharpEssentials.Entity](CSharpEssentials.Entity/Readme.MD)** | DDD base classes with auditing, soft delete, and domain events. |
| **[CSharpEssentials.EntityFrameworkCore](CSharpEssentials.EntityFrameworkCore/Readme.MD)** | EF Core interceptors, pagination, CQRS helpers. |
| **[CSharpEssentials.AspNetCore](CSharpEssentials.AspNetCore/Readme.MD)** | Exception handling, ProblemDetails, API versioning, Swagger. |
| **[CSharpEssentials.Http](CSharpEssentials.Http/Readme.MD)** | `HttpClient` extensions returning `Result<T>`. |
| **[CSharpEssentials.Time](CSharpEssentials.Time/Readme.MD)** | Testable `IDateTimeProvider` abstraction. |
| **[CSharpEssentials.Clone](CSharpEssentials.Clone/Readme.MD)** | Type-safe generic cloning interface. |
| **[CSharpEssentials.Mediator](CSharpEssentials.Mediator/Readme.MD)** | Source-generated CQRS with pipeline behaviors. |
| **[CSharpEssentials.GcpSecretManager](CSharpEssentials.GcpSecretManager/Readme.MD)** | Google Cloud Secret Manager configuration provider. |
| **[CSharpEssentials.RequestResponseLogging](CSharpEssentials.RequestResponseLogging/Readme.MD)** | Configurable HTTP request/response logging middleware. |

## Installation

```bash
dotnet add package CSharpEssentials
```

Or install individual packages as needed.

## Quick Start

### Result Pattern

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public Result<int> Parse(string input)
{
    if (int.TryParse(input, out int n))
        return n;

    return Error.Validation("Parse.Failed", "Not a number.");
}

Result<int> result = Parse("42");

string message = result.Match(
    onSuccess: value => $"Parsed: {value}",
    onError: errors => $"Error: {errors[0].Description}");
```

### Maybe Pattern

```csharp
using CSharpEssentials.Maybe;

public Maybe<User> FindUser(Guid id)
{
    User? user = _repo.Find(id);
    return user; // implicit from T? to Maybe<T>
}

string city = FindUser(id)
    .Map(u => u.Address)
    .Map(a => a.City)
    .GetValueOrDefault("Unknown");
```

### Maybe + Result

```csharp
Maybe<User> maybe = FindUser(id);
Result<User> result = maybe.ToResult(Error.NotFound("User.Missing", "User not found."));
```

### Rules Engine

```csharp
using CSharpEssentials.Rules;

IRule<Context> ageRule = (ctx => ctx.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be 18.")).ToRule();

Result result = RuleEngine.And(new[] { ageRule, licenseRule }, context);
```

### HTTP with Result

```csharp
using CSharpEssentials.Http;

HttpClient client = new() { BaseAddress = new Uri("https://api.example.com") };

Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));
```

## License

MIT — see [LICENSE](LICENSE).
