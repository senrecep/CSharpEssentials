using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorGetProductHandler : IQueryHandler<GetProduct, ProductView>
{
    public ValueTask<ProductView> Handle(GetProduct query, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return new(new ProductView(query.Id, "p", 9.5m));
    }
}
