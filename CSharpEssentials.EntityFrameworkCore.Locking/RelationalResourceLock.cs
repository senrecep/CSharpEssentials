using CSharpEssentials.Locking;
using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Locking;

/// <summary>
/// <see cref="IResourceLock"/> held by a database session, so it serializes requests across every application
/// instance that shares the database. Each acquire resolves <typeparamref name="TContext"/> in a new service scope, opens
/// its connection and runs <see cref="ResourceLockSqlDialect.TryAcquireSql"/> through <c>SqlQueryRaw</c>; the handle keeps that connection open
/// and runs <see cref="ResourceLockSqlDialect.ReleaseSql"/> before closing it. The lock is independent of the request's
/// own context and transactions, so register <c>LockBehavior</c> with <c>LockPlacement.OutsideTransaction</c> (the default).
/// <para>
/// <c>AcquireAsync</c> polls <c>TryAcquireSql</c> every <see cref="RelationalResourceLockOptions.PollInterval"/> until the
/// lock is free, the timeout passes or the token is cancelled, so waiters are not served in arrival order. When the
/// connection drops, the database ends the session and releases the lock while the handle is still held. Each acquire
/// uses one pooled connection for as long as the lock is held. The lock is not reentrant.
/// </para>
/// </summary>
/// <typeparam name="TContext">A relational context whose connection reaches the lock database.</typeparam>
public sealed class RelationalResourceLock<TContext> : IResourceLock
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ResourceLockSqlDialect _dialect;
    private readonly TimeSpan _pollInterval;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates the lock.</summary>
    /// <param name="scopeFactory">Creates the scope that owns each lock's context.</param>
    /// <param name="options">The dialect and poll interval.</param>
    /// <param name="timeProvider">The clock of timeouts and polling; <see cref="TimeProvider.System"/> when <see langword="null"/>.</param>
    /// <exception cref="ArgumentException">No dialect is set, or the poll interval is not positive.</exception>
    public RelationalResourceLock(IServiceScopeFactory scopeFactory, RelationalResourceLockOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        _dialect = options.Dialect
            ?? throw new ArgumentException($"{nameof(RelationalResourceLockOptions)}.{nameof(RelationalResourceLockOptions.Dialect)} must be set.", nameof(options));
        if (options.PollInterval <= TimeSpan.Zero)
            throw new ArgumentException($"{nameof(RelationalResourceLockOptions)}.{nameof(RelationalResourceLockOptions.PollInterval)} must be positive.", nameof(options));
        _scopeFactory = scopeFactory;
        _pollInterval = options.PollInterval;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncDisposable> AcquireAsync(string key, TimeSpan? timeout, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        TimeSpan wait = timeout ?? Timeout.InfiniteTimeSpan;
        if (wait < TimeSpan.Zero && wait != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must be zero, positive, or Timeout.InfiniteTimeSpan.");

        long started = _timeProvider.GetTimestamp();
        while (true)
        {
            Maybe<IAsyncDisposable> handle = await TryAcquireAsync(key, cancellationToken).ConfigureAwait(false);
            if (handle.HasValue)
                return handle.Value;

            TimeSpan delay = _pollInterval;
            if (wait != Timeout.InfiniteTimeSpan)
            {
                TimeSpan remaining = wait - _timeProvider.GetElapsedTime(started);
                if (remaining <= TimeSpan.Zero)
                    throw new TimeoutException($"The lock for '{key}' was not acquired within {wait}.");
                if (remaining < delay)
                    delay = remaining;
            }

            await Task.Delay(delay, _timeProvider, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask<Maybe<IAsyncDisposable>> TryAcquireAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        cancellationToken.ThrowIfCancellationRequested();

        Session session = new(_scopeFactory.CreateAsyncScope());
        bool acquired;
        try
        {
            acquired = await session.TryAcquireAsync(_dialect.TryAcquireSql, key, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await session.CloseAsync().ConfigureAwait(false);
            throw;
        }

        if (acquired)
            return CreateHandle(session, key);

        await session.CloseAsync().ConfigureAwait(false);
        return Maybe<IAsyncDisposable>.None;
    }

    private Maybe<IAsyncDisposable> CreateHandle(Session session, string key) =>
        Maybe<IAsyncDisposable>.From(NewHandle(session, key));

    private Handle NewHandle(Session session, string key) => new(_dialect.ReleaseSql, session, key);

    private static void ValidateKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length == 0)
            throw new ArgumentException("The lock key must not be empty.", nameof(key));
    }

    /// <summary>The scope and open connection that hold one lock.</summary>
    private sealed class Session(AsyncServiceScope scope)
    {
        private TContext? _context;

        public async Task<bool> TryAcquireAsync(string sql, string key, CancellationToken cancellationToken)
        {
            _context = scope.ServiceProvider.GetRequiredService<TContext>();
            await _context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            List<int> result = await _context.Database
                .SqlQueryRaw<int>(sql, key)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return result is [1];
        }

        public Task<int> ReleaseAsync(string sql, string key) =>
            _context!.Database.ExecuteSqlRawAsync(sql, [key], CancellationToken.None);

        public async Task CloseAsync()
        {
            try
            {
                if (_context is not null)
                    await _context.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
            finally
            {
                await scope.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private sealed class Handle(string releaseSql, Session session, string key) : IAsyncDisposable
    {
        private int _disposed;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            try
            {
                await session.ReleaseAsync(releaseSql, key).ConfigureAwait(false);
            }
            finally
            {
                await session.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
