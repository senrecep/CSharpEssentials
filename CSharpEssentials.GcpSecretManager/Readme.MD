# CSharpEssentials.GcpSecretManager

A .NET library for seamlessly integrating Google Cloud Secret Manager with your application's configuration system. This library provides a configuration provider that allows you to store and manage your application secrets in Google Cloud Secret Manager and access them through the standard .NET configuration system.

## Features

- 🔐 Secure secret management using Google Cloud Secret Manager
- 🔄 Automatic secret rotation support
- 🌐 Region-specific secret management
- 🔍 Flexible secret filtering with prefix and exact match options
- 📦 Batch processing for optimal performance
- 🔁 Built-in retry policies for resilience
- 🗃️ JSON flattening support for complex secret values
- 🛠️ Customizable configuration loader

## Installation

Install the package via NuGet:

```bash
dotnet add package CSharpEssentials.GcpSecretManager
```

## Quick Start

1. Add the following configuration to your `appsettings.json`:

```json
{
  "GoogleSecretManager": {
    "Projects": [
      {
        "ProjectId": "your-gcp-project-id",
        "Region": "europe-west1",  // Optional
        "PrefixFilters": ["app1_", "app2_"],  // Optional
        "SecretIds": ["specific_secret_1", "specific_secret_2"]  // Optional
      }
    ]
  }
}
```

2. Configure the secret manager in your application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add GCP Secret Manager configuration
builder.Configuration.AddGcpSecretManager();
```

## Advanced Configuration

### Manual Configuration

```csharp
builder.Configuration.AddGcpSecretManager(options =>
{
    options.CredentialsPath = "/path/to/credentials.json";
    options.BatchSize = 20;
    options.PageSize = 500;
    options.AddProject(new ProjectSecretConfiguration
    {
        ProjectId = "your-gcp-project-id",
        Region = "europe-west1",
        PrefixFilters = ["app1_", "app2_"],
        SecretIds = ["specific_secret_1"]
    });
});
```

### Custom Configuration Loader

```csharp
public class CustomSecretManagerLoader : ISecretManagerConfigurationLoader
{
    public string GetKey(Secret secret) => 
        // Your custom key generation logic
    
    public string GetKey(string keyId) =>
        // Your custom key generation logic
    
    public bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration projectConfig) =>
        // Your custom secret filtering logic
}

// Usage
builder.Configuration.AddGcpSecretManager(options =>
{
    options.Loader = new CustomSecretManagerLoader();
});
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `CredentialsPath` | Path to Google Cloud credentials JSON file | `null` |
| `LoadFromAppSettings` | Whether to load configuration from appsettings.json | `true` when no options provided |
| `ConfigurationSectionName` | Section name in appsettings.json | `"GoogleSecretManager"` |
| `BatchSize` | Number of secrets to load in parallel | `10` |
| `PageSize` | Number of secrets to retrieve per page | `300` |

## Project Configuration

| Property | Description | Required |
|----------|-------------|----------|
| `ProjectId` | Google Cloud project ID | Yes |
| `Region` | Specific region for secrets | No |
| `PrefixFilters` | List of prefixes to filter secrets | No |
| `SecretIds` | List of specific secret IDs to load | No |

## Best Practices

1. **Credentials Management**:
   - Use service account with minimal required permissions
   - Store credentials securely and use environment-specific paths
   - Consider using managed identities in cloud environments

2. **Performance Optimization**:
   - Adjust `BatchSize` based on your application's needs
   - Use `PrefixFilters` or `SecretIds` to limit loaded secrets
   - Consider regional endpoints for better latency

3. **Error Handling**:
   - The library includes built-in retry policies for transient failures
   - Monitor and log secret loading failures
   - Implement proper fallback mechanisms

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
