using System.Net;
using CSharpEssentials.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointAuthorizationExtensionsTests
{
    private static readonly Uri Path = new("/secure", UriKind.Relative);

    public static TheoryData<string[]> InvalidValues =>
    [
        [],
        [" "],
        ["admin", ""],
        ["admin,editor"],
        ["admin", null!],
    ];

    [Fact]
    public async Task RequireRoles_Should_Add_AuthorizeData_With_Joined_Roles()
    {
        await using WebApplication app = EndpointTestApp.Create();
        app.MapGet("/secure", static () => "ok").RequireRoles("admin", "editor");

        IAuthorizeData data = EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<IAuthorizeData>().Single();

        data.Roles.Should().Be("admin,editor");
        data.Policy.Should().BeNull();
        data.AuthenticationSchemes.Should().BeNull();
    }

    [Fact]
    public async Task RequirePolicies_Should_Add_One_AuthorizeData_Per_Policy()
    {
        await using WebApplication app = EndpointTestApp.Create();
        app.MapGet("/secure", static () => "ok").RequirePolicies("CanRead", "CanWrite");

        IReadOnlyList<IAuthorizeData> data = EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<IAuthorizeData>();

        data.Select(static item => item.Policy).Should().Equal("CanRead", "CanWrite");
        data.Should().OnlyContain(static item => item.Roles == null && item.AuthenticationSchemes == null);
    }

    [Fact]
    public async Task RequireAuthSchemes_Should_Add_AuthorizeData_With_Joined_Schemes()
    {
        await using WebApplication app = EndpointTestApp.Create();
        app.MapGet("/secure", static () => "ok").RequireAuthSchemes("Bearer", "Cookies");

        IAuthorizeData data = EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<IAuthorizeData>().Single();

        data.AuthenticationSchemes.Should().Be("Bearer,Cookies");
        data.Roles.Should().BeNull();
        data.Policy.Should().BeNull();
    }

    [Fact]
    public async Task Shortcuts_Should_Combine_Into_Single_Policy_When_Applied_To_Group()
    {
        await using WebApplication app = CreateServerApp();
        RouteGroupBuilder group = app.MapGroup("/group")
            .RequireRoles("admin", "editor")
            .RequireAuthSchemes(HeaderAuthenticationHandler.SchemeName);
        group.MapGet("/secure", static () => "ok").RequirePolicies("CanRead");
        IAuthorizationPolicyProvider provider = app.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        AuthorizationPolicy? policy = await AuthorizationPolicy.CombineAsync(
            provider,
            EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<IAuthorizeData>());

        policy.Should().NotBeNull();
        policy.AuthenticationSchemes.Should().Equal(HeaderAuthenticationHandler.SchemeName);
        policy.Requirements.OfType<RolesAuthorizationRequirement>().Single().AllowedRoles.Should().Equal("admin", "editor");
        policy.Requirements.OfType<ClaimsAuthorizationRequirement>().Single().ClaimType.Should().Be("read");
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("viewer", HttpStatusCode.Forbidden)]
    [InlineData("editor", HttpStatusCode.OK)]
    [InlineData("viewer,admin", HttpStatusCode.OK)]
    public async Task RequireRoles_Should_Allow_Any_Listed_Role(string? roles, HttpStatusCode expected)
    {
        await using WebApplication app = CreateServerApp();
        app.MapGet("/secure", static () => "ok").RequireRoles("admin", "editor");

        HttpStatusCode status = await SendAsync(app, roles, claims: null);

        status.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("read", HttpStatusCode.Forbidden)]
    [InlineData("read,write", HttpStatusCode.OK)]
    public async Task RequirePolicies_Should_Require_Every_Policy(string? claims, HttpStatusCode expected)
    {
        await using WebApplication app = CreateServerApp();
        app.MapGet("/secure", static () => "ok").RequirePolicies("CanRead", "CanWrite");

        HttpStatusCode status = await SendAsync(app, roles: null, claims);

        status.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public async Task Shortcuts_Should_Throw_When_Values_Are_Invalid(string[] values)
    {
        await using WebApplication app = EndpointTestApp.Create();
        RouteHandlerBuilder builder = app.MapGet("/secure", static () => "ok");

        Action roles = () => builder.RequireRoles(values);
        Action policies = () => builder.RequirePolicies(values);
        Action schemes = () => builder.RequireAuthSchemes(values);

        roles.Should().Throw<ArgumentException>().WithParameterName("roles");
        policies.Should().Throw<ArgumentException>().WithParameterName("policies");
        schemes.Should().Throw<ArgumentException>().WithParameterName("schemes");
    }

    [Fact]
    public async Task Shortcuts_Should_Throw_When_Arguments_Are_Null()
    {
        RouteHandlerBuilder builder = null!;
        await using WebApplication app = EndpointTestApp.Create();
        RouteHandlerBuilder mapped = app.MapGet("/secure", static () => "ok");

        Action nullBuilder = () => builder.RequireRoles("admin");
        Action nullValues = () => mapped.RequirePolicies(null!);

        nullBuilder.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        nullValues.Should().Throw<ArgumentNullException>().WithParameterName("policies");
    }

    private static WebApplication CreateServerApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(HeaderAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(HeaderAuthenticationHandler.SchemeName, null);
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("CanRead", static policy => policy.RequireClaim("read"))
            .AddPolicy("CanWrite", static policy => policy.RequireClaim("write"));
        WebApplication app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    private static async Task<HttpStatusCode> SendAsync(WebApplication app, string? roles, string? claims)
    {
        await app.StartAsync();
        using HttpClient client = app.GetTestClient();
        using HttpRequestMessage request = new(HttpMethod.Get, Path);
        if (roles is not null)
        {
            request.Headers.Add(HeaderAuthenticationHandler.RolesHeader, roles);
        }

        if (claims is not null)
        {
            request.Headers.Add(HeaderAuthenticationHandler.ClaimsHeader, claims);
        }

        using HttpResponseMessage response = await client.SendAsync(request);
        return response.StatusCode;
    }
}
