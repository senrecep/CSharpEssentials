using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The endpoints of the cross-layer golden table: per <see cref="CrossLayerRow.Shape"/>, <c>/golden/{shape}/route/{value}</c>,
/// <c>/golden/{shape}/query?value=</c> and <c>/golden/{shape}/header</c> (<see cref="Header"/>) echo the bound value. String
/// shapes belong to the <c>v2</c> document, the number shape to <c>v1</c> with <see cref="EnumWireFormat.Number"/>.
/// </summary>
public static class CrossLayerApi
{
    public const string Header = "X-Value";

    public static IEndpointRouteBuilder MapCrossLayerApi(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder plain = Group(app, "Plain", "v2");
        plain.MapGet("/route/{value}", static (GoldenPlain value) => value);
        plain.MapGet("/query", static ([FromQuery] GoldenPlain value) => value);
        plain.MapGet("/header", static ([FromHeader(Name = Header)] GoldenPlain value) => value);

        RouteGroupBuilder status = Group(app, "Status", "v2");
        status.MapGet("/route/{value}", static (GoldenOrderStatus value) => value);
        status.MapGet("/query", static ([FromQuery] GoldenOrderStatus value) => value);
        status.MapGet("/header", static ([FromHeader(Name = Header)] GoldenOrderStatus value) => value);

        RouteGroupBuilder naming = Group(app, "Naming", "v2");
        naming.MapGet("/route/{value}", static (GoldenNaming value) => value);
        naming.MapGet("/query", static ([FromQuery] GoldenNaming value) => value);
        naming.MapGet("/header", static ([FromHeader(Name = Header)] GoldenNaming value) => value);

        RouteGroupBuilder permissions = Group(app, "Permissions", "v2");
        permissions.MapGet("/route/{value}", static (GoldenPermissions value) => value);
        permissions.MapGet("/query", static ([FromQuery] GoldenPermissions value) => value);
        permissions.MapGet("/header", static ([FromHeader(Name = Header)] GoldenPermissions value) => value);

        RouteGroupBuilder priority = Group(app, "Priority", "v1").WithEnumWireFormat(EnumWireFormat.Number);
        priority.MapGet("/route/{value}", static (GoldenPriority value) => value);
        priority.MapGet("/query", static ([FromQuery] GoldenPriority value) => value);
        priority.MapGet("/header", static ([FromHeader(Name = Header)] GoldenPriority value) => value);

        RouteGroupBuilder optional = Group(app, "OptionalStatus", "v2");
        optional.MapGet("/route/{value?}", static (GoldenOrderStatus? value) => value);
        optional.MapGet("/query", static ([FromQuery] GoldenOrderStatus? value) => value);
        optional.MapGet("/header", static ([FromHeader(Name = Header)] GoldenOrderStatus? value) => value);
        return app;
    }

    private static RouteGroupBuilder Group(IEndpointRouteBuilder app, string shape, string document) =>
        app.MapGroup("/golden/" + shape).WithGroupName(document);
}
