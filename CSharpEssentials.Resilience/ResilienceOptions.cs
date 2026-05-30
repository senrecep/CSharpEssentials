namespace CSharpEssentials.Resilience;

public sealed record RetryOptions
{
    public int MaxAttempts { get; init; } = 3;
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(1);
    public bool ExponentialBackoff { get; init; } = true;
}

public sealed record TimeoutOptions
{
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

public sealed record CircuitBreakerOptions
{
    public int MinimumThroughput { get; init; } = 10;
    public TimeSpan SamplingDuration { get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);
    public double FailureRatio { get; init; } = 0.5;
}

public sealed record ResiliencePolicyOptions
{
    public RetryOptions? Retry { get; init; }
    public TimeoutOptions? Timeout { get; init; }
    public CircuitBreakerOptions? CircuitBreaker { get; init; }
}
