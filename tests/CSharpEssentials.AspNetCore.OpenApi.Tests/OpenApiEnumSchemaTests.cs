using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public class OpenApiEnumSchemaTests
{
    [Fact]
    public async Task Documents_Should_DescribeEnumsDifferently_When_GroupsWriteNumbersAndStrings()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode v1 = Component(documents["v1"], "SampleStatus");
        JsonNode v2 = Component(documents["v2"], "SampleStatus");

        v1["type"]!.GetValue<string>().Should().Be("integer");
        v2["type"]!.GetValue<string>().Should().Be("string");
        v2["enum"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("pending", "pending_approval", "waiting", "unknown");
    }

    [Fact]
    public async Task Component_Should_UseInt64_When_TheEnumIsLongInANumberDocument()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode size = Component(documents["v1"], "SampleSize");

        size["format"]!.GetValue<string>().Should().Be("int64");
        size["enum"]!.AsArray().Select(static value => value!.GetValue<long>()).Should().Equal(1L, 5_000_000_000L);
    }

    [Fact]
    public async Task Component_Should_UseTheJsonMemberName_When_TheMemberIsRenamed()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode naming = Component(documents["v2"], "SampleNaming");

        naming["enum"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Contain("custom-name");
    }

    [Fact]
    public async Task Component_Should_KeepTheFrameworkSchema_When_TheEnumIsPlain()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode plain = Component(documents["v2"], "SamplePlain");

        plain.ToJsonString().Should().NotContain("x-enum-");
        plain["type"]!.GetValue<string>().Should().Be("integer");
    }

    [Fact]
    public async Task Documents_Should_BeGenerated_When_EnumConventionsAreNotAdded()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0, addEnumConventions: false);

        documents.Values.Should().AllSatisfy(static json => json.Should().NotContain("x-enum-varnames"));
    }

    [Fact]
    public async Task NullableProperty_Should_UseAllOfAndNullable_When_OpenApi30()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode previous = Component(documents["v2"], "SampleOrder")["properties"]!["previous"]!;

        previous["nullable"]!.GetValue<bool>().Should().BeTrue();
        previous["allOf"]![0]!["$ref"]!.GetValue<string>().Should().Be("#/components/schemas/SampleStatus");
    }

    [Fact]
    public async Task NullableProperty_Should_UseOneOfWithNull_When_OpenApi31()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_1);

        JsonNode previous = Component(documents["v2"], "SampleOrder")["properties"]!["previous"]!;

        previous["oneOf"]!.AsArray().Select(static branch => branch!.ToJsonString())
            .Should().BeEquivalentTo("{\"$ref\":\"#/components/schemas/SampleStatus\"}", "{\"type\":\"null\"}");
    }

    [Fact]
    public async Task Operation_Should_CarryTheHeaderAndANote_When_TheFormatIsSelectedByAHeader()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode operation = Operation(documents["header"], "/header/orders/{status}", "get");

        operation["x-enum-wire-format-header"]!.GetValue<string>().Should().Be(SampleApi.FormatHeader);
        operation["description"]!.GetValue<string>().Should().Contain(SampleApi.FormatHeader);
        Component(documents["header"], "SampleStatus")["type"]!.GetValue<string>().Should().Be("string");
    }

    [Fact]
    public async Task Operation_Should_BeMarkedAsNumber_When_ItWritesNumbersInAStringDocument()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        JsonNode legacy = Operation(documents["mixed"], "/mixed/legacy/{status}", "get");
        JsonNode orders = Operation(documents["mixed"], "/mixed/orders/{status}", "get");

        legacy["x-enum-wire-format"]!.GetValue<string>().Should().Be("number");
        orders["x-enum-wire-format"].Should().BeNull();
    }

    private static JsonNode Component(string json, string name) =>
        JsonNode.Parse(json)!["components"]!["schemas"]![name]!;

    private static JsonNode Operation(string json, string path, string method) =>
        JsonNode.Parse(json)!["paths"]![path]![method]!;
}
