using System.Text.Json;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern.Interfaces;

using Mediator;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Runs an <see cref="IIdempotentRequest"/> at most once per key and caller through <see cref="IIdempotencyExecutor"/>,
/// and replays the stored response to a retry. The record namespace is the full name of <typeparamref name="TRequest"/>.
/// <list type="bullet">
/// <item><description>A missing or invalid key, an unresolved scope with <see cref="UnscopedKeyBehavior.Reject"/>, a key still in flight and a key reused with another request return a failed <c>Result</c> with an <see cref="IdempotencyErrorCodes"/> code. A response type that is not a <c>Result</c> throws <see cref="EnhancedValidationException"/> instead.</description></item>
/// <item><description>A successful response is stored. A failed <c>Result</c> of an <see cref="ITransactionalRequest"/> releases the key, because its transaction rolled back. Any other failed <c>Result</c> is stored when <see cref="IdempotencyBehaviorOptions.StoreFailures"/> is on: for the failure retention when its first error is <see cref="ErrorType.Unexpected"/>, else as long as a success.</description></item>
/// <item><description>An exception thrown by the handler propagates and keeps the key in flight until its lease expires.</description></item>
/// <item><description>In <see cref="IdempotencyMode.Off"/> the behavior only calls the next step. In <see cref="IdempotencyMode.Observe"/> it never blocks a request: what it would reject is logged and the request runs, and a scope or fingerprint that cannot be resolved runs the request without idempotency instead of throwing.</description></item>
/// <item><description>The mode is read from <see cref="IdempotencyExecutorOptions"/> on every request, so a mode changed at runtime applies to the next one. Decisions are counted with the <c>mediator</c> source and the full name of <typeparamref name="TRequest"/> as the target.</description></item>
/// </list>
/// </summary>
public sealed partial class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentRequest, IMessage
{
    private static readonly string RequestType = typeof(TRequest).FullName ?? typeof(TRequest).Name;
    private static RequestSettings? s_settings;

    private readonly IIdempotencyExecutor _executor;
    private readonly IOptionsMonitor<IdempotencyExecutorOptions> _executorOptions;
    private readonly IdempotencyBehaviorOptions _options;
    private readonly ILogger _logger;
    private readonly IIdempotencyScopeAccessor? _scopeAccessor;
    private readonly IdempotencyExecutionOptions? _requestOptions;
    private readonly ResponseCodec _storeFailures;
    private readonly ResponseCodec _releaseFailures;

    /// <summary>Creates the behavior.</summary>
    /// <param name="executor">Runs the request at most once.</param>
    /// <param name="executorOptions">The defaults and profiles of the executor, read on every request.</param>
    /// <param name="options">The settings of this behavior.</param>
    /// <param name="logger">Logs what Observe mode would reject and the requests that run without idempotency.</param>
    /// <param name="scopeAccessor">Gives the current caller to <see cref="IdempotencyBehaviorOptions.ScopeResolver"/>; without one the resolver gets <see langword="null"/>.</param>
    public IdempotencyBehavior(
        IIdempotencyExecutor executor,
        IOptionsMonitor<IdempotencyExecutorOptions> executorOptions,
        IdempotencyBehaviorOptions options,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger,
        IIdempotencyScopeAccessor? scopeAccessor = null)
    {
        _executor = executor;
        _executorOptions = executorOptions;
        _options = options;
        _logger = logger;
        _scopeAccessor = scopeAccessor;
        _requestOptions = options.Find(typeof(TRequest));
        _storeFailures = new ResponseCodec(options.SerializerOptions, releaseFailures: false);
        _releaseFailures = new ResponseCodec(options.SerializerOptions, releaseFailures: true);
    }

