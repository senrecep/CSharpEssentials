using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRPlaceOrderHandler : IRequestHandler<PlaceOrder, Result<int>>
{
    public Task<Result<int>> Handle(PlaceOrder request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return Task.FromResult(request.Quantity == 0
            ? Result<int>.Failure(Error.Validation("Quantity.Zero", "The quantity must not be zero."))
            : Result<int>.Success(request.Quantity + 1));
    }
}
