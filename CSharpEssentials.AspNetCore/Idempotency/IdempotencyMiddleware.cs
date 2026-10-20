using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Handles <c>Idempotency-Key</c> for endpoints that carry <see cref="IdempotentAttribute"/> metadata.
/// The fingerprint is a SHA-256 of the method, path, query string and body (the body is buffered to compute it).
/// The options are read once per request from <see cref="IOptionsMonitor{TOptions}"/>, so a reload applies to the next
/// request; the endpoint's mode (<see cref="IdempotencyEndpointOptions.Mode"/>, its profile's, else
/// <see cref="IdempotencyOptions.Mode"/>) decides whether the key is ignored, observed or enforced. In Observe an
/// <see cref="ITransactionalIdempotencyRecordStore"/> reserves and releases through its
/// <see cref="ITransactionalIdempotencyRecordStore.Detached"/> view, so the reservation never joins the request's
/// transaction. A response to store releases that reservation and is then written through the store itself, in the
/// transaction open around <c>UseIdempotency</c> if there is one: the record exists only when that transaction
/// commits, so a retry after the switch to Enforce never replays a response whose work rolled back.
/// </summary>
internal sealed partial class IdempotencyMiddleware(
    RequestDelegate next,
    IOptionsMonitor<IdempotencyOptions> optionsMonitor,
    ILogger<IdempotencyMiddleware> logger)
{
    internal const string CodeKey = "code";

    private readonly ConditionalWeakTable<IdempotencyOptions, ConditionalWeakTable<Endpoint, IdempotencyEndpointSettings>> _endpointSettings = [];

    public async Task InvokeAsync(HttpContext context, IIdempotencyRecordStore defaultStore)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IdempotentAttribute>() is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyOptions options = optionsMonitor.CurrentValue;
        if (!options.Methods.Contains(context.Request.Method))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyEndpointSettings settings = GetSettings(context, endpoint, options);
        if (settings.Mode == IdempotencyMode.Off)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        bool observe = settings.Mode == IdempotencyMode.Observe;
        string target = (endpoint as RouteEndpoint)?.RoutePattern.RawText ?? endpoint.DisplayName ?? context.Request.Path.ToString();
        StringValues header = context.Request.Headers[options.HeaderName];
        if (header.Count == 0)
        {
            if (settings.RequireKey && !observe)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Missing idempotency key", Error.Validation(
                    IdempotencyErrorCodes.KeyMissing,
                    $"This endpoint requires the '{options.HeaderName}' header.")).ConfigureAwait(false);
                return;
            }

            if (settings.RequireKey)
                LogObserveWouldRejectKeyMissing(logger, context.Request.Path);
            await next(context).ConfigureAwait(false);
            return;
        }

        if (header.Count > 1 || !IdempotencyKeyHeader.TryParse(header[0], options.MaxKeyLength, out string? key))
        {
            if (observe)
            {
                LogObserveWouldRejectKeyInvalid(logger, context.Request.Path);
                await next(context).ConfigureAwait(false);
                return;
            }

            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid idempotency key", Error.Validation(
                IdempotencyErrorCodes.KeyInvalid,
                $"Send exactly one '{options.HeaderName}' header of 1 to {options.MaxKeyLength} printable ASCII characters other than space, '\"' and '\\', optionally as a quoted string.")).ConfigureAwait(false);
            return;
        }

        if (!TryResolveScope(context, options, observe, out IdempotencyScope? scope))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (scope is null && settings.UnscopedKeyBehavior == UnscopedKeyBehavior.Global)
            scope = IdempotencyScope.Global;
        else if (scope is null && settings.UnscopedKeyBehavior == UnscopedKeyBehavior.Reject)
        {
            IdempotencyMetrics.Record(IdempotencyMetrics.Unscoped, settings.Mode, IdempotencyMetrics.HttpSource, target);
            if (observe)
            {
                LogObserveWouldRejectScopeRequired(logger, context.Request.Path);
                await next(context).ConfigureAwait(false);
                return;
            }

            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Idempotency scope required", Error.Validation(
                IdempotencyErrorCodes.ScopeRequired,
                "An idempotency key needs a caller whose scope can be resolved, such as an authenticated user or client.")).ConfigureAwait(false);
            return;
        }
        else if (scope is null)
        {
            IdempotencyMetrics.Record(IdempotencyMetrics.Unscoped, settings.Mode, IdempotencyMetrics.HttpSource, target);
            LogUnscopedKeySkipped(logger, context.Request.Path);
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyRecordKey recordKey = new(options.Namespace, scope.Value, key);
        IIdempotencyRecordStore store;
        IIdempotencyRecordStore results;
        string? fingerprint;
        try
        {
            store = results = settings.StoreKey is { } storeKey
                ? context.RequestServices.GetRequiredKeyedService<IIdempotencyRecordStore>(storeKey)
                : defaultStore;
            if (observe && store is ITransactionalIdempotencyRecordStore transactional)
                store = transactional.Detached;
            fingerprint = await ComputeFingerprintAsync(context.Request, context.RequestAborted).ConfigureAwait(false);
        }
        catch (Exception exception) when (observe && !context.RequestAborted.IsCancellationRequested)
        {
            LogObserveResolutionFailed(logger, exception, context.Request.Path);
            if (context.Request.Body.CanSeek)
                context.Request.Body.Position = 0;
            store = results = defaultStore;
            fingerprint = null;
        }

        if (fingerprint is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyRecordReservation? reservation;
        try
        {
            reservation = await store.TryReserveAsync(recordKey, fingerprint, settings.InFlightTimeout, context.RequestAborted).ConfigureAwait(false);
        }
        catch (Exception exception) when (observe && !context.RequestAborted.IsCancellationRequested)
        {
            LogObserveReserveFailed(logger, exception, context.Request.Path, new IdempotencyKeyHash(recordKey));
            reservation = null;
        }

        if (reservation is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        RecordReservation(reservation.Status, settings.Mode, target);
        switch (reservation.Status)
        {
            case IdempotencyReservationStatus.Reserved:
                await ExecuteAsync(context, options, settings, store, results, recordKey, reservation.Token!, fingerprint, target).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.Completed when observe:
                LogObserveWouldReplay(logger, context.Request.Path, new IdempotencyKeyHash(recordKey));
                await next(context).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.Completed:
                if (IdempotencyEntrySerializer.TryRead(reservation.Payload.GetValueOrDefault(), out IdempotentResponse? stored))
                {
                    await ReplayAsync(context, options, stored).ConfigureAwait(false);
                    break;
                }

                LogUnreadable(logger, context.Request.Path, new IdempotencyKeyHash(recordKey));
                await EnhancedProblemDetailsWriter.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Stored response unreadable",
                        Detail = "The response stored for this idempotency key cannot be read, so it is neither replayed nor run again.",
                    },
                }).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.InFlight when observe:
                LogObserveWouldRejectInFlight(logger, context.Request.Path, new IdempotencyKeyHash(recordKey));
                await next(context).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.InFlight:
                context.Response.Headers.RetryAfter =
                    Math.Max(1, (long)Math.Ceiling(options.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Request in progress", Error.Conflict(
                    IdempotencyErrorCodes.InFlight,
                    "A request with this idempotency key is still being processed. Retry later.")).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.FingerprintMismatch when observe:
                LogObserveWouldRejectKeyReused(logger, context.Request.Path, new IdempotencyKeyHash(recordKey));
                await next(context).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.FingerprintMismatch:
                await WriteProblemAsync(context, StatusCodes.Status422UnprocessableEntity, "Idempotency key reused", Error.Validation(
                    IdempotencyErrorCodes.KeyReused,
                    "This idempotency key was already used for a different request.")).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException($"{store.GetType().Name} returned an unknown reservation status {reservation.Status}.");
        }
    }

    /// <summary>
    /// Resolves the caller's scope. In <see cref="IdempotencyMode.Observe"/> a resolver that throws, unless the caller
    /// aborted, is logged and the request runs without idempotency.
    /// </summary>
    /// <returns><see langword="false"/> when the resolver threw in Observe.</returns>
    private bool TryResolveScope(HttpContext context, IdempotencyOptions options, bool observe, out IdempotencyScope? scope)
    {
        try
        {
            scope = options.ScopeResolver.Resolve(context.User);
            return true;
        }
        catch (Exception exception) when (observe && !context.RequestAborted.IsCancellationRequested)
        {
            LogObserveResolutionFailed(logger, exception, context.Request.Path);
            scope = null;
            return false;
        }
    }

    private static void RecordReservation(IdempotencyReservationStatus status, IdempotencyMode mode, string target)
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
            IdempotencyMetrics.Record(decision, mode, IdempotencyMetrics.HttpSource, target);
    }

    /// <summary>Counts a settled reservation as <paramref name="decision"/> when the store accepted it.</summary>
    private static bool Settled(bool settled, string decision, IdempotencyMode mode, string target)
    {
        if (settled)
            IdempotencyMetrics.Record(decision, mode, IdempotencyMetrics.HttpSource, target);
        return settled;
    }

    /// <summary>
    /// Runs the request on a held reservation and settles it. An exception after the handler or the response started
    /// keeps the key in flight until the lease expires, because the side effect may already have happened; an
    /// exception before that releases it.
    /// </summary>
    private async Task ExecuteAsync(
        HttpContext context,
        IdempotencyOptions options,
        IdempotencyEndpointSettings settings,
        IIdempotencyRecordStore store,
        IIdempotencyRecordStore results,
        IdempotencyRecordKey recordKey,
        string token,
        string fingerprint,
        string target)
    {
        IdempotencyExecutionFeature execution = new();
        context.Features.Set<IIdempotencyExecutionFeature>(execution);
        HttpResponse response = context.Response;
        Stream original = response.Body;
        using ResponseCaptureStream capture = new(original, options.MaxResponseBodySize, response.Headers);
        response.Body = capture;
        try
        {
            await next(context).ConfigureAwait(false);
            await response.BodyWriter.FlushAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception)
        {
            response.Body = original;
            if (execution.HandlerStarted || response.HasStarted)
                LogKeptInFlight(logger, context.Request.Path, new IdempotencyKeyHash(recordKey), response.StatusCode, "the request failed after its handler started");
            else
                await TryReleaseAsync(store, recordKey, token, context.Request.Path).ConfigureAwait(false);
            throw;
        }

        response.Body = original;
        try
        {
            bool filterInstalled = context.GetEndpoint()?.Metadata.GetMetadata<IdempotencyFilterInstalledMetadata>() is not null;
            if (!await SettleAsync(context, options, settings, store, results, recordKey, token, fingerprint, capture, execution.HandlerStarted, filterInstalled, target).ConfigureAwait(false))
                LogStaleReservation(logger, context.Request.Path, new IdempotencyKeyHash(recordKey));
        }
        catch (Exception exception)
        {
            LogStoreFailed(logger, exception, context.Request.Path, new IdempotencyKeyHash(recordKey));
        }
    }

    /// <summary>
    /// Stores, releases or keeps the reservation for a request that completed. The work is done, so the store calls
    /// ignore <see cref="HttpContext.RequestAborted"/>: a client that went away still gets its outcome on retry.
    /// An endpoint without the execution filter cannot report whether its handler started, so a 2xx from it is
    /// settled as a started handler's response and every other status releases the key. A release or a stored response
    /// is counted in <see cref="IdempotencyTelemetry.DecisionsInstrumentName"/>. A response is written to
    /// <paramref name="results"/>; when that is not the <paramref name="store"/> that holds the reservation, the
    /// reservation is released first, so the response is written as the record of a key that is gone.
    /// </summary>
    /// <returns><see langword="false"/> when the reservation was taken over after <c>InFlightTimeout</c>.</returns>
    private async ValueTask<bool> SettleAsync(
        HttpContext context,
        IdempotencyOptions options,
        IdempotencyEndpointSettings settings,
        IIdempotencyRecordStore store,
        IIdempotencyRecordStore results,
        IdempotencyRecordKey recordKey,
        string token,
        string fingerprint,
        ResponseCaptureStream capture,
        bool handlerStarted,
        bool filterInstalled,
        string target)
    {
        HttpResponse response = context.Response;
        int statusCode = response.StatusCode;
        if (!handlerStarted && (filterInstalled || statusCode is < 200 or >= 300))
            return Settled(await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false), IdempotencyMetrics.Released, settings.Mode, target);

        if (options.NeverStoreStatusCodes.Contains(statusCode))
        {
            LogKeptInFlight(logger, context.Request.Path, new IdempotencyKeyHash(recordKey), statusCode, "its status is in NeverStoreStatusCodes");
            return true;
        }

        if (!settings.ShouldStore(new IdempotencyOutcome { StatusCode = statusCode, HandlerStarted = handlerStarted }))
            return Settled(await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false), IdempotencyMetrics.Released, settings.Mode, target);

        string? notStorable = NotStorableReason(options, response, capture);
        if (notStorable is not null)
        {
            if (statusCode is >= 400 and < 500)
            {
                LogNotStorableReleased(logger, context.Request.Path, new IdempotencyKeyHash(recordKey), statusCode, notStorable);
                return Settled(await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false), IdempotencyMetrics.Released, settings.Mode, target);
            }

            LogKeptInFlight(logger, context.Request.Path, new IdempotencyKeyHash(recordKey), statusCode, notStorable);
            return true;
        }

        Dictionary<string, string[]> headers = [];
        foreach (string name in options.ReplayedHeaders)
        {
            if (!string.Equals(name, "Set-Cookie", StringComparison.OrdinalIgnoreCase)
                && response.Headers.TryGetValue(name, out StringValues values)
                && values.Count > 0)
                headers[name] = [.. values.OfType<string>()];
        }

        if (!ReferenceEquals(store, results))
            await TryReleaseAsync(store, recordKey, token, context.Request.Path).ConfigureAwait(false);
        bool stored = await results.CompleteAsync(
            recordKey,
            token,
            fingerprint,
            IdempotencyEntrySerializer.Write(new IdempotentResponse(statusCode, headers, capture.ToArray())),
            statusCode >= StatusCodes.Status500InternalServerError ? settings.ServerErrorRetention : settings.RetentionPeriod,
            CancellationToken.None).ConfigureAwait(false);
        return Settled(stored, IdempotencyMetrics.Stored, settings.Mode, target);
    }

    private static string? NotStorableReason(IdempotencyOptions options, HttpResponse response, ResponseCaptureStream capture)
    {
        if (capture.Overflowed)
            return string.Create(CultureInfo.InvariantCulture, $"its body exceeds MaxResponseBodySize ({options.MaxResponseBodySize} bytes)");
        if (capture.EncodedByHandler)
            return "the handler set a Content-Encoding";
        if (MediaTypeHeaderValue.TryParse(response.ContentType, out MediaTypeHeaderValue? mediaType)
            && mediaType.MediaType.Equals("text/event-stream", StringComparison.OrdinalIgnoreCase))
            return "it is an event stream";
        return null;
    }

    /// <summary>
    /// Validates the endpoint's setup and returns its effective settings, merged from its
    /// <see cref="IdempotentAttribute"/> metadata (the most specific value wins), its profile and the options.
    /// Without <see cref="IdempotencyFilterInstalledMetadata"/> only its 2xx responses can be stored, which throws
    /// under <see cref="IdempotencyOptions.RequireExecutionFilter"/>. The settings are cached per endpoint and per
    /// options instance, so a reload resolves them again; warnings about the filter and about a request timeout longer
    /// than the lease are logged when they are resolved. An unknown profile or invalid settings throw on every request
    /// to the endpoint.
    /// </summary>
    private IdempotencyEndpointSettings GetSettings(HttpContext context, Endpoint endpoint, IdempotencyOptions options)
    {
        bool filterInstalled = endpoint.Metadata.GetMetadata<IdempotencyFilterInstalledMetadata>() is not null;
        if (!filterInstalled && options.RequireExecutionFilter)
            throw new InvalidOperationException(
                $"The idempotent endpoint '{endpoint.DisplayName}' has no filter that marks its handler as started. Use .WithIdempotency() on Minimal API endpoints and [Idempotent] on MVC actions.");

        ConditionalWeakTable<Endpoint, IdempotencyEndpointSettings> endpointSettings = _endpointSettings.GetValue(options, static _ => []);
        if (endpointSettings.TryGetValue(endpoint, out IdempotencyEndpointSettings? cached))
            return cached;

        string name = endpoint.DisplayName ?? context.Request.Path.ToString();
        IdempotencyEndpointOptions merged = new();
        foreach (IdempotentAttribute attribute in endpoint.Metadata.GetOrderedMetadata<IdempotentAttribute>())
            merged.MergeFrom(attribute.Overrides);
        var settings = IdempotencyEndpointSettings.Resolve(options, merged, $"the endpoint '{name}'");
        if (!endpointSettings.TryAdd(endpoint, settings))
            return endpointSettings.TryGetValue(endpoint, out cached) ? cached : settings;

        if (!filterInstalled)
            LogExecutionFilterMissing(logger, name);
        if (ResolveRequestTimeout(endpoint, context.RequestServices) is { } timeout && timeout > settings.InFlightTimeout)
            LogRequestTimeoutExceedsLease(logger, name, timeout, settings.InFlightTimeout);
        return settings;
    }

    /// <summary>The timeout <c>UseRequestTimeouts()</c> applies to the endpoint, resolved like that middleware does.</summary>
    private static TimeSpan? ResolveRequestTimeout(Endpoint endpoint, IServiceProvider services)
    {
        if (endpoint.Metadata.GetMetadata<DisableRequestTimeoutAttribute>() is not null)
            return null;

        RequestTimeoutOptions? timeouts = services.GetService<IOptions<RequestTimeoutOptions>>()?.Value;
        if (endpoint.Metadata.GetMetadata<RequestTimeoutPolicy>() is { } policy)
            return policy.Timeout;
        if (endpoint.Metadata.GetMetadata<RequestTimeoutAttribute>() is { } attribute)
        {
            if (attribute.Timeout is { } timeout)
                return timeout;
            return attribute.PolicyName is { } policyName && timeouts?.Policies.TryGetValue(policyName, out RequestTimeoutPolicy? named) == true
                ? named.Timeout
                : null;
        }

        return timeouts?.DefaultPolicy?.Timeout;
    }

    private static async Task ReplayAsync(HttpContext context, IdempotencyOptions options, IdempotentResponse stored)
    {
        HttpResponse response = context.Response;
        response.StatusCode = stored.StatusCode;
        foreach (KeyValuePair<string, string[]> header in stored.Headers)
            response.Headers[header.Key] = header.Value;
        response.Headers[options.ReplayedHeaderName] = "true";
        if (stored.Body.Length > 0)
        {
            response.ContentLength = stored.Body.Length;
            await response.Body.WriteAsync(stored.Body, context.RequestAborted).ConfigureAwait(false);
        }
    }

    /// <summary>Releases after a failure; a store error is logged so the request's own exception is the one surfaced.</summary>
    private async Task TryReleaseAsync(IIdempotencyRecordStore store, IdempotencyRecordKey recordKey, string token, PathString path)
    {
        try
        {
            await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogReleaseFailed(logger, exception, path, new IdempotencyKeyHash(recordKey));
        }
    }

    private static async Task<string> ComputeFingerprintAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendPart(hash, request.Method.ToUpperInvariant());
        AppendPart(hash, $"{request.PathBase}{request.Path}");
        AppendPart(hash, request.QueryString.Value);

        request.EnableBuffering();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        try
        {
            int read;
            while ((read = await request.Body.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false)) > 0)
                hash.AppendData(buffer, 0, read);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        request.Body.Position = 0;
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>Length-prefixed, so a part can never run into the next one.</summary>
    private static void AppendPart(IncrementalHash hash, string? value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    /// <summary>
    /// Writes an idempotency problem: <c>application/problem+json</c> built like any other <see cref="Error"/> problem,
    /// plus the top-level <c>code</c> member that clients rely on to tell it apart from the endpoint's own responses.
    /// </summary>
    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, Error error)
    {
        EnhancedProblemDetails problem = EnhancedProblemDetailsFactory.Create([error], null, statusCode, context.RequestServices);
        problem.Title = title;
        problem.Extensions[CodeKey] = error.Code;
        return EnhancedProblemDetailsWriter.WriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
    }

    [LoggerMessage(EventId = 7001, EventName = IdempotencyErrorCodes.UnscopedKeySkipped, Level = LogLevel.Warning,
        Message = "The idempotency key for {Path} was ignored and the request ran without idempotency: the caller's scope could not be resolved.")]
    private static partial void LogUnscopedKeySkipped(ILogger logger, PathString path);

    [LoggerMessage(EventId = 7002, EventName = "Idempotency.StoredResponseUnreadable", Level = LogLevel.Error,
        Message = "The idempotent response stored for {Path} (key hash {KeyHash}) cannot be read; the request was answered with 500 and not run again.")]
    private static partial void LogUnreadable(ILogger logger, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7003, EventName = "Idempotency.KeptInFlight", Level = LogLevel.Warning,
        Message = "The outcome of {Path} (key hash {KeyHash}, status {StatusCode}) was not stored because {Reason}; the idempotency key stays in flight until its lease expires.")]
    private static partial void LogKeptInFlight(ILogger logger, PathString path, IdempotencyKeyHash keyHash, int statusCode, string reason);

    [LoggerMessage(EventId = 7004, EventName = "Idempotency.ResponseNotStorable", Level = LogLevel.Debug,
        Message = "The {StatusCode} response of {Path} (key hash {KeyHash}) was not stored because {Reason}; the idempotency key was released.")]
    private static partial void LogNotStorableReleased(ILogger logger, PathString path, IdempotencyKeyHash keyHash, int statusCode, string reason);

    [LoggerMessage(EventId = 7005, EventName = "Idempotency.StoreFailed", Level = LogLevel.Error,
        Message = "Storing the idempotent outcome for {Path} (key hash {KeyHash}) failed; the key stays in flight until it expires.")]
    private static partial void LogStoreFailed(ILogger logger, Exception exception, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7006, EventName = "Idempotency.StaleReservation", Level = LogLevel.Warning,
        Message = "The request for {Path} (key hash {KeyHash}) outlived its idempotency reservation, which expired or was taken over by another request; its outcome was not stored. Make InFlightTimeout longer than the request timeout.")]
    private static partial void LogStaleReservation(ILogger logger, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7007, EventName = "Idempotency.ReleaseFailed", Level = LogLevel.Warning,
        Message = "Releasing the idempotency key for {Path} (key hash {KeyHash}) failed; it stays in flight until it expires.")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7008, EventName = "Idempotency.ExecutionFilterMissing", Level = LogLevel.Warning,
        Message = "The idempotent endpoint {Endpoint} has no filter that marks its handler as started, so only its 2xx responses are stored and every other outcome releases its key, which lets a failure after a side effect run again. Use .WithIdempotency() on Minimal API endpoints and [Idempotent] on MVC actions.")]
    private static partial void LogExecutionFilterMissing(ILogger logger, string endpoint);

    [LoggerMessage(EventId = 7009, EventName = "Idempotency.RequestTimeoutExceedsLease", Level = LogLevel.Warning,
        Message = "The request timeout of the idempotent endpoint {Endpoint} ({RequestTimeout}) is longer than InFlightTimeout ({InFlightTimeout}); a retry can run the handler again while the first request is still running.")]
    private static partial void LogRequestTimeoutExceedsLease(ILogger logger, string endpoint, TimeSpan requestTimeout, TimeSpan inFlightTimeout);

    [LoggerMessage(EventId = 7010, EventName = "Idempotency.Observe.WouldReplay", Level = LogLevel.Information,
        Message = "Observe mode: {Path} (key hash {KeyHash}) has a stored response that Enforce would replay; the request runs again.")]
    private static partial void LogObserveWouldReplay(ILogger logger, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7011, EventName = "Idempotency.Observe.WouldRejectInFlight", Level = LogLevel.Information,
        Message = "Observe mode: the idempotency key of {Path} (key hash {KeyHash}) is in flight and Enforce would answer 409; the request runs anyway.")]
    private static partial void LogObserveWouldRejectInFlight(ILogger logger, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7012, EventName = "Idempotency.Observe.WouldRejectKeyReused", Level = LogLevel.Information,
        Message = "Observe mode: the idempotency key of {Path} (key hash {KeyHash}) was used for a different request and Enforce would answer 422; the request runs anyway.")]
    private static partial void LogObserveWouldRejectKeyReused(ILogger logger, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7013, EventName = "Idempotency.Observe.WouldRejectScopeRequired", Level = LogLevel.Information,
        Message = "Observe mode: {Path} would be answered 400 because the scope of its idempotency key cannot be resolved; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectScopeRequired(ILogger logger, PathString path);

    [LoggerMessage(EventId = 7014, EventName = "Idempotency.Observe.ResolutionFailed", Level = LogLevel.Warning,
        Message = "Observe mode: the idempotency scope, store or fingerprint of {Path} could not be resolved; it runs without idempotency.")]
    private static partial void LogObserveResolutionFailed(ILogger logger, Exception exception, PathString path);

    [LoggerMessage(EventId = 7015, EventName = "Idempotency.Observe.ReserveFailed", Level = LogLevel.Warning,
        Message = "Observe mode: reserving the idempotency key of {Path} (key hash {KeyHash}) failed; it runs without idempotency.")]
    private static partial void LogObserveReserveFailed(ILogger logger, Exception exception, PathString path, IdempotencyKeyHash keyHash);

    [LoggerMessage(EventId = 7016, EventName = "Idempotency.Observe.WouldRejectKeyMissing", Level = LogLevel.Information,
        Message = "Observe mode: {Path} would be answered 400 because it has no idempotency key; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectKeyMissing(ILogger logger, PathString path);

    [LoggerMessage(EventId = 7017, EventName = "Idempotency.Observe.WouldRejectKeyInvalid", Level = LogLevel.Information,
        Message = "Observe mode: {Path} would be answered 400 because its idempotency key is invalid; it runs without idempotency.")]
    private static partial void LogObserveWouldRejectKeyInvalid(ILogger logger, PathString path);
}
