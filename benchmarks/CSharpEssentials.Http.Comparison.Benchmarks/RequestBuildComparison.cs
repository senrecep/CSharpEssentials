using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using Flurl;
using RestSharp;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Builds <c>GET https://bench.local/users/42?include=profile&amp;page=2</c> with <c>Accept: application/json</c>
/// without sending it, then disposes the message. The BCL row is a hand-written reference. Flurl.Http has no public
/// build-without-send API and RestSharp builds only the URI (<c>BuildUri</c>), so those rows (labelled) are each library's
/// URL builder plus the same hand-made message and header. Refit builds requests only inside a generated call and has no
/// row. CSE's <c>WithQuery("page", 2)</c> goes through the object overload, as in the Readme.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the clients are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[SuppressMessage("Major Code Smell", "S1075:URIs should not be hardcoded", Justification = "Fixed address of the in-memory stub; no request leaves the process.")]
[MemoryDiagnoser]
public class RequestBuildComparison
{
    private StubHandler _stub = null!;
    private HttpClient _client = null!;
    private RestClient _rest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _stub = new StubHandler(Inputs.UsersBody(1));
        _client = SharedClient.Create(_stub);
        _rest = new RestClient(_client, disposeHttpClient: false);

        Parity.Same(nameof(RequestBuildComparison),
            ("expected", "GET https://bench.local/users/42?include=profile&page=2 application/json"),
            (nameof(CSharpEssentials_Build), Describe(BuildCse())),
            (nameof(Bcl_Build), Describe(BuildBcl())),
            (nameof(Flurl_Build), Describe(BuildFlurl())),
            (nameof(RestSharp_Build), Describe(BuildRestSharp())));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rest.Dispose();
        _client.Dispose();
        _stub.Dispose();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#httprequestbuilder
    [Benchmark(Baseline = true)]
    public Uri? CSharpEssentials_Build()
    {
        using HttpRequestMessage message = BuildCse();
        return message.RequestUri;
    }

    // https://learn.microsoft.com/dotnet/api/system.net.http.httprequestmessage
    [Benchmark]
    public Uri? Bcl_Build()
    {
        using HttpRequestMessage message = BuildBcl();
        return message.RequestUri;
    }

    // https://flurl.dev/docs/fluent-url/
    [Benchmark]
    public Uri? Flurl_Build()
    {
        using HttpRequestMessage message = BuildFlurl();
        return message.RequestUri;
    }

    // https://restsharp.dev/docs/usage/request and https://restsharp.dev/migration (BuildUri)
    [Benchmark]
    public Uri? RestSharp_Build()
    {
        using HttpRequestMessage message = BuildRestSharp();
        return message.RequestUri;
    }

    private static HttpRequestMessage BuildCse() =>
        HttpRequestBuilder.Get("https://bench.local/users/{id}")
            .WithRoute("id", 42)
            .WithQuery("include", "profile")
            .WithQuery("page", 2)
            .WithHeader("Accept", "application/json")
            .Build()
            .Value;

    private static HttpRequestMessage BuildBcl() =>
        WithAccept(new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(string.Concat("https://bench.local/users/", 42.ToString(CultureInfo.InvariantCulture), "?include=profile&page=", 2.ToString(CultureInfo.InvariantCulture)))));

    private static HttpRequestMessage BuildFlurl() =>
        WithAccept(new HttpRequestMessage(
            HttpMethod.Get,
            new Url("https://bench.local").AppendPathSegments("users", 42).SetQueryParam("include", "profile").SetQueryParam("page", 2).ToUri()));

    private HttpRequestMessage BuildRestSharp() =>
        WithAccept(new HttpRequestMessage(
            HttpMethod.Get,
            _rest.BuildUri(new RestRequest("users/{id}").AddUrlSegment("id", 42).AddQueryParameter("include", "profile").AddQueryParameter("page", 2))));

    private static HttpRequestMessage WithAccept(HttpRequestMessage message)
    {
        message.Headers.TryAddWithoutValidation("Accept", "application/json");
        return message;
    }

    private static string Describe(HttpRequestMessage message)
    {
        using (message)
        {
            return $"{message.Method} {message.RequestUri!.AbsoluteUri} {string.Join(",", message.Headers.Accept)}";
        }
    }
}
