# CSharpEssentials.GcpSecretManager

`CSharpEssentials.GcpSecretManager` integrates Google Cloud Secret Manager into the .NET Configuration system. It allows you to load secrets directly into `IConfiguration`, making them accessible via standard dependency injection options patterns.

## 🚀 Features

- **Configuration Integration**: Secrets appear as standard configuration values (e.g., `ConnectionStrings:Default`).
- **Multi-Project Support**: Load secrets from multiple Google Cloud projects.
- **Flexible Configuration**: Configure via `appsettings.json` or fluent API code.
- **Batch Processing**: Efficient secret retrieval with configurable batch sizes.
- **Credentials Handling**: Supports standard Google Application Credentials or explicit file paths.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## 🛠 Usage

### 1. Setup via Code

Add Secret Manager in your `Program.cs` (or `Startup.cs`).

```csharp
using CSharpEssentials.GcpSecretManager;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddGcpSecretManager(options =>
{
    options.CredentialsPath = "path/to/key.json"; // Optional, uses default if null
    
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "my-gcp-project-id",
        Prefix = "MyApp" // Optional: Filter secrets starting with this
    });
});

// Now access secrets as config
string dbConn = builder.Configuration["MyApp_ConnectionStrings_Db"];
```

### 2. Setup via AppSettings

You can define the configuration in `appsettings.json` and let the library load it automatically.

**appsettings.json**:
```json
{
  "GoogleSecretManager": {
    "Projects": [
      {
        "ProjectId": "my-production-project",
        "Prefix": "Prod",
        "Exclude": ["Internal_"]
      }
    ]
  }
}
```

**Program.cs**:
```csharp
// Automatically loads from "GoogleSecretManager" section
builder.Configuration.AddGcpSecretManager(); 
```

### 3. Key Mapping

Secrets in GCP are typically named with underscores or dashes. This library maps them to .NET configuration keys.

- GCP Secret: `MyApp__ConnectionStrings__Default`
- .NET Config: `MyApp:ConnectionStrings:Default`

(Note: Double underscores `__` are standard separators in .NET configuration providers).

### 4. Performance Tuning

Adjust batch sizes for large numbers of secrets.

```csharp
options.BatchSize = 20;
options.PageSize = 500;
```
