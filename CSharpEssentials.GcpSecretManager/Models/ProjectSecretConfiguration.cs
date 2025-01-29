using System.Collections.Generic;

namespace CSharpEssentials.GcpSecretManager;

/// <summary>
/// Configuration for a Google Cloud project's secrets.
/// </summary>
public sealed record ProjectSecretConfiguration
{
    /// <summary>
    /// Gets or sets the Google Cloud project ID.
    /// </summary>
    public required string ProjectId { get; init; }

    /// <summary>
    /// Gets or sets the region for the secrets. If null, uses the global endpoint.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Gets the list of prefix filters for secret IDs.
    /// Only secrets with IDs starting with these prefixes will be loaded.
    /// </summary>
    public IReadOnlyList<string> PrefixFilters { get; init; } = [];

    /// <summary>
    /// Gets the list of specific secret IDs to load.
    /// Only these secrets will be loaded if specified.
    /// </summary>
    public IReadOnlyList<string> SecretIds { get; init; } = [];
}