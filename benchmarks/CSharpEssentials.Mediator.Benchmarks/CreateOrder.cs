using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The request of <see cref="BehaviorBenchmarks"/>, one type for every variant.</summary>
/// <param name="Name">Must not be empty.</param>
/// <param name="Quantity">Must be between 1 and 100.</param>
public sealed record CreateOrder(string? Name, int Quantity) : global::Mediator.ICommand<Result<int>>, global::MediatR.IRequest<Result<int>>;
