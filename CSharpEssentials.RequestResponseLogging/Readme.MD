# CSharpEssentials.RequestResponseLogging

Configurable middleware for logging HTTP requests and responses in ASP.NET Core.

## Features

- **Comprehensive Logging** — Request/Response bodies, headers, path, method, timing, sizes.
- **Configurable Fields** — Choose exactly what to log via `LoggingOptions`.
- **Path Filtering** — Ignore specific paths (e.g., health checks).
- **Attributes** — `[SkipRequestLogging]`, `[SkipResponseLogging]`, `[SkipRequestResponseLogging]` for fine-grained control.

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Usage

### Registration

```csharp
using CSharpEssentials.RequestResponseLogging;

var app = builder.Build();

app.AddRequestResponseLogging(opt =>
{
    opt.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    opt.LoggingOptions = LoggingOptions.CreateAllFields();
    opt.IgnoredPaths = ["/health", "/metrics"];
});
```

### Per-Endpoint Control

```csharp
using CSharpEssentials.RequestResponseLogging.Attributes;

[HttpPost("login")]
[SkipRequestLogging] // Don't log passwords
public IActionResult Login(LoginRequest request) { /* ... */ }

[HttpGet("large-data")]
[SkipResponseLogging] // Don't log large bodies
public IActionResult GetData() { /* ... */ }
```

### Custom Fields

```csharp
opt.LoggingOptions.LoggingFields =
[
    LogFields.Method,
    LogFields.Path,
    LogFields.ResponseTiming
];
```
