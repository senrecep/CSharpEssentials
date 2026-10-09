# CSharpEssentials.GcpSecretManager

Integrates Google Cloud Secret Manager into the .NET Configuration system. Load secrets as standard `IConfiguration` values.

## Features

- **Configuration Integration**: Secrets appear as standard config keys.
- **Multi-Project**: Load from multiple GCP projects.
- **AppSettings Support**: Configure via `appsettings.json` or fluent API.
- **Batch Processing**: Efficient retrieval with configurable batch sizes.
- **Transient Retry**: Listing and per-secret access are retried on transient gRPC failures (uses `CSharpEssentials.Resilience`).

## Installation

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Usage

### Setup via Code

```csharp
using CSharpEssentials.GcpSecretManager;            // ProjectSecretConfiguration
using CSharpEssentials.GcpSecretManager.Extensions; // AddGcpSecretManager

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

Secrets are loaded once, synchronously, when `AddGcpSecretManager` is called (`ConfigurationManager` builds the added source immediately); there is no reload. The client uses Application Default Credentials unless `options.CredentialsPath` points to a credentials file. At least one project is required, otherwise `AddGcpSecretManager` throws `ArgumentException`.

`ProjectSecretConfiguration`:

| Property | Notes |
|---|---|
| `ProjectId` | Required. |
| `Region` | Optional. Reads regional secrets of `projects/{id}/locations/{region}` through the endpoint `secretmanager.{region}.rep.googleapis.com`. |
| `PrefixFilters` | Load only secrets whose id starts with one of these prefixes (case-sensitive). |
| `SecretIds` | Load these secrets by id, in addition to the prefix matches. With neither `PrefixFilters` nor `SecretIds`, every secret of the project is loaded. |
| `RawSecretIds`, `RawSecretPrefixes` | Secrets that are stored as one string and not parsed as JSON (see below). |

`SecretManagerConfigurationOptions` also has `BatchSize` (secrets read in parallel per project, default `10`) and `PageSize` (secrets per list request, default `300`). The latest version of every secret is read. When several projects or secrets produce the same key, the first one wins (projects in the order you added them).

### Logging

Configuration providers are built before dependency injection exists, so the provider does not
resolve a logger on its own and writes nothing to the console by default. Pass an `ILoggerFactory`
to get diagnostics (per-secret progress at `Debug`, failures at `Error`):

```csharp
using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Extensions;

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
using CSharpEssentials.GcpSecretManager.Extensions;

builder.Configuration.AddGcpSecretManager();
```

`AddGcpSecretManager()` without a callback reads the projects from the `GoogleSecretManager` section; it throws `InvalidOperationException` when the section, its projects or a `ProjectId` is missing. With a callback the section is not read; set `options.LoadFromAppSettings = true` to read it too, and `options.ConfigurationSectionName` to use another section. The section is read from the sources added before it, so call `AddGcpSecretManager` after `appsettings.json` (the default `builder.Configuration` already has it). `Region`, `SecretIds`, `RawSecretIds` and `RawSecretPrefixes` can be set in the section too.

### Key Mapping

GCP secrets use underscores; .NET config uses colons:
- GCP: `MyApp__ConnectionStrings__Default`
- .NET: `MyApp:ConnectionStrings:Default`

The secret id has every `__` replaced by `:`. To use another rule or another selection of secrets, implement `ISecretManagerConfigurationLoader` (`GetKey(Secret)`, `GetKey(string)`, `ShouldLoadSecret(Secret, ProjectSecretConfiguration)`, namespace `CSharpEssentials.GcpSecretManager.Configuration`) and assign it to `options.Loader`.

### JSON Secrets

A secret whose value is JSON is also flattened into configuration keys. The key of the secret keeps the raw text, and every value below it gets a child key, with `:` between object names and array indexes:

| Secret `MyApp__Database` | Keys |
|---|---|
| `{"Host":"db","Ports":[5432,5433]}` | `MyApp:Database` (the raw JSON), `MyApp:Database:Host` = `db`, `MyApp:Database:Ports:0` = `5432`, `MyApp:Database:Ports:1` = `5433` |

Bind the section as usual (`builder.Configuration.GetSection("MyApp:Database").Get<DatabaseOptions>()`). A value that is not valid JSON is kept as a plain string. List the secret in `RawSecretIds`, or by prefix in `RawSecretPrefixes`, to keep a JSON text (a certificate, a service account file) as one value without child keys.
