using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
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

    private static async Task<IReadOnlyDictionary<string, string>> GetDocumentsAsync(bool addEnumConventions)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();

        builder.Services.AddEnumConventions();
        builder.Services.AddControllers().AddSampleControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            foreach (string document in SampleApi.Documents)
                options.SwaggerDoc(document, new OpenApiInfo { Title = document, Version = document });
            if (addEnumConventions)
                options.AddEnumConventions();
        });

        await using WebApplication app = builder.Build();
        app.UseSwagger();
        app.MapSampleApi();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            var documents = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string document in SampleApi.Documents)
                documents[document] = await client.GetStringAsync($"/swagger/{document}/swagger.json");
            return documents;
        }
        finally
        {
            await app.StopAsync();
        }
    }
}
