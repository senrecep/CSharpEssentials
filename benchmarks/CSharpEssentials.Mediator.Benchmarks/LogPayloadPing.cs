using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>A request of <see cref="LoggingBehaviorBenchmarks"/> that also logs the request and response as JSON.</summary>
/// <param name="Value">The value the handlers add one to.</param>
/// <param name="Name">Text that only adds to the request payload.</param>
public sealed record LogPayloadPing(int Value, string Name) : global::Mediator.IQuery<Result<int>>, global::MediatR.IRequest<Result<int>>, IRequestResponseLoggable;
