using CSharpEssentials.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.DependencyInjection;

public class ServiceRegistrationTests
{
    private interface IStore;

    private sealed class FirstStore : IStore;

    private sealed class SecondStore : IStore;

    private static ServiceDescriptor First(object? key = null) => new(typeof(IStore), key, typeof(FirstStore), ServiceLifetime.Scoped);

    private static ServiceDescriptor Second(object? key = null) => new(typeof(IStore), key, typeof(SecondStore), ServiceLifetime.Scoped);

    private static IServiceCollection Services(params ServiceDescriptor[] descriptors)
    {
        IServiceCollection services = new ServiceCollection();
        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Add(descriptor);
        }

        return services;
    }

    [Fact]
    public void Apply_Should_Append_When_StrategyIsAdd()
    {
        IServiceCollection services = Services(First());

        ServiceRegistration.Apply(services, null, Second(), RegistrationStrategy.Add);

        services.Select(d => d.ImplementationType).Should().Equal(typeof(FirstStore), typeof(SecondStore));
    }

    [Fact]
    public void Apply_Should_Skip_When_StrategyIsTryAddAndServiceKeyMatches()
    {
        IServiceCollection services = Services(First("a"));

        ServiceRegistration.Apply(services, null, Second("a"), RegistrationStrategy.TryAdd);

        services.Should().ContainSingle().Which.KeyedImplementationType.Should().Be<FirstStore>();
    }

    [Fact]
    public void Apply_Should_Append_When_StrategyIsTryAddAndKeyDiffers()
    {
        IServiceCollection services = Services(First("a"), First());

        ServiceRegistration.Apply(services, null, Second("b"), RegistrationStrategy.TryAdd);

        services.Should().HaveCount(3);
    }

    [Fact]
    public void Apply_Should_SkipSameImplementation_When_StrategyIsTryAddEnumerable()
    {
        IServiceCollection services = Services(First("a"));

        ServiceRegistration.Apply(services, null, First("a"), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, Second("a"), RegistrationStrategy.TryAddEnumerable);
        ServiceRegistration.Apply(services, null, First(), RegistrationStrategy.TryAddEnumerable);

        services.Should().HaveCount(3);
    }

    [Fact]
    public void Apply_Should_RemoveAllMatches_When_StrategyIsReplace()
    {
        IServiceCollection services = Services(First("a"), First("a"), First("b"), First());

        ServiceRegistration.Apply(services, null, Second("a"), RegistrationStrategy.Replace);

        services.Where(d => Equals(d.ServiceKey, "a")).Should().ContainSingle().Which.KeyedImplementationType.Should().Be<SecondStore>();
        services.Should().HaveCount(3);
    }

    [Fact]
    public void Apply_Should_ThrowWithServiceKeyAndImplementations_When_StrategyIsThrowAndMatchExists()
    {
        IServiceCollection services = Services(First("a"));

        Action act = () => ServiceRegistration.Apply(services, null, Second("a"), RegistrationStrategy.Throw);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{typeof(IStore).FullName}*'a'*{typeof(FirstStore).FullName}*{typeof(SecondStore).FullName}*");
        services.Should().ContainSingle();
    }

    [Fact]
    public void Apply_Should_Append_When_StrategyIsThrowAndOnlyOtherKeyExists()
    {
        IServiceCollection services = Services(First("a"));

        ServiceRegistration.Apply(services, null, Second(), RegistrationStrategy.Throw);

        services.Should().HaveCount(2);
    }

    [Fact]
    public void Apply_Should_Throw_When_StrategyIsUnknown()
    {
        IServiceCollection services = Services();

        Action act = () => ServiceRegistration.Apply(services, null, First(), (RegistrationStrategy)42);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Apply_Should_LogDuplicateAtDebug_When_ServiceAndKeyAlreadyRegistered()
    {
        IServiceCollection services = Services(First("a"));
        CapturingLogger logger = new();

        ServiceRegistration.Apply(services, logger, Second("a"), RegistrationStrategy.Add);
        ServiceRegistration.Apply(services, logger, Second("b"), RegistrationStrategy.Add);

        logger.Entries.Should().ContainSingle()
            .Which.Should().Match<(LogLevel Level, EventId EventId, string Message)>(entry =>
                entry.Level == LogLevel.Debug &&
                entry.EventId.Id == 2001 &&
                entry.Message.Contains(nameof(FirstStore)) &&
                entry.Message.Contains(nameof(SecondStore)) &&
                entry.Message.Contains("Add"));
    }

    [Fact]
    public void Apply_Should_NotLog_When_DebugIsDisabled()
    {
        IServiceCollection services = Services(First());
        CapturingLogger logger = new(LogLevel.Information);

        ServiceRegistration.Apply(services, logger, Second(), RegistrationStrategy.Add);

        logger.Entries.Should().BeEmpty();
    }
}
