using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Minimal API endpoint filter of <see cref="EnumWireFormatExtensions"/>: when the endpoint's <see cref="IEnumWireFormatMetadata"/>
/// selects the format that is not <see cref="EnumConventions.WriteAs"/>, a returned value or <see cref="IValueHttpResult"/>
/// (also inside <see cref="INestedHttpResult"/>) is written with the options of that format. MVC actions are left to
/// <see cref="EnumWireFormatResultFilter"/>.
/// </summary>
internal static class EnumWireFormatEndpointFilter
{
    // One instance, so a group and an endpoint that both call WithEnumWireFormat add the filter once.
    public static readonly Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> Factory =
        static (_, next) => invocation => InvokeAsync(invocation, next);

    private static async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext invocation, EndpointFilterDelegate next)
    {
        object? result = await next(invocation).ConfigureAwait(false);
        HttpContext httpContext = invocation.HttpContext;
        Endpoint? endpoint = httpContext.GetEndpoint();
        if (endpoint is null || endpoint.Metadata.GetMetadata<ActionDescriptor>() is not null)
            return result;
        if (endpoint.Metadata.GetMetadata<IEnumWireFormatMetadata>() is not { } metadata)
            return result;

        EnumWireFormatOutput output = httpContext.RequestServices.GetService<EnumWireFormatOutput>()
            ?? throw EnumConventionsExtensions.MissingRegistration(nameof(EnumWireFormatExtensions.WithEnumWireFormat));
        if (metadata.VaryHeader is { } header)
            httpContext.Response.Headers.Append(Microsoft.Net.Http.Headers.HeaderNames.Vary, header);
        if (metadata.SelectFormat(httpContext) == output.DefaultFormat)
            return result;

        return result switch
        {
            null or string => result,
            IResult httpResult when Unwrap(httpResult) is IValueHttpResult { Value: not null } =>
                new AlternateJsonOptionsResult(httpResult, output.HttpOptions),
            IResult => result,
            _ => new AlternateJsonOptionsResult(TypedResults.Ok(result), output.HttpOptions),
        };
    }

    private static IResult Unwrap(IResult result)
    {
        while (result is INestedHttpResult nested)
            result = nested.Result;
        return result;
    }

    /// <summary>
    /// Executes a result with <see cref="IOptions{HttpJsonOptions}"/> resolving to the other format, so the result keeps its own
    /// status code, content type and headers (such as <c>Location</c>).
    /// </summary>
    private sealed class AlternateJsonOptionsResult(IResult inner, IOptions<HttpJsonOptions> options) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            IServiceProvider services = httpContext.RequestServices;
            httpContext.RequestServices = new JsonOptionsServiceProvider(services, options);
            try
            {
                await inner.ExecuteAsync(httpContext).ConfigureAwait(false);
            }
            finally
            {
                httpContext.RequestServices = services;
            }
        }
    }

    private sealed class JsonOptionsServiceProvider(IServiceProvider inner, IOptions<HttpJsonOptions> options)
        : IKeyedServiceProvider
    {
        public object? GetService(Type serviceType) =>
            serviceType == typeof(IOptions<HttpJsonOptions>) ? options : inner.GetService(serviceType);

        public object? GetKeyedService(Type serviceType, object? serviceKey) =>
            inner is IKeyedServiceProvider keyed ? keyed.GetKeyedService(serviceType, serviceKey) : null;

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey) =>
            inner is IKeyedServiceProvider keyed
                ? keyed.GetRequiredKeyedService(serviceType, serviceKey)
                : throw new InvalidOperationException("The request services do not support keyed services.");
    }
}
