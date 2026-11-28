---
name: csharpessentials-logging
description: Use when logging HTTP request and response bodies in ASP.NET Core with CSharpEssentials.RequestResponseLogging. Covers app.AddRequestResponseLogging(opt => ...) with UseLogger(ILoggerFactory, LoggingOptions), LoggingOptions.CreateAllFields(), LogFields, HeaderKeys, UseHandler(Func<RequestResponseContext, Task>), IgnorePaths, and the [SkipRequestLogging], [SkipResponseLogging] and [SkipRequestResponseLogging] attributes.
---

# CSharpEssentials.RequestResponseLogging

Middleware that writes one log entry per HTTP request: request and response bodies, selected request headers, path, method, query string, timing and body lengths.

## Install

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Namespace

```csharp
using CSharpEssentials.RequestResponseLogging; // AddRequestResponseLogging, LoggingOptions, LogFields, RequestResponseContext, Skip*Attribute
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.RequestResponseLogging
using CSharpEssentials.RequestResponseLogging;
using Microsoft.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
WebApplication app = builder.Build();

app.UseRouting();
app.AddRequestResponseLogging(opt =>
{
    LoggingOptions logging = LoggingOptions.CreateAllFields();
    logging.HeaderKeys.Add(HeaderNames.AcceptLanguage);
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging);
    opt.IgnorePaths("/health", "/metrics");
});

app.MapControllers();
app.MapPost("/login", (LoginRequest request) => Results.Ok())
    .WithMetadata(new SkipRequestLoggingAttribute());
app.MapGet("/health", () => "ok");

app.Run();

public sealed record LoginRequest(string User, string Password);
```

## Registering the middleware

- `AddRequestResponseLogging` is an `IApplicationBuilder` extension. Call it on the built app, not on `builder.Services`.
- Call it after `app.UseRouting()` when the app calls `UseRouting()` explicitly. Otherwise the skip attributes have no effect.
- Without `UseLogger` or `UseHandler` nothing is written.
- `UseLogger` has two overloads: `UseLogger(ILoggerFactory, LoggingOptions)` and `UseLogger(ILoggerFactory, Action<LoggingOptions>)`.
- `UseHandler` and `UseLogger` can be combined; both run.

## Choosing fields

```csharp
app.AddRequestResponseLogging(opt =>
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), logging =>
    {
        logging.LoggingFields = [LogFields.Method, LogFields.Path, LogFields.ResponseTiming, LogFields.Headers];
        logging.HeaderKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "X-Correlation-Id" };
        logging.LoggingLevel = LogLevel.Debug;          // default Information
        logging.LoggerCategoryName = "Http";            // default "RequestResponseLogger"
        logging.UseSeparateContext = true;              // default true: one named placeholder per field
    }));
```

- `LogFields`: `Request`, `Response`, `HostName`, `Path`, `Method`, `QueryString`, `Headers`, `ResponseTiming`, `RequestLength`, `ResponseLength`.
- `new LoggingOptions()` has no fields. Use `LoggingOptions.CreateAllFields()` or set `LoggingFields`.
- `LoggingFields` is read once at registration. Changing it later has no effect.
- `Headers` writes only names in `HeaderKeys`; the default set is empty and case-sensitive.
- `RequestLength` and `ResponseLength` are character counts of the logged text, not bytes.

## Custom handler

```csharp Program.cs
// dotnet add package CSharpEssentials.RequestResponseLogging
using CSharpEssentials.RequestResponseLogging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();
ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Http");

app.UseRouting();
app.AddRequestResponseLogging(opt => opt.UseHandler(context =>
{
    logger.LogInformation("{Url} {Time} req={RequestLength} res={ResponseLength}",
        context.Url, context.ResponseTime, context.RequestLength, context.ResponseLength);
    return Task.CompletedTask;
}));

app.MapGet("/", () => "hello");
app.Run();
```

`RequestResponseContext` members: `Url`, `RequestBody`, `ResponseBody`, `ResponseCreationTime` (`TimeSpan?`), `ResponseTime` (`mm:ss.fff` string), `RequestLength` (`int?`), `ResponseLength` (`int?`). The constructor is internal; you only receive instances. The handler must return a `Task`.

## Skipping bodies

| Attribute | Logged instead of the body |
|---|---|
| `[SkipRequestLogging]` | `Skipped logging request body` |
| `[SkipResponseLogging]` | `Skipped logging response body` |
| `[SkipRequestResponseLogging]` | both texts |

```csharp
[HttpPost("login")]
[SkipRequestLogging]
public IActionResult Login(LoginRequest request) => Ok();

app.MapGet("/export", () => Results.Ok()).WithMetadata(new SkipResponseLoggingAttribute());
```

## Rules

- Do: skip health and metrics endpoints with `IgnorePaths("/health", "/metrics")`. Matching is by prefix and case-insensitive.
- Do: mark endpoints that receive passwords or tokens with `[SkipRequestLogging]`.
- Do: mark endpoints with large or streamed responses with `[SkipResponseLogging]`; the full response copy stays in memory until the request ends.
- Do: set status code and headers before the first write to `Response.Body`, and write asynchronously.
- Don't: call `builder.Services.AddRequestResponseLogging(...)`; it does not exist.
- Don't: add `Authorization` or `Cookie` to `HeaderKeys`.
- Don't: implement `ILogWriter`; it is internal. Use `UseHandler`.
- Don't: expect the status code in the entry; it is not a logged field.

## Behavior

- A thrown exception is logged with `Exception: {type}, Message: {message}` as the response body, passed to the handler, then rethrown.
- A request over 10 MB is not read: `Request body too large: {n} bytes` (or `over 10485760 bytes` for chunked bodies).
- Empty bodies are logged as `Empty request body` and `Empty response body`.

This package reports no diagnostics (CSExxxx).
