using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRLogPingHandler : IRequestHandler<LogPing, Result<int>>
{
    public Task<Result<int>> Handle(LogPing request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return Task.FromResult(Result<int>.Success(request.Value + 1));
    }
}
