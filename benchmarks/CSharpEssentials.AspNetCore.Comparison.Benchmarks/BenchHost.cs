using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// One in-process Production host per variant over TestServer. Logging providers are cleared; the logging suites add
/// their own counting provider. Every response body is read, so the measured time covers the whole response.
/// </summary>
public sealed class BenchHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private BenchHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
        Counter = app.Services.GetRequiredService<HandlerCounter>();
    }

    /// <summary>The client that sends requests to the in-process server.</summary>
    public HttpClient Client { get; }

    /// <summary>The handler counter of this host.</summary>
    public HandlerCounter Counter { get; }

    /// <summary>The host's services.</summary>
    public IServiceProvider Services => _app.Services;

    /// <summary>Builds and starts a host, for [GlobalSetup].</summary>
    /// <param name="services">Registers the variant's services.</param>
    /// <param name="app">Adds the variant's middleware and endpoints.</param>
    /// <returns>The started host.</returns>
    public static BenchHost Start(Action<WebApplicationBuilder> services, Action<WebApplication> app)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<HandlerCounter>();
        services(builder);

        WebApplication web = builder.Build();
        app(web);
        web.StartAsync().GetAwaiter().GetResult();
        return new BenchHost(web);
    }

    /// <summary>Sends a GET, reads the whole body and returns the status code.</summary>
    /// <param name="path">The relative path and query.</param>
    /// <param name="ifNoneMatch">The If-None-Match value, or <see langword="null"/> for none.</param>
    /// <returns>The response status code.</returns>
    public async Task<HttpStatusCode> GetAsync(string path, string? ifNoneMatch = null)
    {
        using HttpRequestMessage request = Requests.Get(path, ifNoneMatch);
        return await SendAsync(request).ConfigureAwait(false);
    }

    /// <summary>Sends a JSON POST, reads the whole body and returns the status code.</summary>
    /// <param name="path">The relative path.</param>
    /// <param name="body">The JSON body.</param>
    /// <param name="idempotencyKey">The Idempotency-Key value, or <see langword="null"/> for none.</param>
    /// <returns>The response status code.</returns>
    public async Task<HttpStatusCode> PostAsync(string path, byte[] body, string? idempotencyKey = null)
    {
        using HttpRequestMessage request = Requests.PostJson(path, body, idempotencyKey);
        return await SendAsync(request).ConfigureAwait(false);
    }

    /// <summary>Sends a GET and records what it observed, for [GlobalSetup].</summary>
    /// <param name="path">The relative path and query.</param>
    /// <param name="ifNoneMatch">The If-None-Match value, or <see langword="null"/> for none.</param>
    /// <returns>The observation.</returns>
    public Probe ProbeGet(string path, string? ifNoneMatch = null) => ProbeGetAsync(path, ifNoneMatch).GetAwaiter().GetResult();

    /// <summary>Sends a JSON POST and records what it observed, for [GlobalSetup].</summary>
    /// <param name="path">The relative path.</param>
    /// <param name="body">The JSON body.</param>
    /// <param name="idempotencyKey">The Idempotency-Key value, or <see langword="null"/> for none.</param>
    /// <returns>The observation.</returns>
    public Probe ProbePost(string path, byte[] body, string? idempotencyKey = null) => ProbePostAsync(path, body, idempotencyKey).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync().ConfigureAwait(false);
        await _app.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Synchronous <see cref="DisposeAsync"/>, for [GlobalCleanup].</summary>
    public void Stop() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    private async Task<HttpStatusCode> SendAsync(HttpRequestMessage request)
    {
        using HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        return response.StatusCode;
    }

    private async Task<Probe> ProbeGetAsync(string path, string? ifNoneMatch)
    {
        using HttpRequestMessage request = Requests.Get(path, ifNoneMatch);
        return await ProbeAsync(request).ConfigureAwait(false);
    }

    private async Task<Probe> ProbePostAsync(string path, byte[] body, string? idempotencyKey)
    {
        using HttpRequestMessage request = Requests.PostJson(path, body, idempotencyKey);
        return await ProbeAsync(request).ConfigureAwait(false);
    }

    private async Task<Probe> ProbeAsync(HttpRequestMessage request)
    {
        int before = Counter.Count;
        using HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
        byte[] body = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        string headers = string.Join(",", response.Headers.Concat(response.Content.Headers).Select(static h => h.Key).Order(StringComparer.OrdinalIgnoreCase));
        return new Probe(response.StatusCode, response.Content.Headers.ContentType?.MediaType, response.Headers.ETag?.ToString(), body, headers, Counter.Count - before);
    }
}
