namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Counters the pass-through behaviors and notification handlers of both libraries bump, so the self-check can see they ran.</summary>
public static class Probe
{
    /// <summary>Incremented by every pass-through behavior call.</summary>
    public static int BehaviorCalls { get; set; }

    /// <summary>Incremented by every notification handler call.</summary>
    public static int NotificationCalls { get; set; }
}
