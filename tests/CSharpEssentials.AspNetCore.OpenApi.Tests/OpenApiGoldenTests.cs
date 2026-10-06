using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// The enum extract of every sample document matches its golden file. The OpenAPI 3.0 files are shared with the Swashbuckle
/// golden tests of CSharpEssentials.Tests, so both packages describe the enums the same way. <c>CSE_UPDATE_GOLDEN=1</c>
/// rewrites the files.
/// </summary>
public class OpenApiGoldenTests
{
    public static TheoryData<string> Documents() => [.. SampleApi.Documents];

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task Document_Should_MatchTheSharedGoldenFile_When_OpenApi30(string document)
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_0);

        string extract = OpenApiGolden.Extract(documents[document]);

        Golden.Verify(OpenApiGolden.PathOf(document), extract);
    }

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task Document_Should_MatchTheGoldenFile_When_OpenApi31(string document)
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(OpenApiSpecVersion.OpenApi3_1);

        string extract = OpenApiGolden.Extract(documents[document]);

        Golden.Verify(OpenApiGolden.PathOf(document, "openapi31"), extract);
    }
}

internal static class Golden
{
    public static void Verify(string path, string actual)
    {
        if (Environment.GetEnvironmentVariable("CSE_UPDATE_GOLDEN") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, actual);
            return;
        }

        File.Exists(path).Should().BeTrue($"the golden file {path} exists (run the tests with CSE_UPDATE_GOLDEN=1 to create it)");
        actual.Should().Be(File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal));
    }
}
