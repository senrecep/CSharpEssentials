---
name: csharpessentials-logging
description: Use when adding request/response body logging middleware to ASP.NET Core — AddRequestResponseLogging() with configurable body/header capture, UseRequestResponseLogging() pipeline registration, and [SkipRequestResponseLogging] / [SkipRequestLogging] / [SkipResponseLogging] attributes for per-endpoint opt-out.
---

# CSharpEssentials.RequestResponseLogging

Middleware that logs HTTP request and response bodies. Configurable per-endpoint via attributes.

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Namespace

```csharp
using CSharpEssentials.RequestResponseLogging;
```

---

## Register and Use

```csharp
// Program.cs
builder.Services.AddRequestResponseLogging(options =>
{
    options.Request.LogBody    = true;
    options.Request.LogHeaders = false;
    options.Response.LogBody   = true;
    options.IgnorePaths        = ["/health", "/metrics", "/favicon.ico"];
});

app.UseRequestResponseLogging();
```

---

## Per-Endpoint Opt-Out

```csharp
[SkipRequestResponseLogging]   // skip both request and response
[SkipRequestLogging]           // skip request body only
[SkipResponseLogging]          // skip response body only
public IActionResult MyAction() { ... }
```

Apply to individual controller actions, Minimal API handlers, or entire controllers.

---

## Best Practices

- Set `LogBody = false` for endpoints handling auth, passwords, or PII
- Always add health check and metrics paths to `IgnorePaths` — these are high-frequency and low-value
- Apply `[SkipRequestResponseLogging]` rather than `[SkipRequestLogging]` + `[SkipResponseLogging]` when skipping both
- Register `UseRequestResponseLogging()` early in the pipeline, before `UseRouting()`
