# CSharpEssentials.GcpSecretManager Example

This console application demonstrates GCP Secret Manager integration from `CSharpEssentials.GcpSecretManager`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Configuration Options** | `SecretManagerConfigurationOptions` for project/secret/version |
| **Configuration Builder** | How to wire `AddGcpSecretManager` into `IConfigurationBuilder` |
| **Options Pattern** | Typed options for secret manager settings |

## Running

```bash
cd examples/Examples.GcpSecretManager
dotnet run
```

> **Note**: This demo does not connect to actual Google Cloud. It demonstrates the configuration API only.
