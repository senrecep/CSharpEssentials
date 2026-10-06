using System.Reflection;
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

public enum EbHostKind
{
    MinimalApi,
    MvcApiController,
    MvcController,
}

/// <summary>
/// A TestServer host exposing the same echo routes as Minimal API endpoints or as one MVC controller.
/// </summary>
internal sealed class EnumBindingHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private EnumBindingHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public static async Task<EnumBindingHost> StartAsync(
        EbHostKind kind,
        bool useEnumBinding = true,
        Action<EnumConventionsBuilder>? configure = null,
        Func<EnumConventions, EnumConventions>? conventions = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        if (useEnumBinding)
        {
            builder.Services.AddEnhancedProblemDetails();
            EnumConventionsBuilder enumConventions = builder.Services.AddEnumConventions(conventions);
            configure?.Invoke(enumConventions);
        }

        if (kind != EbHostKind.MinimalApi)
        {
            Type controller = kind == EbHostKind.MvcApiController
                ? typeof(EnumBindingControllers.EbApiController)
                : typeof(EnumBindingControllers.EbMvcController);
            builder.Services.AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    foreach (IApplicationFeatureProvider provider in manager.FeatureProviders.OfType<ControllerFeatureProvider>().ToList())
                        manager.FeatureProviders.Remove(provider);
                    manager.FeatureProviders.Add(new SingleControllerFeatureProvider(controller));
                });
        }

        WebApplication app = builder.Build();
        if (useEnumBinding)
            app.UseEnumBinding();

        if (kind == EbHostKind.MinimalApi)
            MapMinimalApi(app);
        else
            app.MapControllers();

        await app.StartAsync();
        return new EnumBindingHost(app);
    }

    private static void MapMinimalApi(WebApplication app)
    {
        app.MapGet("/status", (EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/items/{status}", (EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/nullable", (EbStatus? status) => EbEcho.Of(status));
        app.MapGet("/array", (EbStatus[] s) => EbEcho.Of<EbStatus>(s));
        app.MapGet("/flags", (EbPermission perms) => EbEcho.Of<EbPermission>(perms));
        app.MapGet("/renamed", ([FromQuery(Name = "st")] EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/custom", (EbCustom value) => EbEcho.Of<EbCustom>(value));
        app.MapGet("/plain", (EbPlain plain) => plain.ToString());
        app.MapGet("/multi", (EbStatus a, EbPermission b) => $"{a}|{b}");
        app.MapGet("/none", (string? x) => $"ok:{x}");
        app.MapGet("/asparams-class", ([AsParameters] EbAsParametersClass p) => $"{p.Status}|{p.Perm}");
        app.MapGet("/asparams-record", ([AsParameters] EbAsParametersRecord p) => $"{p.Status}|{EbEcho.Of(p.Other)}");
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }

    private sealed class SingleControllerFeatureProvider(Type controller) : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo info = controller.GetTypeInfo();
            if (!feature.Controllers.Contains(info))
                feature.Controllers.Add(info);
        }
    }
}

internal static class EnumBindingHttp
{
    public static async Task<(int Status, string Body, string? ContentType)> GetAsync(this EnumBindingHost host, string url)
    {
        using HttpResponseMessage response = await host.Client.GetAsync(url);
        string body = await response.Content.ReadAsStringAsync();
        return ((int)response.StatusCode, body, response.Content.Headers.ContentType?.MediaType);
    }
}

internal static class EnumBindingAssertions
{
    public static IReadOnlyList<(string Code, string Description)> ShouldBeEnumBindingProblem(this (int Status, string Body, string? ContentType) response)
    {
        response.Status.Should().Be(400, response.Body);
        response.ContentType.Should().Be("application/problem+json");
        using JsonDocument document = JsonDocument.Parse(response.Body);
        return [.. document.RootElement.GetProperty("errors").EnumerateArray()
            .Select(e => (e.GetProperty("code").GetString()!, e.GetProperty("description").GetString()!))];
    }

    public static TheoryData<EbHostKind, string, string> Cross(EbHostKind[] kinds, params (string Input, string Expected)[] cases)
    {
        var data = new TheoryData<EbHostKind, string, string>();
        foreach (EbHostKind kind in kinds)
            foreach ((string input, string expected) in cases)
                data.Add(kind, input, expected);
        return data;
    }

    public static TheoryData<EbHostKind, string> Cross(EbHostKind[] kinds, params string[] inputs)
    {
        var data = new TheoryData<EbHostKind, string>();
        foreach (EbHostKind kind in kinds)
            foreach (string input in inputs)
                data.Add(kind, input);
        return data;
    }

    public static readonly EbHostKind[] AllHosts = [EbHostKind.MinimalApi, EbHostKind.MvcApiController, EbHostKind.MvcController];

    public static readonly EbHostKind[] MvcHosts = [EbHostKind.MvcApiController, EbHostKind.MvcController];

    public static TheoryData<EbHostKind> Hosts(params EbHostKind[] kinds)
    {
        var data = new TheoryData<EbHostKind>();
        foreach (EbHostKind kind in kinds)
            data.Add(kind);
        return data;
    }
}
