namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The result of an idempotent request, passed to <see cref="IdempotencyOptions.ShouldStore"/> to decide whether it is
/// stored and replayed or its key is released.
/// </summary>
public readonly record struct IdempotencyOutcome
{
    /// <summary>The response status code.</summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Whether the endpoint's handler started (<see cref="IIdempotencyExecutionFeature.HandlerStarted"/>).
    /// <c>UseIdempotency()</c> releases a request whose handler did not start before it asks the policy, so it passes
    /// <see langword="true"/>, except for a 2xx from an endpoint without the execution filter
    /// (<see cref="IdempotencyFilterInstalledMetadata"/>), where it is unknown and <see langword="false"/>.
    /// </summary>
    public bool HandlerStarted { get; init; }
}
