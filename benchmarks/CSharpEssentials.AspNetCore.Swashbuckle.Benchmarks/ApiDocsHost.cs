using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

/// <summary>
/// One in-process Production host per row over TestServer, with logging providers cleared. Every document request reads
/// the whole body, so the measured time covers generating and writing the full document.
/// </summary>
public sealed class ApiDocsHost : IAsyncDisposable
{
    public const string DocumentPath = "/swagger/v1/swagger.json";

    private readonly WebApplication _app;
    private readonly HttpClient _client;

    private ApiDocsHost(WebApplication app)
    {
        _app = app;
        _client = app.GetTestClient();
    }

    /// <summary>Builds and starts a host.</summary>
    /// <param name="services">Registers the row's services.</param>
    /// <param name="app">Maps the row's endpoints.</param>
    /// <returns>The started host.</returns>
    public static ApiDocsHost Start(Action<WebApplicationBuilder> services, Action<WebApplication> app)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        services(builder);

        WebApplication web = builder.Build();
        app(web);
        web.StartAsync().GetAwaiter().GetResult();
        return new ApiDocsHost(web);
    }

    /// <summary>Builds a host, gets its first document, stops it and returns the document length.</summary>
    /// <param name="start">Builds and starts the row's host.</param>
    /// <returns>The document length in bytes.</returns>
    public static async Task<int> FirstDocumentAsync(Func<ApiDocsHost> start)
    {
        ApiDocsHost host = start();
        try
        {
            return await host.GetDocumentAsync().ConfigureAwait(false);
        }
        finally
        {
            await host.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Gets the document, reads the whole body and returns its length.</summary>
    /// <returns>The document length in bytes.</returns>
    public async Task<int> GetDocumentAsync()
    {
        byte[] body = await ReadDocumentAsync().ConfigureAwait(false);
        return body.Length;
    }

    /// <summary>Gets the document for the parity checks.</summary>
    /// <returns>The document body.</returns>
    public byte[] ProbeDocument() => ReadDocumentAsync().GetAwaiter().GetResult();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync().ConfigureAwait(false);
        await _app.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Synchronous <see cref="DisposeAsync"/>, for [GlobalCleanup].</summary>
    public void Stop() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    private async Task<byte[]> ReadDocumentAsync()
    {
        using HttpResponseMessage response = await _client.GetAsync(new Uri(DocumentPath, UriKind.Relative)).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }
}
