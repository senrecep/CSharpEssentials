using Microsoft.Extensions.Configuration;
using CSharpEssentials.GcpSecretManager.Configuration.Internal;

namespace CSharpEssentials.GcpSecretManager;

public static class SecretManagerConfigurationProviderExtensions
{
    /// <summary>
    /// Adds Google Cloud Secret Manager as a configuration source.
    /// </summary>
    /// <param name="configuration">The configuration manager.</param>
    /// <param name="options">Optional configuration options.</param>
    /// <returns>The configuration manager for chaining.</returns>
    public static IConfigurationManager AddGcpSecretManager(
        this IConfigurationManager configuration,
        Action<SecretManagerConfigurationOptions>? options = null)
    {
        var configurationOptions = new SecretManagerConfigurationOptions { LoadFromAppSettings = options == null };

        options?.Invoke(configurationOptions);

        configurationOptions.LoadFromConfiguration(configuration);

        if (configurationOptions.Projects.Count == 0)
        {
            throw new ArgumentException("At least one project configuration is required. Either configure projects manually or set LoadFromAppSettings=true.");
        }

        configuration.Add(new SecretManagerConfigurationSource(configurationOptions));
        return configuration;
    }
}