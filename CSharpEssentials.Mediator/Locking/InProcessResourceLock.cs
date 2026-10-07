using CSharpEssentials.Maybe;

namespace CSharpEssentials.Locking;

/// <summary>
/// <see cref="IResourceLock"/> for a single process: one <see cref="SemaphoreSlim"/> per key, removed when no caller
/// holds or waits for it. It does not coordinate several instances of an application; use a database or
/// distributed lock for that. The lock is not reentrant: acquiring a key you already hold waits for yourself.
/// </summary>
public sealed class InProcessResourceLock : IResourceLock
{
    private readonly Dictionary<string, Entry> _entries = [];
#if NET9_0_OR_GREATER
    private readonly Lock _sync = new();
#else
    private readonly object _sync = new();
#endif

    internal int KeyCount
    {
        get
        {
            lock (_sync)
                return _entries.Count;
        }
    }

    public async ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        TimeSpan wait = timeout ?? Timeout.InfiniteTimeSpan;
        if (wait < TimeSpan.Zero && wait != Timeout.InfiniteTimeSpan || wait.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must be zero, positive up to int.MaxValue milliseconds, or Timeout.InfiniteTimeSpan.");

        Entry entry = Rent(key);
        bool acquired = false;
        try
        {
            acquired = await entry.Semaphore.WaitAsync(wait, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (!acquired)
                Return(key, entry);
        }

        return acquired
            ? CreateHandle(key, entry)
            : throw new TimeoutException($"The lock for '{key}' was not acquired within {wait}.");
    }

    public ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        cancellationToken.ThrowIfCancellationRequested();

        Entry entry = Rent(key);
        // The token was checked above; passing it here could throw after Rent and leak the entry.
        if (entry.Semaphore.Wait(0, CancellationToken.None))
            return new ValueTask<Maybe<IAsyncDisposable>>(Maybe<IAsyncDisposable>.From(CreateHandle(key, entry)));

        Return(key, entry);
        return new ValueTask<Maybe<IAsyncDisposable>>(Maybe<IAsyncDisposable>.None);
    }

    private static void ValidateKey(string key)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
            throw new ArgumentNullException(nameof(key));
#endif
        if (key.Length == 0)
            throw new ArgumentException("The lock key must not be empty.", nameof(key));
    }

    private Handle CreateHandle(string key, Entry entry) => new(this, key, entry);

    private Entry Rent(string key)
    {
        lock (_sync)
        {
            if (!_entries.TryGetValue(key, out Entry? entry))
            {
                entry = new Entry();
                _entries.Add(key, entry);
            }

            entry.References++;
            return entry;
        }
    }

    private void Return(string key, Entry entry)
    {
        lock (_sync)
        {
            if (--entry.References > 0)
                return;

            _entries.Remove(key);
        }

        entry.Semaphore.Dispose();
    }

    private sealed class Entry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int References { get; set; }
    }

    private sealed class Handle(InProcessResourceLock owner, string key, Entry entry) : IAsyncDisposable
    {
        private int _disposed;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                entry.Semaphore.Release();
                owner.Return(key, entry);
            }

            return default;
        }
    }
}
