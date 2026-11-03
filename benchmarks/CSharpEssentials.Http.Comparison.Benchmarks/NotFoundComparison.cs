using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Flurl.Http;
using Flurl.Http.Configuration;
using Refit;
using RestSharp;
using RestSharp.Serializers.Json;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// GET /missing answers 404 with a problem+json body, and every row takes its library's documented non-throwing path:
/// CSE returns a failed Result (code Http.404), the BCL checks the status, Refit returns <c>IApiResponse&lt;T&gt;</c>, Flurl
/// uses <c>AllowHttpStatus(404)</c> and RestSharp <c>ExecuteGetAsync</c>. The default APIs of Refit, Flurl and RestSharp
/// throw on 404. Refit still builds an <c>ApiException</c> for the response. RestSharp reads the body to a string and
/// also deserializes the problem+json error body into <c>User</c> (RestSerializers.DeserializeContent attempts HTTP error
/// bodies by design, RestClient.Extensions.cs:47-48 at 114.1.0). Both are the libraries' documented behaviour.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the clients are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
public class NotFoundComparison
{
    private StubHandler _stub = null!;
    private HttpClient _client = null!;
    private IUsersApi _refit = null!;
    private FlurlClient _flurl = null!;
    private RestClient _rest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _stub = new StubHandler(Inputs.UsersBody(1));
        _client = SharedClient.Create(_stub);
        _refit = RestService.ForGenerated<IUsersApi>(_client, new RefitSettings(new SystemTextJsonContentSerializer(Inputs.WebOptions)));
        _flurl = new FlurlClient(_client);
        _flurl.WithSettings(s => s.JsonSerializer = new DefaultJsonSerializer(Inputs.WebOptions));
        _rest = new RestClient(_client, disposeHttpClient: false, configureSerialization: s => s.UseSystemTextJson(Inputs.WebOptions));

        Result<User> cse = Inputs.Run(CSharpEssentials_NotFound());
        Parity.Same(nameof(NotFoundComparison),
            ("expected", "404 NotFound"),
            (nameof(CSharpEssentials_NotFound), $"{cse.FirstError.Code[5..]} {cse.FirstError.Type}"),
            (nameof(Bcl_NotFound), $"{Inputs.Run(Bcl_NotFound())} NotFound"),
            (nameof(Refit_NotFound), $"{Inputs.Run(Refit_NotFound())} NotFound"),
            (nameof(Flurl_NotFound), $"{Inputs.Run(Flurl_NotFound())} NotFound"),
            (nameof(RestSharp_NotFound), $"{Inputs.Run(RestSharp_NotFound())} NotFound"));
        Parity.Same(nameof(NotFoundComparison), ("expected", "Http.404"), (nameof(CSharpEssentials_NotFound), cse.FirstError.Code));
        SharedClient.AssertUntouched(nameof(NotFoundComparison), _client);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rest.Dispose();
        _flurl.Dispose();
        _client.Dispose();
        _stub.Dispose();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#status-code-mapping
    [Benchmark(Baseline = true)]
    public Task<Result<User>> CSharpEssentials_NotFound() => _client.GetFromJsonAsResultAsync<User>(Inputs.MissingUri, Inputs.WebOptions);

    // https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient#use-http-error-handling
    [Benchmark]
    public async Task<int> Bcl_NotFound()
    {
        using HttpResponseMessage response = await _client.GetAsync(Inputs.MissingUri).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            await response.Content.ReadFromJsonAsync<User>(Inputs.WebOptions).ConfigureAwait(false);
        }

        return (int)response.StatusCode;
    }

    // https://www.reactiveui.net/documentation/refit/results/responses/
    [Benchmark]
    public async Task<int> Refit_NotFound()
    {
        using IApiResponse<User> response = await _refit.GetMissingAsync().ConfigureAwait(false);
        return (int)response.StatusCode.GetValueOrDefault();
    }

    // https://flurl.dev/docs/fluent-http/#responses-and-error-handling
    [Benchmark]
    public async Task<int> Flurl_NotFound()
    {
        using IFlurlResponse response = await _flurl.Request("missing").AllowHttpStatus(404).GetAsync().ConfigureAwait(false);
        return response.StatusCode;
    }

    // https://restsharp.dev/docs/usage/execute#executing-requests
    [Benchmark]
    public async Task<int> RestSharp_NotFound()
    {
        RestResponse<User> response = await _rest.ExecuteGetAsync<User>(new RestRequest("missing")).ConfigureAwait(false);
        return (int)response.StatusCode;
    }
}
