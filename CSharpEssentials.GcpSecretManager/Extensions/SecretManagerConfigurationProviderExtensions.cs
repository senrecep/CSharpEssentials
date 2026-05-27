using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.GcpSecretManager.Extensions;

public static class Extensions
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
        => AddGcpSecretManager(configuration, options, null);

    internal static IConfigurationManager AddGcpSecretManager(
        this IConfigurationManager configuration,
        Action<SecretManagerConfigurationOptions>? options,
        IServiceClientHelper? clientHelper)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
#else
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));
#endif

        var configurationOptions = new SecretManagerConfigurationOptions { LoadFromAppSettings = options == null };

        options?.Invoke(configurationOptions);

        configurationOptions.LoadFromConfiguration(configuration);

        if (configurationOptions.Projects.Count == 0)
        {
            throw new ArgumentException("At least one project configuration is required. Either configure projects manually or set LoadFromAppSettings=true.");
        }

        configuration.Add(new SecretManagerConfigurationSource(configurationOptions, clientHelper));
        return configuration;
    }
}
