using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
        options.Namespace.Should().Be("http");
        options.ScopeResolver.Should().BeSameAs(IdempotencyScopes.User);
        options.AllowUnscopedKeys.Should().BeFalse();
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
            () => new ServiceCollection().AddIdempotency(o => o.MaxKeyLength = 0),
            () => new ServiceCollection().AddIdempotency(o => o.MaxResponseBodySize = -1),
            () => new ServiceCollection().AddIdempotency(o => o.MaxResponseBodySize = (long)int.MaxValue + 1),
            () => new ServiceCollection().AddIdempotency(o => o.ShouldStore = null!),
            () => new ServiceCollection().AddIdempotency(o => o.ScopeResolver = null!),
            () => new ServiceCollection().AddIdempotency(o => o.Namespace = ""),
            () => new ServiceCollection().AddIdempotency(o => o.Namespace = null!),
        ];

        foreach (Action act in invalid)
            act.Should().Throw<ArgumentException>();
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
