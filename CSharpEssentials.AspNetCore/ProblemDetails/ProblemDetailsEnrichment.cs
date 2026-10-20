using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The built-in enrichment driven by <see cref="EnhancedProblemDetailsOptions"/>, followed by the registered
/// <see cref="IProblemDetailsEnricher"/>s.
/// </summary>
internal static class ProblemDetailsEnrichment
{
    private const string FrameworkTypePrefix = "https://tools.ietf.org/html/rfc9110#";
    private static readonly EnhancedProblemDetailsOptions DefaultOptions = new();

    public static EnhancedProblemDetailsOptions GetOptions(IServiceProvider? services) =>
        services?.GetService<IOptions<EnhancedProblemDetailsOptions>>()?.Value ?? DefaultOptions;

    private static readonly ConditionalWeakTable<ProblemDetails, object> Enriched = [];

    /// <summary>
    /// Applies the options and runs the registered enrichers. Runs at most once per <see cref="ProblemDetails"/> instance.
    /// </summary>
    public static void Enrich(ProblemDetailsContext context)
    {
        if (!Enriched.TryAdd(context.ProblemDetails, Enriched))
            return;
        IServiceProvider? services = context.HttpContext.RequestServices;
        Apply(context, GetOptions(services));
        ApplyIdempotencyCode(context, services);
        if (services is null)
            return;
        foreach (IProblemDetailsEnricher enricher in services.GetServices<IProblemDetailsEnricher>())
            enricher.Enrich(context);
    }

    /// <summary>
    /// Applies the options only, without enrichers and without marking the instance as enriched. Used for previews.
    /// </summary>
    public static void ApplyDefaults(ProblemDetailsContext context) =>
        Apply(context, GetOptions(context.HttpContext.RequestServices));

    private static void Apply(ProblemDetailsContext context, EnhancedProblemDetailsOptions options)
    {
        ProblemDetails problemDetails = context.ProblemDetails;
        HttpContext httpContext = context.HttpContext;
        IDictionary<string, object?> extensions = problemDetails.Extensions;
        int status = problemDetails.Status ?? httpContext.Response.StatusCode;

        if (problemDetails.Type is null || problemDetails.Type.StartsWith(FrameworkTypePrefix, StringComparison.Ordinal))
            problemDetails.Type = options.TypeUriResolver(status);

        problemDetails.Instance ??= options.Instance switch
        {
            ProblemInstanceFormat.Path => httpContext.Request.Path.Value,
            ProblemInstanceFormat.MethodAndPath => $"{httpContext.Request.Method} {httpContext.Request.Path}",
            ProblemInstanceFormat.None => null,
            _ => null,
        };

        Activity? activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity ?? Activity.Current;
        ApplyTraceId(extensions, options.TraceId, activity, httpContext);

        if (options.IncludeSpanIds && activity is not null)
        {
            extensions.TryAdd("spanId", activity.SpanId.ToHexString());
            string? parent = activity.ParentSpanId == default ? null : activity.ParentSpanId.ToHexString();
            if (options.TraceId == TraceIdFormat.TraceparentHeader)
                parent = activity.ParentId;
            if (parent is not null)
                extensions.TryAdd("parentSpanId", parent);
        }

        if (options.IncludeRequestId)
            extensions.TryAdd("requestId", httpContext.TraceIdentifier);

        if (options.IncludeUser && httpContext.User.Identity is { IsAuthenticated: true, Name: { } userName })
            extensions.TryAdd("user", userName);

        if (options.ExposeExceptionDetails && context.Exception is { } exception)
            extensions["exception"] = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["type"] = exception.GetType().FullName,
                ["message"] = exception.Message,
                ["stackTrace"] = exception.StackTrace,
                ["innerException"] = exception.InnerException?.Message,
            };
    }

    /// <summary>
    /// Gives a problem that carries an <see cref="IdempotencyErrorCodes"/> code, such as a failed <c>Result</c> of an
    /// idempotent Mediator request, the same shape as the problems of the idempotency middleware: the top-level
    /// <c>code</c> member, and for <see cref="IdempotencyErrorCodes.InFlight"/> a <c>Retry-After</c> from
    /// <see cref="IdempotencyOptions.RetryAfter"/> (1 second when <c>AddIdempotency</c> was not called). A business
    /// error gets neither, so clients retry only a key in flight.
    /// </summary>
    private static void ApplyIdempotencyCode(ProblemDetailsContext context, IServiceProvider? services)
    {
        if (context.ProblemDetails is not EnhancedProblemDetails problem
            || Array.Find(problem.Errors, static error => IsIdempotencyCode(error.Code)) is not { Code: { } code })
            return;

        problem.Extensions[IdempotencyMiddleware.CodeKey] = code;
        HttpResponse response = context.HttpContext.Response;
        if (code != IdempotencyErrorCodes.InFlight || response.HasStarted)
            return;
        TimeSpan retryAfter = services?.GetService<IdempotencyOptions>()?.RetryAfter ?? TimeSpan.FromSeconds(1);
        response.Headers.RetryAfter = Math.Max(1, (long)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsIdempotencyCode(string code) =>
        code is IdempotencyErrorCodes.InFlight
            or IdempotencyErrorCodes.KeyReused
            or IdempotencyErrorCodes.KeyMissing
            or IdempotencyErrorCodes.KeyInvalid
            or IdempotencyErrorCodes.ScopeRequired;

    private static void ApplyTraceId(IDictionary<string, object?> extensions, TraceIdFormat format, Activity? activity, HttpContext httpContext)
    {
        const string key = "traceId";
        string? traceId = format switch
        {
            TraceIdFormat.W3CTraceId when activity is { IdFormat: ActivityIdFormat.W3C } => activity.TraceId.ToHexString(),
            TraceIdFormat.W3CTraceId => httpContext.TraceIdentifier,
            TraceIdFormat.TraceparentHeader => activity?.Id ?? httpContext.TraceIdentifier,
            TraceIdFormat.None => null,
            _ => null,
        };
        if (traceId is null)
            extensions.Remove(key);
        else
            extensions[key] = traceId;
    }
}
