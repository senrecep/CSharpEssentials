using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// <see cref="OpenApiBearerSecurityExtensions.AddBearerSecurityScheme"/> on Minimal API and controller endpoints. The goldens
/// hold <c>components.securitySchemes</c> and the <c>security</c> of every operation (<c>null</c> when it has none).
/// </summary>
public sealed class BearerSecurityOpenApiTests
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, null)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, "openapi31")]
    public async Task AddBearerSecurityScheme_Should_SecureOnlyAuthorizedOperations(OpenApiSpecVersion version, string? suffix)
    {
        string json = await GetJsonAsync(version, static options => options.AddBearerSecurityScheme());

        Golden.Verify(OpenApiGolden.PathOf("bearer-security", suffix), Extract(json));
    }

    [Fact]
    public async Task AddBearerSecurityScheme_Should_UseTheConfiguredNameFormatAndDescription()
    {
        string json = await GetJsonAsync(OpenApiSpecVersion.OpenApi3_1, static options =>
            options.AddBearerSecurityScheme("Jwt", "opaque", "Access token from the identity server."));
        JsonNode root = JsonNode.Parse(json)!;

        JsonObject schemes = root["components"]!["securitySchemes"]!.AsObject();
        schemes.Select(static scheme => scheme.Key).Should().Equal("Jwt");
        schemes["Jwt"]!.ToJsonString().Should().Be(
            """{"type":"http","description":"Access token from the identity server.","scheme":"bearer","bearerFormat":"opaque"}""");
        root["paths"]!["/bearer/secure"]!["get"]!["security"]!.ToJsonString().Should().Be("""[{"Jwt":[]}]""");
    }

    [Fact]
    public async Task AddBearerSecurityScheme_Should_AddOneRequirement_When_CalledTwice()
    {
        string json = await GetJsonAsync(OpenApiSpecVersion.OpenApi3_1, static options =>
            options.AddBearerSecurityScheme().AddBearerSecurityScheme());
        JsonNode root = JsonNode.Parse(json)!;

        root["paths"]!["/bearer/secure"]!["get"]!["security"]!.ToJsonString().Should().Be("""[{"Bearer":[]}]""");
    }

    [Fact]
    public async Task Document_Should_HaveNoSecurity_When_AddBearerSecuritySchemeIsNotCalled()
    {
        string json = await GetJsonAsync(OpenApiSpecVersion.OpenApi3_1, static _ => { });

        json.Should().NotContain("security");
    }

    [Fact]
    public void AddBearerSecurityScheme_Should_Throw_When_TheSchemeNameIsBlank()
    {
        Action act = () => new OpenApiOptions().AddBearerSecurityScheme(" ");

        act.Should().Throw<ArgumentException>().WithParameterName("schemeName");
    }

    [Fact]
    public void AddBearerSecurityScheme_Should_Throw_When_TheBearerFormatIsBlank()
    {
        Action act = () => new OpenApiOptions().AddBearerSecurityScheme("Bearer", " ");

        act.Should().Throw<ArgumentException>().WithParameterName("bearerFormat");
    }

    [Fact]
    public async Task AddBearerSecurityScheme_Should_ReplaceASchemeAlreadyRegisteredUnderTheSameName()
    {
        string json = await GetJsonAsync(OpenApiSpecVersion.OpenApi3_1, static options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Name = "X-Api-Key",
                    In = ParameterLocation.Header,
                };
                return Task.CompletedTask;
            });
            options.AddBearerSecurityScheme();
        });
        JsonNode root = JsonNode.Parse(json)!;

        root["components"]!["securitySchemes"]!["Bearer"]!.ToJsonString().Should().Be(
            """{"type":"http","scheme":"bearer","bearerFormat":"JWT"}""");
    }

    private static async Task<string> GetJsonAsync(OpenApiSpecVersion version, Action<OpenApiOptions> configure) =>
        (await OptionalRouteParameterOpenApiHost.GetJsonDocumentsAsync(
            version,
            [OptionalRouteParameterOpenApiHost.DocumentName],
            configure,
            controllers: [typeof(BearerSecurityController)],
            configureServices: static services => services.AddAuthorization(),
            configureApp: MapEndpoints))[OptionalRouteParameterOpenApiHost.DocumentName];

    private static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/bearer/secure", static () => "secure").RequireAuthorization();
        app.MapGet("/bearer/anonymous", static () => "anonymous").RequireAuthorization().AllowAnonymous();
        app.MapGet("/bearer/plain", static () => "plain");
        RouteGroupBuilder group = app.MapGroup("/bearer/group").RequireAuthorization();
        group.MapGet("/secure", static () => "secure");
        group.MapGet("/anonymous", static () => "anonymous").AllowAnonymous();
    }

    private static string Extract(string json)
    {
        JsonNode root = JsonNode.Parse(json)!;
        var operations = new JsonObject();
        foreach ((string path, JsonNode? item) in root["paths"]!.AsObject().OrderBy(static p => p.Key, StringComparer.Ordinal))
        {
            foreach ((string method, JsonNode? operation) in item!.AsObject())
                operations[$"{method.ToUpperInvariant()} {path}"] = operation!["security"]?.DeepClone();
        }

        var extract = new JsonObject
        {
            ["securitySchemes"] = root["components"]?["securitySchemes"]?.DeepClone(),
            ["operations"] = operations,
        };
        return extract.ToJsonString(_indented).Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}

[ApiController]
[Authorize]
[Route("bearer/controller")]
internal sealed class BearerSecurityController : ControllerBase
{
    [HttpGet("secure")]
    public string Secure() => "secure";

    [HttpGet("anonymous")]
    [AllowAnonymous]
    public string Anonymous() => "anonymous";
}
