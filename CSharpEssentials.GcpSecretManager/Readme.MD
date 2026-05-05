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
        Prefix = "MyApp"
    });
});

string connectionString = builder.Configuration["MyApp:ConnectionStrings:Default"]!;
```

### Setup via AppSettings

```json
{
  "GoogleSecretManager": {
    "Projects": [
      { "ProjectId": "my-project", "Prefix": "Prod" }
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
