using AsyncKeyedLock;
using CSharpEssentials.ResultPattern;
using MediatR;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/jbogard/MediatR/wiki/Behaviors
// https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
public sealed class MediatRLockBehavior : IPipelineBehavior<ReserveStock, Result<int>>
{
    internal static AsyncKeyedLocker<string> Locker { get; } = new();

    public async Task<Result<int>> Handle(ReserveStock request, RequestHandlerDelegate<Result<int>> next, CancellationToken cancellationToken)
    {
        using (await Locker.LockAsync(request.Sku, cancellationToken).ConfigureAwait(false))
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
