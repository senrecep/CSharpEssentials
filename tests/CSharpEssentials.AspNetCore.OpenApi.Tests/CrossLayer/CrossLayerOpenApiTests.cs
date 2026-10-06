using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests.CrossLayer;

/// <summary>
/// The Microsoft.AspNetCore.OpenApi layer of the cross-layer golden table (<see cref="CrossLayerTable"/>, #68). The other
/// layers, Swashbuckle included, run the same table in CSharpEssentials.Tests.
/// </summary>
public class CrossLayerOpenApiTests
{
    private static readonly Lazy<Task<IReadOnlyDictionary<string, string>>> Documents30 = new(() =>
        OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0, configureApp: static app => app.MapCrossLayerApi()));

    private static readonly Lazy<Task<IReadOnlyDictionary<string, string>>> Documents31 = new(() =>
        OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_1, configureApp: static app => app.MapCrossLayerApi()));

    public static TheoryData<string> Rows() => [.. CrossLayerTable.Rows.Select(static row => row.Name)];

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task OpenApi30_Should_DescribeTheParametersAndComponent(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        string document = (await Documents30.Value)[row.Document];

        foreach (string endpoint in CrossLayerOpenApi.Endpoints)
            CrossLayerOpenApi.Parameter(document, row, endpoint).Should().Be(row.OpenApiParameter, endpoint);
        CrossLayerOpenApi.Component(document, row).Should().Be(row.MicrosoftOpenApiComponent ?? row.OpenApiComponent);
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task OpenApi31_Should_DescribeTheParametersAndComponent(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        string document = (await Documents31.Value)[row.Document];

        foreach (string endpoint in CrossLayerOpenApi.Endpoints)
            CrossLayerOpenApi.Parameter(document, row, endpoint).Should().Be(row.OpenApi31Parameter ?? row.OpenApiParameter, endpoint);
        CrossLayerOpenApi.Component(document, row).Should().Be(row.MicrosoftOpenApiComponent ?? row.OpenApiComponent);
    }
}
