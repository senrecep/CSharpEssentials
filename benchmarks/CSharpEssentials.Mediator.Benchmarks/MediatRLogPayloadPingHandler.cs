using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRLogPayloadPingHandler : IRequestHandler<LogPayloadPing, Result<int>>
{
    public Task<Result<int>> Handle(LogPayloadPing request, CancellationToken cancellationToken)
    {
        HandlerProbe.HandlerCalls++;
        return Task.FromResult(Result<int>.Success(request.Value + 1));
    }
}
