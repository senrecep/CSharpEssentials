using System.Reflection;
using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// Builds the Swashbuckle document of <see cref="OptionalRouteParameterControllers"/> and three minimal API endpoints
/// (<c>/minimal/{id:int?}</c>, <c>/minimal-default/{page=1}</c>, <c>/minimal-all/{*rest}</c>).
/// </summary>
internal static class OptionalRouteParameterHost
{
    public static async Task<OpenApiDocument> GetDocumentAsync(
        Action<SwaggerGenOptions>? configure = null, IReadOnlyList<Type>? controllers = null, ILoggerProvider? loggerProvider = null)
    {
        await using WebApplication app = await StartAsync(configure, controllers ?? OptionalRouteParameterControllers.Compliant, loggerProvider);
        return app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
    }

    public static async Task<string> GetJsonAsync(OpenApiSpecVersion version, Action<SwaggerGenOptions>? configure = null, IReadOnlyList<Type>? controllers = null)
    {
        OpenApiDocument document = await GetDocumentAsync(configure, controllers);
        return await document.SerializeAsJsonAsync(version);
    }

    public static async Task<WebApplication> StartAsync(Action<SwaggerGenOptions>? configure, IReadOnlyList<Type> controllers, ILoggerProvider? loggerProvider = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        if (loggerProvider is not null)
            builder.Logging.AddProvider(loggerProvider);
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(OptionalRouteParameterHost).Assembly);
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new ControllersFeatureProvider(controllers));
            });
        if (configure is not null)
            builder.Services.Configure(configure);

        WebApplication app = builder.Build();
        app.MapControllers();
        app.MapGet("/minimal/{id:int?}", static (int? id) => id).WithName("GetMinimal");
        app.MapGet("/minimal-default/{page=1}", static (int page) => page).WithName("GetMinimalDefault");
        app.MapGet("/minimal-all/{*rest}", static (string? rest) => rest).WithName("GetMinimalRest");
        await app.StartAsync();
        return app;
    }

    private sealed class ControllersFeatureProvider(IReadOnlyList<Type> controllers) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (TypeInfo info in controllers.Select(static controller => controller.GetTypeInfo()).Where(info => !feature.Controllers.Contains(info)))
                feature.Controllers.Add(info);
        }
    }
}
