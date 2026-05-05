# CSharpEssentials.AspNetCore

Production-ready ASP.NET Core utilities: global exception handling, structured ProblemDetails, API versioning, Swagger setup, and Result endpoint filters.

## Features

- **Global Exception Handler** — Converts `DomainException`, `EnhancedValidationException`, and others into RFC 7807 `ProblemDetails`.
- **Enhanced Problem Details** — Extend `ProblemDetails` with structured error codes and messages.
- **Result Endpoint Filter** — Automatically convert `Result<T>` return values to `200 OK` or `400 BadRequest`.
- **API Versioning** — Pre-configured URL segment and header-based versioning.
- **Swagger** — Easy setup with versioning support, enum schema filters, and custom schema IDs.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

## Usage

### Global Exception Handling

```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
```

### API Versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();

var app = builder.Build();
var usersGroup = app.CreateVersionedGroup("users", version: 1);
usersGroup.MapGet("/{id}", (int id) => Results.Ok(new { Id = id }));
```

### Swagger with Versioning

```csharp
builder.Services.AddSwagger<ConfigureSwaggerOptions>(securityScheme: SecuritySchemes.Bearer);

var app = builder.Build();
app.UseVersionableSwagger();
```

### Result Endpoint Filter

```csharp
builder.Services.AddScoped<ResultEndpointFilter>();

app.MapGet("/users/{id}", (int id) => GetUser(id))
   .AddEndpointFilter<ResultEndpointFilter>();

// Returns 200 with value on success, 400 with errors on failure
```

### Structured Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": [
    { "code": "User.InvalidEmail", "description": "Email format is incorrect.", "type": "Validation" }
  ]
}
```
