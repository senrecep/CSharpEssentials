using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorPlaceOrderHandler : ICommandHandler<PlaceOrder, Result<int>>
{
    public ValueTask<Result<int>> Handle(PlaceOrder command, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return new(command.Quantity == 0
            ? Result<int>.Failure(Error.Validation("Quantity.Zero", "The quantity must not be zero."))
            : Result<int>.Success(command.Quantity + 1));
    }
}
