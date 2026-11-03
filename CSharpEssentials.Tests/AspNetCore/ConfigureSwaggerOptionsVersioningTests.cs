using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed class ConfigureSwaggerOptionsVersioningTests
{
    [Fact]
    public void Configure_Should_AddOneDocumentPerGroup_When_AnApiVersionProviderIsRegistered()
    {
        SwaggerGenOptions options = Configure(
            new ApiVersionDescription(new ApiVersion(1, 0), "v1"),
            new ApiVersionDescription(new ApiVersion(2, 0), "v2"));

        options.SwaggerGeneratorOptions.SwaggerDocs.Keys.Should().BeEquivalentTo("v1", "v2");
        options.SwaggerGeneratorOptions.SwaggerDocs.Should().NotContainKey("v1.0");
        OpenApiInfo v2 = options.SwaggerGeneratorOptions.SwaggerDocs["v2"];
        v2.Title.Should().Be("Orders");
        v2.Version.Should().Be("v2");
        v2.License!.Name.Should().Be("MIT");
    }

    [Fact]
    public void Configure_Should_PrefixTheDescriptionWithEnvironmentAndVersion_When_AnApiVersionProviderIsRegistered()
    {
        SwaggerGenOptions options = Configure(new ApiVersionDescription(new ApiVersion(1, 0), "v1"));

        options.SwaggerGeneratorOptions.SwaggerDocs["v1"].Description.Should().Be("[Staging] 1.0 Order management");
    }

    [Fact]
    public void Configure_Should_MarkTheDescription_When_TheApiVersionIsDeprecated()
    {
        SwaggerGenOptions options = Configure(
            new ApiVersionDescription(new ApiVersion(1, 0), "v1", deprecated: true),
            new ApiVersionDescription(new ApiVersion(2, 0), "v2"));

        options.SwaggerGeneratorOptions.SwaggerDocs["v1"].Description
            .Should().Be("[Staging] 1.0 Order management This API version has been deprecated");
        options.SwaggerGeneratorOptions.SwaggerDocs["v2"].Description
            .Should().Be("[Staging] 2.0 Order management");
    }

    [Fact]
    public void Configure_Should_AddNoDocument_When_TheApiVersionProviderHasNoVersions()
    {
        SwaggerGenOptions options = Configure();

        options.SwaggerGeneratorOptions.SwaggerDocs.Should().BeEmpty();
    }

    private static SwaggerGenOptions Configure(params ApiVersionDescription[] descriptions)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Swagger:Title"] = "Orders",
                ["Swagger:Description"] = "Order management",
                ["Swagger:License"] = "MIT",
            })
            .Build();

        var provider = new Mock<IApiVersionDescriptionProvider>();
        provider.Setup(static p => p.ApiVersionDescriptions).Returns(descriptions);
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(static e => e.EnvironmentName).Returns("Staging");

        IServiceProvider services = new ServiceCollection()
            .AddSingleton(provider.Object)
            .BuildServiceProvider();

        var options = new SwaggerGenOptions();
        new DefaultConfigureSwaggerOptions(services, environment.Object, configuration).Configure(options);
        return options;
    }
}
