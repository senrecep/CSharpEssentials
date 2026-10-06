using System.Net;
using CSharpEssentials.Endpoints;
using CSharpEssentials.Tests.Fixtures.EndpointsA;
using CSharpEssentials.Tests.Fixtures.EndpointsB;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointRegistryTests
{
    [Fact]
    public async Task Registry_Should_Match_Direct_Mapping_When_Endpoint_Is_At_Root()
    {
        await using WebApplication direct = CreateDocumentedApp();
        RegistryEndpoints.Documented.Map(direct);
        await using WebApplication generated = CreateDocumentedApp();
        MapOnly(generated, typeof(RegistryEndpoints.Documented));

        EndpointShape expected = await DescribeAsync(direct);
        EndpointShape shape = await DescribeAsync(generated);

        shape.Should().BeEquivalentTo(expected);
        shape.Tags.Should().Equal("Docs");
        shape.Should().BeEquivalentTo(new
        {
            Route = "/documented/{id:int}",
            HttpMethod = "GET",
            GroupName = "v1",
            OperationId = "GetDocumented",
        });
    }

    [Fact]
    public async Task Registry_Should_Match_Direct_Mapping_When_Endpoint_Is_In_Nested_Groups()
    {
        await using WebApplication direct = CreateDocumentedApp();
        RouteGroupBuilder orders = direct.MapGroup(RegistryEndpoints.OrdersGroup.Prefix);
        RegistryEndpoints.OrdersGroup.Configure(orders);
        RouteGroupBuilder items = orders.MapGroup(RegistryEndpoints.OrderItemsGroup.Prefix);
        RegistryEndpoints.OrderItemsGroup.Configure(items);
        RegistryEndpoints.ListOrderItems.Map(items);
        await using WebApplication generated = CreateDocumentedApp();
        MapOnly(generated, typeof(RegistryEndpoints.ListOrderItems));

        EndpointShape expected = await DescribeAsync(direct);
        EndpointShape shape = await DescribeAsync(generated);

        shape.Should().BeEquivalentTo(expected);
        shape.Tags.Should().Equal("Orders");
        shape.Should().BeEquivalentTo(new
        {
            Route = "orders/{orderId:int}/items/",
            HttpMethod = "GET",
            GroupName = "v1",
            OperationId = "ListOrderItems",
        });
    }

    [Fact]
    public void Registry_Should_Apply_Conventions_In_Group_Type_Each_Order()
    {
        using WebApplication app = EndpointTestApp.Create();
        app.MapCSharpEssentialsTestsEndpoints(options => options
            .Filter(static type => type == typeof(RegistryEndpoints.ListOrderItems))
            .ConfigureEach(static (builder, _) => builder.WithMetadata(new OrderMarker("each"))));

        IReadOnlyList<object> metadata = EndpointTestApp.Single(app).Metadata;

        int outer = IndexOf(metadata, static m => m is OrderMarker { Name: "outer" });
        int inner = IndexOf(metadata, static m => m is OrderMarker { Name: "inner" });
        int type = IndexOf(metadata, static m => m is EndpointTypeMetadata);
        int each = IndexOf(metadata, static m => m is OrderMarker { Name: "each" });
        new[] { outer, inner, type, each }.Should().BeInAscendingOrder().And.NotContain(-1);
    }

    [Fact]
    public async Task Registry_Should_Route_Nested_Group_Endpoint()
    {
        await using WebApplication app = CreateServerApp();
        MapOnly(app, typeof(RegistryEndpoints.ListOrderItems));
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        string body = await client.GetStringAsync(new Uri("/orders/42/items", UriKind.Relative));

        body.Should().Be("42");
        EndpointTestApp.Single(app).Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType.Should().Be<RegistryEndpoints.ListOrderItems>();
    }

    [Fact]
    public async Task Registry_Should_Keep_Filters_Added_Inside_Map()
    {
        await using WebApplication app = CreateServerApp();
        MapOnly(app, typeof(RegistryEndpoints.Filtered));
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/filtered", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(RegistryEndpoints.FilterHeader).Should().Equal("applied");
    }

    [Fact]
    public async Task Registry_Should_Keep_Authorization_Added_Inside_Map()
    {
        await using WebApplication app = CreateServerApp();
        MapOnly(app, typeof(RegistryEndpoints.Secured));
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/secured", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MapAllEndpoints_Should_Map_Endpoints_From_Referenced_Assemblies()
    {
        await using WebApplication app = CreateServerApp();
        app.MapAllEndpoints();
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        string fixtureA = await client.GetStringAsync(new Uri("/fixture-a/ping", UriKind.Relative));
        string fixtureB = await client.GetStringAsync(new Uri("/fixture-b/ping", UriKind.Relative));
        string fixtureBInA = await client.GetStringAsync(new Uri("/fixture-a/from-b", UriKind.Relative));

        fixtureA.Should().Be("a-pong");
        fixtureB.Should().Be("b-pong");
        fixtureBInA.Should().Be("b-in-a");
    }

    [Fact]
    public void MapAllEndpoints_Should_Map_Each_Endpoint_Type_Once_In_Registry_Order()
    {
        using WebApplication app = EndpointTestApp.Create();

        app.MapAllEndpoints();

        Type[] mapped = [.. EndpointTestApp.Endpoints(app).Select(static endpoint => endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType)];
        mapped.Should().OnlyHaveUniqueItems();
        mapped.Should().Equal(
        [
            .. CSharpEssentialsTestsEndpointRegistry.EndpointTypes,
            .. CSharpEssentialsTestsFixturesEndpointsAEndpointRegistry.EndpointTypes,
            .. CSharpEssentialsTestsFixturesEndpointsBEndpointRegistry.EndpointTypes,
        ]);
        mapped.Should().Contain([typeof(FixtureAPing), typeof(FixtureBPing), typeof(FixtureBInAGroup)]);
    }

    [Fact]
    public async Task MapAllEndpoints_Should_Coexist_With_Mvc_Controllers()
    {
        WebApplicationBuilder builder = CreateServerBuilder();
        builder.Services.AddControllers().ConfigureApplicationPartManager(static manager =>
        {
            manager.ApplicationParts.Clear();
            manager.ApplicationParts.Add(new AssemblyPart(typeof(FixtureAController).Assembly));
        });
        await using WebApplication app = builder.Build();
        app.MapAllEndpoints(static options => options.Filter(static type => type.Assembly == typeof(FixtureAPing).Assembly));
        app.MapControllers();
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        string minimal = await client.GetStringAsync(new Uri("/fixture-a/ping", UriKind.Relative));
        string mvc = await client.GetStringAsync(new Uri("/mvc/fixture-a", UriKind.Relative));

        minimal.Should().Be("a-pong");
        mvc.Should().Be("a-mvc");
    }

    private static void MapOnly(IEndpointRouteBuilder app, Type endpointType) =>
        app.MapCSharpEssentialsTestsEndpoints(options => options.Filter(type => type == endpointType));

    private static WebApplication CreateDocumentedApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(static options => options.SwaggerDoc("v1", new OpenApiInfo { Title = "Endpoints", Version = "v1" }));
        return builder.Build();
    }

    private static WebApplicationBuilder CreateServerBuilder()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, null);
        builder.Services.AddAuthorization();
        return builder;
    }

    private static WebApplication CreateServerApp()
    {
        WebApplication app = CreateServerBuilder().Build();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    private static async Task<EndpointShape> DescribeAsync(WebApplication app)
    {
        await app.StartAsync();
        RouteEndpoint endpoint = EndpointTestApp.Single(app);
        ApiDescription description = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(static group => group.Items).Single();
        OpenApiOperation operation = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1")
            .Paths.Values.Single().Operations.Values.Single();

        return new EndpointShape(
            endpoint.RoutePattern.RawText!,
            description.HttpMethod!,
            description.GroupName,
            operation.OperationId,
            [.. operation.Tags.Select(static tag => tag.Name)]);
    }

    private static int IndexOf(IReadOnlyList<object> metadata, Func<object, bool> predicate)
    {
        for (int index = 0; index < metadata.Count; index++)
        {
            if (predicate(metadata[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private sealed record EndpointShape(string Route, string HttpMethod, string? GroupName, string OperationId, string[] Tags);
}
