using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>A request of <see cref="LoggingBehaviorBenchmarks"/> that opts in to logging without payloads.</summary>
/// <param name="Value">The value the handlers add one to.</param>
public sealed record LogPing(int Value) : global::Mediator.IQuery<Result<int>>, global::MediatR.IRequest<Result<int>>, ILoggableRequest;
