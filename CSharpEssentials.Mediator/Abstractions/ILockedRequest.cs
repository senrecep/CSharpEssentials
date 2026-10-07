namespace CSharpEssentials.Mediator;

/// <summary>
/// Marks a request that <see cref="LockBehavior{TRequest, TResponse}"/> runs while holding the
/// <see cref="Locking.IResourceLock"/> for <see cref="LockKey"/>, so requests with the same key run one at a time.
/// </summary>
public interface ILockedRequest
{
    /// <summary>Lock key; requests with equal keys are serialized.</summary>
    string LockKey { get; }

    /// <summary>Maximum time to wait for the lock; <see langword="null"/> (the default) waits until the request is cancelled.</summary>
    TimeSpan? LockTimeout => null;
}
