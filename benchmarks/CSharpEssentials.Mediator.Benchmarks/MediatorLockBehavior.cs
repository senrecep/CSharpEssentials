using AsyncKeyedLock;
using CSharpEssentials.ResultPattern;
using Mediator;

namespace CSharpEssentials.Mediator.Benchmarks;

// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
public sealed class MediatorLockBehavior : IPipelineBehavior<ReserveStock, Result<int>>
{
    internal static AsyncKeyedLocker<string> Locker { get; } = new();

    public async ValueTask<Result<int>> Handle(ReserveStock message, MessageHandlerDelegate<ReserveStock, Result<int>> next, CancellationToken cancellationToken)
    {
        using (await Locker.LockAsync(message.Sku, cancellationToken).ConfigureAwait(false))
        {
            return await next(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
