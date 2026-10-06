using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Endpoints;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// The Swashbuckle documents of the sample API describe the enums exactly like the Microsoft.AspNetCore.OpenApi documents:
/// both packages are compared with the same OpenAPI 3.0 golden files (<c>tests/golden/openapi</c>), written by
/// CSharpEssentials.AspNetCore.OpenApi.Tests.
/// </summary>
public class SwashbuckleEnumGoldenTests
{
    public static TheoryData<string> Documents() => [.. SampleApi.Documents];

    [Theory]
    [MemberData(nameof(Documents))]
    public async Task Document_Should_MatchTheSharedGoldenFile_When_DescribedBySwashbuckle(string document)
    {
        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(addEnumConventions: true);
        string golden = await File.ReadAllTextAsync(OpenApiGolden.PathOf(document));

        string extract = OpenApiGolden.Extract(documents[document]);

        extract.Should().Be(golden.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Documents_Should_DescribeEnumsDifferently_When_GroupsWriteNumbersAndStrings()
    {
        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(addEnumConventions: true);

        OpenApiGolden.Extract(documents["v1"]).Should().NotBe(OpenApiGolden.Extract(documents["v2"]));
    }

    [Fact]
    public async Task Document_Should_KeepTheSwashbuckleSchema_When_TheEnumIsPlain()
    {
        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(addEnumConventions: true);

        JsonNode plain = JsonNode.Parse(documents["v2"])!["components"]!["schemas"]!["SamplePlain"]!;

        plain.ToJsonString().Should().NotContain("x-enum-");
        plain["type"]!.GetValue<string>().Should().Be("integer");
    }

    [Fact]
    public async Task Documents_Should_BeGenerated_When_EnumConventionsAreNotAdded()
    {
        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(addEnumConventions: false);

        documents.Values.Should().AllSatisfy(json => json.Should().NotContain("x-enum-varnames"));
    }

    [Fact]
    public async Task Documents_Should_WarnOncePerEnum_When_ADocumentMixesNumbersAndStrings()
    {
        using var logs = new CapturingLoggerProvider();

        await GetDocumentsAsync(addEnumConventions: true, logs, passes: 2);

        string[] warnings = [.. logs.Entries
            .Where(static entry => entry.Level == LogLevel.Warning && entry.Message.Contains("x-enum-wire-format: number", StringComparison.Ordinal))
            .Select(static entry => entry.Message)];
        warnings.Should().OnlyHaveUniqueItems();
        warnings.Should().ContainSingle(static message => message.StartsWith($"OpenAPI document 'mixed' writes enum {typeof(SampleStatus).FullName} ", StringComparison.Ordinal));
        warnings.Should().NotContain(static message => message.StartsWith("OpenAPI document 'v1' ", StringComparison.Ordinal) || message.StartsWith("OpenAPI document 'v2' ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Operation_Should_UseWriteAsAndWarnOnce_When_TheHeaderSelectorThrows()
    {
        using var logs = new CapturingLoggerProvider();

        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(
            addEnumConventions: true,
            logs,
            passes: 2,
            configureApp: static app => app.MapGet("/v1/throwing", static (SampleStatus status) => status).WithGroupName("v1")
                .WithEnumWireFormat("X-Enum-Format", static _ => throw new InvalidOperationException("selector failed")));

        // The operation falls back to WriteAs (String): the v1 document, numbers otherwise, now shows the string form.
        JsonNode document = JsonNode.Parse(documents["v1"])!;
        JsonNode operation = document["paths"]!["/v1/throwing"]!["get"]!;
        operation["x-enum-wire-format"].Should().BeNull();
        operation["x-enum-wire-format-header"]!.GetValue<string>().Should().Be("X-Enum-Format");
        document["components"]!["schemas"]!["SampleStatus"]!["type"]!.GetValue<string>().Should().Be("string");
        logs.Entries.Where(static entry => entry.Message.Contains("selector of", StringComparison.Ordinal)).Should().ContainSingle()
            .Which.Message.Should().Contain("/v1/throwing").And.Contain("global format String");
    }

    [Fact]
    public async Task HeaderSelector_Should_RunInARequestScope_When_TheDocumentIsGenerated()
    {
        var providers = new ConcurrentBag<IServiceProvider>();
        IServiceProvider? root = null;

        await GetDocumentsAsync(
            addEnumConventions: true,
            configureApp: app =>
            {
                root = app.Services;
                app.MapGet("/v1/scoped", static (SampleStatus status) => status).WithGroupName("v1").WithEnumWireFormat("X-Enum-Format", context =>
                {
                    providers.Add(context.RequestServices);
                    return EnumWireFormat.Number;
                });
            });

        providers.Should().NotBeEmpty().And.NotContain(root!);
    }

    [Fact]
    public async Task HeaderSelector_Should_ResolveScopedServices_When_TheRootProviderValidatesScopes()
    {
        using var logs = new CapturingLoggerProvider();
        var resolved = new ConcurrentBag<ScopedFormat>();

        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(
            addEnumConventions: true,
            logs,
            configureServices: static services => services.AddScoped<ScopedFormat>(),
            configureApp: app =>
            {
                foreach (string path in (string[])["/v1/scoped-a", "/v1/scoped-b"])
                {
                    app.MapGet(path, static (SampleStatus status) => status).WithGroupName("v1").WithEnumWireFormat("X-Enum-Format", context =>
                    {
                        ScopedFormat format = context.RequestServices.GetRequiredService<ScopedFormat>();
                        resolved.Add(format);
                        return format.Format;
                    });
                }
            },
            validateScopes: true);

        // ValidateScopes makes the root provider refuse scoped services; a fresh scope per selector call gives each its own instance.
        resolved.Should().HaveCountGreaterThanOrEqualTo(2).And.OnlyHaveUniqueItems();
        logs.Entries.Should().NotContain(static entry => entry.Message.Contains("selector of", StringComparison.Ordinal));
        JsonNode.Parse(documents["v1"])!["components"]!["schemas"]!["SampleStatus"]!["type"]!.GetValue<string>().Should().Be("integer");
    }

    [Fact]
    public async Task Operation_Should_UseWriteAs_When_DisposingTheSelectorScopeThrows()
    {
        using var logs = new CapturingLoggerProvider();

        IReadOnlyDictionary<string, string> documents = await GetDocumentsAsync(
            addEnumConventions: true,
            logs,
            configureServices: static services => services.AddScoped<AsyncOnlyDisposable>(),
            configureApp: static app => app.MapGet("/v1/async-disposable", static (SampleStatus status) => status).WithGroupName("v1")
                .WithEnumWireFormat("X-Enum-Format", static context => context.RequestServices.GetRequiredService<AsyncOnlyDisposable>().Format));

        // The synchronous scope Dispose throws for a service that only implements IAsyncDisposable; the operation falls back to WriteAs.
        JsonNode document = JsonNode.Parse(documents["v1"])!;
        document["paths"]!["/v1/async-disposable"]!["get"]!["x-enum-wire-format-header"]!.GetValue<string>().Should().Be("X-Enum-Format");
        document["components"]!["schemas"]!["SampleStatus"]!["type"]!.GetValue<string>().Should().Be("string");
        logs.Entries.Where(static entry => entry.Message.Contains("selector of", StringComparison.Ordinal)).Should().ContainSingle()
            .Which.Message.Should().Contain("/v1/async-disposable");
    }

    internal static async Task<IReadOnlyDictionary<string, string>> GetDocumentsAsync(
        bool addEnumConventions,
        ILoggerProvider? logs = null,
        int passes = 1,
        Action<WebApplication>? configureApp = null,
        Action<IServiceCollection>? configureServices = null,
        bool validateScopes = false)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes);
        builder.Logging.ClearProviders();
        if (logs is not null)
            builder.Logging.AddProvider(logs);

        builder.Services.AddEnumConventions();
        builder.Services.AddControllers().AddSampleControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            foreach (string document in SampleApi.Documents)
                options.SwaggerDoc(document, new OpenApiInfo { Title = document, Version = document });
            if (addEnumConventions)
                options.AddEnumConventions();
            // The derived types of SampleChange get their own components, named like Microsoft.AspNetCore.OpenApi names them.
            options.UseOneOfForPolymorphism();
            options.CustomSchemaIds(static type => type == typeof(SampleGrantChange) ? nameof(SampleChange) + type.Name : type.Name);
        });
        configureServices?.Invoke(builder.Services);

        await using WebApplication app = builder.Build();
        app.UseSwagger();
        app.MapSampleApi();
        app.MapControllers();
        configureApp?.Invoke(app);
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int pass = 0; pass < passes; pass++)
            {
                foreach (string document in SampleApi.Documents)
                    documents[document] = await client.GetStringAsync($"/swagger/{document}/swagger.json");
            }
            return documents;
        }
        finally
        {
            await app.StopAsync();
        }
    }

    private sealed class ScopedFormat
    {
        public EnumWireFormat Format => EnumWireFormat.Number;
    }

    private sealed class AsyncOnlyDisposable : IAsyncDisposable
    {
        public EnumWireFormat Format => EnumWireFormat.Number;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
