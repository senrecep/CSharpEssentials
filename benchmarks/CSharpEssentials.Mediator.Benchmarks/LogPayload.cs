namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Which request a <see cref="LoggingBehaviorBenchmarks"/> row sends.</summary>
public enum LogPayload
{
    /// <summary><see cref="LogPing"/>: <c>ILoggableRequest</c>, names and elapsed time only.</summary>
    None,

    /// <summary><see cref="LogPayloadPing"/>: <c>IRequestResponseLoggable</c>, adds the request and response as JSON.</summary>
    RequestResponse,
}
