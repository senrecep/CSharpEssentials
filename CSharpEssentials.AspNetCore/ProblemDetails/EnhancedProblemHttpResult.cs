using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A Minimal API problem response built from <see cref="Error"/> values. The response is built when it executes,
/// from the request's <see cref="IErrorStatusCodeMapper"/> and <see cref="EnhancedProblemDetailsOptions"/>.
/// </summary>
public sealed class EnhancedProblemHttpResult : IResult, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<ProblemDetails>, IContentTypeHttpResult
{
    private readonly Error[] _errors;
    private readonly ErrorMetadata? _extensions;
    private readonly int? _statusCode;

    internal EnhancedProblemHttpResult(Error[] errors, ErrorMetadata? extensions, int? statusCode)
    {
        _errors = errors;
        _extensions = extensions;
        _statusCode = statusCode;
        ProblemDetails = EnhancedProblemDetailsFactory.Create(errors, extensions, statusCode, services: null);
    }

    /// <summary>
    /// A preview built with the default mapper and options. The written response uses the registered ones.
    /// </summary>
    public EnhancedProblemDetails ProblemDetails { get; }

    /// <summary>
    /// The status code of <see cref="ProblemDetails"/>.
    /// </summary>
    public int StatusCode => ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;

    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    object? IValueHttpResult.Value => ProblemDetails;

    ProblemDetails? IValueHttpResult<ProblemDetails>.Value => ProblemDetails;

    /// <inheritdoc />
    public string ContentType => EnhancedProblemDetailsWriter.ContentType;

    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        EnhancedProblemDetails problemDetails =
            EnhancedProblemDetailsFactory.Create(_errors, _extensions, _statusCode, httpContext.RequestServices);
        return EnhancedProblemDetailsWriter.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        });
    }
}
