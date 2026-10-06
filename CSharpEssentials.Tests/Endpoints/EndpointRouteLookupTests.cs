using CSharpEssentials.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointRouteLookupTests
{
    [Fact]
    public async Task RouteOf_Should_Return_Routable_Path_When_Endpoint_Is_In_Group()
    {
        await using WebApplication app = CreateApp(typeof(RouteLookupEndpoints.GetLookupItem));
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        string path = app.RouteOf<RouteLookupEndpoints.GetLookupItem>(new { tenant = "acme", id = 42 });
        string body = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        path.Should().Be("/lookup/acme/items/42");
        body.Should().Be("\"acme:42\"");
    }

    [Fact]
    public async Task RouteOf_Should_Append_Query_String_When_Value_Has_No_Route_Parameter()
    {
        await using WebApplication app = CreateApp(typeof(RouteLookupEndpoints.GetLookupItem));

        string path = app.RouteOf<RouteLookupEndpoints.GetLookupItem>(new { tenant = "acme", id = 1, page = 2 });

        path.Should().Be("/lookup/acme/items/1?page=2");
    }

    [Fact]
    public async Task RouteOf_Should_Select_Route_When_Http_Method_Is_Given()
    {
        await using WebApplication app = CreateCommandsApp();
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        string path = app.RouteOf<RouteLookupEndpoints.LookupItemCommands>("delete", new { tenant = "acme", id = 7 });
        using HttpResponseMessage response = await client.DeleteAsync(new Uri(path, UriKind.Relative));

        path.Should().Be("/lookup/acme/commands/7/remove");
        (await response.Content.ReadAsStringAsync()).Should().Be("\"removed:7\"");
    }

    [Fact]
    public async Task RouteOf_Should_Select_Route_When_Endpoint_Name_Is_Given()
    {
        await using WebApplication app = CreateCommandsApp();

        string path = app.RouteOf<RouteLookupEndpoints.LookupItemCommands>("UpdateLookupItem", new { tenant = "acme", id = 7 });

        path.Should().Be("/lookup/acme/commands/7");
    }

    [Fact]
    public async Task RouteOf_Should_Throw_When_Endpoint_Maps_Several_Routes()
    {
        await using WebApplication app = CreateCommandsApp();

        Action act = () => app.RouteOf<RouteLookupEndpoints.LookupItemCommands>(new { tenant = "acme", id = 7 });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*maps 2 routes*'lookup/{tenant}/commands/{id:int}'*'lookup/{tenant}/commands/{id:int}/remove'*Pass an endpoint name or HTTP method*");
    }

    [Fact]
    public async Task RouteOf_Should_Throw_When_Endpoint_Is_Not_Mapped()
    {
        await using WebApplication app = CreateApp(typeof(RouteLookupEndpoints.GetLookupItem));

        Action act = () => app.RouteOf<RouteLookupEndpoints.LookupItemCommands>(new { tenant = "acme", id = 7 });

        act.Should().Throw<InvalidOperationException>().WithMessage("No route is mapped for endpoint '*LookupItemCommands'*");
    }

    [Fact]
    public async Task RouteOf_Should_Throw_When_Selection_Matches_No_Route()
    {
        await using WebApplication app = CreateCommandsApp();

        Action act = () => app.RouteOf<RouteLookupEndpoints.LookupItemCommands>("POST", new { tenant = "acme", id = 7 });

        act.Should().Throw<InvalidOperationException>().WithMessage("No route matching 'POST' is mapped*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public async Task RouteOf_Should_Throw_When_Route_Value_Is_Missing_Or_Invalid(string? id)
    {
        await using WebApplication app = CreateApp(typeof(RouteLookupEndpoints.GetLookupItem));

        Action act = () => app.RouteOf<RouteLookupEndpoints.GetLookupItem>(new { tenant = "acme", id });

        act.Should().Throw<InvalidOperationException>().WithMessage("Route 'lookup/{tenant}/items/{id:int}' of endpoint '*GetLookupItem' needs route values*");
    }

    private static WebApplication CreateCommandsApp()
    {
        WebApplication app = CreateApp(typeof(RouteLookupEndpoints.GetLookupItem));
        RouteGroupBuilder group = app.MapGroup(RouteLookupEndpoints.LookupGroup.Prefix)
            .WithMetadata(new EndpointTypeMetadata(typeof(RouteLookupEndpoints.LookupItemCommands)));
        RouteLookupEndpoints.LookupItemCommands.Map(group);
        return app;
    }

    private static WebApplication CreateApp(Type endpointType)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        WebApplication app = builder.Build();
        app.MapCSharpEssentialsTestsEndpoints(options => options.Filter(type => type == endpointType));
        return app;
    }
}
