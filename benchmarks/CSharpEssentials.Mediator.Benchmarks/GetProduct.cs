namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The request of <see cref="CachingBehaviorBenchmarks"/>, written as the CSharpEssentials.Mediator Readme shows <c>ICacheable</c>.</summary>
/// <param name="Id">The product id.</param>
public sealed record GetProduct(int Id) : global::Mediator.IQuery<ProductView>, global::MediatR.IRequest<ProductView>, ICacheable
{
    public string CacheKey => $"product:{Id}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);

    public bool BypassCache => false;

    public bool CacheFailures => false;
}
