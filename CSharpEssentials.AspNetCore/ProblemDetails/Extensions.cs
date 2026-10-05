using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

public static partial class Extensions
{
    /// <summary>
    /// Registers ProblemDetails with the CSharpEssentials enrichment and default options. Every problem response
    /// (CSharpEssentials results, <see cref="GlobalExceptionHandler"/>, status code pages, the framework exception
    /// handler) then goes through the same <see cref="IProblemDetailsEnricher"/> pipeline.
    /// </summary>
    public static IServiceCollection AddEnhancedProblemDetails(this IServiceCollection services) =>
        services.AddEnhancedProblemDetails(static _ => { });

    /// <summary>
    /// Registers ProblemDetails with the CSharpEssentials enrichment, configured by <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddEnhancedProblemDetails(
        this IServiceCollection services,
        Action<EnhancedProblemDetailsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddProblemDetails();
        services.Configure(configure);
        services.TryAddSingleton<EnhancedProblemDetailsMarker>();
        services.TryAddSingleton<IErrorStatusCodeMapper>(DefaultErrorStatusCodeMapper.Instance);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<ProblemDetailsOptions>, EnhancedProblemDetailsPostConfigure>());
        return services;
    }

    /// <summary>
    /// Registers ProblemDetails with default options and runs <paramref name="configure"/> on every problem response
    /// (as an <see cref="IProblemDetailsEnricher"/>, after the built-in enrichment).
    /// </summary>
    public static IServiceCollection AddEnhancedProblemDetails(
        this IServiceCollection services,
        Action<ProblemDetails, HttpContext> configure)
    {
        services.AddSingleton<IProblemDetailsEnricher>(new DelegateProblemDetailsEnricher(configure));
        return services.AddEnhancedProblemDetails();
    }

    /// <summary>
    /// Adds an <see cref="IProblemDetailsEnricher"/>. Enrichers run after the built-in enrichment, in registration order.
    /// </summary>
    public static IServiceCollection AddProblemDetailsEnricher<TEnricher>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TEnricher : class, IProblemDetailsEnricher
    {
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IProblemDetailsEnricher), typeof(TEnricher), lifetime));
        return services;
    }

    /// <summary>
    /// Adds an <see cref="IExceptionProblemMapper"/> for <see cref="GlobalExceptionHandler"/>. Mappers are tried in
    /// registration order before <see cref="DefaultExceptionProblemMapper"/>.
    /// </summary>
    public static IServiceCollection AddExceptionProblemMapper<TMapper>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TMapper : class, IExceptionProblemMapper
    {
        services.TryAddEnumerable(ServiceDescriptor.Describe(typeof(IExceptionProblemMapper), typeof(TMapper), lifetime));
        return services;
    }

    /// <summary>
    /// Replaces the <see cref="IErrorStatusCodeMapper"/>.
    /// </summary>
    public static IServiceCollection AddErrorStatusCodeMapper<TMapper>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TMapper : class, IErrorStatusCodeMapper
    {
        services.Replace(ServiceDescriptor.Describe(typeof(IErrorStatusCodeMapper), typeof(TMapper), lifetime));
        return services;
    }

    /// <summary>
    /// Adds the exception handler and status code pages middleware, so unhandled exceptions and empty error
    /// responses (404, 405, ...) are written as problem details.
    /// </summary>
    public static IApplicationBuilder UseEnhancedProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }

    public static IServiceCollection ConfigureModelValidatorResponse(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add(new ValidateModelAttribute()));
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        return services;
    }

    /// <summary>
    /// Writes the automatic 400 of <see cref="ApiControllerAttribute"/> controllers (invalid model state, including
    /// binding failures such as an unknown enum value) as an enhanced problem response, so it carries the same
    /// <c>errorCodes</c>/<c>errors</c> fields (and honors <see cref="EnhancedProblemDetailsOptions.ErrorFields"/>)
    /// as every other problem response. Each model error becomes an <see cref="ErrorType.Validation"/> error whose
    /// code is the model state key (for example <c>status</c>, <c>dto.status</c> or <c>$.status</c>).
    /// <para>
    /// Registered as a post-configuration, so it can be called before or after <c>AddControllers()</c>.
    /// It has no effect together with <see cref="ConfigureModelValidatorResponse"/>, which turns the automatic 400
    /// off (<see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/>); use one or the other.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="errorFactory">
    /// Creates the error of one model error from its model state key; <see langword="null"/> uses the default
    /// (code: key, description: the model error message).
    /// </param>
    public static IServiceCollection ConfigureInvalidModelStateResponse(
        this IServiceCollection services,
        Func<string, ModelError, Error>? errorFactory = null)
    {
        Func<string, ModelError, Error> factory = errorFactory ?? CreateModelStateError;
        services.PostConfigure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            Error[] errors = [.. context.ModelState
                .Where(static entry => entry.Value is { Errors.Count: > 0 })
                .SelectMany(entry => entry.Value!.Errors.Select(error => factory(entry.Key, error)))];
            if (errors.Length == 0)
                errors = [Error.Validation(code: InvalidModelStateCode, description: InvalidModelStateDescription)];
            return errors.ToActionResult(context.HttpContext, statusCode: StatusCodes.Status400BadRequest);
        });
        return services;
    }

    private const string InvalidModelStateCode = "request";
    private const string InvalidModelStateDescription = "The request is invalid.";

    // Exception messages of model errors are not written: they may expose implementation details.
    private static Error CreateModelStateError(string key, ModelError error) =>
        Error.Validation(
            code: string.IsNullOrEmpty(key) ? InvalidModelStateCode : key,
            description: string.IsNullOrEmpty(error.ErrorMessage) ? InvalidModelStateDescription : error.ErrorMessage);

    /// <summary>
    /// Converts a successful result to a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IResult ToProblemResult(this ResultPattern.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null)
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
    public static IResult ToProblemResult(this Error[] errors, ErrorMetadata? extensions = null, int? statusCode = null) =>
        new EnhancedProblemHttpResult(errors, extensions, statusCode);

    /// <summary>
    /// Converts a <see cref="ProblemDetails"/> object to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="httpContext"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IActionResult ToActionResult(this ResultPattern.Interfaces.IResultBase result, HttpContext? httpContext = null, ErrorMetadata? extensions = null, int? statusCode = null)
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
    public static IActionResult ToActionResult(this Error[] errors, HttpContext? httpContext = null, ErrorMetadata? extensions = null, int? statusCode = null) =>
        new EnhancedProblemObjectResult(errors, extensions, statusCode, httpContext);

    /// <summary>
    /// Converts a <see cref="ProblemDetails"/> object to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="result"></param>
    /// <param name="extensions"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static IActionResult Problem(this ControllerBase controller, ResultPattern.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null) =>
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
    public static EnhancedProblemDetails ToProblemDetails(this ResultPattern.Interfaces.IResultBase result, ErrorMetadata? extensions = null, int? statusCode = null)
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
    /// <remarks>
    /// Built with <see cref="DefaultErrorStatusCodeMapper"/> and default <see cref="EnhancedProblemDetailsOptions"/>,
    /// without request details. Use <see cref="ToProblemResult(Error[], ErrorMetadata?, int?)"/> or
    /// <see cref="ToActionResult(Error[], HttpContext?, ErrorMetadata?, int?)"/> to honor the registered configuration.
    /// </remarks>
    public static EnhancedProblemDetails ToProblemDetails(this Error[] errors, ErrorMetadata? extensions = null, int? statusCode = null) =>
        EnhancedProblemDetailsFactory.Create(errors, extensions, statusCode, services: null);

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
}
