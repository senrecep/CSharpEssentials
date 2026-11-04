using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;
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

/// <summary>
/// A TestServer host with Minimal API endpoints and one MVC controller whose enum values come from explicit
/// route, header and body binding attributes.
/// </summary>
internal sealed class EnumBindingSourceHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private EnumBindingSourceHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public static async Task<EnumBindingSourceHost> StartAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails();
        builder.Services.AddEnumConventions();
        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                     manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                manager.FeatureProviders.Remove(provider);
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(EbSourceController)));
        });

        WebApplication app = builder.Build();
        app.UseEnumBinding();
        app.MapGet("/minimal/route/{status}", ([FromRoute] EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/minimal/renamed-route/{st}", ([FromRoute(Name = "st")] EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/minimal/header", ([FromHeader(Name = "X-Status")] EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapGet("/minimal/header-default", ([FromHeader] EbStatus? status) => EbEcho.Of(status));
        app.MapPost("/minimal/body", ([FromBody] EbStatus status) => EbEcho.Of<EbStatus>(status));
        app.MapControllers();

        await app.StartAsync();
        return new EnumBindingSourceHost(app);
    }

    public async Task<(int Status, string Body, string? ContentType)> SendAsync(HttpRequestMessage request)
    {
        using HttpResponseMessage response = await Client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();
        return ((int)response.StatusCode, body, response.Content.Headers.ContentType?.MediaType);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}

/// <summary>Enum properties bound from explicit sources inside a query model.</summary>
internal sealed class EbSourceDto
{
    [FromQuery(Name = "st")]
    public EbStatus Renamed { get; set; }

    [FromHeader(Name = "X-Status")]
    public EbStatus Header { get; set; }

    [FromRoute]
    public EbStatus Kind { get; set; }

    public EbStatus ReadOnly { get; } = EbStatus.HTTPError;
}

[Route("mvc")]
internal sealed class EbSourceController : ControllerBase
{
    [HttpGet("dto/{kind}")]
    public string Dto([FromQuery] EbSourceDto dto) =>
        ModelState.IsValid ? $"{dto.Renamed}|{dto.Header}|{dto.Kind}|{dto.ReadOnly}" : "invalid";

    [HttpPost("body")]
    public string Body([FromBody] EbStatus status) => ModelState.IsValid ? EbEcho.Of<EbStatus>(status) : "invalid";
}
