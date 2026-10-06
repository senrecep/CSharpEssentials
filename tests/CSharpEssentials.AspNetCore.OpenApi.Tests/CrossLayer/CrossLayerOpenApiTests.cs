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
    private static readonly Lazy<Task<IReadOnlyDictionary<string, string>>> Documents = new(() =>
        OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0, configureApp: static app => app.MapCrossLayerApi()));

    public static TheoryData<string> Rows() => [.. CrossLayerTable.Rows.Select(static row => row.Name)];

    [Theory]
    [MemberData(nameof(Rows))]
    public async Task OpenApi_Should_DescribeTheParameterAndComponent(string name)
    {
        CrossLayerRow row = CrossLayerTable.Get(name);
        IReadOnlyDictionary<string, string> documents = await Documents.Value;

        string document = documents[row.Document];

        CrossLayerOpenApi.Parameter(document, row).Should().Be(row.OpenApiParameter);
        CrossLayerOpenApi.Component(document, row).Should().Be(row.MicrosoftOpenApiComponent ?? row.OpenApiComponent);
    }
}
