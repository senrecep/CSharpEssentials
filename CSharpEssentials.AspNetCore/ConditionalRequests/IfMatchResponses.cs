using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Declares the problem responses of <c>If-Match</c> handling so OpenAPI documents list them: <c>400</c> (malformed header),
/// <c>412</c> (the precondition does not match, returned by the loader of <c>WithIfMatch</c> or by the handler with
/// <see cref="ConditionalRequestErrors.PreconditionFailed"/>) and, when the header is required, <c>428</c>. Only operations
/// that accept <c>POST</c>, <c>PUT</c>, <c>PATCH</c> or <c>DELETE</c> get them, and a status the operation already declares
/// is kept. Minimal API endpoints get endpoint metadata; MVC actions with <see cref="IfMatchAttribute"/>, or whose endpoint
/// has <c>WithIfMatch()</c> from <c>MapControllers()</c> or <c>MapControllerRoute()</c>, get the responses in API Explorer
/// after the default provider, so the response MVC infers from the return type stays. Like the runtime filter, the attribute
/// wins over the endpoint convention.
/// </summary>
internal sealed class IfMatchResponses(IModelMetadataProvider? modelMetadataProvider = null, EndpointDataSource? endpoints = null)
    : IApiDescriptionProvider
{
    public int Order => 0;

    public static void AddTo(EndpointBuilder endpoint)
    {
        if (endpoint.Metadata.OfType<ActionDescriptor>().Any()
            || endpoint.Metadata.OfType<IfMatchMetadata>().LastOrDefault() is not { } settings
            || !AcceptsUnsafeMethod(endpoint.Metadata.OfType<IHttpMethodMetadata>().LastOrDefault()?.HttpMethods))
            return;

        foreach (int statusCode in Statuses(settings.Required))
            HttpResultMetadata.AddProblem(endpoint, statusCode);
    }

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
        Dictionary<ActionDescriptor, IfMatchMetadata>? conventions = null;
        foreach (ApiDescription description in context.Results)
        {
            if (description.ActionDescriptor is not ControllerActionDescriptor action
                || !AcceptsUnsafeMethod(description.HttpMethod is null ? null : [description.HttpMethod])
                || ((action.MethodInfo.GetCustomAttribute<IfMatchAttribute>(inherit: true)
                        ?? action.ControllerTypeInfo.GetCustomAttribute<IfMatchAttribute>(inherit: true))?.Required
                    ?? (conventions ??= Conventions()).GetValueOrDefault(action)?.Required) is not { } required)
                continue;

            foreach (int statusCode in Statuses(required))
            {
                if (description.SupportedResponseTypes.Any(response => response.StatusCode == statusCode))
                    continue;
                description.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = statusCode,
                    Type = typeof(EnhancedProblemDetails),
                    ModelMetadata = modelMetadataProvider?.GetMetadataForType(typeof(EnhancedProblemDetails)),
                    ApiResponseFormats = [new ApiResponseFormat { MediaType = EnhancedProblemDetailsWriter.ContentType }],
                });
            }
        }
    }

    private Dictionary<ActionDescriptor, IfMatchMetadata> Conventions()
    {
        Dictionary<ActionDescriptor, IfMatchMetadata> conventions = [];
        foreach (Endpoint endpoint in endpoints?.Endpoints ?? [])
        {
            if (endpoint.Metadata.GetMetadata<ActionDescriptor>() is { } action && endpoint.Metadata.GetMetadata<IfMatchMetadata>() is { } metadata)
                conventions.TryAdd(action, metadata);
        }

        return conventions;
    }

    private static int[] Statuses(bool required) =>
        required
            ? [StatusCodes.Status400BadRequest, StatusCodes.Status412PreconditionFailed, StatusCodes.Status428PreconditionRequired]
            : [StatusCodes.Status400BadRequest, StatusCodes.Status412PreconditionFailed];

    private static bool AcceptsUnsafeMethod(IReadOnlyCollection<string>? methods) =>
        methods is null or { Count: 0 }
        || methods.Any(static method => HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method));
}
