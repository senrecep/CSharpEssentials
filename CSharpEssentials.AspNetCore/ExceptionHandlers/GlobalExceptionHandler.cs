using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Writes unhandled exceptions as problem details. The exception is mapped by the registered
/// <see cref="IExceptionProblemMapper"/>s (in registration order) and finally by <see cref="DefaultExceptionProblemMapper"/>.
/// Unknown exceptions become a 500 whose message and stack trace go only to the log.
/// </summary>
/// <remarks>
/// Mappers and options are resolved from <see cref="HttpContext.RequestServices"/> for each exception, so they may be
/// registered with any lifetime even though the handler itself is a singleton.
/// </remarks>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        IServiceProvider? services = httpContext.RequestServices;
        ExceptionProblem problem = Map(httpContext, exception, services?.GetServices<IExceptionProblemMapper>());
        ProblemDetails problemDetails = CreateProblemDetails(
            problem,
            services?.GetService<IErrorStatusCodeMapper>(),
            services?.GetService<IOptions<EnhancedProblemDetailsOptions>>()?.Value);
        int statusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        if (statusCode >= StatusCodes.Status500InternalServerError)
            LogServerError(logger, exception, statusCode);
        else
            LogClientError(logger, exception, statusCode);

        await EnhancedProblemDetailsWriter.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
            },
            problemDetailsService).ConfigureAwait(false);
        return true;
    }

    private static ExceptionProblem Map(HttpContext httpContext, Exception exception, IEnumerable<IExceptionProblemMapper>? exceptionMappers)
    {
        if (exceptionMappers is not null)
            foreach (IExceptionProblemMapper mapper in exceptionMappers)
                if (mapper.TryMap(httpContext, exception, out ExceptionProblem? mapped))
                    return mapped;
        DefaultExceptionProblemMapper.Instance.TryMap(httpContext, exception, out ExceptionProblem? fallback);
        return fallback!;
    }

    private static ProblemDetails CreateProblemDetails(
        ExceptionProblem problem,
        IErrorStatusCodeMapper? statusCodeMapper,
        EnhancedProblemDetailsOptions? options)
    {
        if (problem.Errors is { Count: > 0 } errors)
        {
            EnhancedProblemDetails fromErrors = EnhancedProblemDetailsFactory.Create(
                errors,
                extensions: null,
                problem.StatusCode,
                statusCodeMapper ?? DefaultErrorStatusCodeMapper.Instance,
                options ?? new EnhancedProblemDetailsOptions());
            if (problem.Title is not null)
                fromErrors.Title = problem.Title;
            if (problem.Detail is not null)
                fromErrors.Detail = problem.Detail;
            return fromErrors;
        }

        return new ProblemDetails
        {
            Status = problem.StatusCode ?? StatusCodes.Status500InternalServerError,
            Title = problem.Title,
            Detail = problem.Detail,
        };
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception occurred while processing the request. Status code: {StatusCode}")]
    private static partial void LogServerError(ILogger logger, Exception exception, int statusCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "A request failed with a handled exception. Status code: {StatusCode}")]
    private static partial void LogClientError(ILogger logger, Exception exception, int statusCode);
}
