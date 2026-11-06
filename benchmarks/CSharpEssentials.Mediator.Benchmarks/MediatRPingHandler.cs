using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatRPingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(request.Value + 1);
}
