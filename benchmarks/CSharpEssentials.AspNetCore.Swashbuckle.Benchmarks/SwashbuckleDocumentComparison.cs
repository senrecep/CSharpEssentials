using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

/// <summary>
/// Warm document generation: one <c>GET /swagger/v1/swagger.json</c> through TestServer per invocation, on a host built in
/// [GlobalSetup]. Swashbuckle does not cache the document, so every request builds it again.
/// Every host serves the same 50 Minimal API endpoints over 30 string enums (<see cref="SampleApi"/>) with the same
/// metadata, in the Swashbuckle default OpenAPI 3.0 version, as the CSE Readme says to keep. The rows are not like-for-like on output:
/// <list type="bullet">
/// <item><c>Plain_AddSwaggerGen</c> is the baseline: the Swashbuckle schema for every enum, with the values it reads from the app JSON options.</item>
/// <item><c>Plain_AddSwaggerGen_StringEnums</c> uses different JSON options: <c>JsonStringEnumConverter</c> instead of the CSE converter, the framework's documented way to get string enums.</item>
/// <item><c>CSharpEssentials_EnumConventions</c> writes more output: one component per enum with wire names, <c>x-enum-*</c> arrays, a value table in the description and the wire name as <c>default</c>.</item>
/// <item><c>CSharpEssentials_IdempotencyConventions</c> writes more output: the <c>Idempotency-Key</c> header, <c>Idempotency-Replayed</c> on 2xx and 400/409/422 problem responses on the 10 idempotent POSTs.</item>
/// <item><c>CSharpEssentials_OptionalRouteParameters</c> writes 70 operations against plain's 50: each of the 10 <c>{year?}/{month?}</c> routes becomes 3 paths.</item>
/// <item><c>CSharpEssentials_AllConventions</c> is all three conventions on <c>AddSwaggerGen</c>.</item>
/// <item><c>CSharpEssentials_AddSwagger</c> is the full Readme setup and does more work: all three conventions plus a security scheme and requirement, the CSE schema ids, XML comments, <c>ResolveConflictingActions</c> and <c>MapType</c> for <c>TimeSpan</c>/<c>TimeOnly</c>.</item>
/// </list>
/// [GlobalSetup] checks every host before measuring: the same 50 full-form operations, documents 1 and 5 equal, the same
/// openapi version, the CSE additions exactly where expected and none in the plain rows. The plain row writes
/// <c>Enum01</c> as <c>type: string</c> with the CSE wire names (<c>alpha</c> ...), because Swashbuckle writes each value
/// through the app JSON options. The StringEnums row writes <c>type: string</c> with the member names (<c>Alpha</c> ...).
/// Both shapes are asserted exactly (<see cref="SwashbuckleHosts.PlainEnum01"/>, <see cref="SwashbuckleHosts.PlainStringEnumsEnum01"/>).
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the hosts are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
public class SwashbuckleDocumentComparison
{
    private const string Version = "3.0.";

    private readonly Dictionary<string, DocumentFacts> _facts = [];
    private ApiDocsHost _plain = null!;
    private ApiDocsHost _plainStringEnums = null!;
    private ApiDocsHost _enums = null!;
    private ApiDocsHost _idempotency = null!;
    private ApiDocsHost _optionalRoutes = null!;
    private ApiDocsHost _all = null!;
    private ApiDocsHost _addSwagger = null!;

    internal IReadOnlyDictionary<string, DocumentFacts> Facts => _facts;

    [GlobalSetup]
    public void Setup()
    {
        _plain = Check(nameof(Plain_AddSwaggerGen), SwashbuckleHosts.Plain(), DocumentConventions.None, SwashbuckleHosts.PlainShapes);
        _plainStringEnums = Check(nameof(Plain_AddSwaggerGen_StringEnums), SwashbuckleHosts.PlainStringEnums(), DocumentConventions.None, SwashbuckleHosts.StringEnumsShapes);
        _enums = Check(nameof(CSharpEssentials_EnumConventions), SwashbuckleHosts.EnumConventions(), DocumentConventions.Enums, null);
        _idempotency = Check(nameof(CSharpEssentials_IdempotencyConventions), SwashbuckleHosts.IdempotencyConventions(), DocumentConventions.Idempotency, SwashbuckleHosts.PlainShapes);
        _optionalRoutes = Check(nameof(CSharpEssentials_OptionalRouteParameters), SwashbuckleHosts.OptionalRouteParameters(), DocumentConventions.OptionalRoutes, SwashbuckleHosts.PlainShapes);
        _all = Check(nameof(CSharpEssentials_AllConventions), SwashbuckleHosts.AllConventions(), DocumentConventions.All, null);
        _addSwagger = Check(nameof(CSharpEssentials_AddSwagger), SwashbuckleHosts.AddSwagger(), DocumentConventions.All, null);
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
        _addSwagger.Stop();
    }

    // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.3.1/README.md#getting-started
    [Benchmark(Baseline = true)]
    public Task<int> Plain_AddSwaggerGen() => _plain.GetDocumentAsync();

    // https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#configure-json-serialization-options
    [Benchmark]
    public Task<int> Plain_AddSwaggerGen_StringEnums() => _plainStringEnums.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD#enum-schemas
    [Benchmark]
    public Task<int> CSharpEssentials_EnumConventions() => _enums.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD#idempotency
    [Benchmark]
    public Task<int> CSharpEssentials_IdempotencyConventions() => _idempotency.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD#optional-route-parameters
    [Benchmark]
    public Task<int> CSharpEssentials_OptionalRouteParameters() => _optionalRoutes.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD
    [Benchmark]
    public Task<int> CSharpEssentials_AllConventions() => _all.GetDocumentAsync();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD#swagger-with-versioning
    [Benchmark]
    public Task<int> CSharpEssentials_AddSwagger() => _addSwagger.GetDocumentAsync();

    private ApiDocsHost Check(string row, ApiDocsHost host, DocumentConventions conventions, PlainShapes? plain)
    {
        DocumentFacts facts = ApiDocsChecks.Read(row, host.ProbeDocument);
        ApiDocsChecks.Verify(row, facts, Version, conventions, plain);
        _facts[row] = facts;
        return host;
    }
}
