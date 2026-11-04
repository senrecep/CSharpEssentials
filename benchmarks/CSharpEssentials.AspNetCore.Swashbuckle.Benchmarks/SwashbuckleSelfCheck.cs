using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

/// <summary>Runs every class's parity checks without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class SwashbuckleSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine(
            $"ASP.NET Core {VersionOf(typeof(WebApplication))}, Swashbuckle.AspNetCore {VersionOf(typeof(global::Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions))}, Microsoft.OpenApi {VersionOf(typeof(Microsoft.OpenApi.OpenApiDocument))} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");

        var warm = new SwashbuckleDocumentComparison();
        warm.Setup();
        warm.Cleanup();
        foreach ((string row, DocumentFacts facts) in warm.Facts)
            Console.WriteLine($"{row}: {facts}");
        Console.WriteLine($"Plain Enum01: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen)].Enum01Shape}");
        Console.WriteLine($"StringEnums Enum01: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen_StringEnums)].Enum01Shape}");
        Console.WriteLine($"Plain Kind default: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen)].KindDefaults[0]}");
        Console.WriteLine($"StringEnums Kind default: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen_StringEnums)].KindDefaults[0]}");
        Console.WriteLine($"Plain Stage: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen)].StageShape}");
        Console.WriteLine($"StringEnums Stage: {warm.Facts[nameof(SwashbuckleDocumentComparison.Plain_AddSwaggerGen_StringEnums)].StageShape}");

        Cold(warm, nameof(SwashbuckleFirstDocumentComparison.Plain_AddSwaggerGen), SwashbuckleHosts.Plain, DocumentConventions.None, SwashbuckleHosts.PlainShapes);
        Cold(warm, nameof(SwashbuckleFirstDocumentComparison.CSharpEssentials_AllConventions), SwashbuckleHosts.AllConventions, DocumentConventions.All, null);
        Console.WriteLine($"Self-check passed on {AppContext.TargetFrameworkName}.");
    }

    private static void Cold(SwashbuckleDocumentComparison warm, string row, Func<ApiDocsHost> start, DocumentConventions conventions, PlainShapes? plain)
    {
        ApiDocsHost host = start();
        try
        {
            var facts = DocumentFacts.Read(host.ProbeDocument());
            ApiDocsChecks.Verify("first document " + row, facts, "3.0.", conventions, plain);
            Parity.Same("first document " + row, ("warm", warm.Facts[row].Hash), ("first", facts.Hash));
            Console.WriteLine($"First document {row}: {facts}");
        }
        finally
        {
            host.Stop();
        }
    }

    private static string VersionOf(Type type) =>
        type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "?";
}
