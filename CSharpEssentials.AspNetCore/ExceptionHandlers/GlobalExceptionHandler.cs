using System.ComponentModel.DataAnnotations;
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            ApplicationException or
            ValidationException or
            BadHttpRequestException or
            BadHttpRequestException or
            EnhancedValidationException or
            DomainException or
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        logger.LogError(exception, "An error occurred while processing the request. Status code: {StatusCode}", statusCode);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = CreateProblemDetails(exception, statusCode)
        });
    }

    private static EnhancedProblemDetails CreateProblemDetails(Exception exception, int statusCode) =>
        exception switch
        {
            EnhancedValidationException validationException => validationException.Errors.ToProblemDetails(statusCode: statusCode),
            DomainException domainException => domainException.Error.ToProblemDetails(statusCode: statusCode),
            _ => Error.Exception(exception).ToProblemDetails(statusCode: statusCode)
        };
}
