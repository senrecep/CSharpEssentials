using CSharpEssentials.AspNetCore;
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
        provider.GetRequiredService<IIdempotencyStore>().Should().BeOfType<InMemoryIdempotencyStore>();
        provider.GetRequiredService<IIdempotencyStore>().Should().BeSameAs(provider.GetRequiredService<IIdempotencyStore>());
        provider.GetRequiredService<IdempotencyOptions>().HeaderName.Should().Be("Idempotency-Key");
    }

    [Fact]
    public void AddIdempotency_Should_Keep_An_Existing_Store()
    {
        ServiceCollection services = new();
        services.AddSingleton<IIdempotencyStore, CustomStore>();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyStore>().Should().BeOfType<CustomStore>();
    }

    [Fact]
    public void UseInMemoryStore_Should_Replace_An_Existing_Store()
    {
        ServiceCollection services = new();
        services.AddSingleton<IIdempotencyStore, CustomStore>();

        services.AddIdempotency(options => options.UseInMemoryStore());

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyStore>().Should().BeOfType<InMemoryIdempotencyStore>();
    }

    [Fact]
    public void UseDistributedCacheStore_Should_Register_The_Cache_Store()
    {
        ServiceCollection services = new();
        services.AddDistributedMemoryCache();

        services.AddIdempotency(options => options.UseDistributedCacheStore());

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyStore>().Should().BeOfType<DistributedCacheIdempotencyStore>();
    }

    [Fact]
    public void UseStore_Should_Register_The_Store_With_The_Given_Lifetime()
    {
        ServiceCollection services = new();

        services.AddIdempotency(options => options.UseStore<CustomStore>());

        services.Where(d => d.ServiceType == typeof(IIdempotencyStore)).Should().ContainSingle()
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
        services.Count(d => d.ServiceType == typeof(IIdempotencyStore)).Should().Be(1);
    }

    [Fact]
    public void DefaultKeyScope_Should_Use_NameIdentifier_Then_Sub()
    {
        var nameIdentifier = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "u1")], "Test")),
        };
        var sub = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim("sub", "u2")], "Test")),
        };
        var unauthenticated = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim("sub", "u3")])),
        };

        IdempotencyOptions.DefaultKeyScope(nameIdentifier).Should().Be("u1");
        IdempotencyOptions.DefaultKeyScope(sub).Should().Be("u2");
        IdempotencyOptions.DefaultKeyScope(unauthenticated).Should().BeNull();
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
            () => new ServiceCollection().AddIdempotency(o => o.KeyScope = null!),
        ];

        foreach (Action act in invalid)
            act.Should().Throw<ArgumentException>();
    }

    private sealed class CustomStore : IIdempotencyStore
    {
        public ValueTask<IdempotencyReservation> TryReserveAsync(string key, string fingerprint, TimeSpan inFlightTimeout, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(IdempotencyReservation.Reserved("token"));

        public ValueTask<bool> CompleteAsync(string key, string token, string fingerprint, IdempotentResponse response, TimeSpan retention, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(true);

        public ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default) => ValueTask.FromResult(true);
    }
}
