# CSharpEssentials.GcpSecretManager

Integrates Google Cloud Secret Manager into the .NET Configuration system. Load secrets as standard `IConfiguration` values.

## Features

- **Configuration Integration** — Secrets appear as standard config keys.
- **Multi-Project** — Load from multiple GCP projects.
- **AppSettings Support** — Configure via `appsettings.json` or fluent API.
- **Batch Processing** — Efficient retrieval with configurable batch sizes.
- **Transient Retry** — Listing and per-secret access are retried on transient gRPC failures (uses `CSharpEssentials.Resilience`).

## Installation

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Usage

### Setup via Code

```csharp
using CSharpEssentials.GcpSecretManager;

builder.Configuration.AddGcpSecretManager(options =>
{
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "my-gcp-project",
        PrefixFilters = new[] { "MyApp__" }
    });
});

string connectionString = builder.Configuration["MyApp:ConnectionStrings:Default"]!;
```

### Logging

Configuration providers are built before dependency injection exists, so the provider does not
resolve a logger on its own and writes nothing to the console by default. Pass an `ILoggerFactory`
to get diagnostics (per-secret progress at `Debug`, failures at `Error`):

```csharp
using CSharpEssentials.GcpSecretManager;

using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

builder.Configuration.AddGcpSecretManager(options =>
{
    options.LoggerFactory = loggerFactory;
    options.AddProject(new ProjectSecretConfiguration { ProjectId = "my-gcp-project" });
});
```

`SecretManagerConfigurationOptions` is a `sealed record`; `CredentialsPath`, `Loader`, `LoggerFactory`
and `Projects` are settable and `AddProject` returns the options for chaining.

### Retries

Listing a project's secrets and accessing each secret version use `CSharpEssentials.Resilience`
(`RetryIfFailed` with a `shouldRetry` predicate). Only `ResourceExhausted` and `Unavailable` are
retried: 3 retries with exponential backoff (2s, 4s, 8s). Any other status, such as
`PermissionDenied` or `NotFound`, fails immediately. When retries are exhausted the original
`RpcException` is logged once at `Error`: a listing failure skips that project, an access failure
skips that secret, and loading continues.

### Setup via AppSettings

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

### Key Mapping

GCP secrets use underscores; .NET config uses colons:
- GCP: `MyApp__ConnectionStrings__Default`
- .NET: `MyApp:ConnectionStrings:Default`
