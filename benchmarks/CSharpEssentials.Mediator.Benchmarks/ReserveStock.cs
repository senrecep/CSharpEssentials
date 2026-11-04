using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The request of <see cref="LockBehaviorBenchmarks"/>, one type for every variant. The lock key is the SKU; no timeout.</summary>
/// <param name="Sku">The product, used as the lock key.</param>
public sealed record ReserveStock(string Sku) : global::Mediator.ICommand<Result<int>>, global::MediatR.IRequest<Result<int>>, ILockedRequest
{
    public string LockKey => Sku;
}
