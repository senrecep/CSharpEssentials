using System.Reflection;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static CSharpEssentials.Tests.AspNetCore.EnumIntegration.EnumConventionsControllers;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>
/// A TestServer host with Minimal API endpoints and MVC controllers side by side, registered with
/// <c>AddEnhancedProblemDetails()</c>, <c>AddEnumConventions()</c> and <c>UseEnumBinding()</c>.
/// </summary>
internal sealed class EnumConventionsHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private EnumConventionsHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => _app.Services;

    /// <param name="register">Registers the enum conventions; <see langword="null"/> calls <c>AddEnumConventions()</c>.</param>
    /// <param name="map">Maps Minimal API endpoints.</param>
    /// <param name="mapControllers">Applies conventions to <c>MapControllers()</c>, for example a wire format.</param>
    /// <param name="controllers">The controllers of the host.</param>
    /// <param name="useEnumBinding">Whether <c>UseEnumBinding()</c> runs.</param>
    public static async Task<EnumConventionsHost> StartAsync(
        Action<IServiceCollection>? register = null,
        Action<WebApplication>? map = null,
        Action<ControllerActionEndpointConventionBuilder>? mapControllers = null,
        Type[]? controllers = null,
        bool useEnumBinding = true)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddEnhancedProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
        if (register is null)
            builder.Services.AddEnumConventions();
        else
            register(builder.Services);

        Type[] controllerTypes = controllers ?? [];
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new ExplicitControllerFeatureProvider(controllerTypes));
            });

        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        if (useEnumBinding)
            app.UseEnumBinding();

        map?.Invoke(app);
        ControllerActionEndpointConventionBuilder controllerEndpoints = app.MapControllers();
        mapControllers?.Invoke(controllerEndpoints);

        await app.StartAsync();
        return new EnumConventionsHost(app);
    }

    /// <summary>The binding matrix host: the Minimal API routes under <c>/min</c>, <see cref="EcMatrixController"/> under <c>/mvc</c>.</summary>
    public static Task<EnumConventionsHost> StartMatrixAsync(Action<IServiceCollection>? register = null, bool useEnumBinding = true) =>
        StartAsync(register, MapMatrix, controllers: [typeof(EcMatrixController)], useEnumBinding: useEnumBinding);

    public static Func<EnumConventions, EnumConventions> WriteAs(EnumWireFormat format) => c => c with { WriteAs = format };

    private static void MapMatrix(WebApplication app)
    {
        RouteGroupBuilder min = app.MapGroup("/min");
        min.MapGet("/route/{status}", (EcStatus status) => EcEcho.Of<EcStatus>(status));
        min.MapGet("/route-nullable/{status?}", (EcStatus? status) => EcEcho.Of(status));
        min.MapGet("/route-flags/{perms}", (EcPermission perms) => EcEcho.Of<EcPermission>(perms));
        min.MapGet("/query", ([FromQuery] EcStatus status) => EcEcho.Of<EcStatus>(status));
        min.MapGet("/query-nullable", ([FromQuery] EcStatus? status) => EcEcho.Of(status));
        min.MapGet("/query-array", ([FromQuery] EcStatus[] status) => EcEcho.Of<EcStatus>(status));
        min.MapGet("/query-flags", ([FromQuery] EcPermission perms) => EcEcho.Of<EcPermission>(perms));
        min.MapGet("/header", ([FromHeader(Name = "X-Status")] EcStatus status) => EcEcho.Of<EcStatus>(status));
        min.MapGet("/header-nullable", ([FromHeader(Name = "X-Status")] EcStatus? status) => EcEcho.Of(status));
        min.MapGet("/header-array", ([FromHeader(Name = "X-Status")] EcStatus[] status) => EcEcho.Of<EcStatus>(status));
        min.MapGet("/header-flags", ([FromHeader(Name = "X-Perms")] EcPermission perms) => EcEcho.Of<EcPermission>(perms));
        min.MapPost("/form", ([FromForm] EcStatus status) => EcEcho.Of<EcStatus>(status)).DisableAntiforgery();
        min.MapPost("/form-nullable", ([FromForm] EcStatus? status) => EcEcho.Of(status)).DisableAntiforgery();
        min.MapPost("/form-array", ([FromForm] EcStatus[] status) => EcEcho.Of<EcStatus>(status)).DisableAntiforgery();
        min.MapPost("/form-flags", ([FromForm] EcPermission perms) => EcEcho.Of<EcPermission>(perms)).DisableAntiforgery();
        min.MapGet("/plain", (EcPlain plain) => plain.ToString());
        min.MapGet("/plain-order", () => new EcPlainOrder(1, EcPlain.InProgress));
        min.MapPost("/body", (EcStatusBody body) => EcEcho.Of<EcStatus>(body.Status));
    }

    public async Task<EcResponse> GetAsync(string url, params (string Name, string Value)[] headers)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        foreach ((string name, string value) in headers)
            request.Headers.TryAddWithoutValidation(name, value);
        return await SendAsync(request);
    }

    public async Task<EcResponse> PostFormAsync(string url, params (string Name, string Value)[] fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))),
        };
        return await SendAsync(request);
    }

    public async Task<EcResponse> PostJsonAsync(string url, string json)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };
        return await SendAsync(request);
    }

    private async Task<EcResponse> SendAsync(HttpRequestMessage request)
    {
        using HttpResponseMessage response = await Client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        return new EcResponse(
            (int)response.StatusCode,
            body,
            response.Content.Headers.ContentType?.MediaType,
            [.. response.Headers.Vary],
            response.Headers.Location?.OriginalString);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }

    private sealed class ExplicitControllerFeatureProvider(Type[] controllers) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (Type controller in controllers)
            {
                TypeInfo info = controller.GetTypeInfo();
                if (!feature.Controllers.Contains(info))
                    feature.Controllers.Add(info);
            }
        }
    }
}
