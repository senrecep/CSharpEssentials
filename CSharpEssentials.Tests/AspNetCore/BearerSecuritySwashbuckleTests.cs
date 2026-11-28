using System.Reflection;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// <see cref="SwaggerBearerSecurityExtensions.AddBearerSecurityScheme"/> on Minimal API and controller endpoints. The
/// document is compared with the <c>bearer-security</c> golden files written by the Microsoft.AspNetCore.OpenApi tests, so
/// both packages describe bearer security the same way.
/// </summary>
public sealed class BearerSecuritySwashbuckleTests
{
    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, null)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, "openapi31")]
    public async Task AddBearerSecurityScheme_Should_MatchTheOpenApiGolden_When_SecuringOnlyAuthorizedOperations(OpenApiSpecVersion version, string? suffix)
    {
        string golden = await File.ReadAllTextAsync(OpenApiGolden.PathOf("bearer-security", suffix));

        string json = await GetJsonAsync(version, static options => options.AddBearerSecurityScheme());

        BearerSecurityApi.Extract(json).Should().Be(golden.Replace("\r\n", "\n", StringComparison.Ordinal));
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
        Action act = () => new SwaggerGenOptions().AddBearerSecurityScheme(" ");

        act.Should().Throw<ArgumentException>().WithParameterName("schemeName");
    }

    [Fact]
    public void AddBearerSecurityScheme_Should_Throw_When_TheBearerFormatIsBlank()
    {
        Action act = () => new SwaggerGenOptions().AddBearerSecurityScheme("Bearer", " ");

        act.Should().Throw<ArgumentException>().WithParameterName("bearerFormat");
    }

    [Fact]
    public async Task AddBearerSecurityScheme_Should_ReplaceASchemeAlreadyRegisteredUnderTheSameName()
    {
        string json = await GetJsonAsync(OpenApiSpecVersion.OpenApi3_1, static options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
            });
            options.AddBearerSecurityScheme();
        });
        JsonNode root = JsonNode.Parse(json)!;

        root["components"]!["securitySchemes"]!["Bearer"]!.ToJsonString().Should().Be(
            """{"type":"http","scheme":"bearer","bearerFormat":"JWT"}""");
    }

    private static async Task<string> GetJsonAsync(OpenApiSpecVersion version, Action<SwaggerGenOptions> configure)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(BearerSecuritySwashbuckleTests).Assembly.GetName().Name,
        });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthorization();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>();
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(static manager =>
            {
                foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new ControllersFeatureProvider());
            });
        builder.Services.Configure<SwaggerGenOptions>(options =>
        {
            options.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly);
            configure(options);
        });

        await using WebApplication app = builder.Build();
        app.MapControllers();
        app.MapGet("/minimal/{id:int?}", static (int? id) => id).WithName("GetMinimal");
        app.MapGet("/minimal-default/{page=1}", static (int page) => page).WithName("GetMinimalDefault");
        app.MapGet("/minimal-all/{*rest}", static (string? rest) => rest).WithName("GetMinimalRest");
        BearerSecurityApi.MapEndpoints(app);
        await app.StartAsync();

        OpenApiDocument document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        return await document.SerializeAsJsonAsync(version);
    }

    private sealed class ControllersFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (TypeInfo info in BearerSecurityApi.Controllers.Select(static controller => controller.GetTypeInfo()).Where(info => !feature.Controllers.Contains(info)))
                feature.Controllers.Add(info);
        }
    }
}