    /// <inheritdoc />
    public ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        IdempotencyMode mode = _executorOptions.CurrentValue.ResolveMode(_requestOptions);
        return mode == IdempotencyMode.Off
            ? next(message, cancellationToken)
            : HandleAsync(message, next, mode, cancellationToken);
    }

    private async ValueTask<TResponse> HandleAsync(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        IdempotencyMode mode,
        CancellationToken cancellationToken)
    {
        bool observe = mode == IdempotencyMode.Observe;
        string key = message.IdempotencyKey;
        if (!IdempotencyKeys.IsValid(key))
        {
            Error error = string.IsNullOrEmpty(key)
                ? Error.Validation(IdempotencyErrorCodes.KeyMissing, "The request has no idempotency key.")
                : Error.Validation(IdempotencyErrorCodes.KeyInvalid,
                    $"An idempotency key must be 1 to {IdempotencyKeys.MaxKeyLength} printable ASCII characters other than space, '\"' and '\\'.");
            if (!observe)
                return Failure(error);
            if (error.Code == IdempotencyErrorCodes.KeyMissing)
                LogObserveWouldRejectKeyMissing(_logger, RequestType);
            else
                LogObserveWouldRejectKeyInvalid(_logger, RequestType);
            return await next(message, cancellationToken).ConfigureAwait(false);
        }

        IdempotencyScope? scope;
        string fingerprint;
        try
        {
            scope = message.IdempotencyScope ?? _options.ScopeResolver.Resolve(_scopeAccessor?.Principal);
            fingerprint = message.IdempotencyFingerprint ?? Fingerprint(message, _options.SerializerOptions);
        }
        catch (Exception exception) when (observe && !cancellationToken.IsCancellationRequested)
        {
            LogObserveResolutionFailed(_logger, exception, RequestType);
            return await next(message, cancellationToken).ConfigureAwait(false);
        }

        if (scope is null)
        {
            switch (_options.UnscopedKeyBehavior)
            {
                case UnscopedKeyBehavior.Global:
                    scope = IdempotencyScope.Global;
                    break;
                case UnscopedKeyBehavior.Skip:
                    IdempotencyMetrics.Record(IdempotencyMetrics.Unscoped, mode, IdempotencyMetrics.MediatorSource, RequestType);
                    LogUnscopedKeySkipped(_logger, RequestType);
                    return await next(message, cancellationToken).ConfigureAwait(false);
                case UnscopedKeyBehavior.Reject:
                    IdempotencyMetrics.Record(IdempotencyMetrics.Unscoped, mode, IdempotencyMetrics.MediatorSource, RequestType);
                    if (!observe)
                        return Failure(Error.Validation(IdempotencyErrorCodes.ScopeRequired,
                            "An idempotency key needs a caller whose scope can be resolved, such as an authenticated user or client, or a scope set on the request."));
                    LogObserveWouldRejectScopeRequired(_logger, RequestType);
                    return await next(message, cancellationToken).ConfigureAwait(false);
                default:
                    throw new InvalidOperationException($"IdempotencyBehaviorOptions.UnscopedKeyBehavior ({_options.UnscopedKeyBehavior}) is not a defined value.");
            }
        }

        IdempotencyRecordKey recordKey = new(RequestType, scope, key);
        ResponseCodec codec = message is ITransactionalRequest || !_options.StoreFailures ? _releaseFailures : _storeFailures;
        IdempotencyExecution<TResponse> execution = await ExecuteAsync(
            recordKey,
            fingerprint,
            SettingsFor(message, mode),
            message,
            next,
            codec,
            cancellationToken).ConfigureAwait(false);

        return execution.Status switch
        {
            IdempotencyExecutionStatus.Executed or IdempotencyExecutionStatus.Replayed => execution.Value!,
            IdempotencyExecutionStatus.InFlight => Failure(Error.Conflict(IdempotencyErrorCodes.InFlight,
                "A request with this idempotency key is still being processed. Retry later.")),
            IdempotencyExecutionStatus.KeyReused => Failure(Error.Validation(IdempotencyErrorCodes.KeyReused,
                "This idempotency key was already used for a different request.")),
            _ => throw new InvalidOperationException($"The idempotency executor returned the unknown status {execution.Status}."),
        };
    }

    /// <summary>
    /// The settings of the request type with the retention and lease of <paramref name="message"/>, pinned to the mode
    /// the behavior already resolved, so a mode changed while the request runs cannot make the executor reject what
    /// Observe mode let through.
    /// </summary>
    private IdempotencyExecutionOptions SettingsFor(TRequest message, IdempotencyMode mode)
    {
        if (message.IdempotencyRetention is not null || message.IdempotencyLease is not null)
            return Settings(_requestOptions, mode, message.IdempotencyRetention, message.IdempotencyLease);

        RequestSettings settings = SharedSettings(_requestOptions);
        return mode == IdempotencyMode.Observe ? settings.Observe : settings.Enforce;
    }

    private static RequestSettings SharedSettings(IdempotencyExecutionOptions? requestOptions)
    {
        if (s_settings is { } cached && ReferenceEquals(cached.Source, requestOptions) && cached.Matches(requestOptions))
            return cached;

        RequestSettings settings = new(requestOptions);
        s_settings = settings;
        return settings;
    }

    private static IdempotencyExecutionOptions Settings(
        IdempotencyExecutionOptions? requestOptions,
        IdempotencyMode mode,
        TimeSpan? retention,
        TimeSpan? lease)
    {
        return new()
        {
            Profile = requestOptions?.Profile,
            Mode = mode,
            Retention = retention ?? requestOptions?.Retention,
            Lease = lease ?? requestOptions?.Lease,
            FailureRetention = requestOptions?.FailureRetention,
            StoreKey = requestOptions?.StoreKey,
            Source = IdempotencyMetrics.MediatorSource,
        };
    }

    /// <summary>
    /// Runs <paramref name="next"/> through the executor. The built-in executor takes the request and the next step as
    /// state, so a request allocates no closure; another executor gets the operation as a delegate.
    /// </summary>
    private ValueTask<IdempotencyExecution<TResponse>> ExecuteAsync(
        IdempotencyRecordKey recordKey,
        string fingerprint,
        IdempotencyExecutionOptions settings,
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        ResponseCodec codec,
        CancellationToken cancellationToken)
    {
        if (_executor is IdempotencyExecutor executor)
        {
            return executor.ExecuteAsync(
                recordKey,
                fingerprint,
                settings,
                (Message: message, Next: next),
                static (state, token) => state.Next(state.Message, token),
                codec,
                cancellationToken);
        }

        return _executor.ExecuteAsync(recordKey, fingerprint, settings, Operation(message, next), codec, cancellationToken);
    }

    private static Func<CancellationToken, ValueTask<TResponse>> Operation(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next) =>
        token => next(message, token);

    private static string Fingerprint(TRequest message, JsonSerializerOptions? serializerOptions)
    {
#if NET5_0_OR_GREATER
        return Sha256JsonStream.HashToHex(message, serializerOptions);
#else
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(json);
        char[] hex = new char[hash.Length * 2];
        for (int index = 0; index < hash.Length; index++)
        {
            hex[index * 2] = "0123456789ABCDEF"[hash[index] >> 4];
            hex[index * 2 + 1] = "0123456789ABCDEF"[hash[index] & 0xF];
        }

        return new string(hex);
#endif
    }

    private static TResponse Failure(Error error) =>
        FailureResponse<TResponse>.Factory is { } factory
            ? factory.CreateFailure([error])
            : throw new EnhancedValidationException([error]);

    [LoggerMessage(EventId = 7001, EventName = IdempotencyErrorCodes.UnscopedKeySkipped, Level = LogLevel.Warning,
        Message = "The idempotency key of {RequestType} has no scope, so the request runs without idempotency.")]
    private static partial void LogUnscopedKeySkipped(ILogger logger, string requestType);

    [LoggerMessage(EventId = 7013, EventName = "Idempotency.Observe.WouldRejectScopeRequired", Level = LogLevel.Information,
        Message = "Observe mode: {RequestType} would be rejected because the scope of its idempotency key cannot be resolved; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectScopeRequired(ILogger logger, string requestType);

    [LoggerMessage(EventId = 7014, EventName = "Idempotency.Observe.ResolutionFailed", Level = LogLevel.Warning,
        Message = "Observe mode: the idempotency scope or fingerprint of {RequestType} could not be resolved; it runs without idempotency.")]
    private static partial void LogObserveResolutionFailed(ILogger logger, Exception exception, string requestType);

    [LoggerMessage(EventId = 7016, EventName = "Idempotency.Observe.WouldRejectKeyMissing", Level = LogLevel.Information,
        Message = "Observe mode: {RequestType} would be rejected because it has no idempotency key; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectKeyMissing(ILogger logger, string requestType);

    [LoggerMessage(EventId = 7017, EventName = "Idempotency.Observe.WouldRejectKeyInvalid", Level = LogLevel.Information,
        Message = "Observe mode: {RequestType} would be rejected because its idempotency key is invalid; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectKeyInvalid(ILogger logger, string requestType);

    /// <summary>
    /// The settings of a request that sets no retention or lease of its own, built once per request type and shared:
    /// the executor only reads them.
    /// </summary>
    private sealed class RequestSettings(IdempotencyExecutionOptions? source)
    {
        public IdempotencyExecutionOptions? Source { get; } = source;

        public IdempotencyExecutionOptions Observe { get; } = Settings(source, IdempotencyMode.Observe, null, null);

        public IdempotencyExecutionOptions Enforce { get; } = Settings(source, IdempotencyMode.Enforce, null, null);

        public bool Matches(IdempotencyExecutionOptions? options) =>
            options is null
            || Observe.Profile == options.Profile
            && Observe.Retention == options.Retention
            && Observe.Lease == options.Lease
            && Observe.FailureRetention == options.FailureRetention
            && Observe.StoreKey == options.StoreKey;
    }

    private sealed class ResponseCodec(JsonSerializerOptions? serializerOptions, bool releaseFailures) : IIdempotencyResultCodec<TResponse>
    {
        public ReadOnlyMemory<byte> Serialize(TResponse result) => JsonSerializer.SerializeToUtf8Bytes(result, serializerOptions);

        public TResponse Deserialize(ReadOnlyMemory<byte> payload) =>
            JsonSerializer.Deserialize<TResponse>(payload.Span, serializerOptions)
            ?? throw new InvalidOperationException($"The stored response of {RequestType} is empty.");

        public IdempotencyResultHandling Classify(TResponse result)
        {
            if (result is not IResultBase { IsFailure: true } failure)
                return IdempotencyResultHandling.Store;
            if (releaseFailures)
                return IdempotencyResultHandling.Release;
            return failure.FirstError.Type == ErrorType.Unexpected
                ? IdempotencyResultHandling.StoreAsFailure
                : IdempotencyResultHandling.Store;
        }
    }
}
