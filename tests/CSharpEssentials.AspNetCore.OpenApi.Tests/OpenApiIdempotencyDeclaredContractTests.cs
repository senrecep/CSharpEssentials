using System.Text.Json.Nodes;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public sealed class OpenApiIdempotencyDeclaredContractTests
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
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddIdempotency();
        builder.Services.AddOpenApi(Document, static options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            options.AddIdempotencyConventions();
        });

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        RouteGroupBuilder group = app.MapGroup("/declared").WithGroupName(Document);
        group.MapPost("/own-header", static ([FromHeader(Name = "Idempotency-Key")] string? key) => 1)
            .WithIdempotency(static options => options.RequireKey = true);
        group.MapPost("/text-422", static () => 1).WithIdempotency().Produces<string>(StatusCodes.Status422UnprocessableEntity, "text/plain");
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            return JsonNode.Parse(await client.GetStringAsync($"/openapi/{Document}.json"))!["paths"]!;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
