using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>Hosts <see cref="IdempotencyApi"/> with Microsoft.AspNetCore.OpenApi and returns its document as JSON.</summary>
internal static class IdempotencyOpenApiHost
{
    public static async Task<string> GetJsonAsync(
        OpenApiSpecVersion version, bool addConventions = true, Action<IdempotencyOptions>? configureOptions = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddIdempotency(options =>
        {
            IdempotencyApi.ConfigureOptions(options);
            configureOptions?.Invoke(options);
        });
        builder.Services.AddControllers().AddIdempotencyControllers();
        builder.Services.AddOpenApi(IdempotencyApi.Document, options =>
        {
            options.OpenApiVersion = version;
            if (addConventions)
                options.AddIdempotencyConventions();
        });

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapIdempotencyApi();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            return await client.GetStringAsync($"/openapi/{IdempotencyApi.Document}.json");
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
