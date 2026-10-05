using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Tests.AspNetCore.EnumBinding;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// <c>ConfigureInvalidModelStateResponse()</c>: the automatic 400 of <c>[ApiController]</c> becomes an enhanced problem.
/// </summary>
public class InvalidModelStateResponseTests
{
    [Fact]
    public async Task InvalidQueryValue_Should_WriteEnhancedProblem_When_ExtensionIsConfigured()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s.ConfigureInvalidModelStateResponse());

        (int status, string? mediaType, JsonObject json) = await host.GetAsync("/plain?plain=7x");

        status.Should().Be(400);
        mediaType.Should().Be("application/problem+json");
        json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Contain("plain");
        json["errors"]!.AsArray().Select(n => n!["code"]!.GetValue<string>()).Should().Contain("plain");
    }

    [Fact]
    public async Task InvalidQueryValue_Should_UseModelErrorMessageAsDescription_When_ExtensionIsConfigured()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s.ConfigureInvalidModelStateResponse());

        (_, _, JsonObject json) = await host.GetAsync("/plain?plain=7x");

        json["errors"]!.AsArray().Single()!["description"]!.GetValue<string>().Should().Contain("7x");
    }

    [Fact]
    public async Task MissingRequiredField_Should_WriteFieldNameAsErrorCode_When_ExtensionIsConfigured()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s.ConfigureInvalidModelStateResponse());

        (int status, string? mediaType, JsonObject json) = await host.PostAsync("/body", "{}");

        status.Should().Be(400);
        mediaType.Should().Be("application/problem+json");
        json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Contain("Name");
    }

    [Fact]
    public async Task MalformedJsonBody_Should_WriteEnhancedProblem_When_ExtensionIsConfigured()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s.ConfigureInvalidModelStateResponse());

        (int status, string? mediaType, JsonObject json) = await host.PostAsync("/body", "{ not json");

        status.Should().Be(400);
        mediaType.Should().Be("application/problem+json");
        json["errors"]!.AsArray().Should().NotBeEmpty();
        json["errorCodes"]!.AsArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvalidModelState_Should_HonorErrorFields_When_OnlyCodesAreEnabled()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s
            .AddEnhancedProblemDetails(o => o.ErrorFields = ProblemErrorFields.Codes)
            .ConfigureInvalidModelStateResponse());

        (_, _, JsonObject json) = await host.GetAsync("/plain?plain=7x");

        json.ContainsKey("errors").Should().BeFalse();
        json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Contain("plain");
    }

    [Fact]
    public async Task InvalidModelState_Should_UseCustomErrorFactory_When_Provided()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(s => s.ConfigureInvalidModelStateResponse(
            (key, error) => Error.Validation($"validation.{key}", $"custom: {error.ErrorMessage.Length > 0}")));

        (_, _, JsonObject json) = await host.GetAsync("/plain?plain=7x");

        json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("validation.plain");
        json["errors"]!.AsArray().Single()!["description"]!.GetValue<string>().Should().Be("custom: True");
    }

    [Fact]
    public async Task InvalidModelState_Should_ReturnFrameworkValidationProblem_When_ExtensionIsNotConfigured()
    {
        await using ModelStateHost host = await ModelStateHost.StartAsync(_ => { });

        (int status, _, JsonObject json) = await host.GetAsync("/plain?plain=7x");

        status.Should().Be(400);
        json["errors"]!.Should().BeAssignableTo<JsonObject>();
        json.ContainsKey("errorCodes").Should().BeFalse();
    }

    public sealed class MsBody
    {
        [Required]
        public string? Name { get; set; }
    }

    [ApiController]
    [Route("")]
    public sealed class MsApiController : ControllerBase
    {
        [HttpGet("plain")]
        public string Plain(EbPlain plain) => plain.ToString();

        [HttpPost("body")]
        public string Body(MsBody body) => body.Name ?? string.Empty;
    }

    private sealed class ModelStateHost : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private ModelStateHost(WebApplication app)
        {
            _app = app;
            Client = app.GetTestClient();
        }

        private HttpClient Client { get; }

        public static async Task<ModelStateHost> StartAsync(Action<IServiceCollection> configureServices)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Logging.ClearProviders();
            builder.Services.AddEnhancedProblemDetails();
            builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
            {
                foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                         manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                    manager.FeatureProviders.Remove(provider);
                manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(MsApiController)));
            });
            configureServices(builder.Services);

            WebApplication app = builder.Build();
            app.MapControllers();
            await app.StartAsync();
            return new ModelStateHost(app);
        }

        public Task<(int Status, string? MediaType, JsonObject Json)> GetAsync(string url) =>
            SendAsync(new HttpRequestMessage(HttpMethod.Get, url));

        public Task<(int Status, string? MediaType, JsonObject Json)> PostAsync(string url, string json) =>
            SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });

        private async Task<(int, string?, JsonObject)> SendAsync(HttpRequestMessage request)
        {
            using (request)
            using (HttpResponseMessage response = await Client.SendAsync(request))
            {
                string body = await response.Content.ReadAsStringAsync();
                return ((int)response.StatusCode, response.Content.Headers.ContentType?.MediaType, JsonNode.Parse(body)!.AsObject());
            }
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.DisposeAsync();
        }
    }
}
