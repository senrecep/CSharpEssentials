using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// The <see cref="IIdempotencyExecutor"/> over an <see cref="IIdempotencyRecordStore"/>. It reads
/// <see cref="IdempotencyExecutorOptions"/> on every operation, so a mode changed at runtime applies to the next one.
/// Logs name the record namespace and a hash of the key, never the key or the scope. Decisions are counted in
/// <see cref="IdempotencyTelemetry.DecisionsInstrumentName"/> with the record namespace as <c>target</c>.
/// In <see cref="IdempotencyMode.Observe"/> an <see cref="ITransactionalIdempotencyRecordStore"/> reserves and releases
/// through its <see cref="ITransactionalIdempotencyRecordStore.Detached"/> view, so the reservation never waits on or
/// fails the business transaction. A result to store releases that reservation and is then written through the store
/// itself, in the business transaction: the record exists only when the business work committed, so a retry after the
/// switch to <see cref="IdempotencyMode.Enforce"/> replays committed work and runs again what rolled back.
/// </summary>
public sealed partial class IdempotencyExecutor : IIdempotencyExecutor
{
    private readonly IIdempotencyRecordStore _store;
    private readonly IOptionsMonitor<IdempotencyExecutorOptions> _options;
    private readonly IServiceProvider? _services;
    private readonly ILogger _logger;

    /// <param name="store">The default store.</param>
    /// <param name="options">Default settings and profiles, read on every operation.</param>
    /// <param name="services">Resolves keyed stores named by <see cref="IdempotencyExecutionOptions.StoreKey"/>.</param>
    /// <param name="logger">Logs decisions and store failures.</param>
    public IdempotencyExecutor(
        IIdempotencyRecordStore store,
        IOptionsMonitor<IdempotencyExecutorOptions> options,
        IServiceProvider? services = null,
        ILogger<IdempotencyExecutor>? logger = null)
    {
        Guard.NotNull(store);
        Guard.NotNull(options);
        _store = store;
        _options = options;
        _services = services;
        _logger = logger ?? (ILogger)NullLogger.Instance;
    }

    /// <inheritdoc />
    public async ValueTask<IdempotencyExecution<TResult>> ExecuteAsync<TResult>(
        IdempotencyRecordKey key,
        string fingerprint,
        IdempotencyExecutionOptions? options,
        Func<CancellationToken, ValueTask<TResult>> operation,
        IIdempotencyResultCodec<TResult> codec,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(fingerprint);
        Guard.NotNull(operation);
        Guard.NotNull(codec);

        var settings = IdempotencyExecutionSettings.Resolve(_options.CurrentValue, options, $"the idempotent operation in '{key.Namespace}'");
        if (settings.Mode == IdempotencyMode.Off)
            return new(IdempotencyExecutionStatus.Executed, await operation(cancellationToken).ConfigureAwait(false));

        IIdempotencyRecordStore store = ResolveStore(settings.StoreKey);
        if (settings.Mode == IdempotencyMode.Observe)
            return await ObserveAsync(store, key, fingerprint, settings, operation, codec, cancellationToken).ConfigureAwait(false);

        IdempotencyRecordReservation reservation =
            await store.TryReserveAsync(key, fingerprint, settings.Lease, cancellationToken).ConfigureAwait(false);
        Record(reservation.Status, settings, key);
        return reservation.Status switch
        {
            IdempotencyReservationStatus.Reserved =>
                await RunAsync(store, store, key, reservation.Token!, fingerprint, settings, operation, codec, cancellationToken).ConfigureAwait(false),
            IdempotencyReservationStatus.Completed => Replay(key, reservation.Payload.GetValueOrDefault(), codec),
            IdempotencyReservationStatus.InFlight => new(IdempotencyExecutionStatus.InFlight, default),
            IdempotencyReservationStatus.FingerprintMismatch => new(IdempotencyExecutionStatus.KeyReused, default),
            _ => throw new InvalidOperationException($"The idempotency store returned the unknown reservation status {reservation.Status}."),
        };
    }

