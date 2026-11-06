using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRReserveStockHandler : IRequestHandler<ReserveStock, Result<int>>
{
    public Task<Result<int>> Handle(ReserveStock request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        if (LockProbe.Check is { } check)
        {
            LockProbe.Held = check(request.Sku);
        }

        return Task.FromResult(Result<int>.Success(6));
    }
}
