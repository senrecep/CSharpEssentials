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
/// POST /users with <c>NewUser("Ada", "ada@bench.local", 36)</c> and read the 201 <c>User</c>. Post passes the shared Web
/// options through each library's hook; the stub drains every body, so lazy and eager serializers do the same work. The
/// setup checks that every row sends the same method, URI and body bytes and decodes the same user. Content-Type is not
/// compared: the spellings differ (<see cref="ContentTypes"/>). CSE's value parameter is typed object, so it serializes by
/// runtime type. Defaults (labelled) is CSE with its Readme default options.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the clients are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PostJsonComparison
{
    private const string Post = "Post";
    private const string Defaults = "Defaults";

    private StubHandler _stub = null!;
    private HttpClient _client = null!;
    private IUsersApi _refit = null!;
    private FlurlClient _flurl = null!;
    private RestClient _rest = null!;

    public string ContentTypes { get; private set; } = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _stub = new StubHandler(Inputs.UsersBody(1));
        _client = SharedClient.Create(_stub);
        _refit = RestService.ForGenerated<IUsersApi>(_client, new RefitSettings(new SystemTextJsonContentSerializer(Inputs.WebOptions)));
        _flurl = new FlurlClient(_client);
        _flurl.WithSettings(s => s.JsonSerializer = new DefaultJsonSerializer(Inputs.WebOptions));
        _rest = new RestClient(_client, disposeHttpClient: false, configureSerialization: s => s.UseSystemTextJson(Inputs.WebOptions));

        _stub.Capture = true;
        List<string> contentTypes = [];
        string expected = $"POST https://bench.local/users {Inputs.NewUserJson} {Inputs.CreatedUser}";
        Parity.Same(Post,
            ("expected", expected),
            (nameof(CSharpEssentials_Post), Sent(nameof(CSharpEssentials_Post), Inputs.Run(CSharpEssentials_Post()).Value, contentTypes)),
            (nameof(Bcl_Post), Sent(nameof(Bcl_Post), Inputs.Run(Bcl_Post()), contentTypes)),
            (nameof(Refit_Post), Sent(nameof(Refit_Post), Inputs.Run(Refit_Post()), contentTypes)),
            (nameof(Flurl_Post), Sent(nameof(Flurl_Post), Inputs.Run(Flurl_Post()), contentTypes)),
            (nameof(RestSharp_Post), Sent(nameof(RestSharp_Post), Inputs.Run(RestSharp_Post())!, contentTypes)));
        Parity.Same(Defaults,
            ("expected", expected),
            (nameof(CSharpEssentials_PostDefaultOptions), Sent(nameof(CSharpEssentials_PostDefaultOptions), Inputs.Run(CSharpEssentials_PostDefaultOptions()).Value, contentTypes)));
        _stub.Capture = false;
        ContentTypes = string.Join("; ", contentTypes);
        SharedClient.AssertUntouched(nameof(PostJsonComparison), _client);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rest.Dispose();
        _flurl.Dispose();
        _client.Dispose();
        _stub.Dispose();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#get-post-put-patch-delete-as-result
    [Benchmark(Baseline = true), BenchmarkCategory(Post)]
    public Task<Result<User>> CSharpEssentials_Post() => _client.PostAsJsonAsResultAsync<User>(Inputs.UsersUri, Inputs.NewUser, Inputs.WebOptions);

    // https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient#use-an-http-post-request
    [Benchmark, BenchmarkCategory(Post)]
    public async Task<User> Bcl_Post()
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(Inputs.UsersUri, Inputs.NewUser, Inputs.WebOptions).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>(Inputs.WebOptions).ConfigureAwait(false))!;
    }

    // https://www.reactiveui.net/documentation/refit/requests/bodies/
    [Benchmark, BenchmarkCategory(Post)]
    public Task<User> Refit_Post() => _refit.CreateUserAsync(Inputs.NewUser);

    // https://flurl.dev/docs/fluent-http/#basic-usage
    [Benchmark, BenchmarkCategory(Post)]
    public Task<User> Flurl_Post() => _flurl.Request("users").PostJsonAsync(Inputs.NewUser).ReceiveJson<User>();

    // https://restsharp.dev/docs/usage/execute#post-calls
    [Benchmark, BenchmarkCategory(Post)]
    public Task<User?> RestSharp_Post() => _rest.PostJsonAsync<NewUser, User>("users", Inputs.NewUser);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#get-post-put-patch-delete-as-result
    [Benchmark(Baseline = true), BenchmarkCategory(Defaults)]
    public Task<Result<User>> CSharpEssentials_PostDefaultOptions() => _client.PostAsJsonAsResultAsync<User>(Inputs.UsersUri, Inputs.NewUser);

    private string Sent(string row, User user, List<string> contentTypes)
    {
        contentTypes.Add($"{row}: {_stub.LastContentType}");
        return $"{_stub.LastMethod} {_stub.LastUri} {_stub.LastBody} {user}";
    }
}
