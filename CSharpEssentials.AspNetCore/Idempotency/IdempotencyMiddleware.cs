using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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

    public async Task InvokeAsync(HttpContext context, IIdempotencyRecordStore store)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IdempotentAttribute>() is null
            || !options.Methods.Contains(context.Request.Method))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

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
        if (scope is null && !options.AllowUnscopedKeys)
        {
            LogUnscoped(logger, context.Request.Path);
            await next(context).ConfigureAwait(false);
            return;
        }

        IdempotencyRecordKey recordKey = new(options.Namespace, scope ?? IdempotencyScope.Global, key);
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

    private async Task ExecuteAsync(HttpContext context, IIdempotencyRecordStore store, IdempotencyRecordKey recordKey, string token, string fingerprint)
    {
        HttpResponse response = context.Response;
        Stream original = response.Body;
        using ResponseCaptureStream capture = new(original, options.MaxResponseBodySize);
        response.Body = capture;
        try
        {
            await next(context).ConfigureAwait(false);
            await response.BodyWriter.FlushAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception)
        {
            response.Body = original;
            // Once the response has started the handler's work is most likely done (for example the client went away
            // while the body was written). Releasing would let a retry run it again, so the key stays in flight until
            // InFlightTimeout instead.
            if (response.HasStarted)
                LogKeptInFlight(logger, context.Request.Path);
            else
                await TryReleaseAsync(store, recordKey, token, context.Request.Path).ConfigureAwait(false);
            throw;
        }

        response.Body = original;
        try
        {
            if (!await StoreOutcomeAsync(context, store, recordKey, token, fingerprint, capture).ConfigureAwait(false))
                LogStaleReservation(logger, context.Request.Path);
        }
        catch (Exception exception)
        {
            // The response is already sent; a store failure must not turn it into an error. A reservation that could
            // not be completed or released expires after InFlightTimeout.
            LogStoreFailed(logger, exception, context.Request.Path);
        }
    }

    /// <returns><see langword="false"/> when the reservation was taken over after <c>InFlightTimeout</c>.</returns>
    private async Task<bool> StoreOutcomeAsync(
        HttpContext context,
        IIdempotencyRecordStore store,
        IdempotencyRecordKey recordKey,
        string token,
        string fingerprint,
        ResponseCaptureStream capture)
    {
        HttpResponse response = context.Response;

        // The work is done: store the outcome even when the client has gone away, so the key does not stay in flight.
        if (!options.ShouldStore(response.StatusCode))
            return await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);

        if (capture.Overflowed)
        {
            LogTooLarge(logger, context.Request.Path, options.MaxResponseBodySize);
            return await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);
        }

        // Names come from ReplayedHeaders, which is already case-insensitive.
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
            IdempotencyEntrySerializer.Write(new IdempotentResponse(response.StatusCode, headers, capture.ToArray())),
            options.RetentionPeriod,
            CancellationToken.None).ConfigureAwait(false);
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

    private async Task TryReleaseAsync(IIdempotencyRecordStore store, IdempotencyRecordKey recordKey, string token, PathString path)
    {
        try
        {
            await store.ReleaseAsync(recordKey, token, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // The request's own exception is the one to surface; the reservation expires after InFlightTimeout.
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

    [LoggerMessage(Level = LogLevel.Debug, Message = "Idempotency key ignored for {Path}: the request has no key scope.")]
    private static partial void LogUnscoped(ILogger logger, PathString path);

    [LoggerMessage(Level = LogLevel.Error, Message = "The idempotent response stored for {Path} cannot be read; the request was answered with 500 and not run again.")]
    private static partial void LogUnreadable(ILogger logger, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Idempotent response for {Path} was not stored: the body exceeded {MaxResponseBodySize} bytes.")]
    private static partial void LogTooLarge(ILogger logger, PathString path, long maxResponseBodySize);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The request for {Path} failed after its response started; the idempotency key stays in flight until it expires.")]
    private static partial void LogKeptInFlight(ILogger logger, PathString path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Storing the idempotent outcome for {Path} failed; the key stays in flight until it expires.")]
    private static partial void LogStoreFailed(ILogger logger, Exception exception, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The request for {Path} outlived its idempotency reservation, which expired or was taken over by another request; its outcome was not stored. Make InFlightTimeout longer than the request timeout.")]
    private static partial void LogStaleReservation(ILogger logger, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Releasing the idempotency key for {Path} failed; it stays in flight until it expires.")]
    private static partial void LogReleaseFailed(ILogger logger, Exception exception, PathString path);
}
