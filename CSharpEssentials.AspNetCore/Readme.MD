# CSharpEssentials.AspNetCore

`CSharpEssentials.AspNetCore` provides essential utilities and middleware for building production-ready ASP.NET Core Web APIs. It integrates seamlessly with the **CSharpEssentials.Errors** ecosystem to provide structured error responses and simplifies common configurations like Swagger and API Versioning.

## 🚀 Features

- **Global Exception Handler**: Middleware (`IExceptionHandler`) that catches exceptions and converts them into standard RFC 7807 `ProblemDetails` responses.
- **Enhanced Problem Details**: Extends `ProblemDetails` to include structured error codes and messages from `CSharpEssentials.Errors`.
- **Swagger Configuration**: Easy setup for Swagger with support for API Versioning and custom schema filters.
- **API Versioning**: Pre-configured extensions for setting up API versioning (URL segment + Header).

## 📦 Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

## 🛠 Usage

### 1. Global Exception Handling

Register the global exception handler to automatically convert `DomainException`, `ValidationException`, and others into structured JSON error responses.

```csharp
using CSharpEssentials.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add problem details support
builder.Services.AddProblemDetails();

// Register the handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

Then, in your middleware pipeline:

```csharp
var app = builder.Build();

app.UseExceptionHandler(); // Uses the registered GlobalExceptionHandler
```

### 2. API Versioning

Enable standard API versioning (URL Segment + Header support) with a single line.

```csharp
using CSharpEssentials.AspNetCore;

builder.Services.AddAndConfigureApiVersioning();
```

### 3. Swagger with Versioning

Simplify Swagger configuration, especially when using API versioning.

```csharp
using CSharpEssentials.AspNetCore;

// Add Swagger services
builder.Services.AddSwagger<ConfigureSwaggerOptions>(
    securityScheme: SecuritySchemes.Bearer // Or your custom scheme
);

// Use Swagger Middleware
app.UseVersionableSwagger();
```

### 4. Structured Error Responses

When an exception occurs (e.g., `DomainException` from `CSharpEssentials.Errors`), the response will look like this:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": [
    {
      "code": "User.InvalidEmail",
      "description": "The email format is incorrect.",
      "type": "Validation"
    }
  ],
  "errorCodes": [ "User.InvalidEmail" ]
}
```
