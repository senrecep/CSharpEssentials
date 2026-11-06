using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRGetProductHandler : IRequestHandler<GetProduct, ProductView>
{
    public Task<ProductView> Handle(GetProduct request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return Task.FromResult(new ProductView(request.Id, "p", 9.5m));
    }
}
