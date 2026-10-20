using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CSharpEssentials.Idempotency;

/// <summary>Records idempotency decisions into <see cref="IdempotencyTelemetry.DecisionsInstrumentName"/>.</summary>
internal static class IdempotencyMetrics
{
    internal const string Reserved = "reserved";
    internal const string Replay = "replay";
    internal const string KeyReused = "key_reused";
    internal const string InFlight = "in_flight";
    internal const string Unscoped = "unscoped";
    internal const string Released = "released";
    internal const string Stored = "stored";

    internal const string HttpSource = "http";
    internal const string MediatorSource = "mediator";
    internal const string MessagingSource = "messaging";
    internal const string ExecutorSource = "executor";

    private static readonly Meter Meter = new(IdempotencyTelemetry.MeterName);

    private static readonly Counter<long> Decisions = Meter.CreateCounter<long>(
        IdempotencyTelemetry.DecisionsInstrumentName,
        "{decision}",
        "Idempotency decisions by decision, mode, source and target.");

    /// <summary>Counts one decision; <see cref="IdempotencyMode.Off"/> records nothing.</summary>
    internal static void Record(string decision, IdempotencyMode mode, string source, string target)
    {
        if (mode == IdempotencyMode.Off || !Decisions.Enabled)
            return;

        TagList tags = new()
        {
            { "decision", decision },
            { "mode", mode == IdempotencyMode.Observe ? "observe" : "enforce" },
            { "source", source },
            { "target", target },
        };
        Decisions.Add(1, in tags);
    }
}
