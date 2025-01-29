using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.GcpSecretManager;

/// <summary>
/// Defines methods for loading and filtering secrets from Google Cloud Secret Manager.
/// </summary>
public interface ISecretManagerConfigurationLoader
{
    /// <summary>
    /// Gets the configuration key for a secret.
    /// </summary>
    /// <param name="secret">The secret to get the key for.</param>
    /// <returns>The configuration key.</returns>
    string GetKey(Secret secret);

    /// <summary>
    /// Gets the configuration key for a secret ID.
    /// </summary>
    /// <param name="keyId">The secret ID to get the key for.</param>
    /// <returns>The configuration key.</returns>
    string GetKey(string keyId);

    /// <summary>
    /// Determines whether a secret should be loaded based on the project configuration.
    /// </summary>
    /// <param name="secret">The secret to check.</param>
    /// <param name="projectConfig">The project configuration.</param>
    /// <returns>True if the secret should be loaded; otherwise, false.</returns>
    bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration projectConfig);
}

internal sealed class DefaultSecretManagerConfigurationLoader : ISecretManagerConfigurationLoader
{
    public string GetKey(Secret secret)
        => GetKey(secret.SecretName.SecretId);

    public string GetKey(string keyId)
        => keyId.Replace("__", ConfigurationPath.KeyDelimiter);

    public bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration projectConfig)
    {
        ArgumentNullException.ThrowIfNull(secret);
        ArgumentNullException.ThrowIfNull(projectConfig);

        if (projectConfig.PrefixFilters.Count == 0 && projectConfig.SecretIds.Count == 0)
            return true;

        string secretId = secret.SecretName.SecretId;
        return projectConfig.SecretIds.Contains(secretId) ||
               projectConfig.PrefixFilters.Any(secretId.StartsWith);
    }
}