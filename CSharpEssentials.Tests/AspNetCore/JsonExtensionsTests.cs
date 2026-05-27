using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore;

public class JsonExtensionsTests
{
    [Fact]
    public void ConfigureSystemTextJson_Should_ReturnSameServiceCollection_When_Called()
    {
        var services = new ServiceCollection();

        IServiceCollection result = services.ConfigureSystemTextJson();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void ConfigureSystemTextJson_Should_RegisterServices_When_CalledWithDefaults()
    {
        var services = new ServiceCollection();

        services.ConfigureSystemTextJson();

        services.Should().NotBeEmpty();
    }

    [Fact]
    public void ConfigureSystemTextJson_Should_NotThrow_When_CalledWithNullOptions()
    {
        var services = new ServiceCollection();

        Action act = () => services.ConfigureSystemTextJson(null, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void ConfigureSystemTextJson_Should_InvokeConfigureCallback_When_CallbackProvided()
    {
        var services = new ServiceCollection();

        services.ConfigureSystemTextJson(configureOptions: _ => { });

        var provider = services.BuildServiceProvider();
        var mvcOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        mvcOptions.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureSystemTextJson_Should_RegisterJsonOptions_When_Called()
    {
        var services = new ServiceCollection();

        services.ConfigureSystemTextJson();

        var provider = services.BuildServiceProvider();
        var jsonOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>();
        jsonOptions.Should().NotBeNull();
    }
}
