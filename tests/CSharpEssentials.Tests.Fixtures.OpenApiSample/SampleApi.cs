using System.Reflection;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The sample API. Documents (by group name): <c>v1</c> writes numbers, <c>v2</c> strings, <c>header</c> selects the format
/// by the <c>X-Enum-Format</c> request header, <c>mixed</c> has one number operation among string ones, <c>mvc</c> is a
/// controller with a number action.
/// </summary>
public static class SampleApi
{
    public const string FormatHeader = "X-Enum-Format";

    public static IReadOnlyList<string> Documents { get; } = ["v1", "v2", "header", "mixed", "mvc"];

    public static IEndpointRouteBuilder MapSampleApi(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        MapOrders(app.MapGroup("/v1").WithGroupName("v1").WithEnumWireFormat(EnumWireFormat.Number));
        RouteGroupBuilder v2 = app.MapGroup("/v2").WithGroupName("v2");
        MapOrders(v2);
        v2.MapGet("/changes/latest", SampleHandlers.GetChange);
        MapOrders(app.MapGroup("/header").WithGroupName("header").WithEnumWireFormat(FormatHeader, SelectFormat));

        RouteGroupBuilder mixed = app.MapGroup("/mixed").WithGroupName("mixed");
        mixed.MapGet("/orders/{status}", SampleHandlers.GetOrder);
        mixed.MapGet("/legacy/{status}", SampleHandlers.GetOrder).WithEnumWireFormat(EnumWireFormat.Number);
        return app;
    }

    /// <summary>Adds <see cref="SampleOrdersController"/> only (it is internal, so no other host discovers it).</summary>
    public static IMvcBuilder AddSampleControllers(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureApplicationPartManager(static manager =>
            manager.FeatureProviders.Add(new SampleControllerFeatureProvider()));
    }

    private static EnumWireFormat SelectFormat(HttpContext context) =>
        string.Equals(context.Request.Headers[FormatHeader], "number", StringComparison.OrdinalIgnoreCase)
            ? EnumWireFormat.Number
            : EnumWireFormat.String;

    private static void MapOrders(RouteGroupBuilder group)
    {
        group.MapGet("/orders/{status}", SampleHandlers.GetOrder);
        group.MapPost("/orders", SampleHandlers.CreateOrder);
        group.MapGet("/statuses", SampleHandlers.GetStatuses);
        group.MapPost("/status", SampleHandlers.EchoStatus);
        group.MapGet("/plain", SampleHandlers.GetPlain);
    }

    private sealed class SampleControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo controller = typeof(SampleOrdersController).GetTypeInfo();
            if (!feature.Controllers.Contains(controller))
                feature.Controllers.Add(controller);
        }
    }
}
