using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Handles <c>Idempotency-Key</c> for endpoints that carry <see cref="IdempotentAttribute"/> metadata.
/// The fingerprint is a SHA-256 of the method, path, query string and body (the body is buffered to compute it).
/// </summary>
internal sealed partial class IdempotencyMiddleware(
    RequestDelegate next,
    IdempotencyOptions options,
    ILogger<IdempotencyMiddleware> logger)
{
    internal const string CodeKey = "code";

    private static readonly object Checked = new();

    private readonly ConditionalWeakTable<Endpoint, object> _checkedEndpoints = [];

    public async Task InvokeAsync(HttpContext context, IIdempotencyRecordStore store)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IdempotentAttribute>() is null || !options.Methods.Contains(context.Request.Method))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        CheckEndpoint(context, endpoint);

        StringValues header = context.Request.Headers[options.HeaderName];
        if (header.Count == 0)
        {
            if (options.RequireKey)
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Missing idempotency key", Error.Validation(
                    IdempotencyErrorCodes.KeyMissing,
                    $"This endpoint requires the '{options.HeaderName}' header.")).ConfigureAwait(false);
            else
                await next(context).ConfigureAwait(false);
            return;
        }

        if (header.Count > 1 || !TryNormalizeKey(header[0], options.MaxKeyLength, out string? key))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid idempotency key", Error.Validation(
                IdempotencyErrorCodes.KeyInvalid,
                $"Send exactly one '{options.HeaderName}' header of 1 to {options.MaxKeyLength} printable ASCII characters other than space, '\"' and '\\', optionally as a quoted string.")).ConfigureAwait(false);
            return;
        }

        IdempotencyScope? scope = options.ScopeResolver.Resolve(context.User);
        if (scope is null && options.UnscopedKeyBehavior == UnscopedKeyBehavior.Global)
            scope = IdempotencyScope.Global;
        else if (scope is null && options.UnscopedKeyBehavior == UnscopedKeyBehavior.Reject)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Idempotency scope required", Error.Validation(
                IdempotencyErrorCodes.ScopeRequired,
                "An idempotency key needs a caller whose scope can be resolved, such as an authenticated user or client.")).ConfigureAwait(false);
            return;
        }
        else if (scope is null)
        {
            LogUnscopedKeySkipped(logger, context.Request.Path);
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyRecordKey recordKey = new(options.Namespace, scope.Value, key);
        string fingerprint = await ComputeFingerprintAsync(context.Request, context.RequestAborted).ConfigureAwait(false);
        IdempotencyRecordReservation reservation =
            await store.TryReserveAsync(recordKey, fingerprint, options.InFlightTimeout, context.RequestAborted).ConfigureAwait(false);

        switch (reservation.Status)
        {
            case IdempotencyReservationStatus.Reserved:
                await ExecuteAsync(context, store, recordKey, reservation.Token!, fingerprint).ConfigureAwait(false);
                break;
            case IdempotencyReservationStatus.Completed:
                if (IdempotencyEntrySerializer.TryRead(reservation.Payload.GetValueOrDefault(), out IdempotentResponse? stored))
                {
                    await ReplayAsync(context, stored).ConfigureAwait(false);
                    break;
                }

                LogUnreadable(logger, context.Request.Path);
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
            case IdempotencyReservationStatus.InFlight:
                context.Response.Headers.RetryAfter =
                    Math.Max(1, (long)Math.Ceiling(options.RetryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Request in progress", Error.Conflict(
                    IdempotencyErrorCodes.InFlight,
                    "A request with this idempotency key is still being processed. Retry later.")).ConfigureAwait(false);
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
    /// Runs the request on a held reservation and settles it. An exception after the handler or the response started
    /// keeps the key in flight until the lease expires, because the side effect may already have happened; an
    /// exception before that releases it.
    /// </summary>
    private async Task ExecuteAsync(HttpContext context, IIdempotencyRecordStore store, IdempotencyRecordKey recordKey, string token, string fingerprint)
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
                LogKeptInFlight(logger, context.Request.Path, response.StatusCode, "the request failed after its handler started");
            else
                await TryReleaseAsync(store, recordKey, token, context.Request.Path).ConfigureAwait(false);
            throw;
        }

        response.Body = original;
        try
        {
            bool filterInstalled = context.GetEndpoint()?.Metadata.GetMetadata<IdempotencyFilterInstalledMetadata>() is not null;
            if (!await SettleAsync(context, store, recordKey, token, fingerprint, capture, execution.HandlerStarted, filterInstalled).ConfigureAwait(false))
                LogStaleReservation(logger, context.Request.Path);
        }
        catch (Exception exception)
        {
            LogStoreFailed(logger, exception, context.Request.Path);
        }
    }

    /// <summary>
    /// Stores, releases or keeps the reservation for a request that completed. The work is done, so the store calls
    /// ignore <see cref="HttpContext.RequestAborted"/>: a client that went away still gets its outcome on retry.
    /// An endpoint without the execution filter cannot report whether its handler started, so a 2xx from it is
    /// settled as a started handler's response and every other status releases the key.
    /// </summary>
    /// <returns><see langword="false"/> when the reservation was taken over after <c>InFlightTimeout</c>.</returns>
    private async ValueTask<bool> SettleAsync(
        HttpContext context,
        IIdempotencyRecordStore store,
        IdempotencyRecordKey recordKey,
        string token,
        string fingerprint,
        ResponseCaptureStream capture,
        bool handlerStarted,
        bool filterInstalled)
    {
        HttpResponse response = context.Response;
        int statusCode = response.StatusCode;
        if (!handlerStarted && (filterInstalled || statusCode is < 200 or >= 300))
            return await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);

        if (options.NeverStoreStatusCodes.Contains(statusCode))
        {
            LogKeptInFlight(logger, context.Request.Path, statusCode, "its status is in NeverStoreStatusCodes");
            return true;
        }

        if (!options.ShouldStore(new IdempotencyOutcome { StatusCode = statusCode, HandlerStarted = handlerStarted }))
            return await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);

        string? notStorable = NotStorableReason(response, capture);
        if (notStorable is not null)
        {
            if (statusCode is >= 400 and < 500)
            {
                LogNotStorableReleased(logger, context.Request.Path, statusCode, notStorable);
                return await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);
            }

            LogKeptInFlight(logger, context.Request.Path, statusCode, notStorable);
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

        return await store.CompleteAsync(
            recordKey,
            token,
            fingerprint,
            IdempotencyEntrySerializer.Write(new IdempotentResponse(statusCode, headers, capture.ToArray())),
            statusCode >= StatusCodes.Status500InternalServerError ? options.ServerErrorRetention : options.RetentionPeriod,
            CancellationToken.None).ConfigureAwait(false);
    }

    private string? NotStorableReason(HttpResponse response, ResponseCaptureStream capture)
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
    /// Validates the endpoint's setup: without <see cref="IdempotencyFilterInstalledMetadata"/> only its 2xx responses
    /// can be stored, which throws under <see cref="IdempotencyOptions.RequireExecutionFilter"/>. Warnings about the
    /// filter and about a request timeout longer than the lease are logged on the endpoint's first request only.
    /// </summary>
    private void CheckEndpoint(HttpContext context, Endpoint endpoint)
    {
        bool filterInstalled = endpoint.Metadata.GetMetadata<IdempotencyFilterInstalledMetadata>() is not null;
        if (!filterInstalled && options.RequireExecutionFilter)
            throw new InvalidOperationException(
                $"The idempotent endpoint '{endpoint.DisplayName}' has no filter that marks its handler as started. Use .WithIdempotency() on Minimal API endpoints and [Idempotent] on MVC actions.");

        if (_checkedEndpoints.TryGetValue(endpoint, out _) || !_checkedEndpoints.TryAdd(endpoint, Checked))
            return;

        string name = endpoint.DisplayName ?? context.Request.Path.ToString();
        if (!filterInstalled)
            LogExecutionFilterMissing(logger, name);
        if (ResolveRequestTimeout(endpoint, context.RequestServices) is { } timeout && timeout > options.InFlightTimeout)
            LogRequestTimeoutExceedsLease(logger, name, timeout, options.InFlightTimeout);
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

    private async Task ReplayAsync(HttpContext context, IdempotentResponse stored)
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
            LogReleaseFailed(logger, exception, path);
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
    /// Unquotes a structured-field string (<c>"abc"</c>) and validates the key. An escape sequence can only produce
    /// <c>"</c> or <c>\</c>, which keys may not contain, so a value with a backslash or an inner quote is rejected
    /// whether or not its quoting is well formed.
    /// </summary>
    internal static bool TryNormalizeKey(string? value, int maxKeyLength, [NotNullWhen(true)] out string? key)
    {
        key = value is { Length: >= 2 } && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
        return IdempotencyKeys.IsValid(key, maxKeyLength);
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
        Message = "The idempotent response stored for {Path} cannot be read; the request was answered with 500 and not run again.")]
    private static partial void LogUnreadable(ILogger logger, PathString path);

    [LoggerMessage(EventId = 7003, EventName = "Idempotency.KeptInFlight", Level = LogLevel.Warning,
        Message = "The outcome of {Path} ({StatusCode}) was not stored because {Reason}; the idempotency key stays in flight until its lease expires.")]
    private static partial void LogKeptInFlight(ILogger logger, PathString path, int statusCode, string reason);

    [LoggerMessage(EventId = 7004, EventName = "Idempotency.ResponseNotStorable", Level = LogLevel.Debug,
        Message = "The {StatusCode} response of {Path} was not stored because {Reason}; the idempotency key was released.")]
    private static partial void LogNotStorableReleased(ILogger logger, PathString path, int statusCode, string reason);

    [LoggerMessage(EventId = 7005, EventName = "Idempotency.StoreFailed", Level = LogLevel.Error,
        Message = "Storing the idempotent outcome for {Path} failed; the key stays in flight until it expires.")]
    private static partial void LogStoreFailed(ILogger logger, Exception exception, PathString path);

    [LoggerMessage(EventId = 7006, EventName = "Idempotency.StaleReservation", Level = LogLevel.Warning,
        Message = "The request for {Path} outlived its idempotency reservation, which expired or was taken over by another request; its outcome was not stored. Make InFlightTimeout longer than the request timeout.")]
    private static partial void LogStaleReservation(ILogger logger, PathString path);

    [LoggerMessage(EventId = 7007, EventName = "Idempotency.ReleaseFailed", Level = LogLevel.Warning,
        Message = "Releasing the idempotency key for {Path} failed; it stays in flight until it expires.")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception, PathString path);

    [LoggerMessage(EventId = 7008, EventName = "Idempotency.ExecutionFilterMissing", Level = LogLevel.Warning,
        Message = "The idempotent endpoint {Endpoint} has no filter that marks its handler as started, so only its 2xx responses are stored and every other outcome releases its key, which lets a failure after a side effect run again. Use .WithIdempotency() on Minimal API endpoints and [Idempotent] on MVC actions.")]
    private static partial void LogExecutionFilterMissing(ILogger logger, string endpoint);

    [LoggerMessage(EventId = 7009, EventName = "Idempotency.RequestTimeoutExceedsLease", Level = LogLevel.Warning,
        Message = "The request timeout of the idempotent endpoint {Endpoint} ({RequestTimeout}) is longer than InFlightTimeout ({InFlightTimeout}); a retry can run the handler again while the first request is still running.")]
    private static partial void LogRequestTimeoutExceedsLease(ILogger logger, string endpoint, TimeSpan requestTimeout, TimeSpan inFlightTimeout);
}
