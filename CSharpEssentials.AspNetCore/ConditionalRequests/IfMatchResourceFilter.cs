using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// MVC resource filter added by <see cref="ConditionalRequestExtensions.AddConditionalRequests"/>: evaluates <c>If-Match</c>
/// before model binding for actions with <see cref="IfMatchAttribute"/> (action &gt; controller &gt; endpoint metadata such as
/// <c>MapControllers().WithIfMatch()</c>) and writes the problem instead of running the action when it fails.
/// </summary>
internal sealed class IfMatchResourceFilter : IAsyncResourceFilter
{
    // Action and controller attributes, read once per action.
    private readonly ConditionalWeakTable<ActionDescriptor, IfMatchMetadata?[]> _attributes = [];

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        HttpContext httpContext = context.HttpContext;
        if (Select(context.ActionDescriptor, httpContext) is not { } settings)
        {
            await next();
            return;
        }

        ProblemDetails? problem = await IfMatchEvaluator.EvaluateAsync(
            httpContext,
            settings,
            nameof(IfMatchAttribute));
        if (problem is null)
            await next();
        else
            await IfMatchEvaluator.WriteAsync(httpContext, problem);
    }

    private IfMatchMetadata? Select(ActionDescriptor action, HttpContext httpContext)
    {
        IfMatchMetadata?[] attributes = _attributes.GetValue(action, static a => a is ControllerActionDescriptor controllerAction
            ? [ToMetadata(controllerAction.MethodInfo.GetCustomAttribute<IfMatchAttribute>(inherit: true)),
               ToMetadata(controllerAction.ControllerTypeInfo.GetCustomAttribute<IfMatchAttribute>(inherit: true))]
            : []);
        foreach (IfMatchMetadata? attribute in attributes)
        {
            if (attribute is not null)
                return attribute;
        }

        // Endpoint conventions (MapControllers().WithIfMatch(...), groups) come after the attributes.
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<IfMatchMetadata>();
    }

    private static IfMatchMetadata? ToMetadata(IfMatchAttribute? attribute) =>
        attribute is null ? null : new IfMatchMetadata(attribute.Required, LoadCurrent: null);
}
