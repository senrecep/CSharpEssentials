using System.Xml.Linq;
using FluentAssertions;

namespace CSharpEssentials.Tests.Packaging;

/// <summary>
/// Every CSharpEssentials source project (packages, generators and code fixes) has an explicit list of the packages and projects it
/// may reference. The test reads each project file, plus the repository files it imports, and fails when a reference is added or
/// removed without updating <see cref="AllowedReferences"/>. A new third-party dependency, or a layering change such as Results
/// referencing EF Core, has to be made on purpose in this list. Directory.Build.props adds SourceLink and SonarAnalyzer to every
/// project as private build assets; they are pinned by <see cref="AllowedRootReferences"/> instead of the per-project lists.
/// </summary>
public class PackageDependencyAllowListTests
{
    private static readonly Dictionary<string, string[]> AllowedReferences = new(StringComparer.Ordinal)
    {
        ["CSharpEssentials.Any"] = ["CSharpEssentials.Core", "CSharpEssentials.Json"],
        ["CSharpEssentials.AspNetCore.OpenApi"] = ["CSharpEssentials.AspNetCore", "Microsoft.AspNetCore.OpenApi", "Microsoft.OpenApi"],
        ["CSharpEssentials.AspNetCore.Swashbuckle"] = ["Asp.Versioning.Mvc.ApiExplorer", "CSharpEssentials.AspNetCore", "Swashbuckle.AspNetCore"],
        ["CSharpEssentials.AspNetCore"] = ["Asp.Versioning.Http", "Asp.Versioning.Mvc", "Asp.Versioning.Mvc.ApiExplorer", "CSharpEssentials.Errors", "CSharpEssentials.Idempotency", "CSharpEssentials.Json", "CSharpEssentials.Results", "CSharpEssentials.Validation"],
        ["CSharpEssentials.Clone"] = [],
        ["CSharpEssentials.Core"] = ["System.Memory", "System.Threading.Tasks.Extensions"],
        ["CSharpEssentials.DependencyInjection.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.DependencyInjection.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.DependencyInjection"] = ["Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Logging.Abstractions"],
        ["CSharpEssentials.Endpoints.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.Endpoints.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.Endpoints"] = [],
        ["CSharpEssentials.Entity"] = ["CSharpEssentials.Results", "CSharpEssentials.Time"],
        ["CSharpEssentials.EntityFrameworkCore.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.EntityFrameworkCore.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.EntityFrameworkCore.HealthChecks"] = ["CSharpEssentials.EntityFrameworkCore", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.Caching.Memory", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Diagnostics.HealthChecks", "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Options"],
        ["CSharpEssentials.EntityFrameworkCore.Hosting"] = ["CSharpEssentials.EntityFrameworkCore", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.Caching.Memory", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Hosting.Abstractions", "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Options"],
        ["CSharpEssentials.EntityFrameworkCore.Locking"] = ["CSharpEssentials.EntityFrameworkCore", "CSharpEssentials.Mediator", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.Caching.Memory", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Options"],
        ["CSharpEssentials.EntityFrameworkCore"] = ["CSharpEssentials.Core", "CSharpEssentials.Entity", "CSharpEssentials.Enums", "CSharpEssentials.Idempotency", "CSharpEssentials.Json", "CSharpEssentials.Maybe", "CSharpEssentials.Results", "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Relational", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.Caching.Memory", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Options"],
        ["CSharpEssentials.Enums.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.Enums.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.Enums"] = [],
        ["CSharpEssentials.Errors"] = ["CSharpEssentials.Core", "CSharpEssentials.Enums", "CSharpEssentials.Enums.Generators", "System.Text.Json"],
        ["CSharpEssentials.GcpSecretManager"] = ["CSharpEssentials.Core", "CSharpEssentials.Resilience", "Google.Cloud.SecretManager.V1", "Microsoft.Extensions.Configuration", "Microsoft.Extensions.Configuration.Binder", "Microsoft.Extensions.Logging.Abstractions", "System.Text.Json"],
        ["CSharpEssentials.Http"] = ["CSharpEssentials.Errors", "CSharpEssentials.Json", "CSharpEssentials.Resilience", "CSharpEssentials.Results", "Microsoft.Extensions.Http", "System.Net.Http.Json"],
        ["CSharpEssentials.Idempotency.Testing"] = ["CSharpEssentials.Idempotency", "Microsoft.Extensions.TimeProvider.Testing", "xunit"],
        ["CSharpEssentials.Idempotency"] = ["CSharpEssentials.Core", "Microsoft.Bcl.TimeProvider", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Logging.Abstractions", "Microsoft.Extensions.Options"],
        ["CSharpEssentials.Json.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.Json.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.Json"] = ["CSharpEssentials.Enums", "System.Text.Json"],
        ["CSharpEssentials.Maybe"] = ["CSharpEssentials.Errors", "CSharpEssentials.Results"],
        ["CSharpEssentials.Mediator.Testing"] = ["CSharpEssentials.Mediator"],
        ["CSharpEssentials.Mediator"] = ["CSharpEssentials.Errors", "CSharpEssentials.Idempotency", "CSharpEssentials.Json", "CSharpEssentials.Maybe", "CSharpEssentials.Results", "CSharpEssentials.Validation", "Mediator.Abstractions", "Microsoft.Extensions.Caching.Abstractions", "Microsoft.Extensions.DependencyInjection.Abstractions", "Microsoft.Extensions.Logging.Abstractions"],
        ["CSharpEssentials.RequestResponseLogging"] = ["Microsoft.IO.RecyclableMemoryStream"],
        ["CSharpEssentials.Resilience"] = ["CSharpEssentials.Errors", "CSharpEssentials.Results", "Polly.Core"],
        ["CSharpEssentials.Results.CodeFixes"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp", "Microsoft.CodeAnalysis.CSharp.Workspaces"],
        ["CSharpEssentials.Results.Generators"] = ["Microsoft.CodeAnalysis.Analyzers", "Microsoft.CodeAnalysis.CSharp"],
        ["CSharpEssentials.Results.Testing"] = ["CSharpEssentials.Maybe", "CSharpEssentials.Results", "System.Text.Json"],
        ["CSharpEssentials.Results"] = ["CSharpEssentials.Errors", "CSharpEssentials.Json"],
        ["CSharpEssentials.Rules"] = ["CSharpEssentials.Core", "CSharpEssentials.Results"],
        ["CSharpEssentials.These"] = ["CSharpEssentials.Errors", "CSharpEssentials.Maybe", "CSharpEssentials.Results"],
        ["CSharpEssentials.Time"] = ["Microsoft.Bcl.TimeProvider"],
        ["CSharpEssentials.Validation"] = ["CSharpEssentials.Results", "Microsoft.Extensions.DependencyInjection.Abstractions"],
        ["CSharpEssentials"] = ["CSharpEssentials.Any", "CSharpEssentials.Clone", "CSharpEssentials.Core", "CSharpEssentials.Entity", "CSharpEssentials.Enums", "CSharpEssentials.Errors", "CSharpEssentials.Http", "CSharpEssentials.Json", "CSharpEssentials.Maybe", "CSharpEssentials.Results", "CSharpEssentials.Rules", "CSharpEssentials.These", "CSharpEssentials.Time"],
    };

    private static readonly string[] AllowedRootReferences = ["Microsoft.SourceLink.GitHub", "SonarAnalyzer.CSharp"];

    public static TheoryData<string> SourceProjects()
    {
        TheoryData<string> projects = [];
        foreach (string project in ReadSourceProjects(FindSolutionDirectory()))
            projects.Add(project);
        return projects;
    }

    [Fact]
    public void AllowList_Should_CoverEverySourceProject()
    {
        ReadSourceProjects(FindSolutionDirectory()).Should().BeEquivalentTo(AllowedReferences.Keys);
    }

    [Theory]
    [MemberData(nameof(SourceProjects))]
    public void Project_Should_ReferenceOnlyAllowedPackagesAndProjects(string project)
    {
        AllowedReferences.Should().ContainKey(project);
        string[] actual = ReadReferences(Path.Combine(FindSolutionDirectory(), project, project + ".csproj"));

        actual.Except(AllowedReferences[project], StringComparer.Ordinal).Should()
            .BeEmpty($"{project} may only reference what its allow-list names; add a new dependency to the list on purpose");
        AllowedReferences[project].Except(actual, StringComparer.Ordinal).Should()
            .BeEmpty($"{project} no longer references these, so remove them from its allow-list");
    }

    [Fact]
    public void RootBuildFiles_Should_ReferenceOnlyAllowedPackages()
    {
        string solution = FindSolutionDirectory();

        string[] actual = new[] { "Directory.Build.props", "Directory.Build.targets" }
            .Select(file => Path.Combine(solution, file))
            .Where(File.Exists)
            .SelectMany(static file => XDocument.Load(file).Descendants("PackageReference").Select(static element => (string?)element.Attribute("Include")))
            .OfType<string>()
            .ToArray();

        actual.Should().BeEquivalentTo(AllowedRootReferences, "every project imports these files, so a package added there ships in every package");
    }

    [Fact]
    public void AllowList_Should_KeepCorePackagesFreeOfInfrastructure()
    {
        string[] infrastructure = ["Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Npgsql", "StackExchange.Redis", "CSharpEssentials.AspNetCore", "CSharpEssentials.EntityFrameworkCore"];
        foreach (string project in new[] { "CSharpEssentials.Core", "CSharpEssentials.Errors", "CSharpEssentials.Results", "CSharpEssentials.Maybe" })
            AllowedReferences[project].Should().NotContain(reference => infrastructure.Any(prefix => reference.StartsWith(prefix, StringComparison.Ordinal)), project);
    }

    /// <summary>The project directories at the repository root named CSharpEssentials*, without the test project.</summary>
    private static IEnumerable<string> ReadSourceProjects(string solution) =>
        Directory.EnumerateDirectories(solution, "CSharpEssentials*")
            .Select(static directory => Path.GetFileName(directory))
            .Where(name => !name.EndsWith(".Tests", StringComparison.Ordinal) && File.Exists(Path.Combine(solution, name, name + ".csproj")))
            .Order(StringComparer.Ordinal);

    /// <summary>
    /// Package ids and referenced project names, from the project file and from imported repository files. References whose path
    /// is built from MSBuild properties (the generator wiring in build/PackGenerator.targets) are skipped.
    /// </summary>
    private static string[] ReadReferences(string projectFile)
    {
        XDocument document = XDocument.Load(projectFile);
        IEnumerable<string> packages = document.Descendants("PackageReference").Select(static element => (string?)element.Attribute("Include"));
        IEnumerable<string> projects = document.Descendants("ProjectReference")
            .Select(static element => (string)element.Attribute("Include")!)
            .Where(static path => !path.Contains("$(", StringComparison.Ordinal))
            .Select(static path => Path.GetFileNameWithoutExtension(path.Replace('\\', '/')));
        IEnumerable<string> imported = document.Descendants("Import")
            .Select(element => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFile)!, ((string)element.Attribute("Project")!).Replace('\\', '/'))))
            .Where(File.Exists)
            .SelectMany(static file => XDocument.Load(file).Descendants("PackageReference").Select(static element => (string?)element.Attribute("Include")));

        return packages.Concat(imported).OfType<string>().Concat(projects).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private static string FindSolutionDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CSharpEssentials.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("CSharpEssentials.slnx was not found above " + AppContext.BaseDirectory);
    }
}
