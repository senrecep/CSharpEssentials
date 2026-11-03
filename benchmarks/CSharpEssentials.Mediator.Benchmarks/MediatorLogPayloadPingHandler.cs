using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorLogPayloadPingHandler : IQueryHandler<LogPayloadPing, Result<int>>
{
    public ValueTask<Result<int>> Handle(LogPayloadPing query, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return new(Result<int>.Success(query.Value + 1));
    }
}
