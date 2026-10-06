using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>Hosts the sample API with Microsoft.AspNetCore.OpenApi and returns the documents as JSON.</summary>
internal static class OpenApiSampleHost
{
    public static async Task<IReadOnlyDictionary<string, string>> GetDocumentsAsync(
        OpenApiSpecVersion version,
        bool addEnumConventions = true,
        Action<IServiceCollection>? configureServices = null,
        int passes = 1,
        Action<WebApplication>? configureApp = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddEnumConventions();
        builder.Services.AddControllers().AddSampleControllers();
        foreach (string document in SampleApi.Documents)
        {
            builder.Services.AddOpenApi(document, options =>
            {
                options.OpenApiVersion = version;
                if (addEnumConventions)
                    options.AddEnumConventions();
            });
        }

        configureServices?.Invoke(builder.Services);

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapSampleApi();
        app.MapControllers();
        configureApp?.Invoke(app);
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int pass = 0; pass < passes; pass++)
            {
                foreach (string document in SampleApi.Documents)
                    documents[document] = await client.GetStringAsync($"/openapi/{document}.json");
            }
            return documents;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
