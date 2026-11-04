using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpEssentials.AspNetCore.OpenApi.Benchmarks;

/// <summary>Runs every class's parity checks without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class OpenApiSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine(
            $"ASP.NET Core {VersionOf(typeof(WebApplication))}, Microsoft.AspNetCore.OpenApi {VersionOf(typeof(Microsoft.AspNetCore.OpenApi.OpenApiOptions))}, Microsoft.OpenApi {VersionOf(typeof(Microsoft.OpenApi.OpenApiDocument))} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");

        var warm = new OpenApiDocumentComparison();
        warm.Setup();
        warm.Cleanup();
        foreach ((string row, DocumentFacts facts) in warm.Facts)
            Console.WriteLine($"{row}: {facts}");
        Console.WriteLine($"Plain Enum01: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi)].Enum01Shape}");
        Console.WriteLine($"StringEnums Enum01: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi_StringEnums)].Enum01Shape}");
        Console.WriteLine($"Plain Kind default: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi)].KindDefaults[0]}");
        Console.WriteLine($"StringEnums Kind default: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi_StringEnums)].KindDefaults[0]}");
        Console.WriteLine($"Plain Stage: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi)].StageShape}");
        Console.WriteLine($"StringEnums Stage: {warm.Facts[nameof(OpenApiDocumentComparison.Plain_AddOpenApi_StringEnums)].StageShape}");

        Cold(warm, nameof(OpenApiFirstDocumentComparison.Plain_AddOpenApi), OpenApiHosts.Plain, DocumentConventions.None, OpenApiHosts.PlainShapes);
        Cold(warm, nameof(OpenApiFirstDocumentComparison.CSharpEssentials_AllConventions), OpenApiHosts.AllConventions, DocumentConventions.All, null);
        Console.WriteLine($"Self-check passed on {AppContext.TargetFrameworkName}.");
    }

    private static void Cold(OpenApiDocumentComparison warm, string row, Func<ApiDocsHost> start, DocumentConventions conventions, PlainShapes? plain)
    {
        ApiDocsHost host = start();
        try
        {
            var facts = DocumentFacts.Read(host.ProbeDocument());
            ApiDocsChecks.Verify("first document " + row, facts, "3.1.", conventions, plain);
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
