using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

internal static class EnhancedProblemDetailsWriter
{
    internal const string ContentType = "application/problem+json";
    private static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> WithProblemMetadata = [];

    /// <summary>
    /// Writes through <see cref="IProblemDetailsService"/> when <c>AddEnhancedProblemDetails</c> is registered.
    /// When it returns <see langword="false"/>, <see cref="ProblemDetailsContext.ProblemDetails"/> has been enriched
    /// exactly once, <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/> has run, and the caller writes it.
    /// </summary>
    /// <remarks>
    /// Without the registration the service is bypassed: a plain <c>AddProblemDetails()</c> would overwrite the
    /// configured <c>traceId</c> format, so Minimal API and MVC output would differ.
    /// </remarks>
    public static async Task<bool> TryWriteWithServiceAsync(ProblemDetailsContext context, IProblemDetailsService? service = null)
    {
        IServiceProvider? services = context.HttpContext.RequestServices;
        if (services?.GetService<EnhancedProblemDetailsMarker>() is not null)
        {
            service ??= services.GetService<IProblemDetailsService>();
            if (service is not null && !IsSilentlySkippedByMvc(context) && await service.TryWriteAsync(context).ConfigureAwait(false))
                return true;
        }

        ProblemDetailsEnrichment.Enrich(context);
        // The application's CustomizeProblemDetails still runs last, as it would through the service. With
        // AddEnhancedProblemDetails it already contains the enrichment above, which runs only once per instance.
        services?.GetService<IOptions<ProblemDetailsOptions>>()?.Value.CustomizeProblemDetails?.Invoke(context);
        return false;
    }

    /// <summary>
    /// MVC's API problem writer claims every controller endpoint but writes nothing (and still reports success)
    /// for controllers without <c>[ApiController]</c> or when <see cref="ApiBehaviorOptions.SuppressMapClientErrors"/>
    /// is set. Those requests are written directly instead.
    /// </summary>
    private static bool IsSilentlySkippedByMvc(ProblemDetailsContext context)
    {
        EndpointMetadataCollection? endpointMetadata = context.HttpContext.GetEndpoint()?.Metadata;
        if ((context.AdditionalMetadata?.GetMetadata<ControllerAttribute>() ?? endpointMetadata?.GetMetadata<ControllerAttribute>()) is null)
            return false;
        if ((context.AdditionalMetadata?.GetMetadata<IApiBehaviorMetadata>() ?? endpointMetadata?.GetMetadata<IApiBehaviorMetadata>()) is null)
            return true;
        return context.HttpContext.RequestServices?.GetService<IOptions<ApiBehaviorOptions>>()?.Value.SuppressMapClientErrors == true;
    }

    public static async Task WriteAsync(ProblemDetailsContext context, IProblemDetailsService? service = null)
    {
        HttpContext httpContext = context.HttpContext;
        httpContext.Response.StatusCode = context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
        if (await TryWriteWithServiceAsync(context, service).ConfigureAwait(false))
            return;

        JsonSerializerOptions jsonOptions =
            httpContext.RequestServices?.GetService<IOptions<HttpJsonOptions>>()?.Value.SerializerOptions ?? DefaultJsonOptions;
        await httpContext.Response.WriteAsJsonAsync(
            context.ProblemDetails,
            context.ProblemDetails.GetType(),
            AddProblemMetadata(jsonOptions),
            ContentType,
            httpContext.RequestAborted).ConfigureAwait(false);
    }

    /// <summary>
    /// The application's options with <see cref="EnhancedProblemDetailsJsonContext"/> appended to the resolvers, so
    /// the application's metadata still wins and the problem types resolve without reflection (Native AOT).
    /// Options that already resolve the problem types (no resolver or the reflection resolver while reflection is
    /// enabled, or the context already in the chain) are returned unchanged.
    /// </summary>
    internal static JsonSerializerOptions AddProblemMetadata(JsonSerializerOptions options) =>
        ResolvesProblemTypes(options)
            ? options
            : WithProblemMetadata.GetValue(options, static source => new JsonSerializerOptions(source)
            {
                TypeInfoResolver = JsonTypeInfoResolver.Combine(source.TypeInfoResolver, EnhancedProblemDetailsJsonContext.Default),
            });

    private static bool ResolvesProblemTypes(JsonSerializerOptions options) =>
        options.TypeInfoResolverChain.Contains(EnhancedProblemDetailsJsonContext.Default)
        || JsonSerializer.IsReflectionEnabledByDefault
            && (options.TypeInfoResolver is null || options.TypeInfoResolverChain.Any(static resolver => resolver is DefaultJsonTypeInfoResolver));
}
