using System.Globalization;
using System.Net;

using CSharpEssentials.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Idempotency.Benchmarks;

/// <summary>
/// An in-process host with the same handler on two routes: <see cref="PlainPath"/> without idempotency and
/// <see cref="KeyedPath"/> with <c>WithIdempotency</c> on the in-memory store, so a benchmark pair differs only by
/// the idempotency middleware and filter. The handler reads the whole request body, as model binding would. Requests
/// are scoped globally because the benchmark has no authenticated user.
/// </summary>
public sealed class IdempotencyBenchmarkHost : IAsyncDisposable
{
    /// <summary>The route without idempotency.</summary>
    public const string PlainPath = "/plain";

    /// <summary>The route with idempotency.</summary>
    public const string KeyedPath = "/keyed";

    private const string KeyHeader = "Idempotency-Key";
    private const string ReplayedHeader = "Idempotency-Replayed";
    private const int KeysPerRetentionWindow = 4096;

    private static readonly TimeSpan PastRetention = TimeSpan.FromDays(2);

    private readonly WebApplication _app;
    private readonly FakeTimeProvider _clock;
    private long _keys;

    private IdempotencyBenchmarkHost(WebApplication app, FakeTimeProvider clock, HttpClient client)
    {
        _app = app;
        _clock = clock;
        Client = client;
    }

    /// <summary>The client that sends requests to the in-process server.</summary>
    public HttpClient Client { get; }

    /// <summary>Starts the host.</summary>
    /// <returns>The started host.</returns>
    public static async Task<IdempotencyBenchmarkHost> StartAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        FakeTimeProvider clock = new();
        builder.Services.AddSingleton<IIdempotencyRecordStore>(new InMemoryIdempotencyRecordStore(clock));
        builder.Services.AddIdempotency(options => options.ScopeResolver = IdempotencyScopes.Global);

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseIdempotency();
        app.MapPost(PlainPath, AcceptAsync);
        app.MapPost(KeyedPath, AcceptAsync).WithIdempotency();

        await app.StartAsync().ConfigureAwait(false);
        return new IdempotencyBenchmarkHost(app, clock, app.GetTestClient());
    }

    /// <summary>
    /// A key that has not been used yet. Every <see cref="KeysPerRetentionWindow"/> keys the store clock moves past
    /// the retention period, so the next reservation sweeps the completed records and the store stays bounded.
    /// </summary>
    /// <returns>The key.</returns>
    public string NextKey()
    {
        long key = Interlocked.Increment(ref _keys);
        if (key % KeysPerRetentionWindow == 0)
            _clock.Advance(PastRetention);
        return "key-" + key.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Sends <paramref name="body"/> to <paramref name="path"/>, with <paramref name="key"/> when given.</summary>
    /// <param name="path">The route.</param>
    /// <param name="body">The request body.</param>
    /// <param name="key">The idempotency key, or <see langword="null"/> to send none.</param>
    /// <returns>The response status code.</returns>
    public async Task<HttpStatusCode> SendAsync(string path, byte[] body, string? key)
    {
        using HttpResponseMessage response = await SendCoreAsync(path, body, key).ConfigureAwait(false);
        return response.StatusCode;
    }

    /// <summary>
    /// Fails unless <paramref name="key"/> is executed once and then replayed on <see cref="KeyedPath"/>, and
    /// <see cref="PlainPath"/> answers without a replay, so a benchmark never measures a skipped or rejected request.
    /// </summary>
    /// <param name="body">The request body.</param>
    /// <param name="key">An unused idempotency key.</param>
    /// <returns>A task that completes when the check passes.</returns>
    public async Task VerifyAsync(byte[] body, string key)
    {
        using HttpResponseMessage first = await SendCoreAsync(KeyedPath, body, key).ConfigureAwait(false);
        using HttpResponseMessage second = await SendCoreAsync(KeyedPath, body, key).ConfigureAwait(false);
        using HttpResponseMessage plain = await SendCoreAsync(PlainPath, body, null).ConfigureAwait(false);

        if (first.StatusCode != HttpStatusCode.OK || first.Headers.Contains(ReplayedHeader))
            throw new InvalidOperationException($"The first keyed request returned {first.StatusCode} instead of an executed 200.");
        if (second.StatusCode != HttpStatusCode.OK || !IsReplayed(second))
            throw new InvalidOperationException($"The second keyed request returned {second.StatusCode} without a replay.");
        if (plain.StatusCode != HttpStatusCode.OK || plain.Headers.Contains(ReplayedHeader))
            throw new InvalidOperationException($"The plain request returned {plain.StatusCode} instead of an executed 200.");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync().ConfigureAwait(false);
        await _app.DisposeAsync().ConfigureAwait(false);
    }

    private static async Task<IResult> AcceptAsync(HttpRequest request)
    {
        await request.Body.CopyToAsync(Stream.Null, request.HttpContext.RequestAborted).ConfigureAwait(false);
        return TypedResults.Text("accepted");
    }

    private static bool IsReplayed(HttpResponseMessage response) =>
        response.Headers.TryGetValues(ReplayedHeader, out IEnumerable<string>? values)
        && values.Contains("true", StringComparer.OrdinalIgnoreCase);

    private async Task<HttpResponseMessage> SendCoreAsync(string path, byte[] body, string? key)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(path, UriKind.Relative));
        request.Content = new ByteArrayContent(body);
        if (key is not null)
            request.Headers.Add(KeyHeader, key);
        HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false);
        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        return response;
    }
}
