using System.Text.Json.Nodes;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

/// <summary>
/// The packages that depend on CSharpEssentials.Enums let its generator and analyzers flow to their consumers, so a project that
/// references only one of them still gets <c>[StringEnum]</c> metadata. <c>dotnet pack</c> writes the <c>exclude</c> attribute of
/// each nuspec dependency from the private assets that restore records in <c>project.assets.json</c> (<c>build, contentfiles,
/// analyzers</c> when a reference sets none); this test reads the same files of the restored solution and walks the references.
/// The packed consumers are built end to end in CI (<c>tests/PackageConsumer</c>).
/// </summary>
public class EnumsAnalyzerFlowTests
{
    private const string EnumsProject = "CSharpEssentials.Enums";
    private const string DefaultPrivateAssets = "contentfiles;analyzers;build";

    [Theory]
    [InlineData("CSharpEssentials.Json")]
    [InlineData("CSharpEssentials.EntityFrameworkCore")]
    [InlineData("CSharpEssentials.AspNetCore")]
    [InlineData("CSharpEssentials.Http")]
    [InlineData("CSharpEssentials")]
    public void Dependent_Should_FlowEnumsAnalyzersToConsumers_When_Packed(string project)
    {
        string solution = FindSolutionDirectory();

        foreach (string framework in ReadFrameworks(solution, project))
            FlowsAnalyzersToEnums(solution, project, framework, []).Should()
                .BeTrue($"{project} ({framework}) must not exclude Analyzers on the way to {EnumsProject}");
    }

    /// <summary>Whether a reference path from <paramref name="project"/> to CSharpEssentials.Enums keeps the analyzers asset.</summary>
    private static bool FlowsAnalyzersToEnums(string solution, string project, string framework, HashSet<string> visited)
    {
        if (!visited.Add(project))
            return false;

        foreach ((string reference, string privateAssets) in ReadProjectReferences(solution, project, framework))
        {
            bool keepsAnalyzers = !privateAssets.Split([';', ','], StringSplitOptions.TrimEntries)
                .Any(static asset => asset.Equals("analyzers", StringComparison.OrdinalIgnoreCase)
                    || asset.Equals("all", StringComparison.OrdinalIgnoreCase));
            if (!keepsAnalyzers)
                continue;
            if (reference == EnumsProject)
                return true;
            List<string> referenceFrameworks = [.. ReadFrameworks(solution, reference)];
            string? referenceFramework = referenceFrameworks.Find(f => f == framework)
                ?? referenceFrameworks.Find(static f => f.StartsWith("netstandard", StringComparison.Ordinal));
            if (referenceFramework is not null && FlowsAnalyzersToEnums(solution, reference, referenceFramework, visited))
                return true;
        }

        return false;
    }

    private static IEnumerable<string> ReadFrameworks(string solution, string project) =>
        ReadRestoreFrameworks(solution, project).Select(static framework => framework.Key);

    private static IEnumerable<(string Project, string PrivateAssets)> ReadProjectReferences(string solution, string project, string framework)
    {
        JsonObject references = ReadRestoreFrameworks(solution, project)[framework]!["projectReferences"]!.AsObject();
        foreach ((string path, JsonNode? reference) in references)
            yield return (Path.GetFileNameWithoutExtension(path), reference?["privateAssets"]?.GetValue<string>() ?? DefaultPrivateAssets);
    }

    private static JsonObject ReadRestoreFrameworks(string solution, string project)
    {
        string assets = Path.Combine(solution, project, "obj", "project.assets.json");
        File.Exists(assets).Should().BeTrue($"{assets} exists (restore the solution first)");
        return JsonNode.Parse(File.ReadAllText(assets))!["project"]!["restore"]!["frameworks"]!.AsObject();
    }

    private static string FindSolutionDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CSharpEssentials.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("CSharpEssentials.slnx was not found above " + AppContext.BaseDirectory);
    }
}
