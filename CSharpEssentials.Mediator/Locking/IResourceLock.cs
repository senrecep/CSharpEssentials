using CSharpEssentials.Maybe;

namespace CSharpEssentials.Locking;

/// <summary>
/// Named lock over a resource key. Disposing the returned handle releases the lock.
/// <para>
/// Implementations whose lock ends with the surrounding database transaction (for example PostgreSQL
/// <c>pg_advisory_xact_lock</c>) may return a handle whose dispose does nothing; the lock is then released at commit or rollback.
/// Implementations that need a numeric key should derive it with a stable hash, for example FNV-1a 64 over the UTF-8
/// bytes of the key or a server-side hash such as PostgreSQL <c>hashtextextended(key, 0)</c>. Never use
/// <see cref="string.GetHashCode()"/>, which differs between processes.
/// </para>
/// </summary>
public interface IResourceLock
{
    /// <summary>
    /// Waits until the lock for <paramref name="key"/> is acquired.
    /// </summary>
    /// <param name="key">Lock key; must not be empty.</param>
    /// <param name="timeout">Maximum wait; <see langword="null"/> waits until <paramref name="cancellationToken"/> is cancelled.</param>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>A non-null handle that releases the lock when disposed.</returns>
    /// <exception cref="TimeoutException">The lock was not acquired within <paramref name="timeout"/>.</exception>
    ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default);

    /// <summary>Acquires the lock for <paramref name="key"/> when it is free, without waiting; otherwise returns <c>None</c>.</summary>
    ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default);
}
