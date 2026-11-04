using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Http.Features;

namespace CSharpEssentials.RequestResponseLogging.Infrastructure.Middlewares;

internal abstract class BaseMiddleware
{
    private const string DefaultRequestText = "Skipped logging request body";
    private const string DefaultResponseText = "Skipped logging response body";
    private const int DefaultBufferSize = 4096;
    private const int MaxRequestBodySize = 1024 * 1024 * 10;

    private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private readonly ILogWriter? _logWriter;
    private readonly HashSet<string> _ignoredPaths;
    private readonly bool _ignoreRootPath;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly bool _shouldLogErrors = true;
    private readonly int _bufferSize = DefaultBufferSize;

    protected BaseMiddleware(
        ILogWriter logWriter,
        string[] ignoredPaths)
    {
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(logWriter);

#else

            if (logWriter is null)

                throw new ArgumentNullException(nameof(logWriter));

#endif
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(ignoredPaths);

#else

            if (ignoredPaths is null)

                throw new ArgumentNullException(nameof(ignoredPaths));

#endif

        _logWriter = logWriter is not NullLogWriter ? logWriter : null;
        _ignoreRootPath = ignoredPaths.Any(p => !string.IsNullOrEmpty(p) && p.TrimEnd('/').Length == 0);
        _ignoredPaths = new HashSet<string>(
            ignoredPaths.Where(p => p is not null).Select(p => p.TrimEnd('/')).Where(p => p.Length > 0),
            StringComparer.OrdinalIgnoreCase
        );
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    protected bool IsIgnoredPath(HttpContext context)
    {
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(context);

#else

            if (context is null)

                throw new ArgumentNullException(nameof(context));

#endif
        string requestPath = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        if (requestPath.Length == 0)
            return _ignoreRootPath;

        return _ignoredPaths.Any(ignorePath => requestPath.StartsWith(ignorePath, StringComparison.OrdinalIgnoreCase));
    }

    protected async Task<RequestResponseContext> InvokeMiddleware(RequestDelegate next, HttpContext httpContext, Func<RequestResponseContext, Task>? handler = null)
    {
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(next);

#else

            if (next is null)

                throw new ArgumentNullException(nameof(next));

#endif
#if NET6_0_OR_GREATER

        ArgumentNullException.ThrowIfNull(httpContext);

#else

            if (httpContext is null)

                throw new ArgumentNullException(nameof(httpContext));

#endif

        (bool isSkipRequestLogging, bool isSkipResponseLogging) = GetLoggingConfiguration(httpContext);
        isSkipRequestLogging |= handler is null && _logWriter is not { NeedsRequestBody: true };
        isSkipResponseLogging |= handler is null && _logWriter is not { NeedsResponseBody: true };
        string requestText = "Error occurred during request processing";
        Stream originalBodyStream = httpContext.Response.Body;
        TimeSpan executionTime;
        string responseText;
        ExceptionDispatchInfo? copyFailure;

        try
        {
            requestText = await CaptureRequestBodyIfEnabled(httpContext, isSkipRequestLogging).ConfigureAwait(false);
            (executionTime, responseText, copyFailure) = await ProcessResponse(next, httpContext, originalBodyStream, isSkipResponseLogging).ConfigureAwait(false);
        }
        catch (Exception ex) when (_shouldLogErrors)
        {
            await LogError(httpContext, requestText, ex, handler).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (!isSkipResponseLogging)
            {
                httpContext.Response.Body = originalBodyStream;
            }
        }

        RequestResponseContext context = await CreateAndLogContext(httpContext, requestText, responseText, executionTime).ConfigureAwait(false);
        if (handler is not null)
            await handler(context).ConfigureAwait(false);

        if (copyFailure is not null)
        {
            await LogError(httpContext, requestText, copyFailure.SourceException, handler).ConfigureAwait(false);
            copyFailure.Throw();
        }

        return context;
    }

    private static (bool IsSkipRequestLogging, bool IsSkipResponseLogging) GetLoggingConfiguration(HttpContext context)
    {
        Endpoint? endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        EndpointMetadataCollection? metadata = endpoint?.Metadata;

        bool hasSkipAll = metadata?.GetMetadata<SkipRequestResponseLoggingAttribute>() is not null;
        bool hasSkipRequest = metadata?.GetMetadata<SkipRequestLoggingAttribute>() is not null;
        bool hasSkipResponse = metadata?.GetMetadata<SkipResponseLoggingAttribute>() is not null;

        return (
            IsSkipRequestLogging: hasSkipAll || hasSkipRequest,
            IsSkipResponseLogging: hasSkipAll || hasSkipResponse
        );
    }

    private async Task<string> CaptureRequestBodyIfEnabled(HttpContext context, bool isSkipRequestLogging)
    {
        if (isSkipRequestLogging)
        {
            return DefaultRequestText;
        }

        if (context.Request.ContentLength > MaxRequestBodySize)
        {
            return $"Request body too large: {context.Request.ContentLength} bytes";
        }

        context.Request.EnableBuffering();
        byte[]? buffer = null;

        try
        {
            RecyclableMemoryStream requestStream = _recyclableMemoryStreamManager.GetStream();
            await using ConfiguredAsyncDisposable requestStreamScope = requestStream.ConfigureAwait(false);
            buffer = _arrayPool.Rent(_bufferSize);

            int bytesRead;
            long totalBytesRead = 0;
            while ((bytesRead = await context.Request.Body.ReadAsync(buffer.AsMemory(0, _bufferSize), CancellationToken.None).ConfigureAwait(false)) > 0)
            {
                totalBytesRead += bytesRead;
                if (totalBytesRead > MaxRequestBodySize)
                {
                    return $"Request body too large: over {MaxRequestBodySize} bytes";
                }

                await requestStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken.None).ConfigureAwait(false);
            }

            if (requestStream.Length == 0)
            {
                return "Empty request body";
            }

            requestStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(requestStream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return $"Error capturing request body: {ex.Message}";
        }
        finally
        {
            if (buffer is not null)
            {
                _arrayPool.Return(buffer);
            }

            RewindRequestBody(context.Request.Body);
        }
    }

    private static void RewindRequestBody(Stream body)
    {
        if (!body.CanSeek)
        {
            return;
        }

        try
        {
            body.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            return;
        }
    }

    private async Task<(TimeSpan ElapsedTime, string ResponseText, ExceptionDispatchInfo? CopyFailure)> ProcessResponse(
        RequestDelegate next,
        HttpContext httpContext,
        Stream originalBodyStream,
        bool isSkipResponseLogging)
    {
        if (isSkipResponseLogging)
        {
            return (await ExecuteWithTiming(next, httpContext).ConfigureAwait(false), DefaultResponseText, null);
        }

        RecyclableMemoryStream responseBody = _recyclableMemoryStreamManager.GetStream();
        await using ConfiguredAsyncDisposable responseBodyScope = responseBody.ConfigureAwait(false);
        httpContext.Response.Body = responseBody;

        TimeSpan elapsedTime = await ExecuteWithTiming(next, httpContext).ConfigureAwait(false);
        (string responseText, ExceptionDispatchInfo? copyFailure) = await CaptureResponseBody(responseBody, originalBodyStream).ConfigureAwait(false);
        return (elapsedTime, responseText, copyFailure);
    }

    private static async Task<TimeSpan> ExecuteWithTiming(RequestDelegate next, HttpContext httpContext)
    {
        long startTime = Stopwatch.GetTimestamp();
        await next(httpContext).ConfigureAwait(false);
#if NET7_0_OR_GREATER
        return Stopwatch.GetElapsedTime(startTime);
#else
        long endTime = Stopwatch.GetTimestamp();
        long elapsed = endTime - startTime;
        return TimeSpan.FromTicks((long)(elapsed * (10000000.0 / Stopwatch.Frequency)));
#endif
    }

    private async Task<(string ResponseText, ExceptionDispatchInfo? CopyFailure)> CaptureResponseBody(MemoryStream responseBody, Stream originalBodyStream)
    {
        if (responseBody.Length == 0)
        {
            return ("Empty response body", null);
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        string responseText;

        try
        {
            using var reader = new StreamReader(responseBody, leaveOpen: true);
            responseText = await reader.ReadToEndAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ($"Error capturing response body: {ex.Message}", null);
        }

        byte[] buffer = _arrayPool.Rent(_bufferSize);
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            int bytesRead;
            while ((bytesRead = await responseBody.ReadAsync(buffer.AsMemory(0, _bufferSize), CancellationToken.None).ConfigureAwait(false)) > 0)
            {
                await originalBodyStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            return (responseText, ExceptionDispatchInfo.Capture(ex));
        }
        finally
        {
            _arrayPool.Return(buffer);
        }

        return (responseText, null);
    }

    private async Task<RequestResponseContext> CreateAndLogContext(
        HttpContext httpContext,
        string requestText,
        string responseText,
        TimeSpan elapsedTime)
    {
        RequestResponseContext context = new(httpContext)
        {
            RequestBody = requestText,
            ResponseBody = responseText,
            ResponseCreationTime = elapsedTime
        };

        try
        {
            if (_logWriter is not null)
                await _logWriter.Write(context).ConfigureAwait(false);
        }
        catch (Exception ex) when (_shouldLogErrors)
        {
            context.ResponseBody = $"Error writing log: {ex.Message}";
        }

        return context;
    }

    private async Task LogError(HttpContext context, string requestText, Exception exception, Func<RequestResponseContext, Task>? handler)
    {
        if (_logWriter is null && handler is null)
        {
            return;
        }

        RequestResponseContext errorContext = new(context)
        {
            RequestBody = requestText,
            ResponseBody = $"Exception: {exception.GetType().Name}, Message: {exception.Message}",
            ResponseCreationTime = TimeSpan.Zero
        };

        await TryWriteErrorLog(errorContext).ConfigureAwait(false);
        if (handler is null)
            return;

        try
        {
            await handler(errorContext).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await TryWriteErrorLog(new RequestResponseContext(context)
            {
                RequestBody = requestText,
                ResponseBody = $"Error writing log: {ex.Message}",
                ResponseCreationTime = TimeSpan.Zero
            }).ConfigureAwait(false);
        }
    }

    private async Task TryWriteErrorLog(RequestResponseContext errorContext)
    {
        if (_logWriter is null)
            return;

        try
        {
            await Task.Run(() => _logWriter.Write(errorContext), CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Suppress logging errors during error handling
        }
    }
}
