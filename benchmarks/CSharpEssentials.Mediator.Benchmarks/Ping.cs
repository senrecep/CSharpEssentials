namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The request of <see cref="SendBenchmarks"/>, one type for both libraries.</summary>
/// <param name="Value">The value the handlers add one to.</param>
public sealed record Ping(int Value) : global::Mediator.IRequest<int>, global::MediatR.IRequest<int>;
