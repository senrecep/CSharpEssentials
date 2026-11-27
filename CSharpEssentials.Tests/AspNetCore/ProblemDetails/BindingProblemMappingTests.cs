using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.AspNetCore.EnumIntegration;
using FluentAssertions;
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

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// Request body binding failures become field-keyed 400 validation problems without framework text, for Minimal APIs
/// (<see cref="GlobalExceptionHandler"/>) and MVC (<c>ConfigureInvalidModelStateResponse()</c>).
/// </summary>
public class BindingProblemMappingTests
{
    private static readonly string[] LeakedText =
    [
        "System.", "LineNumber", "BytePositionInLine", "Failed to read", "JSON deserialization", "BmOrder", "BmLine",
        "text/plain", "Request body too large", "max request body size", "Expected a supported JSON media type",
    ];

    private const string MvcParameterName = "order";
    private const string Invalid = "The value is invalid.";
    private const string Required = "The field is required.";
    private const string WrongNestedType = """{"customerName":"a","items":[{"quantity":1,"sku":"x"},{"quantity":"many","sku":"y"}]}""";
    private const string MissingNestedRequired = """{"customerName":"a","items":[{"quantity":1,"sku":"x"},{}]}""";

    [Fact]
    public async Task MvcBodyError_Should_KeepParameterRequiredError_NextToFieldErrors()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/mvc/order", WrongNestedType);

