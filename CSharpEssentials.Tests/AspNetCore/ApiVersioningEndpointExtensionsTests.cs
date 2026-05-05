using Asp.Versioning;
using Asp.Versioning.Builder;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.AspNetCore;

public class ApiVersioningEndpointExtensionsTests
{
    [Fact]
    public void CreateVersionSet_Should_Return_ApiVersionSet()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAndConfigureApiVersioning();
        WebApplication app = builder.Build();

        ApiVersionSet versionSet = app.CreateVersionSet(2);

        versionSet.Should().NotBeNull();
    }

    [Fact]
    public void CreateVersionedGroup_Should_Return_RouteGroupBuilder()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Services.AddAndConfigureApiVersioning();
        WebApplication app = builder.Build();

        RouteGroupBuilder group = app.CreateVersionedGroup("users", 1);

        group.Should().NotBeNull();
    }
}
