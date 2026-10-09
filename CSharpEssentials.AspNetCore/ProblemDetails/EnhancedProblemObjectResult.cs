using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// An MVC problem response built from <see cref="Error"/> values. The response is built when it executes,
/// from the request's <see cref="IErrorStatusCodeMapper"/> and <see cref="EnhancedProblemDetailsOptions"/>,
/// so its JSON matches <see cref="EnhancedProblemHttpResult"/>.
/// </summary>
public sealed class EnhancedProblemObjectResult : ObjectResult
{
    private readonly Error[] _errors;
    private readonly ErrorMetadata? _extensions;
    private readonly int? _statusCode;

    internal EnhancedProblemObjectResult(Error[] errors, ErrorMetadata? extensions, int? statusCode, HttpContext? httpContext)
        : base(null)
    {
        _errors = errors;
        _extensions = extensions;
        _statusCode = statusCode;

        EnhancedProblemDetails preview =
            EnhancedProblemDetailsFactory.Create(errors, extensions, statusCode, httpContext?.RequestServices);
        if (httpContext is not null)
            ProblemDetailsEnrichment.ApplyDefaults(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = preview });
        Value = preview;
        StatusCode = preview.Status;
    }

    /// <inheritdoc />
    public override async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        HttpContext httpContext = context.HttpContext;
        EnhancedProblemDetails problemDetails =
            EnhancedProblemDetailsFactory.Create(_errors, _extensions, _statusCode, httpContext.RequestServices);
        Value = problemDetails;
        StatusCode = problemDetails.Status;
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var problemContext = new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problemDetails };
        if (await EnhancedProblemDetailsWriter.TryWriteWithServiceAsync(problemContext).ConfigureAwait(false))
            return;
        await base.ExecuteResultAsync(context).ConfigureAwait(false);
    }
}
