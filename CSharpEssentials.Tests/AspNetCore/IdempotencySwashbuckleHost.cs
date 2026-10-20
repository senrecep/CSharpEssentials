using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>Builds the Swashbuckle document of <see cref="IdempotencyApi"/>.</summary>
internal static class IdempotencySwashbuckleHost
{
    public static async Task<string> GetJsonAsync(
        OpenApiSpecVersion version, Action<SwaggerGenOptions>? configure = null, Action<IdempotencyOptions>? configureOptions = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddIdempotency(options =>
        {
            IdempotencyApi.ConfigureOptions(options);
            configureOptions?.Invoke(options);
        });
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(IdempotencySwashbuckleHost).Assembly);
        builder.Services.AddControllers().AddIdempotencyControllers();
        builder.Services.Configure<SwaggerGenOptions>(options => options.SwaggerDoc(IdempotencyApi.Document, new OpenApiInfo { Title = "Idempotency", Version = "1" }));
        if (configure is not null)
            builder.Services.PostConfigure(configure);

        await using WebApplication app = builder.Build();
        app.MapControllers();
        app.MapIdempotencyApi();
        await app.StartAsync();
        try
        {
            OpenApiDocument document = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(IdempotencyApi.Document);
            return await document.SerializeAsJsonAsync(version);
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
