using CSharpEssentials.Entity.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Entity;

public sealed class DomainEventTests
{
    private sealed record OrderCreated(Guid OrderId) : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }

    [Fact]
    public void DomainEvent_Should_Set_OccurredOn_To_UtcNow_By_Default()
    {
        var before = DateTimeOffset.UtcNow.AddMilliseconds(-100);
        var evt = new OrderCreated(Guid.NewGuid());
        var after = DateTimeOffset.UtcNow.AddMilliseconds(100);

        evt.OccurredOn.Should().BeOnOrAfter(before);
        evt.OccurredOn.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void DomainEvent_Should_Allow_Custom_OccurredOn()
    {
        var custom = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var evt = new OrderCreated(Guid.NewGuid()) with { OccurredOn = custom };

        evt.OccurredOn.Should().Be(custom);
    }

    [Fact]
    public void DomainEvent_Should_Implement_IDomainEvent()
    {
        var evt = new OrderCreated(Guid.NewGuid());
        evt.Should().BeAssignableTo<IDomainEvent>();
    }
}
