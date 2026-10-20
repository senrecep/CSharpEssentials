using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyRegistrationTests
{
    [Fact]
    public void AddIdempotency_Should_Register_The_InMemory_Store_By_Default()
    {
        ServiceCollection services = new();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<InMemoryIdempotencyRecordStore>();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeSameAs(provider.GetRequiredService<IIdempotencyRecordStore>());
        provider.GetRequiredService<IdempotencyOptions>().HeaderName.Should().Be("Idempotency-Key");
    }

    [Fact]
    public void AddIdempotency_Should_Keep_An_Existing_Store()
    {
        ServiceCollection services = new();
        services.AddSingleton<IIdempotencyRecordStore, CustomStore>();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<CustomStore>();
    }

    [Fact]
    public void UseInMemoryStore_Should_Replace_An_Existing_Store()
    {
        ServiceCollection services = new();
        services.AddSingleton<IIdempotencyRecordStore, CustomStore>();

        services.AddIdempotency(options => options.UseInMemoryStore());

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<InMemoryIdempotencyRecordStore>();
    }

    [Fact]
    public void UseDistributedCacheStore_Should_Register_The_Cache_Store()
    {
        ServiceCollection services = new();
        services.AddDistributedMemoryCache();

        services.AddIdempotency(options => options.UseDistributedCacheStore());

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<DistributedCacheIdempotencyRecordStore>();
    }

    [Fact]
    public void UseStore_Should_Register_The_Store_With_The_Given_Lifetime()
    {
        ServiceCollection services = new();

        services.AddIdempotency(options => options.UseStore<CustomStore>());

        services.Where(d => d.ServiceType == typeof(IIdempotencyRecordStore)).Should().ContainSingle()
            .Which.Should().Match<ServiceDescriptor>(d => d.Lifetime == ServiceLifetime.Scoped && d.ImplementationType == typeof(CustomStore));
    }

    [Fact]
    public void AddIdempotency_Twice_Should_Use_The_Last_Options()
    {
        ServiceCollection services = new();

        services.AddIdempotency(options => options.HeaderName = "A");
        services.AddIdempotency(options => options.HeaderName = "B");

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IdempotencyOptions>().HeaderName.Should().Be("B");
        services.Count(d => d.ServiceType == typeof(IIdempotencyRecordStore)).Should().Be(1);
    }

    [Fact]
    public void AddIdempotency_Should_Default_To_The_Http_Namespace_And_User_Scope()
    {
        ServiceCollection services = new();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        IdempotencyOptions options = provider.GetRequiredService<IdempotencyOptions>();
        options.Mode.Should().Be(IdempotencyMode.Enforce);
        options.Namespace.Should().Be("http");
        options.ScopeResolver.Should().BeSameAs(IdempotencyScopes.User);
        options.UnscopedKeyBehavior.Should().Be(UnscopedKeyBehavior.Skip);
        options.ShouldStore.Should().BeSameAs(IdempotencyStorePolicies.Default);
        options.NeverStoreStatusCodes.Should().BeEquivalentTo([503, 504]);
        options.ServerErrorRetention.Should().Be(TimeSpan.FromMinutes(10));
        options.MaxInFlightTimeout.Should().Be(TimeSpan.FromMinutes(15));
        options.RequireExecutionFilter.Should().BeFalse();
        options.ReplayedHeaders.Should().Contain("Retry-After");
    }

    [Fact]
    public void AddIdempotency_Should_Reject_Invalid_Options()
    {
        Action[] invalid =
        [
            () => new ServiceCollection().AddIdempotency(o => o.HeaderName = " "),
            () => new ServiceCollection().AddIdempotency(o => o.ReplayedHeaderName = ""),
            () => new ServiceCollection().AddIdempotency(o => o.InFlightTimeout = TimeSpan.Zero),
            () => new ServiceCollection().AddIdempotency(o => o.RetentionPeriod = TimeSpan.FromSeconds(-1)),
            () => new ServiceCollection().AddIdempotency(o => o.RetryAfter = TimeSpan.FromSeconds(-1)),
            () => new ServiceCollection().AddIdempotency(o => o.RetryAfter = TimeSpan.Zero),
            () => new ServiceCollection().AddIdempotency(o => o.MaxKeyLength = 0),
            () => new ServiceCollection().AddIdempotency(o => o.MaxResponseBodySize = -1),
            () => new ServiceCollection().AddIdempotency(o => o.MaxResponseBodySize = (long)int.MaxValue + 1),
            () => new ServiceCollection().AddIdempotency(o => o.ShouldStore = null!),
            () => new ServiceCollection().AddIdempotency(o => o.ScopeResolver = null!),
            () => new ServiceCollection().AddIdempotency(o => o.Namespace = ""),
            () => new ServiceCollection().AddIdempotency(o => o.Namespace = null!),
            () => new ServiceCollection().AddIdempotency(o => o.InFlightTimeout = TimeSpan.FromMinutes(16)),
            () => new ServiceCollection().AddIdempotency(o =>
            {
                o.InFlightTimeout = TimeSpan.FromMinutes(10);
                o.RetentionPeriod = TimeSpan.FromMinutes(5);
                o.ServerErrorRetention = TimeSpan.FromMinutes(5);
            }),
            () => new ServiceCollection().AddIdempotency(o => o.MaxInFlightTimeout = TimeSpan.Zero),
            () => new ServiceCollection().AddIdempotency(o => o.ServerErrorRetention = TimeSpan.Zero),
            () => new ServiceCollection().AddIdempotency(o => o.ServerErrorRetention = TimeSpan.FromHours(25)),
            () => new ServiceCollection().AddIdempotency(o => o.NeverStoreStatusCodes.Clear()),
            () => new ServiceCollection().AddIdempotency(o => o.NeverStoreStatusCodes.Add(99)),
            () => new ServiceCollection().AddIdempotency(o => o.NeverStoreStatusCodes.Add(600)),
            () => new ServiceCollection().AddIdempotency(o => o.UnscopedKeyBehavior = (UnscopedKeyBehavior)3),
            () => new ServiceCollection().AddIdempotency(o => o.Mode = (IdempotencyMode)3),
        ];

        foreach (Action act in invalid)
            act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIdempotency_Should_Accept_The_Boundaries_Of_The_Timing_Rules()
    {
        Action act = () => new ServiceCollection().AddIdempotency(o =>
        {
            o.MaxInFlightTimeout = TimeSpan.FromMinutes(30);
            o.InFlightTimeout = TimeSpan.FromMinutes(30);
            o.RetentionPeriod = TimeSpan.FromMinutes(30);
            o.ServerErrorRetention = TimeSpan.FromMinutes(30);
            o.NeverStoreStatusCodes.Add(100);
            o.NeverStoreStatusCodes.Add(599);
        });

        act.Should().NotThrow();
    }

    [Fact]
    public void AddIdempotency_Should_Expose_The_Options_Through_The_Monitor_With_Later_Configuration()
    {
        ServiceCollection services = new();

        services.AddIdempotency(options => options.HeaderName = "A");
        services.Configure<IdempotencyOptions>(options => options.Mode = IdempotencyMode.Observe);

        using ServiceProvider provider = services.BuildServiceProvider();
        IdempotencyOptions current = provider.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>().CurrentValue;
        current.HeaderName.Should().Be("A");
        current.Mode.Should().Be(IdempotencyMode.Observe);
        provider.GetRequiredService<IdempotencyOptions>().Should().BeSameAs(current);
    }

    [Fact]
    public void AddIdempotency_Twice_Should_Drop_The_First_Configure()
    {
        ServiceCollection services = new();

        services.AddIdempotency(options => options.AddProfile("payments", profile => profile.Mode = IdempotencyMode.Observe));
        services.AddIdempotency(options => options.AddProfile("payments", profile => profile.Mode = IdempotencyMode.Off));

        using ServiceProvider provider = services.BuildServiceProvider();
        Action act = () => _ = provider.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>().CurrentValue;
        act.Should().NotThrow();
    }

    [Fact]
    public void A_Reload_Should_Change_The_Mode_And_An_Invalid_Reload_Should_Throw()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mode"] = "Observe", ["RetryAfter"] = "00:00:02" })
            .Build();
        ServiceCollection services = new();
        services.AddIdempotency();
        services.Configure<IdempotencyOptions>(configuration);
        using ServiceProvider provider = services.BuildServiceProvider();
        IOptionsMonitor<IdempotencyOptions> monitor = provider.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>();
        IdempotencyMode before = monitor.CurrentValue.Mode;

        configuration["Mode"] = "Enforce";
        configuration.Reload();
        IdempotencyMode after = monitor.CurrentValue.Mode;
        configuration["RetryAfter"] = "00:00:00";
        Action invalidReload = configuration.Reload;

        before.Should().Be(IdempotencyMode.Observe);
        after.Should().Be(IdempotencyMode.Enforce);
        invalidReload.Should().Throw<AggregateException>().WithInnerException<OptionsValidationException>();
        Action read = () => _ = monitor.CurrentValue;
        read.Should().Throw<OptionsValidationException>().WithMessage("*RetryAfter*");
    }

    private sealed class CustomStore : IIdempotencyRecordStore
    {
        public ValueTask<IdempotencyRecordReservation> TryReserveAsync(IdempotencyRecordKey key, string fingerprint, TimeSpan lease, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(IdempotencyRecordReservation.Reserved("token"));

        public ValueTask<bool> CompleteAsync(IdempotencyRecordKey key, string token, string fingerprint, ReadOnlyMemory<byte> payload, TimeSpan retention, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);

        public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
    }
}
