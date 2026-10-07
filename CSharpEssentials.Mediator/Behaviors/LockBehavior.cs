using CSharpEssentials.Locking;

using Mediator;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Acquires the <see cref="IResourceLock"/> for <see cref="ILockedRequest.LockKey"/> before the handler and releases it
/// afterwards. A lock that is not acquired within <see cref="ILockedRequest.LockTimeout"/> throws <see cref="TimeoutException"/>.
/// </summary>
public sealed class LockBehavior<TRequest, TResponse>(IResourceLock resourceLock) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ILockedRequest, IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        IAsyncDisposable handle = await resourceLock.AcquireAsync(message.LockKey, message.LockTimeout, cancellationToken).ConfigureAwait(false);
        await using (handle.ConfigureAwait(false))
            return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
