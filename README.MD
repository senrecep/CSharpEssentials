# CSharpEssentials

![CSharpEssentials Banner](icon.png)

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials)
[![GitHub Stars](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)
[![License](https://img.shields.io/github/license/senrecep/CSharpEssentials)](LICENSE)

**CSharpEssentials** is a comprehensive ecosystem of .NET libraries designed to modernize C# development. It bridges the gap between Object-Oriented and Functional Programming, providing robust tools for error handling, domain modeling, and enterprise application development.

> **Goal:** Make C# code more expressive, safer, and easier to test without sacrificing performance.

## 📦 Ecosystem Overview

CSharpEssentials is modular. You can use the meta-package to get everything, or install specific modules to keep dependencies light.

| Package | Description | NuGet |
|---------|-------------|-------|
| **[CSharpEssentials](CSharpEssentials/Readme.MD)** | The meta-package including all core functional modules and exclusive bridge extensions. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.svg)](https://www.nuget.org/packages/CSharpEssentials) |
| **[CSharpEssentials.Core](CSharpEssentials.Core/README.MD)** | Foundational extensions, guard clauses, and utilities (Strings, Collections, GUIDs). | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Core.svg)](https://www.nuget.org/packages/CSharpEssentials.Core) |
| **[CSharpEssentials.Results](CSharpEssentials.Results/Readme.MD)** | Railway-oriented programming with `Result` and `Result<T>` patterns. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Results.svg)](https://www.nuget.org/packages/CSharpEssentials.Results) |
| **[CSharpEssentials.Maybe](CSharpEssentials.Maybe/Readme.MD)** | `Maybe<T>` monad for null-safe optional value handling. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Maybe.svg)](https://www.nuget.org/packages/CSharpEssentials.Maybe) |
| **[CSharpEssentials.Errors](CSharpEssentials.Errors/Readme.MD)** | Structured error handling with unique codes and standard types (NotFound, Validation, etc.). | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Errors.svg)](https://www.nuget.org/packages/CSharpEssentials.Errors) |
| **[CSharpEssentials.Rules](CSharpEssentials.Rules/Readme.MD)** | Composable business rule engine (And, Or, Linear, Conditional logic). | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Rules.svg)](https://www.nuget.org/packages/CSharpEssentials.Rules) |
| **[CSharpEssentials.Any](CSharpEssentials.Any/Readme.MD)** | Discriminated Unions (`Any<T1, T2, ...>`) for type-safe polymorphic returns. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Any.svg)](https://www.nuget.org/packages/CSharpEssentials.Any) |
| **[CSharpEssentials.Entity](CSharpEssentials.Entity/Readme.MD)** | Domain-Driven Design base classes (`EntityBase`, `SoftDeletableEntityBase`). | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Entity.svg)](https://www.nuget.org/packages/CSharpEssentials.Entity) |
| **[CSharpEssentials.EntityFrameworkCore](CSharpEssentials.EntityFrameworkCore/Readme.MD)** | EF Core extensions, Interceptors (Slow Query), and Pagination support. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.EntityFrameworkCore.svg)](https://www.nuget.org/packages/CSharpEssentials.EntityFrameworkCore) |
| **[CSharpEssentials.AspNetCore](CSharpEssentials.AspNetCore/Readme.MD)** | Web API utilities: Global Exception Handler, Problem Details, API Versioning. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) |
| **[CSharpEssentials.Json](CSharpEssentials.Json/Readme.MD)** | Advanced JSON serialization: Polymorphism, String Enums, Multi-format Dates. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Json.svg)](https://www.nuget.org/packages/CSharpEssentials.Json) |
| **[CSharpEssentials.Time](CSharpEssentials.Time/Readme.MD)** | Testable `IDateTimeProvider` abstraction and extensions. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Time.svg)](https://www.nuget.org/packages/CSharpEssentials.Time) |
| **[CSharpEssentials.RequestResponseLogging](CSharpEssentials.RequestResponseLogging/Readme.MD)** | Highly configurable HTTP traffic logging middleware. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.RequestResponseLogging.svg)](https://www.nuget.org/packages/CSharpEssentials.RequestResponseLogging) |
| **[CSharpEssentials.GcpSecretManager](CSharpEssentials.GcpSecretManager/Readme.MD)** | Google Cloud Secret Manager integration for IConfiguration. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.GcpSecretManager.svg)](https://www.nuget.org/packages/CSharpEssentials.GcpSecretManager) |
| **[CSharpEssentials.Clone](CSharpEssentials.Clone/Readme.MD)** | Type-safe deep cloning interfaces and extensions. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Clone.svg)](https://www.nuget.org/packages/CSharpEssentials.Clone) |
| **[CSharpEssentials.Enums](CSharpEssentials.Enums/Readme.MD)** | Attributes for controlling Enum serialization behavior. | [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Enums.svg)](https://www.nuget.org/packages/CSharpEssentials.Enums) |

## 🚀 Getting Started

Install the main package to get the core functional tools:

```bash
dotnet add package CSharpEssentials
```

Or pick individual packages as needed (e.g., `dotnet add package CSharpEssentials.Results`).

## 💡 Feature Highlights

### 1. Functional Error Handling (Results & Errors)
Stop throwing exceptions for flow control. Use `Result<T>` to express success or failure explicitly.

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Errors;

public Result<User> RegisterUser(string email)
{
    if (string.IsNullOrEmpty(email))
        return Error.Validation("User.EmailRequired", "Email cannot be empty.");

    if (_repo.Exists(email))
        return Error.Conflict("User.Exists", "User already exists.");

    var user = new User(email);
    return user; // Implicit conversion to Result<User>
}
```

### 2. Safe Optionals (Maybe)
Eliminate `NullReferenceException` by explicitly modeling the absence of a value.

```csharp
using CSharpEssentials.Maybe;

public Maybe<User> GetUserById(Guid id)
{
    var user = _repo.Find(id);
    return user; // Implicit conversion from User?
}

// Chaining with LINQ
var zipCode = GetUserById(id)
    .Map(u => u.Address)
    .Map(a => a.ZipCode)
    .GetValueOrDefault("00000");
```

### 3. Business Rules Engine
Compose complex logic from simple, reusable rules.

```csharp
var rule = Rule.And(
    new MinimumAgeRule(18),
    new KycCompletedRule()
);

var result = RuleEngine.Evaluate(rule, userContext);
```

### 4. Robust Web APIs (AspNetCore)
Automatically convert `Result` failures into standard RFC 7807 Problem Details responses using the Global Exception Handler.

```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEnhancedProblemDetails();

// Controller
[HttpPost]
public IActionResult Create(CreateUserRequest request)
{
    Result<User> result = _service.Create(request);
    
    // Returns 200 OK with data, or 400/404/409 ProblemDetails based on Error Type
    return result.ToActionResult(); 
}
```

### 5. Domain Modeling (Entity)
Build rich domain models with built-in Audit logs and Domain Events.

```csharp
public class Order : EntityBase<Guid>
{
    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
        Raise(new OrderPaidEvent(Id)); // Domain Event
        SetUpdatedInfo(DateTimeOffset.UtcNow, "System"); // Auditing
    }
}
```

## 🛠️ Development & Publishing

This repository includes a smart build script (`build-and-publish-nugets.sh`) that manages dependency orders and versioning for all packages.

```bash
# Build and Test all packages
./build-and-publish-nugets.sh

# Publish to NuGet (requires API Key)
./build-and-publish-nugets.sh --parallel YOUR_API_KEY
```

## 🤝 Contributing

Contributions are welcome! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) guide.

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Inspired by excellent community projects like [ErrorOr](https://github.com/amantinband/error-or), [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions), and [OneOf](https://github.com/mcintyre321/OneOf).