    private static void Record(IdempotencyReservationStatus status, IdempotencyExecutionSettings settings, IdempotencyRecordKey key)
    {
        string? decision = status switch
        {
            IdempotencyReservationStatus.Reserved => IdempotencyMetrics.Reserved,
            IdempotencyReservationStatus.Completed => IdempotencyMetrics.Replay,
            IdempotencyReservationStatus.InFlight => IdempotencyMetrics.InFlight,
            IdempotencyReservationStatus.FingerprintMismatch => IdempotencyMetrics.KeyReused,
            _ => null,
        };
        if (decision is not null)
            IdempotencyMetrics.Record(decision, settings.Mode, settings.Source, key.Namespace);
    }

    private async ValueTask<IdempotencyExecution<TResult>> ObserveAsync<TResult>(
        IIdempotencyRecordStore store,
        IdempotencyRecordKey key,
        string fingerprint,
        IdempotencyExecutionSettings settings,
        Func<CancellationToken, ValueTask<TResult>> operation,
        IIdempotencyResultCodec<TResult> codec,
        CancellationToken cancellationToken)
    {
        IIdempotencyRecordStore reservations = store is ITransactionalIdempotencyRecordStore transactional ? transactional.Detached : store;
        IdempotencyRecordReservation? reservation = null;
        try
        {
            reservation = await reservations.TryReserveAsync(key, fingerprint, settings.Lease, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogObserveReserveFailed(_logger, exception, key.Namespace, new IdempotencyKeyHash(key));
        }

        if (reservation is not null)
            Record(reservation.Status, settings, key);

        switch (reservation?.Status)
        {
            case IdempotencyReservationStatus.Reserved:
                return await RunAsync(reservations, store, key, reservation.Token!, fingerprint, settings, operation, codec, cancellationToken).ConfigureAwait(false);
            case IdempotencyReservationStatus.Completed:
                LogWouldReplay(_logger, key.Namespace, new IdempotencyKeyHash(key));
                break;
            case IdempotencyReservationStatus.InFlight:
                LogWouldRejectInFlight(_logger, key.Namespace, new IdempotencyKeyHash(key));
                break;
            case IdempotencyReservationStatus.FingerprintMismatch:
                LogWouldRejectKeyReused(_logger, key.Namespace, new IdempotencyKeyHash(key));
                break;
            default:
                break;
        }

        return new(IdempotencyExecutionStatus.Executed, await operation(cancellationToken).ConfigureAwait(false));
    }

    private async ValueTask<IdempotencyExecution<TResult>> RunAsync<TResult>(
        IIdempotencyRecordStore store,
        IIdempotencyRecordStore results,
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        IdempotencyExecutionSettings settings,
        Func<CancellationToken, ValueTask<TResult>> operation,
        IIdempotencyResultCodec<TResult> codec,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await TryReleaseAsync(store, key, token).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        TResult result;
        try
        {
            result = await operation(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            LogKeptInFlight(_logger, key.Namespace, new IdempotencyKeyHash(key));
            throw;
        }

        await SettleAsync(store, results, key, token, fingerprint, settings, result, codec).ConfigureAwait(false);
        return new(IdempotencyExecutionStatus.Executed, result);
    }

    /// <summary>
    /// Stores, stores as a failure or releases the result of an operation that ran. The work is done, so the store
    /// calls ignore cancellation, and a failure here is logged instead of thrown: the caller gets its result and the
    /// key stays in flight until its lease expires. A result is written to <paramref name="results"/>; when that is not
    /// the <paramref name="store"/> that holds the reservation, the reservation is released first, so the result is
    /// written as the record of a key that is gone.
    /// </summary>
    private async ValueTask SettleAsync<TResult>(
        IIdempotencyRecordStore store,
        IIdempotencyRecordStore results,
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        IdempotencyExecutionSettings settings,
        TResult result,
        IIdempotencyResultCodec<TResult> codec)
    {
        try
        {
            IdempotencyResultHandling handling = codec.Classify(result);
            if (handling != IdempotencyResultHandling.Release && !ReferenceEquals(store, results))
                await TryReleaseAsync(store, key, token).ConfigureAwait(false);
            bool settled = handling switch
            {
                IdempotencyResultHandling.Store => await results
                    .CompleteAsync(key, token, fingerprint, codec.Serialize(result), settings.Retention, CancellationToken.None)
                    .ConfigureAwait(false),
                IdempotencyResultHandling.StoreAsFailure => await results
                    .CompleteAsync(key, token, fingerprint, codec.Serialize(result), settings.FailureRetention, CancellationToken.None)
                    .ConfigureAwait(false),
                IdempotencyResultHandling.Release => await store.ReleaseAsync(key, token, CancellationToken.None).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"The result codec returned the unknown handling {handling}."),
            };
            if (!settled)
                LogStaleReservation(_logger, key.Namespace, new IdempotencyKeyHash(key));
            else
                IdempotencyMetrics.Record(
                    handling == IdempotencyResultHandling.Release ? IdempotencyMetrics.Released : IdempotencyMetrics.Stored,
                    settings.Mode,
                    settings.Source,
                    key.Namespace);
        }
        catch (Exception exception)
        {
            LogSettleFailed(_logger, exception, key.Namespace, new IdempotencyKeyHash(key));
        }
    }

    private static IdempotencyExecution<TResult> Replay<TResult>(IdempotencyRecordKey key, ReadOnlyMemory<byte> payload, IIdempotencyResultCodec<TResult> codec)
    {
        TResult value;
        try
        {
            value = codec.Deserialize(payload);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"The idempotent result stored in '{key.Namespace}' cannot be read; the operation was not run again.", exception);
        }

        return new(IdempotencyExecutionStatus.Replayed, value);
    }

