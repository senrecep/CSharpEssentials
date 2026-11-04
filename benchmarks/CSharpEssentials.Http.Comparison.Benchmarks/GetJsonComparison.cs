using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Flurl.Http;
using Flurl.Http.Configuration;
using Refit;
using RestSharp;
using RestSharp.Serializers.Json;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// GET /users and deserialize <c>User[]</c> of <see cref="Count"/> elements. Every library wraps the same
/// <see cref="HttpClient"/> over <see cref="StubHandler"/>. Get passes the one shared Web options instance through each
/// library's documented hook. Defaults (labelled) uses each library's default options: CSE's Readme defaults
/// (polymorphic, enum and DateTime converter factories) and Flurl's case-insensitive defaults. SourceGen (labelled) has no
/// CSE row: this branch has no JsonTypeInfo overloads.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the clients are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class GetJsonComparison
{
    private const string Get = "Get";
    private const string Defaults = "Defaults";
    private const string SourceGen = "SourceGen";

    private StubHandler _stub = null!;
    private HttpClient _client = null!;
    private IUsersApi _refit = null!;
    private IUsersApi _refitSourceGen = null!;
    private FlurlClient _flurl = null!;
    private FlurlClient _flurlDefaults = null!;
    private RestClient _rest = null!;

    [Params(1, 50)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _stub = new StubHandler(Inputs.UsersBody(Count));
        _client = SharedClient.Create(_stub);
        _refit = RestService.ForGenerated<IUsersApi>(_client, new RefitSettings(new SystemTextJsonContentSerializer(Inputs.WebOptions)));
        _refitSourceGen = RestService.ForGenerated<IUsersApi>(_client, BenchJsonContext.Default);
        _flurl = new FlurlClient(_client);
        _flurl.WithSettings(s => s.JsonSerializer = new DefaultJsonSerializer(Inputs.WebOptions));
        _flurlDefaults = new FlurlClient(_client);
        _rest = new RestClient(_client, disposeHttpClient: false, configureSerialization: s => s.UseSystemTextJson(Inputs.WebOptions));

        string expected = Inputs.Describe(Inputs.Users(Count));
        Parity.Same(Get,
            ("expected", expected),
            (nameof(CSharpEssentials_Get), Inputs.Describe(Inputs.Run(CSharpEssentials_Get()).Value)),
            (nameof(Bcl_Get), Inputs.Describe(Inputs.Run(Bcl_Get())!)),
            (nameof(Refit_Get), Inputs.Describe(Inputs.Run(Refit_Get()))),
            (nameof(Flurl_Get), Inputs.Describe(Inputs.Run(Flurl_Get()))),
            (nameof(RestSharp_Get), Inputs.Describe(Inputs.Run(RestSharp_Get())!)));
        Parity.Same(Defaults,
            ("expected", expected),
            (nameof(CSharpEssentials_GetDefaultOptions), Inputs.Describe(Inputs.Run(CSharpEssentials_GetDefaultOptions()).Value)),
            (nameof(Flurl_GetDefaultOptions), Inputs.Describe(Inputs.Run(Flurl_GetDefaultOptions()))));
        Parity.Same(SourceGen,
            ("expected", expected),
            (nameof(Bcl_GetSourceGen), Inputs.Describe(Inputs.Run(Bcl_GetSourceGen())!)),
            (nameof(Refit_GetSourceGen), Inputs.Describe(Inputs.Run(Refit_GetSourceGen()))));
        SharedClient.AssertUntouched(nameof(GetJsonComparison), _client);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rest.Dispose();
        _flurl.Dispose();
        _flurlDefaults.Dispose();
        _client.Dispose();
        _stub.Dispose();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#get-post-put-patch-delete-as-result
    [Benchmark(Baseline = true), BenchmarkCategory(Get)]
    public Task<Result<User[]>> CSharpEssentials_Get() => _client.GetFromJsonAsResultAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions);

    // https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient#use-an-http-get-request
    [Benchmark, BenchmarkCategory(Get)]
    public Task<User[]?> Bcl_Get() => _client.GetFromJsonAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions);

    // https://www.reactiveui.net/documentation/refit/clients/creation/#use-settings-instead-of-a-context
    [Benchmark, BenchmarkCategory(Get)]
    public Task<User[]> Refit_Get() => _refit.GetUsersAsync();

    // https://flurl.dev/docs/clients/#explicit-usage and https://flurl.dev/docs/configuration/#serializers
    [Benchmark, BenchmarkCategory(Get)]
    public Task<User[]> Flurl_Get() => _flurl.Request("users").GetJsonAsync<User[]>();

    // https://restsharp.dev/docs/usage/execute#get-calls and https://restsharp.dev/docs/advanced/serialization#json
    [Benchmark, BenchmarkCategory(Get)]
    public Task<User[]?> RestSharp_Get() => _rest.GetAsync<User[]>("users");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#get-post-put-patch-delete-as-result
    [Benchmark(Baseline = true), BenchmarkCategory(Defaults)]
    public Task<Result<User[]>> CSharpEssentials_GetDefaultOptions() => _client.GetFromJsonAsResultAsync<User[]>(Inputs.UsersUri);

    // https://flurl.dev/docs/configuration/#serializers
    [Benchmark, BenchmarkCategory(Defaults)]
    public Task<User[]> Flurl_GetDefaultOptions() => _flurlDefaults.Request("users").GetJsonAsync<User[]>();

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation
    [Benchmark(Baseline = true), BenchmarkCategory(SourceGen)]
    public Task<User[]?> Bcl_GetSourceGen() => _client.GetFromJsonAsync(Inputs.UsersUri, BenchJsonContext.Default.UserArray);

    // https://www.reactiveui.net/documentation/refit/clients/creation/#use-settings-instead-of-a-context
    [Benchmark, BenchmarkCategory(SourceGen)]
    public Task<User[]> Refit_GetSourceGen() => _refitSourceGen.GetUsersAsync();
}
