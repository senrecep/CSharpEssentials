---
name: csharpessentials-gcpsecretmanager
description: Use when loading Google Cloud Secret Manager secrets into IConfiguration at startup. Covers builder.Configuration.AddGcpSecretManager(o => o.AddProject(new ProjectSecretConfiguration { ProjectId, SecretIds, PrefixFilters, RawSecretIds, Region })) or the "GoogleSecretManager" appsettings section, with `__` to `:` key mapping, JSON secret flattening, LoggerFactory diagnostics and transient gRPC retries.
---

# CSharpEssentials.GcpSecretManager

Configuration provider that loads Google Cloud Secret Manager secrets into `IConfiguration` once, at startup.

## Installation

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Namespace

```csharp
using CSharpEssentials.GcpSecretManager;               // ProjectSecretConfiguration
using CSharpEssentials.GcpSecretManager.Configuration; // SecretManagerConfigurationOptions
using CSharpEssentials.GcpSecretManager.Extensions;    // AddGcpSecretManager
```

## Register in code

```csharp
builder.Configuration.AddGcpSecretManager(options =>
{
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "my-gcp-project",
        PrefixFilters = ["MyApp__"],          // load secrets whose ID starts with a prefix
        SecretIds = ["stripe-api-key"],        // and/or these exact IDs
        RawSecretIds = ["stripe-api-key"],     // never parse these as JSON
        Region = null                          // null = global endpoint
    });
    options.CredentialsPath = null;            // null = Application Default Credentials
    options.BatchSize = 10;                    // secrets loaded in parallel
    options.PageSize = 300;                    // secrets listed per page
});

string connectionString = builder.Configuration["MyApp:ConnectionStrings:Default"]!;
```

- With no `PrefixFilters` and no `SecretIds`, every secret of the project is loaded.
- `AddProject` returns the options, so calls chain. A `null` project throws `ArgumentNullException`; no project at all throws `ArgumentException`.
- `SecretManagerConfigurationOptions` and `ProjectSecretConfiguration` are `sealed record`s. `BatchSize` <= 0 or `PageSize` < 0 throws `ArgumentOutOfRangeException`.
- `AddGcpSecretManager` extends `IConfigurationManager` (`builder.Configuration`), not `IConfigurationBuilder`.

## Register from appsettings.json

Without a delegate, `AddGcpSecretManager()` reads the `GoogleSecretManager` section and throws `InvalidOperationException` if the section, its projects or a `ProjectId` is missing:

```json
{
  "GoogleSecretManager": {
    "Projects": [
      { "ProjectId": "my-project", "PrefixFilters": ["Prod__"] }
    ]
  }
}
```

```csharp
builder.Configuration.AddGcpSecretManager();
```

With a delegate, set `LoadFromAppSettings = true` (and optionally `ConfigurationSectionName`) to combine both.

## Key mapping and JSON secrets

- Every `__` in the secret id becomes `:`, so `MyApp__ConnectionStrings__Default` maps to `MyApp:ConnectionStrings:Default`. Keys are case-insensitive; on a duplicate key the first project or secret wins. Implement `ISecretManagerConfigurationLoader` and set `options.Loader` for another mapping or selection.
- A secret whose value is a JSON object or array is also flattened into child keys (`Db` = `{"Host":"x"}` adds `Db:Host`). Secrets listed in `RawSecretIds` or matching `RawSecretPrefixes` are kept as the raw string only.

## Logging and retries

Configuration providers are built before dependency injection, so pass a logger factory explicitly. Without one, nothing is logged.

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

builder.Configuration.AddGcpSecretManager(options =>
{
    options.LoggerFactory = loggerFactory;
    options.AddProject(new ProjectSecretConfiguration { ProjectId = "my-gcp-project" });
});
```

Listing and secret access are retried through `CSharpEssentials.Resilience` on `ResourceExhausted` and `Unavailable` only (3 retries, exponential backoff from 2 s). Other failures are logged at `Error`; a listing failure skips the project, an access failure skips the secret, and loading continues.

## Guidelines

- Call `AddGcpSecretManager()` after the JSON file sources so secrets override local values.
- Name secrets with `__` separators so they bind to options sections.
- Narrow loading with `PrefixFilters` or `SecretIds`; loading a whole project is slow and over-privileged.
- Grant the service account `Secret Manager Secret Accessor` only.
- Use Application Default Credentials in GCP (`gcloud auth application-default login` locally) instead of a credentials file.
