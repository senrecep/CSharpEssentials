using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// MVC result filter added by <see cref="EnumConventionsExtensions.AddEnumConventions"/>. When the action selects the format that is not
/// <see cref="EnumConventions.WriteAs"/> (action <see cref="EnumWireFormatAttribute"/> &gt; controller attribute &gt; endpoint
/// metadata such as <c>MapControllers().WithEnumWireFormat(...)</c>), an <see cref="ObjectResult"/> is written by an output
/// formatter with the options of that format, and a <see cref="JsonResult"/> without its own settings gets those options.
/// </summary>
internal sealed class EnumWireFormatResultFilter(EnumWireFormatOutput output) : IResultFilter
{
    // Action and controller attributes, read once per action.
    private readonly ConditionalWeakTable<ActionDescriptor, IEnumWireFormatMetadata?[]> _attributes = [];

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (Select(context.ActionDescriptor, context.HttpContext) is not { } metadata)
            return;
        if (metadata.VaryHeader is { } header)
            context.HttpContext.Response.Headers.Append(Microsoft.Net.Http.Headers.HeaderNames.Vary, header);
        if (metadata.SelectFormat(context.HttpContext) == output.DefaultFormat)
            return;

        switch (context.Result)
        {
            case ObjectResult { Value: not (null or string) } objectResult when objectResult.Formatters.Count == 0:
                objectResult.Formatters.Add(output.MvcFormatter);
                break;
            case JsonResult { SerializerSettings: null } jsonResult:
                jsonResult.SerializerSettings = output.MvcOptions;
                break;
            default:
                break;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Nothing to do after the result is written.
    }

    private IEnumWireFormatMetadata? Select(ActionDescriptor action, HttpContext httpContext)
    {
        IEnumWireFormatMetadata?[] attributes = _attributes.GetValue(action, static a => a is ControllerActionDescriptor controllerAction
            ? [controllerAction.MethodInfo.GetCustomAttribute<EnumWireFormatAttribute>(inherit: true),
               controllerAction.ControllerTypeInfo.GetCustomAttribute<EnumWireFormatAttribute>(inherit: true)]
            : []);
        foreach (IEnumWireFormatMetadata? attribute in attributes)
        {
            if (attribute is not null)
                return attribute;
        }

        // Endpoint conventions (MapControllers().WithEnumWireFormat(...), groups) come after the attributes.
        return httpContext.GetEndpoint()?.Metadata.GetMetadata<IEnumWireFormatMetadata>();
    }
}
