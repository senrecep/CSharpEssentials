using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public sealed class EnhancedProblemDetailsJsonOptionsSetupTests
{
    private static IPostConfigureOptions<HttpJsonOptions> GetSetup(ServiceProvider provider) =>
        provider.GetServices<IPostConfigureOptions<HttpJsonOptions>>()
            .Single(s => s.GetType().Name == "EnhancedProblemDetailsJsonOptionsSetup");

    [Fact]
    public void PostConfigure_Should_LeaveResolverUnset_When_ResolverIsNullAndReflectionIsEnabled()
    {
        using ServiceProvider provider = new ServiceCollection().AddEnhancedProblemDetails().BuildServiceProvider();
        var options = new HttpJsonOptions();
        options.SerializerOptions.TypeInfoResolver = null;

        GetSetup(provider).PostConfigure(Options.DefaultName, options);

        options.SerializerOptions.TypeInfoResolver.Should().BeNull();
        options.SerializerOptions.TypeInfoResolverChain.Should().BeEmpty();
    }

    [Fact]
    public void PostConfigure_Should_AppendContextOnce_When_RunTwiceOnAConfiguredResolver()
    {
        using ServiceProvider provider = new ServiceCollection().AddEnhancedProblemDetails().BuildServiceProvider();
        IPostConfigureOptions<HttpJsonOptions> setup = GetSetup(provider);
        var options = new HttpJsonOptions();
        int before = options.SerializerOptions.TypeInfoResolverChain.Count;

        setup.PostConfigure(Options.DefaultName, options);
        setup.PostConfigure(Options.DefaultName, options);

        options.SerializerOptions.TypeInfoResolverChain.Should().HaveCount(before + 1);
        options.SerializerOptions.TypeInfoResolverChain[^1].GetType().Name.Should().Be("EnhancedProblemDetailsJsonContext");
    }
}
