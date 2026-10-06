using System.Net;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore;

public class ApiVersioningVersionedGroupTests
{
    [Fact]
    public async Task MapVersionedGroup_Should_Route_Requests_With_Version_Segment()
    {
        await using WebApplication app = CreateApp();
        app.MapVersionedGroup(2).MapGet("/ping", static () => "pong");
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage matching = await client.GetAsync(new Uri("/v2/ping", UriKind.Relative));
        using HttpResponseMessage other = await client.GetAsync(new Uri("/v1/ping", UriKind.Relative));

        matching.StatusCode.Should().Be(HttpStatusCode.OK);
        (await matching.Content.ReadAsStringAsync()).Should().Be("pong");
        other.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MapVersionedGroup_Should_Set_ApiExplorer_Group_Name_From_Version()
    {
        await using WebApplication app = CreateApp();
        app.MapVersionedGroup(2).MapGet("/ping", static () => "pong");
        await app.StartAsync();

        ApiDescription description = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(static group => group.Items).Single();

        description.GroupName.Should().Be("v2");
        description.RelativePath.Should().Be("v2/ping");
    }

    [Fact]
    public void AspNetCore_Should_Not_Reference_Endpoints_Package()
    {
        string?[] references = [.. typeof(Extensions).Assembly.GetReferencedAssemblies().Select(static name => name.Name)];

        references.Should().NotContain("CSharpEssentials.Endpoints");
    }

    private static WebApplication CreateApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAndConfigureApiVersioning();
        return builder.Build();
    }
}
