using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public class ConfigureSwaggerOptionsTests
{
    private static (IServiceProvider serviceProvider, IHostEnvironment environment, IConfiguration configuration) BuildDependencies(
        Dictionary<string, string?>? configValues = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues ?? [])
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        var provider = services.BuildServiceProvider();

        return (provider, envMock.Object, config);
    }

    [Fact]
    public void Configure_Should_AddV1SwaggerDoc_When_NoApiVersionProviderRegistered()
    {
        var (provider, env, config) = BuildDependencies(new Dictionary<string, string?>
        {
            ["Swagger:Title"] = "Test API",
            ["Swagger:Description"] = "Test Description"
        });

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Should().ContainKey("v1");
        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].Title.Should().Be("Test API");
    }

    [Fact]
    public void Configure_Should_UseDefaultTitle_When_ConfigurationKeyMissing()
    {
        var (provider, env, config) = BuildDependencies();

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].Title.Should().Be("API");
    }

    [Fact]
    public void Configure_Should_UseDefaultDescription_When_ConfigurationKeyMissing()
    {
        var (provider, env, config) = BuildDependencies();

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].Description.Should().Contain("API Description");
    }

    [Fact]
    public void Configure_Should_SetLicenseUrl_When_LicenseUrlConfigured()
    {
        var (provider, env, config) = BuildDependencies(new Dictionary<string, string?>
        {
            ["Swagger:LicenseUrl"] = "https://example.com/license"
        });

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].License!.Url
            .Should().Be(new Uri("https://example.com/license"));
    }

    [Fact]
    public void Configure_Should_SetDefaultLicenseUrl_When_LicenseUrlNotConfigured()
    {
        var (provider, env, config) = BuildDependencies();

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs["v1"].License!.Url
            .Should().Be(new Uri("https://opensource.org/license/mit"));
    }

    [Fact]
    public void Configure_Should_RegisterTimeSpanMapping_When_Called()
    {
        var (provider, env, config) = BuildDependencies();

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure(swaggerGenOptions);

        swaggerGenOptions.SchemaGeneratorOptions.CustomTypeMappings.Should().ContainKey(typeof(TimeSpan));
        swaggerGenOptions.SchemaGeneratorOptions.CustomTypeMappings.Should().ContainKey(typeof(TimeOnly));
    }

    [Fact]
    public void Configure_Named_Should_DelegateToConfigureMethod()
    {
        var (provider, env, config) = BuildDependencies();

        var sut = new DefaultConfigureSwaggerOptions(provider, env, config);
        var swaggerGenOptions = new SwaggerGenOptions();

        sut.Configure("anyName", swaggerGenOptions);

        swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Should().ContainKey("v1");
    }
}
