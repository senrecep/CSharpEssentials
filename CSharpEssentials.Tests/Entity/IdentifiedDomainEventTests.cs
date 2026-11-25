using System.Text.Json;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.Entity;

public sealed class IdentifiedDomainEventTests
{
    public sealed record OrderPlaced(Guid OrderId) : DomainEvent;

    public sealed record OrderShipped : DomainEvent
    {
        public OrderShipped(Guid orderId, TimeProvider clock)
            : base(clock) => OrderId = orderId;

        public Guid OrderId { get; init; }
    }

    [Fact]
    public void Fake_clock_stamps_OccurredAt_and_a_version_7_id_with_that_time()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero));

        var evt = new OrderShipped(Guid.NewGuid(), clock);

        evt.OccurredAt.Should().Be(clock.GetUtcNow());
        evt.EventId.Version.Should().Be(7);
        long unixMs = Convert.ToInt64(evt.EventId.ToString("N")[..12], 16);
        unixMs.Should().Be(clock.GetUtcNow().ToUnixTimeMilliseconds());
    }

    [Fact]
    public void Default_constructor_uses_the_system_clock_and_unique_ids()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;
        var first = new OrderPlaced(Guid.NewGuid());
        var second = new OrderPlaced(Guid.NewGuid());

        first.OccurredAt.Should().BeOnOrAfter(before.AddMilliseconds(-1));
        first.EventId.Should().NotBe(second.EventId);
        first.Should().BeAssignableTo<IIdentifiedDomainEvent>().And.BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void Json_round_trip_keeps_the_id_and_time()
    {
        var evt = new OrderPlaced(Guid.NewGuid());

        OrderPlaced? back = JsonSerializer.Deserialize<OrderPlaced>(JsonSerializer.Serialize(evt));

        back.Should().Be(evt);
    }

    [Fact]
    public void Null_time_provider_throws()
    {
        Action act = () => _ = new OrderShipped(Guid.NewGuid(), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("timeProvider");
    }
}
