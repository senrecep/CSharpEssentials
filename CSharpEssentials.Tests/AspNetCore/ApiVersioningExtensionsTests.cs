using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.AspNetCore;

public class ApiVersioningExtensionsTests
{
    [Fact]
    public void AddAndConfigureApiVersioning_Should_RegisterRequiredServices_When_CalledWithDefaults()
    {
        var services = new ServiceCollection();

        services.AddAndConfigureApiVersioning();

        var provider = services.BuildServiceProvider();
        var apiVersioningOptions = provider.GetService<IApiVersioningFeature>();
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddAndConfigureApiVersioning_Should_ReturnSameServiceCollection_When_Called()
    {
        var services = new ServiceCollection();

        IServiceCollection result = services.AddAndConfigureApiVersioning();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAndConfigureApiVersioning_Should_ApplyCustomOptions_When_ConfigureOptionsProvided()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAndConfigureApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(2, 0);
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiVersioningOptions>>();

        options.Value.DefaultApiVersion.Should().Be(new ApiVersion(2, 0));
    }

    [Fact]
    public void AddAndConfigureApiVersioning_Should_ApplyCustomExplorerOptions_When_ConfigureExplorerProvided()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAndConfigureApiVersioning(
            configureExplorer: explorer =>
            {
                explorer.GroupNameFormat = "'ver'VVV";
            });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiExplorerOptions>>();

        options.Value.GroupNameFormat.Should().Be("'ver'VVV");
    }

    [Fact]
    public void AddAndConfigureApiVersioning_Should_WorkWithNullOptions_When_NoCallbacksProvided()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddAndConfigureApiVersioning(null, null);

        act.Should().NotThrow();
    }
}
