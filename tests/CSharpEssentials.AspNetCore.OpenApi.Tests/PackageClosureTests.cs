using System.Text.Json.Nodes;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// One host, one OpenAPI package: the restored dependency closure of CSharpEssentials.AspNetCore.OpenApi has no Swashbuckle,
/// that of CSharpEssentials.AspNetCore.Swashbuckle has no Microsoft.AspNetCore.OpenApi, both use Microsoft.OpenApi 2.x, and
/// CSharpEssentials.AspNetCore has neither. Reads the <c>project.assets.json</c> files of a restored solution.
/// </summary>
public class PackageClosureTests
{
    [Fact]
    public void OpenApiPackage_Should_UseMicrosoftOpenApi2AndNoSwashbuckle_When_Restored()
    {
        IReadOnlyDictionary<string, string> libraries = ReadLibraries("CSharpEssentials.AspNetCore.OpenApi");

        libraries.Keys.Should().NotContain(static name => name.StartsWith("Swashbuckle.", StringComparison.OrdinalIgnoreCase));
        libraries.Should().ContainKey("Microsoft.OpenApi").WhoseValue.Should().StartWith("2.");
    }

    [Fact]
    public void SwashbucklePackage_Should_UseMicrosoftOpenApi2AndNoMicrosoftAspNetCoreOpenApi_When_Restored()
    {
        IReadOnlyDictionary<string, string> libraries = ReadLibraries("CSharpEssentials.AspNetCore.Swashbuckle");

        libraries.Keys.Should().NotContain("Microsoft.AspNetCore.OpenApi");
        libraries.Should().ContainKey("Microsoft.OpenApi").WhoseValue.Should().StartWith("2.");
    }

    [Fact]
    public void AspNetCorePackage_Should_ReferenceNoOpenApiPackage_When_Restored()
    {
        IReadOnlyDictionary<string, string> libraries = ReadLibraries("CSharpEssentials.AspNetCore");

        libraries.Keys.Should().NotContain(static name =>
            name.StartsWith("Swashbuckle.", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Microsoft.AspNetCore.OpenApi", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Microsoft.OpenApi", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The package libraries (name to version) of every target framework of the project.</summary>
    private static Dictionary<string, string> ReadLibraries(string project)
    {
        // OpenApiGolden.Directory is <solution>/tests/golden/openapi.
        string solution = Path.GetFullPath(Path.Combine(OpenApiGolden.Directory, "..", "..", ".."));
        string assets = Path.Combine(solution, project, "obj", "project.assets.json");
        File.Exists(assets).Should().BeTrue($"{assets} exists (restore the solution first)");

        var libraries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, JsonNode? library) in JsonNode.Parse(File.ReadAllText(assets))!["libraries"]!.AsObject())
        {
            if (library?["type"]?.GetValue<string>() != "package")
                continue;
            int slash = key.IndexOf('/', StringComparison.Ordinal);
            libraries[key[..slash]] = key[(slash + 1)..];
        }

        return libraries;
    }
}
