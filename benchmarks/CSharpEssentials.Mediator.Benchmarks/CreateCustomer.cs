using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The request of <see cref="ValidationFanOutBenchmarks"/> and <see cref="ValidationScopeBenchmarks"/>, one type for every variant.</summary>
/// <param name="Name">Must not be empty.</param>
/// <param name="Email">Must not be empty.</param>
/// <param name="Age">Must be between 18 and 120.</param>
public sealed record CreateCustomer(string? Name, string? Email, int Age) : global::Mediator.ICommand<Result<int>>, global::MediatR.IRequest<Result<int>>;
