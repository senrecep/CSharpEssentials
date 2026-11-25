using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Plain = CSharpEssentials.Tests.Fixtures.TypedHttp;
using Rdg = CSharpEssentials.Tests.Fixtures.TypedHttpRdg;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// The typed result endpoints of <c>CSharpEssentials.Tests.Fixtures.TypedHttp</c> describe one problem response per error slot,
/// and send the same responses and documents with and without the request delegate generator (the RDG fixture compiles the same
/// source). <c>CSE_UPDATE_GOLDEN=1</c> rewrites the golden files.
/// </summary>
public class TypedHttpOpenApiTests
{
    private static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static TheoryData<bool, OpenApiSpecVersion> Variants() => new()
    {
        { false, OpenApiSpecVersion.OpenApi3_0 },
        { true, OpenApiSpecVersion.OpenApi3_0 },
        { false, OpenApiSpecVersion.OpenApi3_1 },
        { true, OpenApiSpecVersion.OpenApi3_1 },
    };

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task Document_Should_MatchTheGoldenFile(bool rdg, OpenApiSpecVersion version)
    {
        string json = await WithHostAsync(rdg, version, static client => client.GetStringAsync(new Uri("/openapi/typed.json", UriKind.Relative)));

        Golden.Verify(OpenApiGolden.PathOf("typed-results", version == OpenApiSpecVersion.OpenApi3_1 ? "openapi31" : null), Extract(json));
    }

    [Fact]
    public async Task Document_Should_ListTheSuccessAndEachErrorSlot()
    {
        string json = await WithHostAsync(false, OpenApiSpecVersion.OpenApi3_0, static client => client.GetStringAsync(new Uri("/openapi/typed.json", UriKind.Relative)));

        JsonNode paths = JsonNode.Parse(json)!["paths"]!;
        Statuses(paths["/typed/orders/{id}"]!["get"]!).Should().Equal("200", "404", "409");
        Statuses(paths["/typed/async/{id}"]!["get"]!).Should().Equal("200", "404", "409");
        Statuses(paths["/typed/orders"]!["post"]!).Should().Equal("201", "404", "409");
        Statuses(paths["/typed/orders/{id}"]!["delete"]!).Should().Equal("204", "404", "409");
        paths["/typed/orders/{id}"]!["get"]!["responses"]!["404"]!["content"]!.AsObject().Single().Key.Should().Be("application/problem+json");
        Statuses(paths["/typed/filtered/{id}"]!["get"]!).Should().Equal("200");
    }

    [Fact]
    public void RdgFixture_Should_BeCompiledWithTheRequestDelegateGenerator()
    {
        Assembly rdg = typeof(Rdg.TypedHttpApi).Assembly;

        rdg.GetTypes().Should().Contain(static type => type.Namespace == "Microsoft.AspNetCore.Http.Generated");
        typeof(Plain.TypedHttpApi).Assembly.GetTypes().Should().NotContain(static type => type.Namespace == "Microsoft.AspNetCore.Http.Generated");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Endpoints_Should_SendTheValueOrTheProblem(bool rdg)
    {
        string[] responses = await WithHostAsync(rdg, OpenApiSpecVersion.OpenApi3_0, static async client =>
        {
            var results = new List<string>();
            foreach (string path in (string[])["/typed/orders/3", "/typed/orders/0", "/typed/orders/7", "/typed/async/7", "/typed/filtered/3", "/typed/filtered/7"])
                results.Add(await DescribeAsync(await client.GetAsync(new Uri(path, UriKind.Relative))));
            results.Add(await DescribeAsync(await client.PostAsJsonAsync(new Uri("/typed/orders", UriKind.Relative), new { id = 5 })));
            results.Add(await DescribeAsync(await client.DeleteAsync(new Uri("/typed/orders/5", UriKind.Relative))));
            return results.ToArray();
        });

        responses.Should().Equal(
            "200 application/json {\"id\":3,\"total\":30}",
            "404 application/problem+json Order.NotFound",
            "409 application/problem+json Order.Locked",
            "409 application/problem+json Order.Locked",
            "200 application/json {\"id\":3,\"total\":30}",
            "409 application/problem+json Order.Locked",
            "201 application/json {\"id\":5,\"total\":50} /typed/orders/5",
            "204  ");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RawReturn_Should_FailWithoutWritingTheResult(bool rdg)
    {
        (HttpStatusCode status, string body) = await WithHostAsync(rdg, OpenApiSpecVersion.OpenApi3_0, static async client =>
        {
            using HttpResponseMessage response = await client.GetAsync(new Uri("/typed/raw/7", UriKind.Relative));
            return (response.StatusCode, await response.Content.ReadAsStringAsync());
        });

        status.Should().Be(HttpStatusCode.InternalServerError);
        body.Should().BeEmpty();
    }

    private static async Task<string> DescribeAsync(HttpResponseMessage response)
    {
        using (response)
        {
            string body = await response.Content.ReadAsStringAsync();
            string? code = response.Content.Headers.ContentType?.MediaType == "application/problem+json"
                ? (string?)JsonNode.Parse(body)!["errorCodes"]![0]
                : body;
            string location = response.Headers.Location is { } uri ? " " + uri : string.Empty;
            return $"{(int)response.StatusCode} {response.Content.Headers.ContentType?.MediaType} {code}{location}";
        }
    }

    private static string[] Statuses(JsonNode operation) =>
        [.. operation["responses"]!.AsObject().Select(static pair => pair.Key).Order(StringComparer.Ordinal)];

    /// <summary>Per operation, each response with its content types and schema references, keys sorted.</summary>
    private static string Extract(string openApiJson)
    {
        JsonNode root = JsonNode.Parse(openApiJson)!;
        var operations = new JsonObject();
        foreach ((string path, JsonNode? item) in root["paths"]!.AsObject().OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            foreach ((string method, JsonNode? operation) in item!.AsObject().OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                var responses = new JsonObject();
                foreach ((string status, JsonNode? response) in operation!["responses"]!.AsObject().OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                {
                    var content = new JsonObject();
                    if (response?["content"] is JsonObject media)
                    {
                        foreach ((string contentType, JsonNode? value) in media.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
                            content[contentType] = value?["schema"]?["$ref"]?.DeepClone() ?? value?["schema"]?.DeepClone();
                    }

                    responses[status] = new JsonObject { ["description"] = response?["description"]?.DeepClone(), ["content"] = content };
                }

                operations[$"{method.ToUpperInvariant()} {path}"] = responses;
            }
        }

        return operations.ToJsonString(Indented).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }

    private static async Task<T> WithHostAsync<T>(bool rdg, OpenApiSpecVersion version, Func<HttpClient, Task<T>> run)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails();
        builder.Services.AddOpenApi(Plain.TypedHttpApi.Document, options => options.OpenApiVersion = version);

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        if (rdg)
            Rdg.TypedHttpApi.MapTypedHttpApi(app);
        else
            Plain.TypedHttpApi.MapTypedHttpApi(app);
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            return await run(client);
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
