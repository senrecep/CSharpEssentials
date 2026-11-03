using System.ComponentModel;
using System.Net;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

public class SwashbuckleEnumDefaultTests
{
    [Fact]
    public async Task Document_Should_OmitTheDefault_When_DefaultValueIsAnUndefinedEnumValue()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnumConventions();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleEnumDefaultTests).Assembly);

        await using WebApplication app = builder.Build();
        app.UseSwagger();
        app.MapPost("/undefined-default", static (UndefinedDefaultOrder order) => order);
        await app.StartAsync();

        using HttpResponseMessage response = await app.GetTestClient().GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonObject schemas = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["components"]!["schemas"]!.AsObject();
        JsonNode property = schemas.Single(static pair => pair.Key.EndsWith(nameof(UndefinedDefaultOrder), StringComparison.Ordinal)).Value!["properties"]!["status"]!;
        property.ToJsonString().Should().NotContain("\"default\"");
        await app.StopAsync();
    }

    internal sealed class UndefinedDefaultOrder
    {
        [DefaultValue((SampleStatus)42)]
        public SampleStatus Status { get; init; }
    }
}
