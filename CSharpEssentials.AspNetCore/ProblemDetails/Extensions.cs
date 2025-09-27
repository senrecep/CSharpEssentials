using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

public static partial class Extensions
{
    private static Action<ProblemDetails, HttpContext>? _problemDetailsEnhancer;
    /// <summary>
    /// Adds the <see cref="ProblemDetails"/> to the pipeline.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddEnhancedProblemDetails(
        this IServiceCollection services,
        Action<ProblemDetails, HttpContext>? configure = null)
    {
        _problemDetailsEnhancer = configure;
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context => context.ProblemDetails.AddHttpContextDetails(context.HttpContext));
        return services;
    }

    public static IServiceCollection ConfigureModelValidatorResponse(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add(new ValidateModelAttribute()));
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        return services;
    }

    /// <summary>
    /// Converts a successful result to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IResult ToProblemResult(this Results.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to a problem result");
        return result.Errors.ToProblemResult(extensions, statusCode);
    }

    /// <summary>
    /// Converts an <see cref="Error"/> to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IResult ToProblemResult(this Error error, ErrorMetadata? extensions = null, int? statusCode = null) => ToProblemResult([error], extensions, statusCode);

    /// <summary>
    /// Converts an array of <see cref="Error"/> to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="errors"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IResult ToProblemResult(this Error[] errors, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        EnhancedProblemDetails problemDetails = errors.ToProblemDetails(extensions, statusCode);
        return Microsoft.AspNetCore.Http.Results.Problem(problemDetails);
    }

    /// <summary>
    /// Converts a <see cref="ProblemDetails"/> object to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="httpContext"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IActionResult ToActionResult(this Results.Interfaces.IResultBase result, HttpContext? httpContext = null, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to an action result");
        return result.Errors.ToActionResult(httpContext, extensions, statusCode);

    }
    /// <summary>
    /// Converts an <see cref="Error"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="httpContext"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult ToActionResult(this Error error, HttpContext? httpContext = null, ErrorMetadata? extensions = null, int? statusCode = null) =>
        ToActionResult([error], httpContext, extensions, statusCode);

    /// <summary>
    /// Converts an array of <see cref="Error"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="errors"></param>
    /// <param name="httpContext"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult ToActionResult(this Error[] errors, HttpContext? httpContext = null, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        EnhancedProblemDetails problemDetails = errors.ToProblemDetails(extensions, statusCode);
        if (httpContext is null)
            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };

        problemDetails.AddHttpContextDetails(httpContext);

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    /// <summary>
    /// Converts a <see cref="ProblemDetails"/> object to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="result"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult Problem(this ControllerBase controller, Results.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null) =>
        result.ToActionResult(controller.HttpContext, extensions, statusCode);
    /// <summary>
    /// Converts an <see cref="Error"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="error"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult Problem(this ControllerBase controller, Error error, ErrorMetadata? extensions = null, int? statusCode = null) =>
        error.ToActionResult(controller.HttpContext, extensions, statusCode);
    /// <summary>
    /// Converts an array of <see cref="Error"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="errors"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult Problem(this ControllerBase controller, Error[] errors, ErrorMetadata? extensions = null, int? statusCode = null) =>
        errors.ToActionResult(controller.HttpContext, extensions, statusCode);

    /// <summary>
    /// Converts a <see cref="ProblemDetails"/> object to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EnhancedProblemDetails ToProblemDetails(this Results.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to a problem details");
        return result.Errors.ToProblemDetails(extensions, statusCode);
    }
    /// <summary>
    /// Converts an <see cref="Error"/> to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="error"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static EnhancedProblemDetails ToProblemDetails(this Error error, ErrorMetadata? extensions = null, int? statusCode = null) => ToProblemDetails([error], extensions, statusCode);
    /// <summary>
    /// Converts an array of <see cref="Error"/> to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="errors"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static EnhancedProblemDetails ToProblemDetails(this Error[] errors, ErrorMetadata? extensions = null, int? statusCode = null)
    {
        ErrorMetadata metadata = extensions.IsNotNull() ? new ErrorMetadata(extensions) : ErrorMetadata.CreateEmpty();
        Error error = errors.MaxBy(e => e.Type.ToHttpStatusCode());
        statusCode ??= error.Type.ToHttpStatusCode();

        var errorCodes = errors.Select(e => e.Code).ToHashSet();
        var errorMessages = errors.Select(e => e.Description).ToHashSet();

        return new EnhancedProblemDetails
        {
            Status = statusCode,
            Title = error.Type.GetProblemTitle(),
            Detail = error.Description,
            Type = GetProblemRfcType(statusCode.Value),
            Errors = errors,
            ErrorCodes = errorCodes,
            ErrorMessages = errorMessages,
            Extensions = metadata
        };
    }
    /// <summary>
    /// Produces a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder ProducesProblem(this RouteHandlerBuilder builder, int statusCode = HttpCodes.BadRequest) =>
        builder.ProducesProblem<EnhancedProblemDetails>(statusCode);
    /// <summary>
    /// Produces a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <typeparam name="TProblemDetails"></typeparam>
    /// <param name="builder"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder ProducesProblem<TProblemDetails>(this RouteHandlerBuilder builder, int statusCode = HttpCodes.BadRequest)
        where TProblemDetails : ProblemDetails
    {
        return OpenApiRouteHandlerBuilderExtensions.Produces<TProblemDetails>(builder, statusCode, "application/problem+json");
    }

    /// <summary>
    /// Adds the HTTP context details to the <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="problemDetails"></param>
    /// <param name="httpContext"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    internal static ProblemDetails AddHttpContextDetails(this ProblemDetails problemDetails, HttpContext? httpContext, Action<ProblemDetails, HttpContext>? configure = null)
    {
        if (httpContext is null)
            return problemDetails;

        problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        problemDetails.Extensions.TryAdd("requestId", httpContext.TraceIdentifier);

        System.Diagnostics.Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        activity.IfNotNull((activity) =>
        {
            activity.Id.IfNotNull((id) => problemDetails.Extensions.TryAdd("traceId", id));
            activity.ParentId.IfNotNull((parentId) => problemDetails.Extensions.TryAdd("parentSpanId", parentId));
            activity.SpanId.IfNotNull((spanId) => problemDetails.Extensions.TryAdd("spanId", spanId.ToString()));
        });

        httpContext.User.Identity?.IsAuthenticated.IfTrue(() =>
                problemDetails.Extensions.TryAdd("user", httpContext.User.Identity.Name));

        (configure ?? _problemDetailsEnhancer)?.Invoke(problemDetails, httpContext);
        return problemDetails;
    }


    /// <summary>
    /// Gets the title for the <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="errorType"></param>
    /// <returns></returns>
    internal static string GetProblemTitle(this ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Conflict",
        ErrorType.NotFound => "Not Found",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Unexpected => "Unexpected Error",
        ErrorType.Failure => "Server Failure",
        ErrorType.Unknown => "Unknown Error",
        _ => throw new NotImplementedException(),
    };

    /// <summary>
    /// Gets the RFC type for the <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    internal static string GetProblemRfcType(int statusCode) => statusCode switch
    {
        HttpCodes.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        HttpCodes.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        HttpCodes.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        HttpCodes.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        HttpCodes.MethodNotAllowed => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
        HttpCodes.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        HttpCodes.NotImplemented => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };
}
