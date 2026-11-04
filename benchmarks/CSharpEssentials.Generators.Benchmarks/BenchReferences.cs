using System.Collections.Immutable;
using System.Reflection;
using CSharpEssentials.DependencyInjection;
using CSharpEssentials.Endpoints;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// One reference set for every class and every row: the host runtime's framework, the ASP.NET Core shared framework, the
/// three CSE runtime DLLs and the four competitor attribute and runtime DLLs. CSE's Host steps walk every reference, so a
/// shared set keeps that work equal across rows. A Microsoft.Extensions.* NuGet DLL in the output folder that is newer than
/// the shared framework copy replaces it, as SDK conflict resolution does in a CSE app (on net8.0 the 9.0.4 abstractions
/// the CSE runtime DLLs need).
/// </summary>
public static class BenchReferences
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> s_all = new(Load);

    public static ImmutableArray<MetadataReference> All => s_all.Value;

    private static ImmutableArray<MetadataReference> Load()
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        IEnumerable<string> framework = trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(path => string.Equals(Path.GetDirectoryName(path), runtimeDirectory, StringComparison.Ordinal));
        IEnumerable<string> aspNetCore = Directory.GetFiles(Path.GetDirectoryName(typeof(RouteGroupBuilder).Assembly.Location)!, "*.dll");
        string[] cse =
        [
            typeof(IEndpoint).Assembly.Location,
            typeof(RegisterScopedAttribute).Assembly.Location,
            typeof(StringEnumAttribute).Assembly.Location,
        ];
        string[] competitors =
        [
            BenchPaths.References("Immediate.Apis.Shared.dll"),
            BenchPaths.References("Immediate.Handlers.Shared.dll"),
            BenchPaths.References("NetEscapades.EnumGenerators.Attributes.dll"),
            BenchPaths.References("NetEscapades.EnumGenerators.RuntimeDependencies.dll"),
        ];

        return [.. framework.Concat(aspNetCore).Select(PreferNewerPackage).Concat(cse).Concat(competitors)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))];
    }

    private static string PreferNewerPackage(string frameworkPath)
    {
        string name = Path.GetFileName(frameworkPath);
        string package = Path.Combine(AppContext.BaseDirectory, name);
        return name.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal) &&
            File.Exists(package) &&
            AssemblyName.GetAssemblyName(package).Version > AssemblyName.GetAssemblyName(frameworkPath).Version
                ? package
                : frameworkPath;
    }
}