        response.Json["errors"]!.AsArray().Select(n => n!["code"]!.GetValue<string>())
            .Should().BeEquivalentTo(MvcParameterName, "items[1].quantity");
    }

    public static TheoryData<string> Paths => new() { "/min", "/mvc" };

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task MalformedJson_Should_Return400ValidationProblem_WithoutFrameworkText(string prefix)
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync($"{prefix}/order", "{ not json");

        response.Status.Should().Be(400);
        response.Errors.Should().Equal(("$", Invalid));
        response.ShouldNotLeak();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task WrongTypeAtNestedPath_Should_KeyErrorByCamelCaseJsonPath(string prefix)
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync($"{prefix}/order", WrongNestedType);

        response.Status.Should().Be(400);
        response.Errors.Should().Equal(("items[1].quantity", Invalid));
        response.ShouldNotLeak();
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task BodyError_Should_BeKeyedByField_When_ByFieldIsConfigured(string prefix)
    {
        await using BindingHost host = await BindingHost.StartAsync(s =>
            s.Configure<EnhancedProblemDetailsOptions>(o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField));

        BindingResponse response = await host.PostAsync($"{prefix}/order", WrongNestedType);

        response.Status.Should().Be(400);
        JsonObject errors = response.Json["errors"]!.AsObject();
        errors["items[1].quantity"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal(Invalid);
    }

    [Theory]
    [MemberData(nameof(Paths))]
    public async Task MissingRequiredProperties_Should_ReturnOneErrorPerMember(string prefix)
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync($"{prefix}/order", MissingNestedRequired);

        response.Status.Should().Be(400);
        response.Errors.Should().Equal(
            ("items[1].quantity", Required),
            ("items[1].sku", Required));
        response.ShouldNotLeak();
    }

    [Fact]
    public async Task MissingRequiredRootProperty_Should_BeKeyedByMemberName()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/min/order", """{"items":[]}""");

        response.Errors.Should().Equal(("customerName", Required));
        response.ShouldNotLeak();
    }

    [Fact]
    public async Task OversizeBody_Should_Return413WithFixedDetail()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/min/too-large", "{}");

        response.Status.Should().Be(413);
        response.Json["detail"]!.GetValue<string>().Should().Be("The request body is too large.");
        response.ShouldNotLeak();
    }

    [Fact]
    public async Task WrongContentType_Should_Return415Problem_WithoutFrameworkText()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/min/order", WrongNestedType, "text/plain");

        response.Status.Should().Be(415);
        response.Json["status"]!.GetValue<int>().Should().Be(415);
        response.Json.ContainsKey("detail").Should().BeFalse();
        response.ShouldNotLeak();
    }

    [Fact]
    public async Task ThrownUnsupportedMediaType_Should_Return415WithFixedDetail()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/min/unsupported", "{}");

        response.Status.Should().Be(415);
        response.Json["detail"]!.GetValue<string>().Should().Be("The request content type is not supported.");
        response.ShouldNotLeak();
    }

    [Fact]
    public async Task EnumBodyError_Should_StillBeMappedByEnumConventions_When_RegisteredAfterProblemDetails()
    {
        await using BindingHost host = await BindingHost.StartAsync(s => s.AddEnumConventions());

        BindingResponse response = await host.PostAsync("/min/status", """{"status":"99"}""");

        response.Status.Should().Be(400);
        response.Errors.Should().Equal(
            ("status", "'99' is not a valid EcStatus. Allowed values: pending, pending_approval, shipped."));
    }

    [Theory]
    [InlineData("/min/bare-json")]
    [InlineData("/min/wrapped-json")]
    public async Task JsonException_Should_Return500WithoutPath_When_NotABodyBindingFailure(string endpoint)
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync(endpoint, "{}");

        response.Status.Should().Be(500);
        response.Body.Should().NotContain("secret").And.NotContain("a.b");
    }

    [Fact]
    public async Task UserMapper_Should_RunBeforeBindingMapper_When_RegisteredAfterProblemDetails()
    {
        await using BindingHost host = await BindingHost.StartAsync(s => s.AddExceptionProblemMapper<TeapotMapper>());

        BindingResponse response = await host.PostAsync("/min/order", WrongNestedType);

        response.Status.Should().Be(418);
    }

    [Fact]
    public async Task OtherBadRequest_Should_Return400WithFixedDetail_When_NoJsonExceptionIsInside()
    {
        await using BindingHost host = await BindingHost.StartAsync();

        BindingResponse response = await host.PostAsync("/min/bad-request", "{}");

        response.Status.Should().Be(400);
        response.Json["detail"]!.GetValue<string>().Should().Be("The request is invalid.");
        response.Body.Should().NotContain("secret");
    }

    private sealed class TeapotMapper : IExceptionProblemMapper
    {
        public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
        {
            problem = exception.InnerException is JsonException ? new ExceptionProblem(StatusCodes.Status418ImATeapot) : null;
            return problem is not null;
        }
    }

    internal sealed record BmLine
    {
        public required int Quantity { get; init; }

        public required string Sku { get; init; }
    }

    internal sealed record BmOrder
    {
        public required string CustomerName { get; init; }

        public List<BmLine> Items { get; init; } = [];
    }

    [ApiController]
    [Route("mvc")]
    internal sealed class BmController : ControllerBase
    {
        [HttpPost("order")]
        public int Order(BmOrder order) => order.Items.Count;
    }

    private sealed record BindingResponse(int Status, string Body, JsonObject Json)
    {
        public (string Code, string Description)[] Errors =>
        [
            .. Json["errors"]!.AsArray()
                .Select(n => (n!["code"]!.GetValue<string>(), n["description"]!.GetValue<string>()))
                .Where(e => e.Item1 != MvcParameterName),
        ];

        public void ShouldNotLeak()
        {
            foreach (string text in LeakedText)
                Body.Should().NotContain(text);
        }
    }

    private sealed class BindingHost : IAsyncDisposable
    {
        private readonly WebApplication _app;
        private readonly HttpClient _client;

        private BindingHost(WebApplication app)
        {
            _app = app;
            _client = app.GetTestClient();
        }

        public static async Task<BindingHost> StartAsync(Action<IServiceCollection>? configure = null)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Logging.ClearProviders();
            builder.Services.AddEnhancedProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
            builder.Services.ConfigureInvalidModelStateResponse();
            configure?.Invoke(builder.Services);
            builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
            {
                foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                         manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(BmController)));
            });

            WebApplication app = builder.Build();
            app.UseEnhancedProblemDetails();
            app.MapPost("/min/order", (BmOrder order) => order.Items.Count);
            app.MapPost("/min/status", (EcStatusBody body) => body.Status.ToString());
            app.MapPost("/min/too-large", int () => throw new BadHttpRequestException(
                "Request body too large. The max request body size is 30000000 bytes.", StatusCodes.Status413PayloadTooLarge));
            app.MapPost("/min/unsupported", int () => throw new BadHttpRequestException(
                "Expected a supported JSON media type but got \"text/plain\".", StatusCodes.Status415UnsupportedMediaType));
            app.MapPost("/min/bare-json", int () => throw new JsonException("secret", "$.a.b", 1, 2));
            app.MapPost("/min/wrapped-json", int () => throw new InvalidOperationException(
                "outer", new JsonException("secret", "$.a.b", 1, 2)));
            app.MapPost("/min/bad-request", int () => throw new BadHttpRequestException("Failed to bind parameter secret"));
            app.MapControllers();
            await app.StartAsync();
            return new BindingHost(app);
        }

        public async Task<BindingResponse> PostAsync(string url, string body, string mediaType = "application/json")
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType),
            };
            using HttpResponseMessage response = await _client.SendAsync(request);
            string text = await response.Content.ReadAsStringAsync();
            return new BindingResponse((int)response.StatusCode, text, JsonNode.Parse(text)!.AsObject());
        }

        public async ValueTask DisposeAsync()
        {
            _client.Dispose();
            await _app.DisposeAsync();
        }
    }
}