    private async ValueTask TryReleaseAsync(IIdempotencyRecordStore store, IdempotencyRecordKey key, string token)
    {
        try
        {
            await store.ReleaseAsync(key, token, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogReleaseFailed(_logger, exception, key.Namespace, new IdempotencyKeyHash(key));
        }
    }

    private IIdempotencyRecordStore ResolveStore(string? storeKey)
    {
        if (storeKey is null)
            return _store;
        if (_services is null)
            throw new InvalidOperationException(
                $"The idempotency store '{storeKey}' cannot be resolved: the executor was created without an IServiceProvider.");
        return _services.GetRequiredKeyedService<IIdempotencyRecordStore>(storeKey);
    }

    [LoggerMessage(EventId = 7003, EventName = "Idempotency.KeptInFlight", Level = LogLevel.Warning,
        Message = "The idempotent operation in {Namespace} (key hash {KeyHash}) threw after it started; its key stays in flight until its lease expires.")]
    private static partial void LogKeptInFlight(ILogger logger, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7005, EventName = "Idempotency.StoreFailed", Level = LogLevel.Error,
        Message = "Storing the outcome of the idempotent operation in {Namespace} (key hash {KeyHash}) failed; its key stays in flight until its lease expires.")]
    private static partial void LogSettleFailed(ILogger logger, Exception exception, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7006, EventName = "Idempotency.StaleReservation", Level = LogLevel.Warning,
        Message = "The idempotent operation in {Namespace} (key hash {KeyHash}) outlived its reservation, which expired or was taken over; its outcome was not stored. Make the lease longer than the operation's timeout.")]
    private static partial void LogStaleReservation(ILogger logger, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7007, EventName = "Idempotency.ReleaseFailed", Level = LogLevel.Warning,
        Message = "Releasing the idempotency key in {Namespace} (key hash {KeyHash}) failed; it stays in flight until its lease expires.")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7010, EventName = "Idempotency.Observe.WouldReplay", Level = LogLevel.Information,
        Message = "Observe mode: the idempotent operation in {Namespace} (key hash {KeyHash}) has a stored result that Enforce would replay; it runs again.")]
    private static partial void LogWouldReplay(ILogger logger, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7011, EventName = "Idempotency.Observe.WouldRejectInFlight", Level = LogLevel.Information,
        Message = "Observe mode: the key of the idempotent operation in {Namespace} (key hash {KeyHash}) is in flight and Enforce would reject it; it runs anyway.")]
    private static partial void LogWouldRejectInFlight(ILogger logger, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7012, EventName = "Idempotency.Observe.WouldRejectKeyReused", Level = LogLevel.Information,
        Message = "Observe mode: the key of the idempotent operation in {Namespace} (key hash {KeyHash}) was used with another fingerprint and Enforce would reject it; it runs anyway.")]
    private static partial void LogWouldRejectKeyReused(ILogger logger, string @namespace, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7015, EventName = "Idempotency.Observe.ReserveFailed", Level = LogLevel.Warning,
        Message = "Observe mode: reserving the key of the idempotent operation in {Namespace} (key hash {KeyHash}) failed; it runs without idempotency.")]
    private static partial void LogObserveReserveFailed(ILogger logger, Exception exception, string @namespace, IdempotencyKeyHash keyHash);
}
