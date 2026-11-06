using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.AspNetCore.OpenApi.Benchmarks;

/// <summary>
/// Warm document generation: one <c>GET /openapi/v1.json</c> through TestServer per invocation, on a host built in
/// [GlobalSetup]. Microsoft.AspNetCore.OpenApi does not cache the document, so every request builds it again.
/// Every host serves the same 50 Minimal API endpoints over 30 string enums (<see cref="SampleApi"/>) with the same
/// metadata, in the default OpenAPI 3.1 version. The rows are not like-for-like on output:
/// <list type="bullet">
/// <item><c>Plain_AddOpenApi</c> is the baseline: the framework schema for every enum.</item>
/// <item><c>Plain_AddOpenApi_StringEnums</c> uses different JSON options: <c>JsonStringEnumConverter</c> instead of the CSE converter, the framework's documented way to get string enums.</item>
/// <item><c>CSharpEssentials_EnumConventions</c> writes more output: one component per enum with wire names, <c>x-enum-*</c> arrays, a value table in the description and the wire name as <c>default</c>.</item>
/// <item><c>CSharpEssentials_IdempotencyConventions</c> writes more output: the <c>Idempotency-Key</c> header, <c>Idempotency-Replayed</c> on 2xx and 400/409/422 problem responses on the 10 idempotent POSTs.</item>
/// <item><c>CSharpEssentials_OptionalRouteParameters</c> writes 70 operations against plain's 50: each of the 10 <c>{year?}/{month?}</c> routes becomes 3 paths.</item>
/// <item><c>CSharpEssentials_AllConventions</c> is the Readme setup, all three conventions.</item>
/// </list>
/// [GlobalSetup] checks every host before measuring: the same 50 full-form operations, documents 1 and 5 equal, the same
/// openapi version, the CSE additions exactly where expected and none in the plain rows. The plain row writes
/// <c>Enum01</c> as a component with only its description (no <c>type</c>, no <c>enum</c>), because the framework does not
/// see through the CSE converter. The StringEnums row writes the six member names in <c>enum</c> without a <c>type</c>.
/// Both shapes are asserted exactly (<see cref="OpenApiHosts.PlainEnum01"/>, <see cref="OpenApiHosts.PlainStringEnumsEnum01"/>).
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
public class OpenApiDocumentComparison
{
    private const string Version = "3.1.";

    private readonly Dictionary<string, DocumentFacts> _facts = [];
    private ApiDocsHost _plain = null!;
    private ApiDocsHost _plainStringEnums = null!;
    private ApiDocsHost _enums = null!;
    private ApiDocsHost _idempotency = null!;
    private ApiDocsHost _optionalRoutes = null!;
    private ApiDocsHost _all = null!;

    internal IReadOnlyDictionary<string, DocumentFacts> Facts => _facts;

    [GlobalSetup]
    public void Setup()
    {
        _plain = Check(nameof(Plain_AddOpenApi), OpenApiHosts.Plain(), DocumentConventions.None, OpenApiHosts.PlainShapes);
        _plainStringEnums = Check(nameof(Plain_AddOpenApi_StringEnums), OpenApiHosts.PlainStringEnums(), DocumentConventions.None, OpenApiHosts.StringEnumsShapes);
        _enums = Check(nameof(CSharpEssentials_EnumConventions), OpenApiHosts.EnumConventions(), DocumentConventions.Enums, null);
        _idempotency = Check(nameof(CSharpEssentials_IdempotencyConventions), OpenApiHosts.IdempotencyConventions(), DocumentConventions.Idempotency, OpenApiHosts.PlainShapes);
        _optionalRoutes = Check(nameof(CSharpEssentials_OptionalRouteParameters), OpenApiHosts.OptionalRouteParameters(), DocumentConventions.OptionalRoutes, OpenApiHosts.PlainShapes);
        _all = Check(nameof(CSharpEssentials_AllConventions), OpenApiHosts.AllConventions(), DocumentConventions.All, null);
        Parity.Same("openapi version", [.. _facts.Select(static f => (f.Key, f.Value.Version))]);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _plain.Stop();
        _plainStringEnums.Stop();
        _enums.Stop();
        _idempotency.Stop();
        _optionalRoutes.Stop();
        _all.Stop();
    }

    // https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi
    [Benchmark(Baseline = true)]
    public Task<int> Plain_AddOpenApi() => _plain.GetDocumentAsync();

    // https://learn.microsoft.com/aspnet/core/fundamentals/openapi/include-metadata#enum
    [Benchmark]
    public Task<int> Plain_AddOpenApi_StringEnums() => _plainStringEnums.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD#enum-schemas
    [Benchmark]
    public Task<int> CSharpEssentials_EnumConventions() => _enums.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD#idempotency
    [Benchmark]
    public Task<int> CSharpEssentials_IdempotencyConventions() => _idempotency.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD#optional-route-parameters
    [Benchmark]
    public Task<int> CSharpEssentials_OptionalRouteParameters() => _optionalRoutes.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD
    [Benchmark]
    public Task<int> CSharpEssentials_AllConventions() => _all.GetDocumentAsync();

    private ApiDocsHost Check(string row, ApiDocsHost host, DocumentConventions conventions, PlainShapes? plain)
    {
        DocumentFacts facts = ApiDocsChecks.Read(row, host.ProbeDocument);
        ApiDocsChecks.Verify(row, facts, Version, conventions, plain);
        _facts[row] = facts;
        return host;
    }
}
