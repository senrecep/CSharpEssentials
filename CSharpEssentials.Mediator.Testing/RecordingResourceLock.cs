using CSharpEssentials.Maybe;

namespace CSharpEssentials.Locking.Testing;

/// <summary>
/// An in-process <see cref="IResourceLock"/> for tests that records every call. It locks for real (through
/// <see cref="InProcessResourceLock"/>), so it passes <see cref="ResourceLockContract"/>, and lets a test check which
/// keys <c>LockBehavior</c> took, with which timeout, and that every handle was released. Set
/// <see cref="FailWith"/> to make the next acquires throw. Safe to use from several threads.
/// </summary>
public sealed class RecordingResourceLock : IResourceLock
{
    private readonly InProcessResourceLock _inner = new();
    private readonly List<LockAcquisition> _acquisitions = [];
    private readonly List<string> _releases = [];
    private readonly Dictionary<string, int> _held = new(StringComparer.Ordinal);
#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();
#else
    private readonly object _sync = new();
#endif

    /// <summary>When set, <see cref="AcquireAsync"/> and <see cref="TryAcquireAsync"/> record the call and throw it.</summary>
    public Exception? FailWith { get; set; }

    /// <summary>Every acquire and try-acquire call, in order, whether it succeeded or not.</summary>
    public IReadOnlyList<LockAcquisition> Acquisitions
    {
        get
        {
            lock (_sync)
                return [.. _acquisitions];
        }
    }

    /// <summary>The key of every released handle, in release order.</summary>
    public IReadOnlyList<string> Releases
    {
        get
        {
            lock (_sync)
                return [.. _releases];
        }
    }

    /// <summary>The keys currently held, each once.</summary>
    public IReadOnlyCollection<string> HeldKeys
    {
        get
        {
            lock (_sync)
                return [.. _held.Keys];
        }
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        LockAcquisition acquisition = Record(key, timeout, tryOnly: false);
        IAsyncDisposable handle = await _inner.AcquireAsync(key, timeout, cancellationToken).ConfigureAwait(false);
        return Track(acquisition, handle);
    }

    /// <inheritdoc />
    public async ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        LockAcquisition acquisition = Record(key, null, tryOnly: true);
        Maybe<IAsyncDisposable> handle = await _inner.TryAcquireAsync(key, cancellationToken).ConfigureAwait(false);
        return handle.HasValue ? Maybe<IAsyncDisposable>.From(Track(acquisition, handle.Value)) : handle;
    }

    private LockAcquisition Record(string key, TimeSpan? timeout, bool tryOnly)
    {
        LockAcquisition acquisition = new(key, timeout, tryOnly);
        lock (_sync)
            _acquisitions.Add(acquisition);
        return FailWith is { } failure ? throw failure : acquisition;
    }

    private Release Track(LockAcquisition acquisition, IAsyncDisposable inner)
    {
        lock (_sync)
            _held[acquisition.Key] = _held.TryGetValue(acquisition.Key, out int count) ? count + 1 : 1;
        return new Release(this, acquisition.Key, inner);
    }

    private void Released(string key)
    {
        lock (_sync)
        {
            _releases.Add(key);
            if (_held.TryGetValue(key, out int count) && count > 1)
                _held[key] = count - 1;
            else
                _held.Remove(key);
        }
    }

    private sealed class Release(RecordingResourceLock owner, string key, IAsyncDisposable inner) : IAsyncDisposable
    {
        private int _disposed;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            owner.Released(key);
            await inner.DisposeAsync().ConfigureAwait(false);
        }
    }
}
