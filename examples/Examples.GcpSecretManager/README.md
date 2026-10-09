# CSharpEssentials.GcpSecretManager Example

This console application shows the configuration types of `CSharpEssentials.GcpSecretManager`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Configuration Options** | `SecretManagerConfigurationOptions` collects projects with `AddProject` |
| **Project Configuration** | `ProjectSecretConfiguration` with `ProjectId`, `SecretIds`, `PrefixFilters` and `RawSecretPrefixes`; `IsRawSecret` tells which secrets are kept as raw text instead of being parsed as JSON |
| **Multiple Projects** | Two projects with different filters in one options object |
| **Wiring (commented)** | `Program.cs` only mentions `AddGcpSecretManager` in a comment, on an `IConfigurationBuilder` variable; that call does not compile against `IConfigurationBuilder` (see the note below) |

## Running

```bash
cd examples/Examples.GcpSecretManager
dotnet run
```

> **Note**: This demo does not connect to Google Cloud and does not call `AddGcpSecretManager`; it only builds and prints the options. The commented call in `Program.cs` is written against an `IConfigurationBuilder`, but `AddGcpSecretManager` is an extension of `IConfigurationManager` (`builder.Configuration` of a `WebApplicationBuilder` or `HostApplicationBuilder`, or a `ConfigurationManager`) and needs `using CSharpEssentials.GcpSecretManager.Extensions;`. It loads the secrets when it is called, using Application Default Credentials or `CredentialsPath`. See the [package README](../../CSharpEssentials.GcpSecretManager/Readme.MD).
