using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static CSharpEssentials.Tests.AspNetCore.EnumIntegration.EnumConventionsControllers;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>
/// <c>WithEnumWireFormat</c> and <see cref="EnumWireFormatAttribute"/>: per group, endpoint, controller and action output
/// formats in both directions, without changing the host's JSON options.
/// </summary>
public class EnumWireFormatTests
{
    private const string StatusString = "\"pending_approval\"";
    private const string StatusNumber = "1";
    private const string PermissionsNumber = "3";

    private static readonly Type[] WireControllers =
        [typeof(EcStringController), typeof(EcNumberController), typeof(EcNoAttributeController)];

    private static void MapVersions(WebApplication app)
    {
        app.MapGroup("/v1").WithEnumWireFormat(EnumWireFormat.Number).MapGet("/order", () => EcEcho.Order);
        app.MapGroup("/v2").WithEnumWireFormat(EnumWireFormat.String).MapGet("/order", () => EcEcho.Order);
        app.MapGet("/default/order", () => EcEcho.Order);
    }

    private static void RegisterNumber(IServiceCollection services) =>
        services.AddEnumConventions(EnumConventionsHost.WriteAs(EnumWireFormat.Number));

    // ---------- Minimal API groups ----------

    [Fact]
    public async Task VersionGroups_Should_WriteDifferentFormats_When_DtoIsTheSame()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: MapVersions);

        EcResponse v1 = await host.GetAsync("/v1/order");
        EcResponse v2 = await host.GetAsync("/v2/order");

        (v1.Json("status"), v1.Json("permissions"), v2.Json("status"))
            .Should().Be((StatusNumber, PermissionsNumber, StatusString));
    }

    [Fact]
    public async Task Endpoint_Should_UseGlobalFormat_When_NoWireFormatIsSet()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: MapVersions);

        EcResponse response = await host.GetAsync("/default/order");

        response.Json("status").Should().Be(StatusString);
    }

    [Fact]
    public async Task StringGroup_Should_OptOutOfGlobalNumber_When_WriteAsIsNumber()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(RegisterNumber, MapVersions);

        EcResponse v2 = await host.GetAsync("/v2/order");
        EcResponse fallback = await host.GetAsync("/default/order");

        (v2.Json("status"), fallback.Json("status")).Should().Be((StatusString, StatusNumber));
    }

    [Fact]
    public async Task Endpoint_Should_OverrideGroup_When_BothSetAWireFormat()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: app =>
        {
            RouteGroupBuilder group = app.MapGroup("/v1").WithEnumWireFormat(EnumWireFormat.Number);
            group.MapGet("/inherit", () => EcEcho.Order);
            group.MapGet("/string", () => EcEcho.Order).WithEnumWireFormat(EnumWireFormat.String);
        });

        EcResponse inherit = await host.GetAsync("/v1/inherit");
        EcResponse overridden = await host.GetAsync("/v1/string");

        (inherit.Json("status"), overridden.Json("status")).Should().Be((StatusNumber, StatusString));
    }

    [Theory]
    [InlineData("string", StatusString)]
    [InlineData("number", StatusNumber)]
    public async Task HeaderSelector_Should_SelectFormatPerRequest_When_HeaderIsSent(string header, string expected)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: app =>
            app.MapGet("/order", () => EcEcho.Order).WithEnumWireFormat("X-Enum-Format", SelectFromHeader));

        EcResponse response = await host.GetAsync("/order", ("X-Enum-Format", header));

        (response.Json("status"), response.Vary.Contains("X-Enum-Format")).Should().Be((expected, true));
    }

    [Fact]
    public async Task HeaderSelector_Should_AddVary_When_FormatIsTheDefault()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: app =>
            app.MapGet("/order", () => EcEcho.Order).WithEnumWireFormat("X-Enum-Format", SelectFromHeader));

        EcResponse response = await host.GetAsync("/order");

        response.Vary.Should().Contain("X-Enum-Format");
    }

    [Fact]
    public async Task NestedResult_Should_UseGroupFormat_When_EndpointReturnsResultsUnion()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: app =>
            app.MapGroup("/v1").WithEnumWireFormat(EnumWireFormat.Number)
                .MapGet("/union/{id:int}", Results<Ok<EcOrder>, NotFound> (int id) =>
                    id == 1 ? TypedResults.Ok(EcEcho.Order) : TypedResults.NotFound()));

        EcResponse found = await host.GetAsync("/v1/union/1");
        EcResponse missing = await host.GetAsync("/v1/union/2");

        (found.Json("status"), missing.Status).Should().Be((StatusNumber, 404));
    }

    [Fact]
    public async Task CreatedResult_Should_KeepStatusAndLocation_When_FormatChanges()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: app =>
            app.MapGroup("/v1").WithEnumWireFormat(EnumWireFormat.Number)
                .MapGet("/created", () => TypedResults.Created("/orders/1", EcEcho.Order)));

        EcResponse response = await host.GetAsync("/v1/created");

        (response.Status, response.Location, JsonStatus(response.Body)).Should().Be((201, "/orders/1", StatusNumber));
    }

    // ---------- MVC attributes and conventions ----------

    [Theory]
    [InlineData("/wire/string-controller/inherit", StatusString)]
    [InlineData("/wire/string-controller/action-number", StatusNumber)]
    [InlineData("/wire/string-controller/json-result", StatusNumber)]
    [InlineData("/wire/number-controller/inherit", StatusNumber)]
    [InlineData("/wire/number-controller/action-string", StatusString)]
    [InlineData("/wire/plain-controller/inherit", StatusString)]
    [InlineData("/wire/plain-controller/action-number", StatusNumber)]
    public async Task Mvc_Should_ApplyActionThenControllerAttribute_When_WriteAsIsString(string path, string expected)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(controllers: WireControllers);

        EcResponse response = await host.GetAsync(path);

        response.Json("status").Should().Be(expected);
    }

    [Theory]
    [InlineData("/wire/string-controller/inherit", StatusString)]
    [InlineData("/wire/string-controller/action-number", StatusNumber)]
    [InlineData("/wire/number-controller/inherit", StatusNumber)]
    [InlineData("/wire/number-controller/action-string", StatusString)]
    [InlineData("/wire/plain-controller/inherit", StatusNumber)]
    [InlineData("/wire/plain-controller/action-string", StatusString)]
    public async Task Mvc_Should_ApplyActionThenControllerAttribute_When_WriteAsIsNumber(string path, string expected)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(RegisterNumber, controllers: WireControllers);

        EcResponse response = await host.GetAsync(path);

        response.Json("status").Should().Be(expected);
    }

    [Theory]
    [InlineData("/wire/plain-controller/inherit", StatusNumber)]
    [InlineData("/wire/plain-controller/action-string", StatusString)]
    [InlineData("/wire/string-controller/inherit", StatusString)]
    public async Task Mvc_Should_PreferAttributesOverEndpointConvention_When_ConventionIsNumber(string path, string expected)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(
            mapControllers: c => c.WithEnumWireFormat(EnumWireFormat.Number), controllers: WireControllers);

        EcResponse response = await host.GetAsync(path);

        response.Json("status").Should().Be(expected);
    }

    [Theory]
    [InlineData("/wire/plain-controller/inherit", StatusString)]
    [InlineData("/wire/plain-controller/action-number", StatusNumber)]
    [InlineData("/wire/number-controller/inherit", StatusNumber)]
    public async Task Mvc_Should_PreferAttributesOverEndpointConvention_When_ConventionIsString(string path, string expected)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(
            RegisterNumber, mapControllers: c => c.WithEnumWireFormat(EnumWireFormat.String), controllers: WireControllers);

        EcResponse response = await host.GetAsync(path);

        response.Json("status").Should().Be(expected);
    }

    [Fact]
    public async Task MvcCreated_Should_KeepStatusAndLocation_When_FormatChanges()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(controllers: WireControllers);

        EcResponse response = await host.GetAsync("/wire/plain-controller/created");

        (response.Status, response.Location, JsonStatus(response.Body)).Should().Be((201, "/orders/1", StatusNumber));
    }

    [Fact]
    public async Task MvcHeaderSelector_Should_AddVaryAndSelectFormat_When_ConventionUsesHeader()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(
            mapControllers: c => c.WithEnumWireFormat("X-Enum-Format", SelectFromHeader), controllers: WireControllers);

        EcResponse response = await host.GetAsync("/wire/plain-controller/inherit", ("X-Enum-Format", "number"));

        (response.Json("status"), response.Vary.Contains("X-Enum-Format")).Should().Be((StatusNumber, true));
    }

    // ---------- host JSON options ----------

    [Fact]
    public async Task WireFormat_Should_NotChangeHostJsonOptions_When_OtherFormatWasWritten()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(map: MapVersions, controllers: WireControllers);
        await host.GetAsync("/v1/order");
        await host.GetAsync("/wire/number-controller/inherit");

        string http = JsonSerializer.Serialize(EcEcho.Order, host.Services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions);
        string mvc = JsonSerializer.Serialize(EcEcho.Order, host.Services.GetRequiredService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions);

        (JsonStatus(http), JsonStatus(mvc)).Should().Be((StatusString, StatusString));
    }

    private static EnumWireFormat SelectFromHeader(HttpContext context) =>
        context.Request.Headers["X-Enum-Format"] == "number" ? EnumWireFormat.Number : EnumWireFormat.String;

    private static string JsonStatus(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("status").GetRawText();
    }
}
