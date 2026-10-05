# CSharpEssentials.GcpSecretManager

Integrates Google Cloud Secret Manager into the .NET Configuration system. Load secrets as standard `IConfiguration` values.

## Features

- **Configuration Integration** — Secrets appear as standard config keys.
- **Multi-Project** — Load from multiple GCP projects.
- **AppSettings Support** — Configure via `appsettings.json` or fluent API.
- **Batch Processing** — Efficient retrieval with configurable batch sizes.

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
using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddConsole());

builder.Configuration.AddGcpSecretManager(options =>
{
    options.LoggerFactory = loggerFactory;
    options.AddProject(new ProjectSecretConfiguration { ProjectId = "my-gcp-project" });
});
```

`SecretManagerConfigurationOptions` is a `sealed record`; `CredentialsPath`, `Loader`, `LoggerFactory`
and `Projects` are settable and `AddProject` returns the options for chaining.

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
