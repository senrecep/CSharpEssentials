using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Endpoints;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed class SwashbuckleEnumUsageTests
{
    [Fact]
    public async Task Property_Should_UseTheWireNameAsDefault_When_TheDefaultValueIsANumber()
    {
        JsonNode properties = await DefaultsPropertiesAsync();

        Default(properties["fromNumber"]!).Should().Be("pending_approval");
    }

    [Fact]
    public async Task Property_Should_HaveNoDefault_When_TheDefaultValueIsTextThatNamesNoMember()
    {
        JsonNode properties = await DefaultsPropertiesAsync();

        properties["fromText"]!["default"].Should().BeNull();
        properties["fromText"]!["$ref"]!.GetValue<string>().Should().EndWith(nameof(SampleStatus));
    }

    [Fact]
    public async Task Property_Should_BeAnArrayOfTheEnum_When_ItIsAnIEnumerable()
    {
        JsonNode properties = await DefaultsPropertiesAsync();

        JsonNode sequence = properties["sequence"]!;
        sequence["type"]!.GetValue<string>().Should().Be("array");
        sequence["items"]!["$ref"]!.GetValue<string>().Should().EndWith(nameof(SampleStatus));
    }

    [Fact]
    public async Task Operation_Should_BeMarkedAsNumber_When_ItsControllerSelectsNumbers()
    {
        IReadOnlyDictionary<string, string> documents = await SwashbuckleEnumGoldenTests.GetDocumentsAsync(
            addEnumConventions: true,
            configureServices: static services => services.AddControllers().ConfigureApplicationPartManager(static manager =>
                manager.FeatureProviders.Add(new NumberControllerFeatureProvider())));

        JsonNode operation = JsonNode.Parse(documents["v2"])!["paths"]!["/v2/number-controller/{status}"]!["get"]!;

        operation["x-enum-wire-format"]!.GetValue<string>().Should().Be("number");
        JsonNode.Parse(documents["v2"])!["paths"]!["/v2/orders/{status}"]!["get"]!["x-enum-wire-format"].Should().BeNull();
    }

    [Fact]
    public async Task Document_Should_NotWarn_When_ItsNumberOperationsShareNoEnumWithItsStringOperations()
    {
        using var logs = new CapturingLoggerProvider();

        IReadOnlyDictionary<string, string> documents = await SwashbuckleEnumGoldenTests.GetDocumentsAsync(
            addEnumConventions: true,
            logs,
            configureApp: static app => app.MapGet("/v2/plain-number", static () => SamplePlain.Second).WithGroupName("v2")
                .WithEnumWireFormat(EnumWireFormat.Number));

        JsonNode.Parse(documents["v2"])!["paths"]!["/v2/plain-number"]!["get"]!["x-enum-wire-format"]!.GetValue<string>().Should().Be("number");
        logs.Entries.Should().Contain(static entry => entry.Message.StartsWith("OpenAPI document 'mixed' ", StringComparison.Ordinal));
        logs.Entries.Should().NotContain(static entry => entry.Message.StartsWith("OpenAPI document 'v2' ", StringComparison.Ordinal));
    }

    private static async Task<JsonNode> DefaultsPropertiesAsync()
    {
        IReadOnlyDictionary<string, string> documents = await SwashbuckleEnumGoldenTests.GetDocumentsAsync(
            addEnumConventions: true,
            configureApp: static app => app.MapPost("/v2/defaults", static (EnumDefaults body) => body).WithGroupName("v2"));

        return JsonNode.Parse(documents["v2"])!["components"]!["schemas"]!.AsObject()
            .Select(static pair => pair.Value!["properties"])
            .Single(static properties => properties?["fromNumber"] is not null)!;
    }

    private static string? Default(JsonNode property) => property["default"]?.GetValue<string>();

    internal sealed class EnumDefaults
    {
        [DefaultValue(1)]
        public SampleStatus FromNumber { get; init; } = SampleStatus.PendingApproval;

        [DefaultValue("not-a-member")]
        public SampleStatus FromText { get; init; }

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
