using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorReserveStockHandler : ICommandHandler<ReserveStock, Result<int>>
{
    public ValueTask<Result<int>> Handle(ReserveStock command, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        if (LockProbe.Check is { } check)
        {
            LockProbe.Held = check(command.Sku);
        }

        return new(Result<int>.Success(6));
    }
}
