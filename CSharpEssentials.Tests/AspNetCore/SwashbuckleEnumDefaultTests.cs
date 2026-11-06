using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
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

    [Fact]
    public async Task Document_Should_OmitTheDefault_When_AddSwaggerGenIsFollowedByAddSwaggerEnumConventions()
    {
        JsonNode document = await GetDocumentAsync(static services =>
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(static options => options.AddEnumConventions());
            services.AddSwaggerEnumConventions();
        });

        Property(document, nameof(UndefinedDefaultOrder), "status").ToJsonString().Should().NotContain("\"default\"");
        Property(document, nameof(UndefinedDefaultOrder), "definedStatus")["default"]!.GetValue<string>().Should().Be("pending_approval");
    }

    [Fact]
    public void AddSwaggerEnumConventions_Should_RegisterOnce_When_CalledTwice()
    {
        var services = new ServiceCollection();
        services.AddSwaggerEnumConventions();
        ServiceDescriptor resolver = ResolverDescriptor(services);

        services.AddSwaggerEnumConventions();

        ResolverDescriptor(services).Should().BeSameAs(resolver);
        EnumSchemaFilterCount(services).Should().Be(1);
    }

    [Fact]
    public void AddSwaggerEnumConventions_Should_RegisterOnce_When_CalledAfterAddSwagger()
    {
        var services = new ServiceCollection();
        AddSwagger(services);
        ServiceDescriptor resolver = ResolverDescriptor(services);

        services.AddSwaggerEnumConventions();

        ResolverDescriptor(services).Should().BeSameAs(resolver);
    }

    [Fact]
    public void AddSwagger_Should_KeepTheWrappedResolver_When_CalledAfterAddSwaggerEnumConventions()
    {
        var services = new ServiceCollection();
        services.AddSwaggerEnumConventions();
        ServiceDescriptor resolver = ResolverDescriptor(services);

        AddSwagger(services);

        ResolverDescriptor(services).Should().BeSameAs(resolver);
    }

    [Fact]
    public void AddSwaggerEnumConventions_Should_NotRegisterSwaggerGenAgain_When_CalledAfterAddSwaggerGen()
    {
        var services = new ServiceCollection();
        services.AddSwaggerGen();
        int configurators = GeneratorConfiguratorCount(services);

        services.AddSwaggerEnumConventions();

        GeneratorConfiguratorCount(services).Should().Be(configurators);
    }

    [Fact]
    public void Resolver_Should_WriteNull_When_TheConverterRejectsAnInstanceRegisteredValue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISerializerDataContractResolver>(new RejectingEnumResolver());
        services.AddSwaggerEnumConventions();

        Func<object, string> converter = EnumContract(services).JsonConverter;

        converter((SampleStatus)42).Should().Be("null");
        converter(SampleStatus.PendingApproval).Should().Be("\"defined\"");
    }

    [Fact]
    public void Resolver_Should_WriteNull_When_TheConverterRejectsAFactoryRegisteredValue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISerializerDataContractResolver>(static _ => new RejectingEnumResolver());
        services.AddSwaggerEnumConventions();

        EnumContract(services).JsonConverter((SampleStatus)42).Should().Be("null");
    }

    [Fact]
    public void AddSwaggerEnumConventions_Should_KeepAResolverRegisteredByType()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISerializerDataContractResolver, RejectingEnumResolver>();
        services.AddSwaggerEnumConventions();

        ResolverDescriptor(services).ImplementationType.Should().Be<RejectingEnumResolver>();
        Func<string> convert = () => EnumContract(services).JsonConverter((SampleStatus)42);
        convert.Should().Throw<EnumValueException>();
    }

    [Fact]
    public void AddSwaggerEnumConventions_Should_RegisterNoResolver_When_ASwaggerProviderIsRegisteredWithoutOne()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISwaggerProvider, SwaggerGenerator>();

        services.AddSwaggerEnumConventions();

        services.Should().NotContain(static descriptor => descriptor.ServiceType == typeof(ISerializerDataContractResolver));
    }

    private static DataContract EnumContract(ServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ISerializerDataContractResolver>().GetDataContractForType(typeof(SampleStatus));
    }

    private static int GeneratorConfiguratorCount(ServiceCollection services) =>
        services.Count(static descriptor => descriptor.ServiceType == typeof(IConfigureOptions<SwaggerGeneratorOptions>));

    private static ServiceDescriptor ResolverDescriptor(ServiceCollection services) =>
        services.Single(static descriptor => descriptor.ServiceType == typeof(ISerializerDataContractResolver));

    private static int EnumSchemaFilterCount(ServiceCollection services)
    {
        services.AddLogging();
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value
            .SchemaFilterDescriptors.Count(static descriptor => descriptor.Type == typeof(EnumSchemaFilter));
    }

    private static void AddSwagger(IServiceCollection services) =>
        services.AddSwagger<DefaultConfigureSwaggerOptions>(
            "Bearer",
            new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" },
            typeof(SwashbuckleEnumDefaultTests).Assembly);

    private static async Task<JsonNode> GetDocumentAsync(Action<IServiceCollection>? configureServices = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnumConventions();
        (configureServices ?? AddSwagger)(builder.Services);

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

    private sealed class RejectingEnumResolver : ISerializerDataContractResolver
    {
        public DataContract GetDataContractForType(Type type) =>
            DataContract.ForPrimitive(type, DataType.String, null, static value => Enum.IsDefined(value.GetType(), value)
                ? "\"defined\""
                : throw new EnumValueException(new EnumValueError(value.GetType(), value.ToString(), [], null)));
    }

    private sealed class ObjectEnumResolver : ISerializerDataContractResolver
    {
        public static readonly DataContract Contract = DataContract.ForObject(typeof(SampleStatus), []);

        private readonly JsonSerializerDataContractResolver inner = new(new JsonSerializerOptions());

        public DataContract GetDataContractForType(Type type) =>
            type == typeof(SampleStatus) ? Contract : inner.GetDataContractForType(type);
    }
}
