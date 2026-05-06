---
name: csharpessentials-gcpsecretmanager
description: Use when loading secrets from Google Cloud Secret Manager into IConfiguration at startup — AddGcpSecretManager() registers a configuration provider that pulls named secrets so they are available as standard config values throughout the application.
---

# CSharpEssentials.GcpSecretManager

Configuration provider that loads secrets from Google Cloud Secret Manager into the standard `IConfiguration` system. Secrets become available like any other config value after startup.

## Installation

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Namespace

```csharp
using CSharpEssentials.GcpSecretManager;
```

---

## Register

```csharp
// Program.cs — add before building the app
builder.Configuration.AddGcpSecretManager(options =>
{
    options.ProjectId = "my-gcp-project";
    options.Secrets   = ["db-connection-string", "stripe-api-key", "jwt-secret"];
});

// Secrets available anywhere via IConfiguration
var connStr = config["db-connection-string"];
var apiKey  = config["stripe-api-key"];

// Or via IOptions<T> / strongly-typed binding
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));
```

---

## Authentication

Uses Application Default Credentials (ADC). In GCP environments (Cloud Run, GKE, Compute Engine), the service account is used automatically. For local development:

```bash
gcloud auth application-default login
```

---

## Best Practices

- Add `AddGcpSecretManager()` after `AddJsonFile()` calls so GCP secrets override local config
- Use secret names that match `IConfiguration` key conventions (`db-connection-string` → `config["db-connection-string"]`)
- In production, grant the service account the `Secret Manager Secret Accessor` IAM role only — not `Viewer`
- Do not list secrets in `appsettings.json` — the point is to keep them out of source control
