using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public class SwashbuckleEnumDefaultTests
{
    [Fact]
    public async Task Document_Should_OmitTheDefault_When_DefaultValueIsAnUndefinedEnumValue()
    {
        JsonNode document = await GetDocumentAsync();

        Property(document, nameof(UndefinedDefaultOrder), "status").ToJsonString().Should().NotContain("\"default\"");
    }

    [Fact]
    public async Task Document_Should_OmitTheDefault_When_ANullableEnumPropertyHasAnUndefinedDefault()
    {
        JsonNode document = await GetDocumentAsync();

        Property(document, nameof(UndefinedDefaultOrder), "nullableStatus").ToJsonString().Should().NotContain("\"default\"");
    }

    [Fact]
    public async Task Document_Should_WriteTheWireName_When_DefaultValueIsADefinedEnumValue()
    {
        JsonNode document = await GetDocumentAsync();

        Property(document, nameof(UndefinedDefaultOrder), "definedStatus")["default"]!.GetValue<string>().Should().Be("pending_approval");
    }

    [Fact]
    public async Task Document_Should_OmitTheDefault_When_AParameterDefaultIsAnUndefinedEnumValue()
    {
        JsonNode document = await GetDocumentAsync();

        JsonNode parameter = document["paths"]!["/undefined-parameter"]!["get"]!["parameters"]!.AsArray()
            .Single(static node => node!["name"]!.GetValue<string>() == "status")!;
        parameter.ToJsonString().Should().NotContain("\"default\"");
    }

    [Fact]
    public void Resolver_Should_KeepTheContract_When_ACustomResolverDescribesAnEnumAsAnObject()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISerializerDataContractResolver>(new ObjectEnumResolver());
        services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleEnumDefaultTests).Assembly);
        using ServiceProvider provider = services.BuildServiceProvider();

        DataContract contract = provider.GetRequiredService<ISerializerDataContractResolver>().GetDataContractForType(typeof(SampleStatus));

        contract.Should().BeSameAs(ObjectEnumResolver.Contract);
    }

    private static async Task<JsonNode> GetDocumentAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnumConventions();
        builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleEnumDefaultTests).Assembly);

        await using WebApplication app = builder.Build();
        app.UseSwagger();
        app.MapPost("/undefined-default", static (UndefinedDefaultOrder order) => order);
        app.MapGet("/undefined-parameter", static ([FromQuery] SampleStatus status = (SampleStatus)42) => status);
        await app.StartAsync();

        using HttpResponseMessage response = await app.GetTestClient().GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonNode document = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        await app.StopAsync();
        return document;
    }

    private static JsonNode Property(JsonNode document, string schema, string property) =>
        document["components"]!["schemas"]!.AsObject()
            .Single(pair => pair.Key.EndsWith(schema, StringComparison.Ordinal)).Value!["properties"]![property]!;

    internal sealed class UndefinedDefaultOrder
    {
        [DefaultValue((SampleStatus)42)]
        public SampleStatus Status { get; init; }

        [DefaultValue((SampleStatus)42)]
        public SampleStatus? NullableStatus { get; init; }

        [DefaultValue(SampleStatus.PendingApproval)]
        public SampleStatus DefinedStatus { get; init; }
    }

    private sealed class ObjectEnumResolver : ISerializerDataContractResolver
    {
        public static readonly DataContract Contract = DataContract.ForObject(typeof(SampleStatus), []);

        private readonly JsonSerializerDataContractResolver inner = new(new JsonSerializerOptions());

        public DataContract GetDataContractForType(Type type) =>
            type == typeof(SampleStatus) ? Contract : inner.GetDataContractForType(type);
    }
}
