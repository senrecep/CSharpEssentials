using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed class SwashbuckleIfMatchResponseTests
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

    private static IEnumerable<string> StatusCodes(JsonNode responses) =>
        responses.AsObject().Select(static pair => pair.Key);

    private static async Task<JsonNode> GetPathsAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddConditionalRequests();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleIfMatchResponseTests).Assembly);
        builder.Services.AddControllers().AddIfMatchControllers();
        builder.Services.ConfigureSwaggerGen(static options => options.SwaggerDoc(IfMatchApi.Document, new OpenApiInfo { Title = IfMatchApi.Document, Version = "1" }));

        await using WebApplication app = builder.Build();
        app.MapControllers().WithIfMatch(required: true);
        app.MapIfMatchApi();
        await app.StartAsync();
        try
        {
            OpenApiDocument document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(IfMatchApi.Document);
            return JsonNode.Parse(await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
