using System.Text.Json.Nodes;
using System.Xml.Linq;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

/// <summary>
/// Any CSharpEssentials package brings the CSharpEssentials.Enums generator and analyzers, so a project that references only one of
/// them still gets <c>[StringEnum]</c> metadata. <c>dotnet pack</c> writes the <c>exclude</c> attribute of each nuspec dependency from
/// the private assets that restore records in <c>project.assets.json</c> (<c>build, contentfiles, analyzers</c> when a reference sets
/// none); this test reads the same files of the restored solution for every packable project in CSharpEssentials.slnx and fails when
/// any reference on the way to Enums excludes analyzers. The packed consumers are built end to end in CI (<c>tests/PackageConsumer</c>).
/// </summary>
public class EnumsAnalyzerFlowTests
{
    private const string EnumsProject = "CSharpEssentials.Enums";
    private const string DefaultPrivateAssets = "contentfiles;analyzers;build";

    public static TheoryData<string> PackableProjects()
    {
        TheoryData<string> projects = [];
        foreach (string project in ReadPackableProjects(FindSolutionDirectory()))
            projects.Add(project);
        return projects;
    }

    [Fact]
    public void Solution_Should_ListPackagesThatReachEnums()
    {
        string solution = FindSolutionDirectory();

        ReadPackableProjects(solution)
            .Where(project => ReadFrameworks(solution, project).Any(framework => ReachesEnums(solution, project, framework, [])))
            .Should().Contain(["CSharpEssentials.Json", "CSharpEssentials.Validation", "CSharpEssentials.Any", "CSharpEssentials.Mediator"]);
    }

    [Theory]
    [MemberData(nameof(PackableProjects))]
    public void Package_Should_FlowEnumsAnalyzersToConsumers_When_Packed(string project)
    {
        string solution = FindSolutionDirectory();
        List<string> excludingEdges = [];

        foreach (string framework in ReadFrameworks(solution, project))
            CollectExcludingEdges(solution, project, framework, [], excludingEdges);

        excludingEdges.Should().BeEmpty($"every reference from {project} on the way to {EnumsProject} must keep the analyzers asset");
    }

    /// <summary>Records each reference in the closure of <paramref name="project"/> that reaches Enums but excludes analyzers.</summary>
    private static void CollectExcludingEdges(string solution, string project, string framework, HashSet<string> visited, List<string> excludingEdges)
    {
        if (!visited.Add(project + "|" + framework))
            return;

        foreach ((string reference, string privateAssets) in ReadProjectReferences(solution, project, framework))
        {
            string? referenceFramework = SelectFramework(solution, reference, framework);
            bool reachesEnums = reference == EnumsProject
                || (referenceFramework is not null && ReachesEnums(solution, reference, referenceFramework, []));
            if (!reachesEnums)
                continue;
            if (!KeepsAnalyzers(privateAssets))
                excludingEdges.Add($"{project} ({framework}) -> {reference}: PrivateAssets=\"{privateAssets}\"");
            if (referenceFramework is not null)
                CollectExcludingEdges(solution, reference, referenceFramework, visited, excludingEdges);
        }
    }

    private static bool ReachesEnums(string solution, string project, string framework, HashSet<string> visited)
    {
        if (!visited.Add(project + "|" + framework))
            return false;

        foreach ((string reference, _) in ReadProjectReferences(solution, project, framework))
        {
            if (reference == EnumsProject)
                return true;
            string? referenceFramework = SelectFramework(solution, reference, framework);
            if (referenceFramework is not null && ReachesEnums(solution, reference, referenceFramework, visited))
                return true;
        }

        return false;
    }

    private static bool KeepsAnalyzers(string privateAssets) =>
        !privateAssets.Split([';', ','], StringSplitOptions.TrimEntries)
            .Any(static asset => asset.Equals("analyzers", StringComparison.OrdinalIgnoreCase)
                || asset.Equals("all", StringComparison.OrdinalIgnoreCase));

    private static string? SelectFramework(string solution, string project, string framework)
    {
        List<string> frameworks = [.. ReadFrameworks(solution, project)];
        return frameworks.Find(f => f == framework)
            ?? frameworks.Find(static f => f.StartsWith("netstandard", StringComparison.Ordinal));
    }

    /// <summary>The projects in CSharpEssentials.slnx that pack: not tests, fixtures, benchmarks, examples, generators or code fixes.</summary>
    private static IEnumerable<string> ReadPackableProjects(string solution) =>
        XDocument.Load(Path.Combine(solution, "CSharpEssentials.slnx")).Descendants("Project")
            .Select(static element => (string)element.Attribute("Path")!)
            .Where(path => IsPackable(Path.Combine(solution, path)))
            .Select(static path => Path.GetFileNameWithoutExtension(path))
            .Where(static name => !name.EndsWith(".Generators", StringComparison.Ordinal)
                && !name.EndsWith(".CodeFixes", StringComparison.Ordinal)
                && !name.EndsWith(".Tests", StringComparison.Ordinal)
                && !name.Contains(".Tests.", StringComparison.Ordinal)
                && !name.Contains(".Benchmarks", StringComparison.Ordinal)
                && !name.StartsWith("Examples.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);

    private static bool IsPackable(string projectFile) =>
        !XDocument.Load(projectFile).Descendants("IsPackable")
            .Any(static element => element.Value.Trim().Equals("false", StringComparison.OrdinalIgnoreCase));

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
