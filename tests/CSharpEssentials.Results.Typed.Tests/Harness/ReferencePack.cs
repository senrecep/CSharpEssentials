using System.Reflection;
using System.Runtime.Versioning;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Results.Typed.Tests.Harness;

/// <summary>
/// The metadata references a consumer of the loaded CSharpEssentials assets compiles against: the ref pack of the framework
/// those assets were built for, never the host's trusted platform assemblies, so a net9.0 or netstandard2.1 asset is checked
/// against the API surface its consumers really see.
/// </summary>
internal sealed class ReferencePack
{
    private static readonly Lazy<ReferencePack> LazyCurrent = new(Create);

    private ReferencePack(FrameworkName framework, string directory, IReadOnlyList<Assembly> library, IReadOnlyList<MetadataReference> references)
    {
        Framework = framework;
        Directory = directory;
        Library = library;
        References = references;
    }

    public static ReferencePack Current => LazyCurrent.Value;

    public FrameworkName Framework { get; }

    public string Directory { get; }

    /// <summary>
    /// The CSharpEssentials assemblies the snippets compile against: Results, Errors and everything they reference transitively.
    /// </summary>
    public IReadOnlyList<Assembly> Library { get; }

    public IReadOnlyList<MetadataReference> References { get; }

    public bool HasUnionTypes => Framework.Identifier == ".NETCoreApp" && Framework.Version.Major >= 11;

    private static ReferencePack Create()
    {
        Assembly errors = typeof(NotFoundError).Assembly;
        string frameworkName = errors.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName
            ?? throw new InvalidOperationException($"{errors.GetName().Name} has no TargetFrameworkAttribute.");
        var framework = new FrameworkName(frameworkName);
        string directory = FindRefPackDirectory(framework);

        IEnumerable<string> platform = System.IO.Directory.GetFiles(directory, "*.dll");
        Assembly[] library = [.. CSharpEssentialsClosure(typeof(Result).Assembly, errors)];
        MetadataReference[] references = [.. platform.Concat(library.Select(a => a.Location)).Select(path => MetadataReference.CreateFromFile(path))];
        return new ReferencePack(framework, directory, library, references);
    }

    private static string FindRefPackDirectory(FrameworkName framework)
    {
        string packs = Path.Combine(DotnetRoot(), "packs");
        (string pack, string tfm, string versionPrefix) = framework.Identifier switch
        {
            ".NETCoreApp" => ("Microsoft.NETCore.App.Ref", $"net{framework.Version.Major}.{framework.Version.Minor}", $"{framework.Version.Major}."),
            ".NETStandard" => ("NETStandard.Library.Ref", $"netstandard{framework.Version.Major}.{framework.Version.Minor}", $"{framework.Version.Major}."),
            _ => throw new InvalidOperationException($"No ref pack is known for {framework}."),
        };

        string packDirectory = Path.Combine(packs, pack);
        string? version = System.IO.Directory.Exists(packDirectory)
            ? System.IO.Directory.GetDirectories(packDirectory)
                .Select(Path.GetFileName)
                .OfType<string>()
                .Where(v => v.StartsWith(versionPrefix, StringComparison.Ordinal) && System.IO.Directory.Exists(Path.Combine(packDirectory, v, "ref", tfm)))
                .OrderBy(v => v.Contains('-', StringComparison.Ordinal) ? 0 : 1)
                .ThenBy(v => Version.Parse(v.Split('-')[0]))
                .LastOrDefault()
            : null;

        return version is null
            ? throw new InvalidOperationException($"The {pack} ref pack for {tfm} is not installed under {packs}. Install the matching .NET SDK.")
            : Path.Combine(packDirectory, version, "ref", tfm);
    }

    private static string DotnetRoot()
    {
        string? fromEnvironment = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrEmpty(fromEnvironment) && System.IO.Directory.Exists(Path.Combine(fromEnvironment, "packs")))
            return fromEnvironment;

        string coreLib = typeof(object).Assembly.Location;
        DirectoryInfo? root = new FileInfo(coreLib).Directory?.Parent?.Parent?.Parent;
        return root?.FullName ?? throw new InvalidOperationException($"Cannot find the dotnet root from {coreLib}.");
    }

    private static Dictionary<string, Assembly>.ValueCollection CSharpEssentialsClosure(params Assembly[] roots)
    {
        var seen = new Dictionary<string, Assembly>(StringComparer.Ordinal);
        var pending = new Stack<Assembly>(roots);
        while (pending.Count > 0)
        {
            Assembly assembly = pending.Pop();
            if (!seen.TryAdd(assembly.GetName().Name!, assembly))
                continue;

            foreach (AssemblyName dependency in assembly.GetReferencedAssemblies())
            {
                if (dependency.Name?.StartsWith("CSharpEssentials.", StringComparison.Ordinal) == true && !seen.ContainsKey(dependency.Name))
                    pending.Push(Assembly.Load(dependency));
            }
        }

        return seen.Values;
    }
}
