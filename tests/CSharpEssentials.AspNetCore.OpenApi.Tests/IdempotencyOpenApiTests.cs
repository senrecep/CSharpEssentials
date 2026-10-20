using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// The idempotency contract in the documents of <see cref="IdempotencyApi"/> matches its golden files. The OpenAPI 3.0 file is
/// shared with the Swashbuckle golden test of CSharpEssentials.Tests. <c>CSE_UPDATE_GOLDEN=1</c> rewrites the files.
/// </summary>
public class IdempotencyOpenApiTests
{
    [Fact]
    public async Task Document_Should_MatchTheSharedGoldenFile_When_OpenApi30()
    {
        string json = await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0);

        Golden.Verify(OpenApiIdempotencyGolden.PathOf(), OpenApiIdempotencyGolden.Extract(json));
    }

    [Fact]
    public async Task Document_Should_MatchTheGoldenFile_When_OpenApi31()
    {
        string json = await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_1);

        Golden.Verify(OpenApiIdempotencyGolden.PathOf("openapi31"), OpenApiIdempotencyGolden.Extract(json));
    }

    [Fact]
    public async Task Document_Should_NotChangeOperations_When_TheyAreNotIdempotent()
    {
        JsonNode paths = JsonNode.Parse(await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!;

        foreach (string path in (string[])["/idempotency/plain", "/idempotency/mvc/plain", "/idempotency/off", "/idempotency/observe", "/idempotency/get"])
        {
            JsonNode operation = paths[path]!.AsObject().Single().Value!;
            operation["parameters"]?.AsArray().Select(static parameter => (string?)parameter!["in"]).Should().NotContain("header", path);
            operation["responses"]!.AsObject().Select(static pair => pair.Key).Should().BeEquivalentTo(["200"], path);
        }
    }

    [Fact]
    public async Task Document_Should_KeepTheEndpointsOwn409_When_ItAlsoDescribesInFlight()
    {
        JsonNode responses = JsonNode.Parse(await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!["/idempotency/business-conflict"]!["post"]!["responses"]!;

        string description = (string)responses["409"]!["description"]!;
        description.Should().Contain("Conflict").And.Contain("Idempotency.InFlight");
        description.IndexOf("Conflict", StringComparison.Ordinal).Should().BeLessThan(description.IndexOf("Idempotency.InFlight", StringComparison.Ordinal));
        responses["409"]!["headers"]!["Retry-After"].Should().NotBeNull();
        responses["409"]!["content"]!["application/problem+json"]!["schema"]!["oneOf"]!.AsArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task Document_Should_NotDescribeIdempotency_When_ConventionsAreNotAdded()
    {
        string json = await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0, addConventions: false);

        json.Should().NotContain("Idempotency-Key").And.NotContain("Idempotency.InFlight");
    }

    [Fact]
    public async Task Document_Should_UseTheConfiguredHeaderNames_When_TheOptionsRename_Them()
    {
        string json = await IdempotencyOpenApiHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0, configureOptions: static options =>
        {
            options.HeaderName = "X-Request-Key";
            options.ReplayedHeaderName = "X-Replayed";
        });

        json.Should().Contain("X-Request-Key").And.Contain("X-Replayed").And.NotContain("Idempotency-Key");
    }
}
