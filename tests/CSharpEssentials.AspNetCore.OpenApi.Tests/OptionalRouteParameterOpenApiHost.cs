using System.Reflection;
using CSharpEssentials.Tests.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// Hosts <see cref="OptionalRouteParameterControllers"/> and three minimal API endpoints (<c>/minimal/{id:int?}</c>,
/// <c>/minimal-default/{page=1}</c>, <c>/minimal-all/{*rest}</c>) with Microsoft.AspNetCore.OpenApi. Without
/// <c>configure</c>, the document uses <see cref="OpenApiOptionalRouteParameterExtensions.AddOptionalRouteParameters"/>.
/// </summary>
internal static class OptionalRouteParameterOpenApiHost
{
    public const string DocumentName = "v1";

    /// <summary>Generates the document <paramref name="passes"/> times and returns the last one.</summary>
    public static async Task<OpenApiDocument> GetDocumentAsync(
        Action<OpenApiOptions>? configure = null, IReadOnlyList<Type>? controllers = null, ILoggerProvider? loggerProvider = null, int passes = 1)
    {
        await using WebApplication app = await StartAsync(OpenApiSpecVersion.OpenApi3_1, configure, controllers, loggerProvider);
        try
        {
            IOpenApiDocumentProvider provider = app.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>(DocumentName);
            OpenApiDocument document = await provider.GetOpenApiDocumentAsync();
            for (int pass = 1; pass < passes; pass++)
                document = await provider.GetOpenApiDocumentAsync();
            return document;
        }
        finally
        {
            await app.StopAsync();
        }
    }

    /// <summary>The document as <c>MapOpenApi</c> serves it.</summary>
    public static async Task<string> GetJsonAsync(OpenApiSpecVersion version, Action<OpenApiOptions>? configure = null) =>
        (await GetJsonDocumentsAsync(version, [DocumentName], configure))[DocumentName];

    /// <summary>The documents named <paramref name="documentNames"/> as <c>MapOpenApi</c> serves them.</summary>
    public static async Task<IReadOnlyDictionary<string, string>> GetJsonDocumentsAsync(
        OpenApiSpecVersion version,
        IReadOnlyList<string> documentNames,
        Action<OpenApiOptions>? configure = null,
        IReadOnlyList<Type>? controllers = null,
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configureApp = null)
    {
        await using WebApplication app = await StartAsync(version, configure, controllers, null, documentNames, configureServices, configureApp);
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string documentName in documentNames)
                documents[documentName] = await client.GetStringAsync($"/openapi/{documentName}.json");
            return documents;
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private static async Task<WebApplication> StartAsync(
        OpenApiSpecVersion version,
        Action<OpenApiOptions>? configure,
        IReadOnlyList<Type>? controllers,
        ILoggerProvider? loggerProvider,
        IReadOnlyList<string>? documentNames = null,
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configureApp = null)
    {
        // The application name is the default tag of Minimal API operations; the test runner's would change with the runner.
        WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(OptionalRouteParameterOpenApiHost).Assembly.GetName().Name,
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        if (loggerProvider is not null)
            builder.Logging.AddProvider(loggerProvider);

        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new ControllersFeatureProvider(controllers ?? OptionalRouteParameterControllers.Compliant));
            });
        foreach (string documentName in documentNames ?? [DocumentName])
        {
            builder.Services.AddOpenApi(documentName, options =>
            {
                options.OpenApiVersion = version;
                (configure ?? (static o => o.AddOptionalRouteParameters())).Invoke(options);
            });
        }
        configureServices?.Invoke(builder.Services);

        WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapControllers();
        app.MapGet("/minimal/{id:int?}", static (int? id) => id).WithName("GetMinimal");
        app.MapGet("/minimal-default/{page=1}", static (int page) => page).WithName("GetMinimalDefault");
        app.MapGet("/minimal-all/{*rest}", static (string? rest) => rest).WithName("GetMinimalRest");
        configureApp?.Invoke(app);
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
