namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Request feature that <c>UseIdempotency()</c> sets on a request that holds an idempotency reservation. It records
/// whether the endpoint's handler started, which decides whether the outcome may be stored or the key released.
/// </summary>
public interface IIdempotencyExecutionFeature
{
    /// <summary>
    /// <see langword="true"/> once the endpoint's handler is about to run. <c>.WithIdempotency()</c> and
    /// <see cref="IdempotentAttribute"/> install the filter that sets it right before the handler is invoked; responses
    /// produced earlier (rate limiting, load shedding, authorization, model binding) leave it <see langword="false"/>.
    /// An endpoint that runs its handler without that filter can set it itself and add
    /// <see cref="IdempotencyFilterInstalledMetadata.Instance"/> to its metadata.
    /// </summary>
    bool HandlerStarted { get; set; }
}
