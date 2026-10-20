namespace CSharpEssentials.Idempotency;

/// <summary>
/// Names of the idempotency metrics, for wiring them into OpenTelemetry or another <c>MeterListener</c>. The HTTP
/// middleware, the executor, the Mediator behavior and the inbox all record into the same counter.
/// </summary>
public static class IdempotencyTelemetry
{
    /// <summary>The name of the <c>Meter</c>: <c>CSharpEssentials.Idempotency</c>.</summary>
    public const string MeterName = "CSharpEssentials.Idempotency";

    /// <summary>
    /// The counter of decisions: <c>idempotency.decisions</c>. Tags: <c>decision</c> (<c>reserved</c>, <c>replay</c>,
    /// <c>key_reused</c>, <c>in_flight</c>, <c>unscoped</c>, <c>released</c>, <c>stored</c>), <c>mode</c>
    /// (<c>observe</c> or <c>enforce</c>), <c>source</c> (<c>http</c>, <c>mediator</c>, <c>messaging</c> or
    /// <c>executor</c>) and <c>target</c> (the route template, the request type or the record namespace). Keys and
    /// scopes are never tags. Nothing is recorded in <see cref="IdempotencyMode.Off"/>.
    /// </summary>
    public const string DecisionsInstrumentName = "idempotency.decisions";
}
