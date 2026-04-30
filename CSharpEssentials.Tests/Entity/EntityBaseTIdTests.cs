using CSharpEssentials.Entity;
using FluentAssertions;

namespace CSharpEssentials.Tests.Entity;

public class EntityBaseTIdTests
{
    private sealed class TestEntityWithGuid : EntityBase<Guid>
    {
        public void SetTestId(Guid id) => Id = id;
    }

    private sealed class TestEntityWithInt : EntityBase<int>
    {
        public void SetTestId(int id) => Id = id;
    }

    private sealed class TestEntityWithString : EntityBase<string>
    {
        public void SetTestId(string id) => Id = id;
    }

    [Fact]
    public void Id_ShouldBeDefaultByDefault_ForGuid()
    {
        TestEntityWithGuid entity = new();

        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Id_ShouldBeDefaultByDefault_ForInt()
    {
        TestEntityWithInt entity = new();

        entity.Id.Should().Be(default);
    }

    [Fact]
    public void Id_ShouldBeNullByDefault_ForString()
    {
        TestEntityWithString entity = new();

        entity.Id.Should().BeNull();
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForGuid()
    {
        TestEntityWithGuid entity = new();
        var expectedId = Guid.NewGuid();

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForInt()
    {
        TestEntityWithInt entity = new();
        int expectedId = 42;

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForString()
    {
        TestEntityWithString entity = new();
        string expectedId = "test-id-123";

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void EntityBaseTId_ShouldInheritFromEntityBase()
    {
        TestEntityWithGuid entity = new();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        const string createdBy = "TestUser";

        entity.SetCreatedInfo(createdAt, createdBy);

        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void EntityBaseTId_ShouldSupportDomainEvents()
    {
        TestEntityWithGuid entity = new();
        TestDomainEvent domainEvent = new() { Message = "Test" };

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents[0].Should().Be(domainEvent);
    }

    [Fact]
    public void EntityBaseTId_ShouldSupportUpdatedInfo()
    {
        TestEntityWithGuid entity = new();
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
        const string updatedBy = "UpdateUser";

        entity.SetUpdatedInfo(updatedAt, updatedBy);

        entity.UpdatedAt.Should().Be(updatedAt);
        entity.UpdatedBy.Should().Be(updatedBy);
    }

    private sealed class TestDomainEvent : CSharpEssentials.Entity.Interfaces.IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }
}
