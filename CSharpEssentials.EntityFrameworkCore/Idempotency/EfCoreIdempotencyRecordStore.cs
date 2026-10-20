using System.Data.Common;

using CSharpEssentials.Idempotency;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Idempotency;

/// <summary>
/// <see cref="IIdempotencyRecordStore"/> that keeps <see cref="IdempotencyRecord"/> rows in <typeparamref name="TContext"/>
/// and writes them on the context's connection, inside its current transaction when there is one. A record then
/// commits and rolls back with the business data of the same unit of work: a rolled-back request leaves no record and
/// its retry runs again. Use it with Mediator's <c>IdempotencyPlacement.InsideTransaction</c>; outside a transaction
/// every call commits on its own.
/// <para>
/// A reservation inserts against the primary key <see cref="IdempotencyRecord.KeyHash"/> (<c>INSERT … ON CONFLICT DO
/// NOTHING</c> on PostgreSQL and SQLite; on other providers a plain <c>INSERT</c> behind a savepoint, whose unique
/// violation is rolled back to the savepoint so the transaction stays usable) and reads the row only when the key
/// exists. A concurrent request for the same key waits on the index until the first transaction ends, then replays its
/// completed response, or reserves the key when it rolled back. Complete and release change the row only while it
/// carries the reservation's token; an expired row counts as absent and is taken over. All of this needs the
/// <c>READ COMMITTED</c> isolation level (the default of PostgreSQL, SQL Server and MySQL); under stricter levels a
/// concurrent reservation fails with a serialization error.
/// </para>
/// <para>
/// The key hash is fixed-length binary, so no collation can make two keys one record. Every write is also narrowed by
/// the namespace, scope and key text, and a row read by its hash must hold the same text, compared ordinally; a row
/// that does not (a hash collision) is never returned: the reservation throws <see cref="InvalidOperationException"/>.
/// </para>
/// <para>
/// The rows are written with set-based SQL, never through the change tracker, so <c>SaveChanges</c>, interceptors and
/// query filters do not touch them. Expired rows stay until <see cref="PurgeExpiredAsync"/> deletes them. Like the
/// context itself, an instance is not thread-safe: register it scoped with
/// <see cref="EfCoreIdempotencyStoreServiceCollectionExtensions.AddEfCoreIdempotencyRecordStore{TContext}"/>.
/// </para>
/// <para>
/// In <see cref="IdempotencyMode.Observe"/> the executor and the HTTP middleware reserve and release through
/// <see cref="Detached"/>, which writes from a new <typeparamref name="TContext"/> of its own scope, outside the business
/// and ambient transactions, so a reservation neither waits for a concurrent request nor can fail the business
/// transaction. A result to store releases that reservation and is written through this store, in the business
/// transaction, so it commits and rolls back with the business data as in <see cref="IdempotencyMode.Enforce"/>.
/// A result for a token this instance did not reserve, as in Observe, is written behind a savepoint of the context's
/// current transaction (on PostgreSQL also of an ambient <c>System.Transactions</c> transaction the connection is
/// enlisted in; when the connection is not enlisted, the result is written without a savepoint): a database error rolls
/// back to the savepoint and is rethrown, so the transaction stays usable and the business data commits without the
/// record. Two limits remain. On PostgreSQL, Observe stores results only under <c>READ COMMITTED</c>: under a stricter
/// level the write fails with a serialization error, because the release changed the row after the transaction's
/// snapshot was taken; other providers may succeed. On SQL Server a savepoint cannot save a transaction that the server
/// has doomed, which happens when the connection has <c>XACT_ABORT ON</c> or under <c>SNAPSHOT</c> isolation after an
/// update conflict (error 3960): the rollback to the savepoint fails, the transaction is lost with the business data,
/// and the original error is thrown.
/// </para>
/// </summary>
/// <typeparam name="TContext">The context that maps <see cref="IdempotencyRecord"/>.</typeparam>
public sealed class EfCoreIdempotencyRecordStore<TContext> : ITransactionalIdempotencyRecordStore
    where TContext : DbContext
{
    private const int MaxReserveAttempts = 8;
    private const string SavepointName = "cse_idempotency";
    private const string ResultSavepointName = "cse_idempotency_result";

    private readonly TContext _context;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly bool _plainInsert;
    private readonly HashSet<string> _reservedTokens = [];
    private DetachedEfCoreIdempotencyRecordStore<TContext>? _detached;

    /// <summary>Creates a store on <paramref name="context"/>.</summary>
    /// <param name="context">The context whose connection and current transaction the records are written in.</param>
    /// <param name="scopeFactory">Creates the scopes from which <see cref="Detached"/> resolves its own <typeparamref name="TContext"/>.</param>
    /// <param name="timeProvider">Clock for leases and retention; <see cref="TimeProvider.System"/> when <see langword="null"/>.</param>
    public EfCoreIdempotencyRecordStore(TContext context, IServiceScopeFactory scopeFactory, TimeProvider? timeProvider = null)
        : this(context, scopeFactory, timeProvider ?? TimeProvider.System, plainInsert: false)
    {
    }

    /// <summary>Creates a store on <paramref name="context"/> that can be forced onto the savepoint insert path.</summary>
    /// <param name="context">The context whose connection and current transaction the records are written in.</param>
    /// <param name="scopeFactory">Creates the scopes of <see cref="Detached"/>.</param>
    /// <param name="timeProvider">Clock for leases and retention.</param>
    /// <param name="plainInsert">
    /// <see langword="true"/> inserts with the savepoint path even where <c>ON CONFLICT DO NOTHING</c> is available, so
    /// tests can run that path on PostgreSQL.
    /// </param>
    internal EfCoreIdempotencyRecordStore(TContext context, IServiceScopeFactory scopeFactory, TimeProvider timeProvider, bool plainInsert)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _context = context;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _plainInsert = plainInsert;
    }

    /// <summary>
    /// The same records, written from a new <typeparamref name="TContext"/> resolved in a scope of its own for every call,
    /// with any ambient <c>System.Transactions</c> transaction suppressed: each call commits on its own and never waits
    /// for an uncommitted reservation. The context must not depend on request-scoped state that a new scope does not
    /// have (a connection opened by the request, for example).
    /// </summary>
    public IIdempotencyRecordStore Detached => _detached ??= new DetachedEfCoreIdempotencyRecordStore<TContext>(_scopeFactory, _timeProvider, _plainInsert);

    /// <inheritdoc />
    public async ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lease, TimeSpan.Zero);

        byte[] hash = IdempotencyRecord.HashKey(key);
        for (int attempt = 0; attempt < MaxReserveAttempts; attempt++)
        {
            DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
            DateTime expiresAt = now + lease;
            string token = Guid.NewGuid().ToString("N");

            if (await TryInsertAsync(key, hash, token, fingerprint, payload: null, expiresAt, cancellationToken).ConfigureAwait(false))
            {
                _reservedTokens.Add(token);
                return IdempotencyRecordReservation.Reserved(token);
            }

            int takenOver = await Records(key, hash)
                .Where(record => record.ExpiresAt <= now)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(record => record.Token, token)
                        .SetProperty(record => record.Fingerprint, fingerprint)
                        .SetProperty(record => record.Payload, (byte[]?)null)
                        .SetProperty(record => record.ExpiresAt, expiresAt),
                    cancellationToken)
                .ConfigureAwait(false);
            if (takenOver == 1)
            {
                _reservedTokens.Add(token);
                return IdempotencyRecordReservation.Reserved(token);
            }

            var existing = await Records(hash)
                .Select(record => new { record.Namespace, record.Scope, record.Key, record.Fingerprint, record.Payload, record.ExpiresAt })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
            if (existing is null)
                continue;
            if (!string.Equals(existing.Namespace, key.Namespace, StringComparison.Ordinal)
                || !string.Equals(existing.Scope, key.Scope ?? IdempotencyRecord.GlobalScope, StringComparison.Ordinal)
                || !string.Equals(existing.Key, key.Key, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The {nameof(IdempotencyRecord.KeyHash)} of the idempotency key belongs to a record of another key. The record is not used.");
            }

            if (existing.ExpiresAt <= now)
                continue;
            if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                return IdempotencyRecordReservation.FingerprintMismatch;
            return existing.Payload is null
                ? IdempotencyRecordReservation.InFlight
                : IdempotencyRecordReservation.Completed(existing.Payload);
        }

        throw new InvalidOperationException(
            $"The idempotency key could not be reserved after {MaxReserveAttempts} attempts: its row kept changing between the insert and the read. The store needs the READ COMMITTED isolation level.");
    }

    /// <inheritdoc />
    public async ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);

        if (_reservedTokens.Remove(token))
            return await CompleteCoreAsync(key, token, fingerprint, payload, retention, cancellationToken).ConfigureAwait(false);

        IDbContextTransaction? transaction = _context.Database.CurrentTransaction;
        bool savepoint = transaction is null
            ? System.Transactions.Transaction.Current is not null && IdempotencyRecordSql.For(_context).AmbientSavepoints
            : transaction.SupportsSavepoints;
        if (!savepoint)
            return await CompleteCoreAsync(key, token, fingerprint, payload, retention, cancellationToken).ConfigureAwait(false);

        try
        {
            await CreateResultSavepointAsync(transaction, cancellationToken).ConfigureAwait(false);
        }
        catch (DbException) when (transaction is null)
        {
            return await CompleteCoreAsync(key, token, fingerprint, payload, retention, cancellationToken).ConfigureAwait(false);
        }

        bool completed;
        try
        {
            completed = await CompleteCoreAsync(key, token, fingerprint, payload, retention, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is DbException || exception.InnerException is DbException)
        {
            _ = await TryRollbackToResultSavepointAsync(transaction).ConfigureAwait(false);
            throw;
        }

        await ReleaseResultSavepointAsync(transaction, cancellationToken).ConfigureAwait(false);
        return completed;
    }

    /// <inheritdoc />
    public async ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrEmpty(token);

        _reservedTokens.Remove(token);
        int released = await Records(key, IdempotencyRecord.HashKey(key))
            .Where(record => record.Token == token && record.Payload == null)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        return released == 1;
    }

    private Task CreateResultSavepointAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null
            ? _context.Database.ExecuteSqlRawAsync("SAVEPOINT " + ResultSavepointName, cancellationToken)
            : transaction.CreateSavepointAsync(ResultSavepointName, cancellationToken);

    /// <summary>
    /// Rolls back to the result savepoint; <see langword="false"/> when the rollback itself failed. The failure is not
    /// thrown, so that the caller rethrows the database error that made the write fail instead of the rollback's.
    /// </summary>
    private async Task<bool> TryRollbackToResultSavepointAsync(IDbContextTransaction? transaction)
    {
        try
        {
            await (transaction is null
                ? _context.Database.ExecuteSqlRawAsync("ROLLBACK TO SAVEPOINT " + ResultSavepointName, CancellationToken.None)
                : transaction.RollbackToSavepointAsync(ResultSavepointName, CancellationToken.None)).ConfigureAwait(false);
            return true;
        }
        catch (Exception rollbackFailure) when (rollbackFailure is DbException or InvalidOperationException)
        {
            return false;
        }
    }

    private Task ReleaseResultSavepointAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null
            ? _context.Database.ExecuteSqlRawAsync("RELEASE SAVEPOINT " + ResultSavepointName, cancellationToken)
            : transaction.ReleaseSavepointAsync(ResultSavepointName, cancellationToken);

    private async ValueTask<bool> CompleteCoreAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken)
    {
        byte[] hash = IdempotencyRecord.HashKey(key);
        byte[] stored = payload.ToArray();
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        DateTime expiresAt = now + retention;

        int completed = await Records(key, hash)
            .Where(record => record.Token == token && record.Payload == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(record => record.Fingerprint, fingerprint)
                    .SetProperty(record => record.Payload, stored)
                    .SetProperty(record => record.ExpiresAt, expiresAt),
                cancellationToken)
            .ConfigureAwait(false);
        if (completed == 1)
            return true;

        int takenOver = await Records(key, hash)
            .Where(record => record.ExpiresAt <= now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(record => record.Token, token)
                    .SetProperty(record => record.Fingerprint, fingerprint)
                    .SetProperty(record => record.Payload, stored)
                    .SetProperty(record => record.ExpiresAt, expiresAt),
                cancellationToken)
            .ConfigureAwait(false);
        if (takenOver == 1)
            return true;

        return await TryInsertAsync(key, hash, token, fingerprint, stored, expiresAt, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes every record whose lease or retention has ended, in one statement on the context's connection (inside
    /// its current transaction when there is one). An expired record already counts as absent, so purging changes no
    /// decision; it only keeps the table small. Run it periodically, for example from a hosted service.
    /// </summary>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>The number of deleted records.</returns>
    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        return _context.Set<IdempotencyRecord>()
            .IgnoreQueryFilters()
            .Where(record => record.ExpiresAt <= now)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private IQueryable<IdempotencyRecord> Records(byte[] hash) =>
        _context.Set<IdempotencyRecord>()
            .IgnoreQueryFilters()
            .Where(record => record.KeyHash == hash);

    /// <summary>
    /// The row of <paramref name="hash"/>, narrowed by the text of <paramref name="key"/> so that a write never changes
    /// the row of another key, whatever the collation of the text columns.
    /// </summary>
    private IQueryable<IdempotencyRecord> Records(IdempotencyRecordKey key, byte[] hash)
    {
        string @namespace = key.Namespace;
        string scope = key.Scope ?? IdempotencyRecord.GlobalScope;
        string value = key.Key;
        return Records(hash).Where(record => record.Namespace == @namespace && record.Scope == scope && record.Key == value);
    }

    /// <summary>
    /// Inserts the row; <see langword="false"/> when the key already has one. Without <c>ON CONFLICT</c> the unique
    /// violation is caught, the transaction is rolled back to the savepoint taken before the insert, and the row is read
    /// to tell a conflict from any other failure.
    /// </summary>
    private async Task<bool> TryInsertAsync(
        IdempotencyRecordKey key,
        byte[] hash,
        string token,
        string fingerprint,
        byte[]? payload,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var sql = IdempotencyRecordSql.For(_context);
        string scope = key.Scope ?? IdempotencyRecord.GlobalScope;
        object[] parameters = payload is null
            ? [hash, key.Namespace, scope, key.Key, token, fingerprint, expiresAt]
            : [hash, key.Namespace, scope, key.Key, token, fingerprint, payload, expiresAt];

        string? orNothing = payload is null ? sql.InsertInFlightOrNothing : sql.InsertCompletedOrNothing;
        if (orNothing is not null && !_plainInsert)
            return await _context.Database.ExecuteSqlRawAsync(orNothing, parameters, cancellationToken).ConfigureAwait(false) == 1;

        IDbContextTransaction? transaction = _context.Database.CurrentTransaction;
        bool savepoint = transaction is { SupportsSavepoints: true };
        if (savepoint)
            await transaction!.CreateSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);

        try
        {
            await _context.Database
                .ExecuteSqlRawAsync(payload is null ? sql.InsertInFlight : sql.InsertCompleted, parameters, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (DbException)
        {
            if (savepoint)
                await transaction!.RollbackToSavepointAsync(SavepointName, CancellationToken.None).ConfigureAwait(false);
            if (await Records(hash).AnyAsync(CancellationToken.None).ConfigureAwait(false))
                return false;
            throw;
        }

        if (savepoint)
            await transaction!.ReleaseSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
