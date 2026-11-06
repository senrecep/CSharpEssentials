using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

public sealed class MediatorPingHandler : IRequestHandler<Ping, int>
{
    public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => new(request.Value + 1);
}
