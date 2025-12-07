using System.ComponentModel.DataAnnotations;
using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore;

#if NET7_0_OR_GREATER
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
#else
public sealed class GlobalExceptionHandler
{
    // IExceptionHandler is not available in .NET 6
    // This class is not functional in .NET 6
#pragma warning disable IDE0060 // Remove unused parameter
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        // Constructor required for DI, but class is not functional in .NET 6
    }
#pragma warning restore IDE0060
#endif
}
