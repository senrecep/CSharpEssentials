using Microsoft.AspNetCore.Http.Features;
using System.Buffers;

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
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly bool _shouldLogErrors = true;
    private readonly int _bufferSize = DefaultBufferSize;

    protected BaseMiddleware(
        ILogWriter logWriter,
        string[] ignoredPaths)
    {
        ArgumentNullException.ThrowIfNull(logWriter);
        ArgumentNullException.ThrowIfNull(ignoredPaths);

        _logWriter = logWriter is not NullLogWriter ? logWriter : null;
        _ignoredPaths = new HashSet<string>(
            ignoredPaths.Select(p => p.TrimEnd('/')),
            StringComparer.OrdinalIgnoreCase
        );
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    protected bool IsIgnoredPath(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string? requestPath = context.Request.Path.Value?.TrimEnd('/');
        return !string.IsNullOrEmpty(requestPath) &&
               _ignoredPaths.Any(ignorePath => requestPath.StartsWith(ignorePath, StringComparison.OrdinalIgnoreCase));
    }

    protected async Task<RequestResponseContext> InvokeMiddleware(RequestDelegate next, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(httpContext);

        (bool isSkipRequestLogging, bool isSkipResponseLogging) = GetLoggingConfiguration(httpContext);
        string requestText;
        Stream originalBodyStream = httpContext.Response.Body;
        TimeSpan executionTime = TimeSpan.Zero;
        string responseText = DefaultResponseText;

        try
        {
            requestText = await CaptureRequestBodyIfEnabled(httpContext, isSkipRequestLogging).ConfigureAwait(false);
            (executionTime, responseText) = await ProcessResponse(next, httpContext, originalBodyStream, isSkipResponseLogging).ConfigureAwait(false);
        }
        catch (Exception ex) when (_shouldLogErrors)
        {
            await LogError(httpContext, ex).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (!isSkipResponseLogging)
            {
                httpContext.Response.Body = originalBodyStream;
            }
        }

        return CreateAndLogContext(httpContext, requestText, responseText, executionTime);
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
            await using RecyclableMemoryStream requestStream = _recyclableMemoryStreamManager.GetStream();
            buffer = _arrayPool.Rent(_bufferSize);

            int bytesRead;
            while ((bytesRead = await context.Request.Body.ReadAsync(buffer.AsMemory(0, _bufferSize)).ConfigureAwait(false)) > 0)
            {
                await requestStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
            }

            if (requestStream.Length == 0)
            {
                return "Empty request body";
            }

            requestStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(requestStream, Encoding.UTF8, leaveOpen: true);
            string requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);

            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return requestBody;
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
        }
    }

    private async Task<(TimeSpan ElapsedTime, string ResponseText)> ProcessResponse(
        RequestDelegate next,
        HttpContext httpContext,
        Stream originalBodyStream,
        bool isSkipResponseLogging)
    {
        if (isSkipResponseLogging)
        {
            return (await ExecuteWithTiming(next, httpContext).ConfigureAwait(false), DefaultResponseText);
        }

        await using RecyclableMemoryStream responseBody = _recyclableMemoryStreamManager.GetStream();
        httpContext.Response.Body = responseBody;

        TimeSpan elapsedTime = await ExecuteWithTiming(next, httpContext).ConfigureAwait(false);
        string responseText = await CaptureResponseBody(responseBody, originalBodyStream).ConfigureAwait(false);
        return (elapsedTime, responseText);
    }

    private static async Task<TimeSpan> ExecuteWithTiming(RequestDelegate next, HttpContext httpContext)
    {
        long startTime = Stopwatch.GetTimestamp();
        await next(httpContext).ConfigureAwait(false);
        return Stopwatch.GetElapsedTime(startTime);
    }

    private async Task<string> CaptureResponseBody(MemoryStream responseBody, Stream originalBodyStream)
    {
        if (responseBody.Length == 0)
        {
            return "Empty response body";
        }

        responseBody.Seek(0, SeekOrigin.Begin);
        byte[]? buffer = null;
        string responseText;

        try
        {
            buffer = _arrayPool.Rent(_bufferSize);
            using var reader = new StreamReader(responseBody, leaveOpen: true);
            responseText = await reader.ReadToEndAsync().ConfigureAwait(false);

            responseBody.Seek(0, SeekOrigin.Begin);
            int bytesRead;
            while ((bytesRead = await responseBody.ReadAsync(buffer.AsMemory(0, _bufferSize)).ConfigureAwait(false)) > 0)
            {
                await originalBodyStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            return $"Error capturing response body: {ex.Message}";
        }
        finally
        {
            if (buffer is not null)
            {
                _arrayPool.Return(buffer);
            }
        }

        return responseText;
    }

    private RequestResponseContext CreateAndLogContext(
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
            _logWriter?.Write(context);
        }
        catch (Exception ex) when (_shouldLogErrors)
        {
            context.ResponseBody = $"Error writing log: {ex.Message}";
        }

        return context;
    }

    private async Task LogError(HttpContext context, Exception exception)
    {
        if (_logWriter is null)
        {
            return;
        }

        RequestResponseContext errorContext = new(context)
        {
            RequestBody = "Error occurred during request processing",
            ResponseBody = $"Exception: {exception.GetType().Name}, Message: {exception.Message}",
            ResponseCreationTime = TimeSpan.Zero
        };

        try
        {
            await Task.Run(() => _logWriter.Write(errorContext)).ConfigureAwait(false);
        }
        catch
        {
            // Suppress logging errors during error handling
        }
    }
}
