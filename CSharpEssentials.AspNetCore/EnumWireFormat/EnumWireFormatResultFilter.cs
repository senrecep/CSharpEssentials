using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// MVC result filter added by <see cref="EnumConventionsExtensions.AddEnumConventions"/>. When the action selects the format that is not
/// <see cref="EnumConventions.WriteAs"/> (action <see cref="EnumWireFormatAttribute"/> &gt; controller attribute &gt; endpoint
/// metadata such as <c>MapControllers().WithEnumWireFormat(...)</c>), an <see cref="ObjectResult"/> is written by the host's
/// output formatters with the JSON formatter swapped for one with the options of that format (content negotiation, XML and
/// 406 behave as before), and a <see cref="JsonResult"/> without its own settings gets those options.
/// </summary>
internal sealed class EnumWireFormatResultFilter(EnumWireFormatOutput output, IOptions<MvcOptions> mvcOptions) : IResultFilter
{
    private readonly Lazy<IOutputFormatter[]> _formatters = new(() => SwapJsonFormatter(mvcOptions.Value.OutputFormatters, output.MvcFormatter));

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
                foreach (IOutputFormatter formatter in _formatters.Value)
                    objectResult.Formatters.Add(formatter);
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

    private static IOutputFormatter[] SwapJsonFormatter(IEnumerable<IOutputFormatter> formatters, SystemTextJsonOutputFormatter replacement)
    {
        IOutputFormatter[] swapped = [.. formatters.Select(f => f is SystemTextJsonOutputFormatter ? replacement : f)];
        return Array.IndexOf(swapped, replacement) >= 0 ? swapped : [replacement];
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
