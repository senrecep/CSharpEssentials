using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// Covers the obsolete <see cref="OptionalRouteParameterMode.LegacyNonCompliant"/> on purpose; the class is obsolete so it
/// can use it without CS0618. The fixtures are the documents 6.0.0 generated for <see cref="OptionalRouteParameterControllers.Legacy"/>.
/// <c>CSE_UPDATE_GOLDEN=1</c> rewrites them.
/// </summary>
[Obsolete("Covers the obsolete OptionalRouteParameterMode.LegacyNonCompliant.")]
public class LegacyOptionalRouteParameterTests
{
    private const string Fixture = "swashbuckle-optional-route-legacy";

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi3_0, null)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1, "openapi31")]
    public async Task LegacyNonCompliant_Should_Reproduce_Earlier_Output(OpenApiSpecVersion version, string? suffix)
    {
        string path = OpenApiGolden.PathOf(Fixture, suffix);

        string json = Normalize(await OptionalRouteParameterHost.GetJsonAsync(version, Legacy, OptionalRouteParameterControllers.Legacy));

        if (Environment.GetEnvironmentVariable("CSE_UPDATE_GOLDEN") == "1")
            await File.WriteAllTextAsync(path, json);
        json.Should().Be(Normalize(await File.ReadAllTextAsync(path)));
    }

    [Fact]
    public async Task LegacyNonCompliant_Should_Fail_OpenApi_Validation()
    {
        string json = await OptionalRouteParameterHost.GetJsonAsync(OpenApiSpecVersion.OpenApi3_0, Legacy);

        IReadOnlyList<OpenApiError> errors = OptionalRouteParameterDocumentFilterTests.Validate(json);

        errors.Should().NotBeEmpty();
    }

    private static void Legacy(SwaggerGenOptions options) =>
        options.AddOptionalRouteParameters(OptionalRouteParameterMode.LegacyNonCompliant);

    private static string Normalize(string json) => json.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd();
}
