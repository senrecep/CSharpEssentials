using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.Idempotency;

/// <summary>
/// Collects the idempotency decisions recorded for one target. Tests run in parallel and share the meter, so each test
/// listens to a target no other test uses.
/// </summary>
internal sealed class IdempotencyDecisionListener : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly ConcurrentQueue<IdempotencyDecision> _decisions = new();
    private readonly string _target;

    public IdempotencyDecisionListener(string target)
    {
        _target = target;
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == IdempotencyTelemetry.MeterName && instrument.Name == IdempotencyTelemetry.DecisionsInstrumentName)
                listener.EnableMeasurementEvents(instrument);
        };
        _listener.SetMeasurementEventCallback<long>(OnMeasurement);
        _listener.Start();
    }

    public IReadOnlyList<IdempotencyDecision> Decisions => [.. _decisions];

    public void Dispose() => _listener.Dispose();

    private void OnMeasurement(Instrument instrument, long value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        string? decision = null, mode = null, source = null, target = null;
        foreach (KeyValuePair<string, object?> tag in tags)
        {
            switch (tag.Key)
            {
                case "decision":
                    decision = tag.Value as string;
                    break;
                case "mode":
                    mode = tag.Value as string;
                    break;
                case "source":
                    source = tag.Value as string;
                    break;
                case "target":
                    target = tag.Value as string;
                    break;
            }
        }

        if (target == _target)
        {
            for (long count = 0; count < value; count++)
                _decisions.Enqueue(new IdempotencyDecision(decision!, mode!, source!, tags.Length));
        }
    }
}
