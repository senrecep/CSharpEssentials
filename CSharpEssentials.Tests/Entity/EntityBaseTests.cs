using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using FluentAssertions;
using static CSharpEssentials.Tests.TestData;

namespace CSharpEssentials.Tests.Entity;

public class EntityBaseTests
{
    private sealed class TestEntity : EntityBase
    {
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    [Fact]
    public void SetCreatedInfo_ShouldSetCreatedAtAndCreatedBy()
    {
        TestEntity entity = new();
        DateTimeOffset createdAt = Dates.UtcNow;
        const string createdBy = "TestUser";

        entity.SetCreatedInfo(createdAt, createdBy);

        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void SetUpdatedInfo_ShouldSetUpdatedAtAndUpdatedBy()
    {
        TestEntity entity = new();
        DateTimeOffset updatedAt = Dates.UtcNow;
        const string updatedBy = "TestUser";

        entity.SetUpdatedInfo(updatedAt, updatedBy);

        entity.UpdatedAt.Should().Be(updatedAt);
        entity.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Raise_WithNull_ShouldThrowArgumentNullException()
    {
        TestEntity entity = new();

        Action act = () => entity.Raise(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("domainEvent");
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Raise_ShouldAddDomainEvent()
    {
        TestEntity entity = new();
        TestDomainEvent domainEvent = new() { Message = "Test" };

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents[0].Should().Be(domainEvent);
    }

    [Fact]
    public void Raise_WithMultipleEvents_ShouldAddAllEvents()
    {
        TestEntity entity = new();
        TestDomainEvent event1 = new() { Message = "Event1" };
        TestDomainEvent event2 = new() { Message = "Event2" };
        TestDomainEvent event3 = new() { Message = "Event3" };

        entity.Raise(event1);
        entity.Raise(event2);
        entity.Raise(event3);

        entity.DomainEvents.Should().HaveCount(3);
        entity.DomainEvents.Should().Contain(event1);
        entity.DomainEvents.Should().Contain(event2);
        entity.DomainEvents.Should().Contain(event3);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        TestEntity entity = new();
        TestDomainEvent event1 = new() { Message = "Event1" };
        TestDomainEvent event2 = new() { Message = "Event2" };

        entity.Raise(event1);
        entity.Raise(event2);
        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyList()
    {
        TestEntity entity = new();
        TestDomainEvent domainEvent = new() { Message = "Test" };

        entity.Raise(domainEvent);
        IReadOnlyList<IDomainEvent> events = entity.DomainEvents;

        events.Should().HaveCount(1);
        events.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    [Fact]
    public void DomainEvents_ShouldNotAllowDirectModification()
    {
        TestEntity entity = new();
        TestDomainEvent domainEvent = new() { Message = "Test" };

        entity.Raise(domainEvent);
        IReadOnlyList<IDomainEvent> events = entity.DomainEvents;

        events.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<IDomainEvent>>();
        ((ICollection<IDomainEvent>)events).IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_ShouldBeLiveView_ThatReflectsLaterRaiseAndClear()
    {
        TestEntity entity = new();
        TestDomainEvent first = new() { Message = "First" };
        TestDomainEvent second = new() { Message = "Second" };
        entity.Raise(first);

        IReadOnlyList<IDomainEvent> events = entity.DomainEvents;
        entity.Raise(second);

        events.Should().Equal(first, second);
        entity.ClearDomainEvents();
        events.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnSameInstance_OnEveryRead()
    {
        TestEntity entity = new();

        entity.DomainEvents.Should().BeSameAs(entity.DomainEvents);
    }

    [Fact]
    public void CreatedAt_ShouldBeDefaultValue_Initially()
    {
        TestEntity entity = new();

        entity.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void UpdatedAt_ShouldBeNull_Initially()
    {
        TestEntity entity = new();

        entity.UpdatedAt.Should().BeNull();
    }
}
