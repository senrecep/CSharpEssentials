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
    /// Gets the list of prefix filters for secret IDs. When this list or <see cref="SecretIds"/> is set, a secret
    /// loads only if its ID starts with one of these prefixes or is listed in <see cref="SecretIds"/>.
    /// </summary>
    public IReadOnlyList<string> PrefixFilters { get; init; } = [];

    /// <summary>
    /// Gets the list of specific secret IDs to load. When this list or <see cref="PrefixFilters"/> is set, only
    /// these secrets and those matching <see cref="PrefixFilters"/> are loaded.
    /// </summary>
    public IReadOnlyList<string> SecretIds { get; init; } = [];

    /// <summary>
    /// Gets the list of specific secret IDs that should be treated as raw strings (not parsed as JSON).
    /// </summary>
    public IReadOnlyList<string> RawSecretIds { get; init; } = [];

    /// <summary>
    /// Gets the list of prefixes for secrets that should be treated as raw strings (not parsed as JSON).
    /// Any secret whose ID starts with one of these prefixes is treated as raw.
    /// </summary>
    public IReadOnlyList<string> RawSecretPrefixes { get; init; } = [];

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
