using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public sealed class IfMatchOpenApiTests
{
    [Theory]
    [InlineData("/if-match/optional")]
    [InlineData("/if-match/mvc/optional")]
    public async Task Responses_Should_ListBadRequestAndPreconditionFailed_When_TheHeaderIsOptional(string path)
    {
        JsonNode responses = (await GetPathsAsync())[path]!["put"]!["responses"]!;

        StatusCodes(responses).Should().Contain(["200", "400", "412"]).And.NotContain("428");
        ((string?)responses["412"]!["content"]!.AsObject().Single().Key).Should().Be("application/problem+json");
    }

    [Theory]
    [InlineData("/if-match/required")]
    [InlineData("/if-match/mvc/required")]
    [InlineData("/if-match/mvc-convention/required")]
    public async Task Responses_Should_ListPreconditionRequired_When_TheHeaderIsRequired(string path)
    {
        JsonNode responses = (await GetPathsAsync())[path]!["put"]!["responses"]!;

        StatusCodes(responses).Should().Contain(["200", "400", "412", "428"]);
    }

    [Theory]
    [InlineData("/if-match/own-412")]
    [InlineData("/if-match/mvc/own-412")]
    public async Task Responses_Should_KeepTheDeclaredResponse_When_TheEndpointDeclaresTheStatusCode(string path)
    {
        JsonNode responses = (await GetPathsAsync())[path]!["put"]!["responses"]!;

        responses["412"]!["content"]!.AsObject().Select(static pair => pair.Key).Should().Equal("text/plain");
    }

    [Theory]
    [InlineData("/if-match/read")]
    [InlineData("/if-match/mvc/read")]
    [InlineData("/if-match/mvc-convention/read")]
    public async Task Responses_Should_NotListIfMatchStatuses_When_TheMethodIsSafe(string path)
    {
        JsonNode responses = (await GetPathsAsync())[path]!["get"]!["responses"]!;

        StatusCodes(responses).Should().NotContain(["400", "412", "428"]);
    }

    [Fact]
    public async Task Responses_Should_ListIfMatchStatuses_When_TheHostHasNoControllers()
    {
        JsonNode responses = (await GetPathsAsync(withControllers: false))["/if-match/required"]!["put"]!["responses"]!;

        StatusCodes(responses).Should().Contain(["200", "400", "412", "428"]);
    }

    private static IEnumerable<string> StatusCodes(JsonNode responses) =>
        responses.AsObject().Select(static pair => pair.Key);

    private static async Task<JsonNode> GetPathsAsync(bool withControllers = true)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddConditionalRequests();
        if (withControllers)
            builder.Services.AddControllers().AddIfMatchControllers();
        builder.Services.AddOpenApi(IfMatchApi.Document, static options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        if (withControllers)
            app.MapControllers().WithIfMatch(required: true);
        app.MapIfMatchApi();
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            return JsonNode.Parse(await client.GetStringAsync($"/openapi/{IfMatchApi.Document}.json"))!["paths"]!;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
