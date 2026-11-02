using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotentAttributeTests
{
    [Fact]
    public void Unset_Arguments_Should_Read_As_Their_Type_Defaults()
    {
        IdempotentAttribute attribute = new();

        attribute.Profile.Should().BeNull();
        attribute.Mode.Should().Be(IdempotencyMode.Off);
        attribute.RetentionSeconds.Should().Be(0);
        attribute.LeaseSeconds.Should().Be(0);
        attribute.ServerErrorRetentionSeconds.Should().Be(0);
        attribute.RequireKey.Should().BeFalse();
        attribute.UnscopedKeyBehavior.Should().Be(UnscopedKeyBehavior.Skip);
        attribute.StoreKey.Should().BeNull();
    }

    [Fact]
    public void Set_Arguments_Should_Read_Back_Their_Values()
    {
        IdempotentAttribute attribute = new()
        {
            Profile = "payments",
            Mode = IdempotencyMode.Observe,
            RetentionSeconds = 3600,
            LeaseSeconds = 90,
            ServerErrorRetentionSeconds = 30,
            RequireKey = true,
            UnscopedKeyBehavior = UnscopedKeyBehavior.Reject,
            StoreKey = "orders-store",
        };

        attribute.Profile.Should().Be("payments");
        attribute.Mode.Should().Be(IdempotencyMode.Observe);
        attribute.RetentionSeconds.Should().Be(3600);
        attribute.LeaseSeconds.Should().Be(90);
        attribute.ServerErrorRetentionSeconds.Should().Be(30);
        attribute.RequireKey.Should().BeTrue();
        attribute.UnscopedKeyBehavior.Should().Be(UnscopedKeyBehavior.Reject);
        attribute.StoreKey.Should().Be("orders-store");
    }

    [Fact]
    public async Task Mode_Argument_Should_Turn_Idempotency_Off_For_The_Endpoint()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureEndpoints: app => app.MapPost("/attribute-off", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                .WithIdempotency()
                .WithMetadata(new IdempotentAttribute { Mode = IdempotencyMode.Off }));

        IdempotencyResponse first = await host.PostAsync("/attribute-off");
        IdempotencyResponse second = await host.PostAsync("/attribute-off");

        second.Replayed.Should().BeFalse();
        second.Body.Should().NotBe(first.Body);
        host.Probe.Executions.Should().Be(2);
    }
}
