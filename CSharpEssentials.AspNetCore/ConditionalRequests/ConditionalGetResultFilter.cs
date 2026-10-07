using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// MVC result filter added by <see cref="ConditionalRequestExtensions.AddConditionalRequests"/>. For actions with
/// <see cref="ConditionalGetAttribute"/> (action, controller or endpoint metadata), a <c>GET</c>/<c>HEAD</c>
/// <see cref="ObjectResult"/> with a value and a 2xx or no status gets the validators of the value (of the
/// <c>Result&lt;T&gt;</c> value when the action returns a successful result), and is replaced with 304 when they match.
/// Strings and <see cref="ProblemDetails"/> pass through, also inside a result.
/// </summary>
internal sealed class ConditionalGetResultFilter : IResultFilter
{
    // Action and controller attributes, read once per action.
    private readonly ConditionalWeakTable<ActionDescriptor, ConditionalGetAttribute?[]> _attributes = [];

    public void OnResultExecuting(ResultExecutingContext context)
    {
        HttpContext httpContext = context.HttpContext;
        if (!ConditionalGet.IsGetOrHead(httpContext.Request.Method) || !IsEnabled(context.ActionDescriptor, httpContext))
            return;
        if (context.Result is not ObjectResult { Value: { } value, StatusCode: null or >= 200 and <= 299 })
            return;
        if (ConditionalGet.GetResource(value) is not { } resource)
            return;

        if (ConditionalGet.Apply(httpContext, resource, nameof(ConditionalGetAttribute)))
            context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Nothing to do after the result is written.
    }

    private bool IsEnabled(ActionDescriptor action, HttpContext httpContext)
    {
        ConditionalGetAttribute?[] attributes = _attributes.GetValue(action, static a => a is ControllerActionDescriptor controllerAction
            ? [controllerAction.MethodInfo.GetCustomAttribute<ConditionalGetAttribute>(inherit: true),
               controllerAction.ControllerTypeInfo.GetCustomAttribute<ConditionalGetAttribute>(inherit: true)]
            : []);
        return Array.Exists(attributes, static a => a is not null)
            || httpContext.GetEndpoint()?.Metadata.GetMetadata<ConditionalGetAttribute>() is not null;
    }
}
