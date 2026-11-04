using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorLogPingHandler : IQueryHandler<LogPing, Result<int>>
{
    public ValueTask<Result<int>> Handle(LogPing query, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return new(Result<int>.Success(query.Value + 1));
    }
}
