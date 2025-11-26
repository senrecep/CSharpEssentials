# CSharpEssentials.RequestResponseLogging

`CSharpEssentials.RequestResponseLogging` provides configurable middleware for logging HTTP requests and responses in ASP.NET Core applications. It supports detailed logging of headers, bodies, timings, and more, with options to filter specific paths or modify behavior per endpoint.

## 🚀 Features

- **Comprehensive Logging**: Log Request/Response bodies, Headers, Path, Method, QueryString, Timing, and Sizes.
- **Configurable**: Choose exactly what fields to log (`LogFields`).
- **Filtering**: Ignore specific paths globally.
- **Attributes**: Use `[SkipRequestLogging]`, `[SkipResponseLogging]`, or `[SkipRequestResponseLogging]` on controllers or actions.
- **Custom Handlers**: Inject custom logic to handle the log context.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## 🛠 Usage

### 1. Registration

Register the middleware in your `Program.cs` or `Startup.cs`.

```csharp
using CSharpEssentials.RequestResponseLogging;

var app = builder.Build();

// ... other middleware ...

app.AddRequestResponseLogging(opt =>
{
    // REQUIRED: Provide a LoggerFactory
    opt.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    
    // Configure what to log
    opt.LoggingOptions = LoggingOptions.CreateAllFields();
    
    // Customize specific options
    opt.LoggingOptions.LoggingLevel = LogLevel.Information;
    opt.LoggingOptions.HeaderKeys = ["User-Agent", "Correlation-ID"]; // Whitelist headers
    
    // Ignore specific paths (e.g., health checks)
    opt.IgnoredPaths = ["/health", "/metrics"];
});

app.Run();
```

### 2. Fine-Grained Control

You can disable logging for specific endpoints using attributes.

```csharp
using CSharpEssentials.RequestResponseLogging.Attributes;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [SkipRequestLogging] // Don't log the request body (e.g. password)
    public IActionResult Login(LoginRequest request)
    {
        // ...
    }
    
    [HttpGet("heavy-data")]
    [SkipResponseLogging] // Don't log the large response body
    public IActionResult GetHeavyData()
    {
        // ...
    }
}
```

### 3. Customizing Log Fields

If you don't want to log everything, specify only the fields you need.

```csharp
opt.LoggingOptions.LoggingFields = 
[
    LogFields.Method,
    LogFields.Path,
    LogFields.ResponseTiming,
    LogFields.Response // Only response body
];
```
