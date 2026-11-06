using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public sealed class OpenApiEnumUsageTests
{
    [Fact]
    public async Task Property_Should_UseTheWireNameAsDefault_When_TheDefaultValueIsANumber()
    {
        JsonNode properties = await SequencePropertiesAsync();

        properties["fromNumber"]!["default"]!.GetValue<string>().Should().Be("pending_approval");
    }

    [Fact]
    public async Task Property_Should_BeAnArrayOfTheEnum_When_ItIsAnIEnumerable()
    {
        JsonNode properties = await SequencePropertiesAsync();

        JsonNode sequence = properties["sequence"]!;
        sequence["type"]!.GetValue<string>().Should().Be("array");
        sequence["items"]!["$ref"]!.GetValue<string>().Should().Be("#/components/schemas/SampleStatus");
    }

    [Fact]
    public async Task Property_Should_InlineTheEnum_When_TheDocumentGivesTheEnumNoComponent()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_0,
            configureServices: static services => services.Configure<OpenApiOptions>("v2", static options =>
            {
                Func<JsonTypeInfo, string?> createId = options.CreateSchemaReferenceId;
                options.CreateSchemaReferenceId = typeInfo => typeInfo.Type == typeof(SampleStatus) ? null : createId(typeInfo);
            }));

        JsonNode schemas = JsonNode.Parse(documents["v2"])!["components"]!["schemas"]!;
        documents["v2"].Should().NotContain("#/components/schemas/SampleStatus");
        schemas["SampleOrder"]!["properties"]!["status"]!["enum"]!.AsArray().Select(static value => value!.GetValue<string>())
            .Should().Equal("pending", "pending_approval", "waiting", "unknown");
    }

    [Fact]
    public async Task Operation_Should_BeMarkedAsNumber_When_ItsControllerSelectsNumbers()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_0,
            configureServices: static services => services.AddControllers().ConfigureApplicationPartManager(static manager =>
                manager.FeatureProviders.Add(new NumberControllerFeatureProvider())));

        JsonNode paths = JsonNode.Parse(documents["v2"])!["paths"]!;

        paths["/v2/number-controller/{status}"]!["get"]!["x-enum-wire-format"]!.GetValue<string>().Should().Be("number");
        paths["/v2/orders/{status}"]!["get"]!["x-enum-wire-format"].Should().BeNull();
    }

    [Fact]
    public async Task Document_Should_NotWarn_When_ItsNumberOperationsShareNoEnumWithItsStringOperations()
    {
        using var logs = new CapturingLoggerProvider();

        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_0,
            configureServices: services => services.AddSingleton<ILoggerProvider>(logs),
            configureApp: static app => app.MapGet("/v2/plain-number", static () => SamplePlain.Second).WithGroupName("v2")
                .WithEnumWireFormat(EnumWireFormat.Number));

        JsonNode.Parse(documents["v2"])!["paths"]!["/v2/plain-number"]!["get"]!["x-enum-wire-format"]!.GetValue<string>().Should().Be("number");
        logs.Messages.Should().Contain(static message => message.StartsWith("OpenAPI document 'mixed' ", StringComparison.Ordinal));
        logs.Messages.Should().NotContain(static message => message.StartsWith("OpenAPI document 'v2' ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Component_Should_UseTheStringEnumDefaults_When_OnlyTheDocumentAddsEnumConventions()
    {
        IReadOnlyDictionary<string, string> documents = await OptionalRouteParameterOpenApiHost.GetJsonDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_0,
            [OptionalRouteParameterOpenApiHost.DocumentName],
            static options => options.AddEnumConventions(),
            controllers: [],
            configureApp: static app => app.MapGet("/status", static () => SampleStatus.PendingApproval));

        JsonNode status = JsonNode.Parse(documents[OptionalRouteParameterOpenApiHost.DocumentName])!["components"]!["schemas"]!["SampleStatus"]!;
        status["type"]!.GetValue<string>().Should().Be("string");
        status["enum"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("pending", "pending_approval", "waiting", "unknown");
    }

    private static async Task<JsonNode> SequencePropertiesAsync()
    {
        IReadOnlyDictionary<string, string> documents = await OpenApiSampleHost.GetDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_0,
            configureApp: static app => app.MapPost("/v2/sequence", static (EnumSequence body) => body).WithGroupName("v2"));

        return JsonNode.Parse(documents["v2"])!["components"]!["schemas"]!.AsObject()
            .Select(static pair => pair.Value!["properties"])
            .Single(static properties => properties?["sequence"] is not null)!;
    }

    internal sealed class EnumSequence
    {
        [DefaultValue(1)]
        public SampleStatus FromNumber { get; init; } = SampleStatus.PendingApproval;

        public IEnumerable<SampleStatus> Sequence { get; init; } = [];
    }

    [ApiController]
    [EnumWireFormat(EnumWireFormat.Number)]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("v2/number-controller")]
    internal sealed class NumberController : ControllerBase
    {
        [HttpGet("{status}")]
        public SampleStatus Get(SampleStatus status) => status;
    }

    private sealed class NumberControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo controller = typeof(NumberController).GetTypeInfo();
            if (!feature.Controllers.Contains(controller))
                feature.Controllers.Add(controller);
        }
    }
}
