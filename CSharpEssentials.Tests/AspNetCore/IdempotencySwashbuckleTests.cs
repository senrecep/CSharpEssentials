using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// The idempotency contract in the Swashbuckle document of <see cref="IdempotencyApi"/> matches the golden file of
/// CSharpEssentials.AspNetCore.OpenApi.Tests, so both packages describe it the same way.
/// </summary>
public class IdempotencySwashbuckleTests
{
    [Fact]
    public async Task Document_Should_MatchTheSharedGoldenFile_When_DescribedBySwashbuckle()
    {
        string json = await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0);
        string golden = await File.ReadAllTextAsync(OpenApiIdempotencyGolden.PathOf());

        OpenApiIdempotencyGolden.Extract(json).Should().Be(golden.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Document_Should_ResolveEveryReference_When_SchemaIdsAreNamespaceQualified()
    {
        string json = await IdempotencySwashbuckleHost.GetJsonAsync(
            OpenApiSpecVersion.OpenApi3_0, static options => options.CustomSchemaIds(new SwashbuckleSchemaIdFactory().GetSchemaId));
        JsonNode document = JsonNode.Parse(json)!;
        JsonObject schemas = document["components"]!["schemas"]!.AsObject();

        string[] references = [.. System.Text.RegularExpressions.Regex.Matches(json, "\"#/components/schemas/([^\"]+)\"").Select(static match => match.Groups[1].Value).Distinct()];

        references.Should().NotBeEmpty().And.Contain("CSharpEssentials.AspNetCore.EnhancedProblemDetails");
        references.Should().OnlyContain(reference => schemas.ContainsKey(reference));
    }

    [Fact]
    public async Task Document_Should_MatchTheGoldenFile_When_OpenApi31()
    {
        string json = await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_1);
        string golden = await File.ReadAllTextAsync(OpenApiIdempotencyGolden.PathOf("openapi31"));

        OpenApiIdempotencyGolden.Extract(json).Should().Be(golden.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Document_Should_NotChangeOperations_When_TheyAreNotIdempotent()
    {
        JsonNode paths = JsonNode.Parse(await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!;

        foreach (string path in (string[])["/idempotency/plain", "/idempotency/mvc/plain", "/idempotency/off", "/idempotency/get"])
        {
            JsonNode operation = paths[path]!.AsObject().Single().Value!;
            operation["parameters"]?.AsArray().Select(static parameter => (string?)parameter!["in"]).Should().NotContain("header", path);
            operation["responses"]!.AsObject().Select(static pair => pair.Key).Should().BeEquivalentTo(["200"], path);
        }
    }

    [Fact]
    public async Task Document_Should_DescribeOnlyAnOptionalKeyHeader_When_TheEndpointObserves()
    {
        JsonNode operation = JsonNode.Parse(await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!["/idempotency/observe"]!["post"]!;

        JsonNode header = operation["parameters"]!.AsArray().Single(static parameter => (string?)parameter!["in"] == "header")!;
        ((string?)header["name"]).Should().Be("Idempotency-Key");
        ((bool?)header["required"] ?? false).Should().BeFalse();
        operation["responses"]!.AsObject().Select(static pair => pair.Key).Should().BeEquivalentTo("200");
        operation["responses"]!["200"]!["headers"].Should().BeNull();
    }

    [Fact]
    public async Task Document_Should_KeepTheEndpointsOwn409_When_ItAlsoDescribesInFlight()
    {
        JsonNode responses = JsonNode.Parse(await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0))!["paths"]!["/idempotency/business-conflict"]!["post"]!["responses"]!;

        string description = (string)responses["409"]!["description"]!;
        description.Should().Contain("Conflict").And.Contain("Idempotency.InFlight");
        description.IndexOf("Conflict", StringComparison.Ordinal).Should().BeLessThan(description.IndexOf("Idempotency.InFlight", StringComparison.Ordinal));
        responses["409"]!["headers"]!["Retry-After"].Should().NotBeNull();
        responses["409"]!["content"]!["application/problem+json"]!["schema"]!["oneOf"]!.AsArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task Document_Should_NotDescribeIdempotency_When_TheFilterIsRemoved()
    {
        string json = await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0, static options =>
            options.OperationFilterDescriptors.RemoveAll(static descriptor => descriptor.Type.Name == "IdempotencyOperationFilter"));

        json.Should().NotContain("Idempotency-Key").And.NotContain("Idempotency.InFlight");
    }

    [Fact]
    public async Task Document_Should_UseTheConfiguredHeaderNames_When_TheOptionsRenameThem()
    {
        string json = await IdempotencySwashbuckleHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0, configureOptions: static options =>
        {
            options.HeaderName = "X-Request-Key";
            options.ReplayedHeaderName = "X-Replayed";
        });

        json.Should().Contain("X-Request-Key").And.Contain("X-Replayed").And.NotContain("Idempotency-Key");
    }

    [Fact]
    public void AddIdempotencyConventions_Should_AddTheFilterOnce_When_CalledTwice()
    {
        var options = new Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions();

        options.AddIdempotencyConventions().AddIdempotencyConventions();

        options.OperationFilterDescriptors.Count(static descriptor => descriptor.Type.Name == "IdempotencyOperationFilter").Should().Be(1);
    }
}
