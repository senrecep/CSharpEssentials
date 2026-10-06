# CSharpEssentials.RequestResponseLogging

Configurable middleware for logging HTTP requests and responses in ASP.NET Core.

## Features

- **Detailed Logging**: Request/Response bodies, headers, path, method, timing, sizes.
- **Configurable Fields**: Choose exactly what to log via `LoggingOptions`.
- **Path Filtering**: Ignore specific paths (e.g., health checks).
- **Attributes**: `[SkipRequestLogging]`, `[SkipResponseLogging]`, `[SkipRequestResponseLogging]` for fine-grained control.

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
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), LoggingOptions.CreateAllFields());
    opt.IgnorePaths("/health", "/metrics");
});
```

Without `UseLogger` (or `UseHandler`), the middleware runs but writes nothing.

### Per-Endpoint Control

```csharp
using CSharpEssentials.RequestResponseLogging;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [SkipRequestLogging] // don't log the request body (passwords)
    public IActionResult Login(LoginRequest request) => Ok();

    [HttpGet("large-data")]
    [SkipResponseLogging] // don't log the response body
    public IActionResult GetData() => Ok();
}
```

The attributes are read from endpoint metadata, so Minimal APIs use `.WithMetadata(new SkipRequestLoggingAttribute())`. A skipped body is logged as `Skipped logging request body` / `Skipped logging response body`.

### Custom Fields

```csharp
app.AddRequestResponseLogging(opt =>
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging =>
    {
        logging.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming];
        logging.LoggingLevel = LogLevel.Debug;
    }));
```
