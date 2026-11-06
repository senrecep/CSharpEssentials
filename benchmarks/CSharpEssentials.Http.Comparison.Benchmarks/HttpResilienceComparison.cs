using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// GET /users (one user) through retry(3, zero delay) then timeout(30 s), on the success path. CSE and bare Polly wrap
/// the whole call, deserialization included, on the shared <see cref="HttpClient"/>. The Microsoft.Extensions.Http.Resilience
/// rows (labelled) wrap only the send inside an IHttpClientFactory client, with the factory's logging handlers and the
/// resilience telemetry on; the standard handler runs five strategies, not two. Factory_NoResilience is the factory
/// pipeline alone. Every factory client is resolved once in setup and, like the shared client, has an infinite timeout.
/// CSE's row wraps GetFromJsonAsResultAsync (buffered, Result.TryAsync); the Polly and factory rows wrap the BCL
/// GetFromJsonAsync (streamed). The CSE-vs-Polly difference includes the GetJsonComparison Get-category gap, not only the
/// policy wrapper; PolicyOverheadComparison measures the wrapper on its own.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the clients are created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
public class HttpResilienceComparison
{
    private StubHandler _stub = null!;
    private HttpClient _client = null!;
    private ServiceProvider _services = null!;
    private HttpClient _handlerClient = null!;
    private HttpClient _standardClient = null!;
    private HttpClient _factoryClient = null!;
    private ResiliencePolicy _policy;
    private ResiliencePipeline _pipeline = null!;
    private Func<CancellationToken, Task<Result<User[]>>> _getUsers = null!;

    [GlobalSetup]
    public void Setup()
    {
        _stub = new StubHandler(Inputs.UsersBody(1));
        _client = SharedClient.Create(_stub);
        _policy = HttpClientResilienceExtensions.CreateResiliencePolicy(maxRetryAttempts: 3, timeout: PollyPipelines.Timeout, retryDelay: TimeSpan.Zero);
        _getUsers = token => _client.GetFromJsonAsResultAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions, token);
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.Zero, BackoffType = DelayBackoffType.Exponential })
            .AddTimeout(PollyPipelines.Timeout)
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient("handler", ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(() => _stub)
            .AddResilienceHandler("bench", static builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.Zero });
                builder.AddTimeout(PollyPipelines.Timeout);
            });
        services.AddHttpClient("standard", ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(() => _stub)
            .AddStandardResilienceHandler();
        services.AddHttpClient("plain", ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(() => _stub);
        _services = services.BuildServiceProvider();
        IHttpClientFactory factory = _services.GetRequiredService<IHttpClientFactory>();
        _handlerClient = factory.CreateClient("handler");
        _standardClient = factory.CreateClient("standard");
        _factoryClient = factory.CreateClient("plain");

        string expected = Inputs.Describe(Inputs.Users(1));
        Parity.Same(nameof(HttpResilienceComparison),
            ("expected", expected),
            (nameof(CSharpEssentials_Policy), Inputs.Describe(Inputs.Run(CSharpEssentials_Policy()).Value)),
            (nameof(Polly_Pipeline), Inputs.Describe(Inputs.Run(Polly_Pipeline())!)),
            (nameof(MicrosoftResilience_Handler), Inputs.Describe(Inputs.Run(MicrosoftResilience_Handler())!)),
            (nameof(MicrosoftResilience_Standard), Inputs.Describe(Inputs.Run(MicrosoftResilience_Standard())!)),
            (nameof(Factory_NoResilience), Inputs.Describe(Inputs.Run(Factory_NoResilience())!)));
        SharedClient.AssertUntouched(nameof(CSharpEssentials_Policy), _client);
        SharedClient.AssertUntouched(nameof(MicrosoftResilience_Handler), _handlerClient);
        SharedClient.AssertUntouched(nameof(MicrosoftResilience_Standard), _standardClient);
        SharedClient.AssertUntouched(nameof(Factory_NoResilience), _factoryClient);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _services.Dispose();
        _client.Dispose();
        _stub.Dispose();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Http/Readme.MD#polly-resilience
    [Benchmark(Baseline = true)]
    public Task<Result<User[]>> CSharpEssentials_Policy() => _policy.ExecuteAsync(_getUsers);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/advanced/performance.md#use-static-lambdas
    [Benchmark]
    public ValueTask<User[]?> Polly_Pipeline() =>
        _pipeline.ExecuteAsync(
            static (client, token) => new ValueTask<User[]?>(client.GetFromJsonAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions, token)),
            _client,
            CancellationToken.None);

    // https://learn.microsoft.com/dotnet/core/resilience/http-resilience#add-custom-resilience-handlers
    [Benchmark]
    public Task<User[]?> MicrosoftResilience_Handler() => _handlerClient.GetFromJsonAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions);

    // https://learn.microsoft.com/dotnet/core/resilience/http-resilience#add-standard-resilience-handler
    [Benchmark]
    public Task<User[]?> MicrosoftResilience_Standard() => _standardClient.GetFromJsonAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions);

    // https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory
    [Benchmark]
    public Task<User[]?> Factory_NoResilience() => _factoryClient.GetFromJsonAsync<User[]>(Inputs.UsersUri, Inputs.WebOptions);

    private static void ConfigureClient(HttpClient client)
    {
        client.BaseAddress = Inputs.BaseAddress;
        client.Timeout = Timeout.InfiniteTimeSpan;
    }
}
