using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed class SwashbuckleIdempotencyDeclaredContractTests
{
    private const string Document = "declared";

    [Fact]
    public async Task KeyHeader_Should_ReuseTheDeclaredParameter_When_TheEndpointBindsTheHeader()
    {
        JsonNode operation = (await GetPathsAsync())["/declared/own-header"]!["post"]!;

        JsonNode header = operation["parameters"]!.AsArray()
            .Single(static parameter => (string?)parameter!["in"] == "header")!;
        ((string?)header["name"]).Should().Be("Idempotency-Key");
        ((bool?)header["required"]).Should().BeTrue();
        ((string?)header["description"]).Should().StartWith("Makes the request safe to retry");
    }

    [Fact]
    public async Task ProblemResponse_Should_BeAddedNextToTheDeclaredContent_When_TheEndpointDeclaresTheStatusCode()
    {
        JsonNode response = (await GetPathsAsync())["/declared/text-422"]!["post"]!["responses"]!["422"]!;

        response["content"]!.AsObject().Select(static pair => pair.Key).Should().BeEquivalentTo("text/plain", "application/problem+json");
        response["content"]!["application/problem+json"]!["schema"]!["properties"]!["code"]!["enum"]!.AsArray()
            .Select(static code => (string?)code).Should().Equal("Idempotency.KeyReused");
        ((string?)response["description"]).Should().EndWith("Send a new key for a different request.");
    }

    private static async Task<JsonNode> GetPathsAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddIdempotency();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleIdempotencyDeclaredContractTests).Assembly);
        builder.Services.ConfigureSwaggerGen(static options => options.SwaggerDoc(Document, new OpenApiInfo { Title = Document, Version = "1" }));

        await using WebApplication app = builder.Build();
        RouteGroupBuilder group = app.MapGroup("/declared").WithGroupName(Document);
        group.MapPost("/own-header", static ([FromHeader(Name = "Idempotency-Key")] string? key) => 1)
            .WithIdempotency(static options => options.RequireKey = true);
        group.MapPost("/text-422", static () => 1).WithIdempotency().Produces<string>(StatusCodes.Status422UnprocessableEntity, "text/plain");
        await app.StartAsync();
        try
        {
            OpenApiDocument document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(Document);
            return JsonNode.Parse(await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
