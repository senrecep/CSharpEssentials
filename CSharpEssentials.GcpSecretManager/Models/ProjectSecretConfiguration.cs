
namespace CSharpEssentials.GcpSecretManager;

/// <summary>
/// Configuration for a Google Cloud project's secrets.
/// </summary>
public sealed record ProjectSecretConfiguration
{
    /// <summary>
    /// Gets or sets the Google Cloud project ID.
    /// </summary>
#if NET7_0_OR_GREATER
    public required string ProjectId { get; init; }
#else
    public string ProjectId { get; init; } = null!;
#endif

    /// <summary>
    /// Gets or sets the region for the secrets. If null, uses the global endpoint.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Gets the list of prefix filters for secret IDs.
    /// Only secrets with IDs starting with these prefixes will be loaded.
    /// </summary>
#if NET8_0_OR_GREATER
    public IReadOnlyList<string> PrefixFilters { get; init; } = [];
#else
    public IReadOnlyList<string> PrefixFilters { get; init; } = Array.Empty<string>();
#endif

    /// <summary>
    /// Gets the list of specific secret IDs to load.
    /// Only these secrets will be loaded if specified.
    /// </summary>
#if NET8_0_OR_GREATER
    public IReadOnlyList<string> SecretIds { get; init; } = [];
#else
    public IReadOnlyList<string> SecretIds { get; init; } = Array.Empty<string>();
#endif

    /// <summary>
    /// Gets the list of specific secret IDs that should be treated as raw strings (not parsed as JSON).
    /// </summary>
#if NET8_0_OR_GREATER
    public IReadOnlyList<string> RawSecretIds { get; init; } = [];
#else
    public IReadOnlyList<string> RawSecretIds { get; init; } = Array.Empty<string>();
#endif

    /// <summary>
    /// Gets the list of prefixes for secrets that should be treated as raw strings (not parsed as JSON).
    /// Any secret with an ID starting with these prefixes will be treated as raw.
    /// </summary>
#if NET8_0_OR_GREATER
    public IReadOnlyList<string> RawSecretPrefixes { get; init; } = [];
#else
    public IReadOnlyList<string> RawSecretPrefixes { get; init; } = Array.Empty<string>();
#endif

    /// <summary>
    /// Checks if a secret should be treated as raw string based on its ID.
    /// </summary>
    /// <param name="secretId">The secret ID to check.</param>
    /// <returns>True if the secret should be treated as raw string; otherwise, false.</returns>
    public bool IsRawSecret(string secretId)
    {
        return RawSecretIds.Contains(secretId) ||
               RawSecretPrefixes.Any(prefix => secretId.StartsWith(prefix, StringComparison.Ordinal));
    }
}