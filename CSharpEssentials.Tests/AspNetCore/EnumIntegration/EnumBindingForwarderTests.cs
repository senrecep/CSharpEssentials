using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>The obsolete 4.x <c>AddEnumBinding</c> forwards its options to <c>AddEnumConventions</c>.</summary>
[Obsolete("Tests the obsolete 4.x enum binding registration.")]
public class EnumBindingForwarderTests
{
    [Fact]
    public void AddEnumBinding_Should_MapOptionsToConventions_When_OptionsAreSet()
    {
        ServiceCollection services = new();
        services.AddEnumBinding(o =>
        {
            o.AllowIntegerValues = false;
            o.CanBind = t => t == typeof(EcStatus);
        });
        using ServiceProvider provider = services.BuildServiceProvider();

        EnumConventions conventions = provider.GetRequiredService<EnumConventions>();

        (conventions.AcceptNumbers, conventions.CanHandle(typeof(EcStatus)), conventions.CanHandle(typeof(EcPermission)))
            .Should().Be((false, true, false));
    }

    [Fact]
    public void AddEnumBinding_Should_Throw_When_NamingPolicyIsNotSnakeCase()
    {
        ServiceCollection services = new();

        Action act = () => services.AddEnumBinding(o => o.NamingPolicy = JsonNamingPolicy.CamelCase);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void AddEnumBinding_Should_AcceptSnakeCaseNamingPolicy_When_ItMatchesTheDefault()
    {
        ServiceCollection services = new();

        Action act = () => services.AddEnumBinding(o => o.NamingPolicy = JsonNamingPolicy.SnakeCaseLower);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task AddEnumBinding_Should_UseErrorFactory_When_ValueIsRejected()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync(s => s.AddEnumBinding(o =>
            o.ErrorFactory = (key, type, allowed) => Error.Validation(code: $"{key}:{type.Name}", description: string.Join("|", allowed))));

        EcResponse response = await host.GetAsync("/min/query?status=99");

        response.ShouldBeProblem().Should().ContainSingle().Which.Should().Be(
            ("status:EcStatus", "pending|pending_approval|shipped"));
    }

    [Fact]
    public async Task AddEnumBinding_Should_RejectNumbers_When_AllowIntegerValuesIsFalse()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync(s => s.AddEnumBinding(o => o.AllowIntegerValues = false));

        EcResponse response = await host.GetAsync("/mvc/query?status=1");

        response.Status.Should().Be(400);
    }
}
