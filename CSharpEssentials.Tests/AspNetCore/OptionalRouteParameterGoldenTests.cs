using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// The documents of <see cref="OptionalRouteParameterControllers.Compliant"/> and the minimal API endpoints of
/// <see cref="OptionalRouteParameterHost"/> in <see cref="OptionalRouteParameterMode.SplitPaths"/> and
/// <see cref="OptionalRouteParameterMode.RequiredOnly"/> match their golden files. <c>CSE_UPDATE_GOLDEN=1</c> rewrites them.
/// </summary>
public class OptionalRouteParameterGoldenTests
{
    [Theory]
    [InlineData(OptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_0, "swashbuckle-optional-route-split-paths", null)]
    [InlineData(OptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_1, "swashbuckle-optional-route-split-paths", "openapi31")]
    [InlineData(OptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_0, "swashbuckle-optional-route-required-only", null)]
    [InlineData(OptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_1, "swashbuckle-optional-route-required-only", "openapi31")]
    public async Task Document_Should_Match_Golden_File(OptionalRouteParameterMode mode, OpenApiSpecVersion version, string fixture, string? suffix)
    {
        string path = OpenApiGolden.PathOf(fixture, suffix);

        string json = Normalize(await OptionalRouteParameterHost.GetJsonAsync(version, options => options.AddOptionalRouteParameters(mode)));

        if (Environment.GetEnvironmentVariable("CSE_UPDATE_GOLDEN") == "1")
            await File.WriteAllTextAsync(path, json);
        json.Should().Be(Normalize(await File.ReadAllTextAsync(path)));
    }

    private static string Normalize(string json) => json.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd();
}
