namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The <c>ILogger&lt;T&gt;</c> every container of <see cref="LoggingBehaviorBenchmarks"/> registers.</summary>
public enum LoggerMode
{
    /// <summary><c>NullLogger&lt;T&gt;</c>: <c>IsEnabled</c> is false, nothing is formatted.</summary>
    Disabled,

    /// <summary><see cref="CountingLogger{T}"/>: every message is formatted and its length counted.</summary>
    Enabled,
}
