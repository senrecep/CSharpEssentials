namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Endpoint metadata stating that the endpoint sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/> right
/// before its handler runs. <c>.WithIdempotency()</c> and <see cref="IdempotentAttribute"/> add it together with their
/// filters. An idempotent endpoint without it is treated as if its handler never started: every outcome releases the
/// key, a warning is logged once, and <see cref="IdempotencyOptions.RequireExecutionFilter"/> turns it into an error.
/// </summary>
public sealed class IdempotencyFilterInstalledMetadata
{
    private IdempotencyFilterInstalledMetadata()
    {
    }

    /// <summary>The single instance.</summary>
    public static IdempotencyFilterInstalledMetadata Instance { get; } = new();
}
