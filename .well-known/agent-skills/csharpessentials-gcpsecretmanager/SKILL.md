---
name: csharpessentials-gcpsecretmanager
description: Use when loading Google Cloud Secret Manager secrets into IConfiguration at startup. Covers builder.Configuration.AddGcpSecretManager with AddProject(new ProjectSecretConfiguration { ProjectId, PrefixFilters, SecretIds, RawSecretIds, RawSecretPrefixes, Region }) or the "GoogleSecretManager" appsettings section, SecretManagerConfigurationOptions (CredentialsPath, BatchSize, PageSize, LoggerFactory, Loader), the __ to : key mapping, JSON secret flattening, custom ISecretManagerConfigurationLoader and transient gRPC retries.
---

# CSharpEssentials.GcpSecretManager

Configuration provider that reads Secret Manager secrets once, inside the `AddGcpSecretManager` call, and exposes them as `IConfiguration` keys.

## Install

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Namespaces

```csharp
using CSharpEssentials.GcpSecretManager;               // ProjectSecretConfiguration
using CSharpEssentials.GcpSecretManager.Configuration; // SecretManagerConfigurationOptions, ISecretManagerConfigurationLoader
using CSharpEssentials.GcpSecretManager.Extensions;    // AddGcpSecretManager (class Extensions)
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.GcpSecretManager
// build-only: needs Google Cloud credentials and a project with secrets
using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Extensions;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationManager(); // in apps: builder.Configuration

configuration.AddGcpSecretManager(options =>
{
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "my-gcp-project",
        PrefixFilters = ["MyApp__"],       // secret ids starting with this
        SecretIds = ["stripe-api-key"],    // plus these exact ids
        RawSecretIds = ["stripe-api-key"], // never flatten as JSON
    });
    options.BatchSize = 10;  // parallel reads per project
    options.PageSize = 300;  // secrets per list call, 0 = server default
});

// Secret "MyApp__ConnectionStrings__Default" -> key "MyApp:ConnectionStrings:Default"
string? connectionString = configuration["MyApp:ConnectionStrings:Default"];
Console.WriteLine(connectionString is null ? "not found" : "loaded");
```

## Registering from appsettings.json

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
builder.Configuration.AddGcpSecretManager(); // no callback: reads "GoogleSecretManager"
```

- No callback: `LoadFromAppSettings` is true. With a callback it is false; set `options.LoadFromAppSettings = true` (and `options.ConfigurationSectionName`) to read the section too.
- Call it after `appsettings.json` is added; the section is read from earlier sources.

## Options

| `SecretManagerConfigurationOptions` | Default |
|---|---|
| `AddProject(ProjectSecretConfiguration)` (returns the options) / `Projects` | empty |
| `CredentialsPath` | `null` = Application Default Credentials |
| `LoadFromAppSettings`, `ConfigurationSectionName` | see above, `"GoogleSecretManager"` |
| `BatchSize` | 10, must be > 0 |
| `PageSize` | 300, must be >= 0 |
| `LoggerFactory` | `null` = no logs |
| `Loader` | `null` = built-in `__` mapping and filters |

`ProjectSecretConfiguration`: `ProjectId` (required), `Region` (regional endpoint `secretmanager.{region}.rep.googleapis.com`), `PrefixFilters` (case-sensitive), `SecretIds`, `RawSecretIds`, `RawSecretPrefixes`, and `IsRawSecret(secretId)`. No `PrefixFilters` and no `SecretIds` means every secret in the project.

## Keys and JSON

- Key = secret id with `__` replaced by `:`. Keys are case-insensitive; on a duplicate the first secret or project wins.
- The `latest` version is read.
- A JSON object or array value is also flattened: `MyApp__Db` = `{"Host":"db","Ports":[1,2]}` gives `MyApp:Db` (raw JSON), `MyApp:Db:Host`, `MyApp:Db:Ports:0`, `MyApp:Db:Ports:1`. Invalid JSON stays a plain string. Raw ids and prefixes are not flattened.

## Custom loader

```csharp
public sealed class DashLoader : ISecretManagerConfigurationLoader
{
    public string GetKey(Secret secret) => GetKey(secret.SecretName.SecretId); // called by the provider
    public string GetKey(string keyId) => keyId.Replace("--", ":");
    public bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration project) =>
        project.PrefixFilters.Count == 0 && project.SecretIds.Count == 0
        || project.SecretIds.Contains(secret.SecretName.SecretId)
        || project.PrefixFilters.Any(p => secret.SecretName.SecretId.StartsWith(p, StringComparison.Ordinal));
}

builder.Configuration.AddGcpSecretManager(o =>
{
    o.Loader = new DashLoader();
    o.AddProject(new ProjectSecretConfiguration { ProjectId = "my-gcp-project" });
});
```

`Secret` and `SecretName` come from `Google.Cloud.SecretManager.V1`. A custom loader replaces the built-in filter too, so reimplement `PrefixFilters` / `SecretIds` if they should still apply.

## Logging and failures

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole()); // Microsoft.Extensions.Logging.Console
builder.Configuration.AddGcpSecretManager(o =>
{
    o.LoggerFactory = loggerFactory;
    o.AddProject(new ProjectSecretConfiguration { ProjectId = "my-gcp-project" });
});
```

- `ResourceExhausted` and `Unavailable` are retried 3 times (2, 4, 8 s). Other gRPC errors fail at once.
- A failed secret read skips that secret; a failed listing skips the project. Both are logged at `Error` and startup continues, so missing keys are the only sign without a `LoggerFactory`.
- Missing credentials throw out of `AddGcpSecretManager`.

## Rules

- `AddGcpSecretManager` extends `IConfigurationManager` (`builder.Configuration`, `new ConfigurationManager()`), not `IConfigurationBuilder` or `IServiceCollection`. Namespace `CSharpEssentials.GcpSecretManager.Extensions`.
- There is no async version and no reload. Secrets load during the call.
- At least one project is required (`ArgumentException`). A missing section, empty project list or missing `ProjectId` from configuration throws `InvalidOperationException`. `BatchSize` <= 0 or `PageSize` < 0 throws `ArgumentOutOfRangeException` when set.
- `ProjectSecretConfiguration` is a record with `init` properties; use object initializers, collection expressions work (`PrefixFilters = ["App__"]`).
- Name secrets with `__` between levels (`MyApp__Db__Password`); secret ids cannot contain `:`.
- Narrow with `PrefixFilters` or `SecretIds` and grant only Secret Manager Secret Accessor plus list permission.
- Not Native AOT compatible.
- No analyzer diagnostics ship with this package.
