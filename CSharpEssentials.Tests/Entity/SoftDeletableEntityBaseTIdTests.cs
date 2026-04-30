using CSharpEssentials.Entity;
using FluentAssertions;

namespace CSharpEssentials.Tests.Entity;

public class SoftDeletableEntityBaseTIdTests
{
    private sealed class TestSoftDeletableEntityWithGuid : SoftDeletableEntityBase<Guid>
    {
        public void SetTestId(Guid id) => Id = id;
    }

    private sealed class TestSoftDeletableEntityWithInt : SoftDeletableEntityBase<int>
    {
        public void SetTestId(int id) => Id = id;
    }

    private sealed class TestSoftDeletableEntityWithString : SoftDeletableEntityBase<string>
    {
        public void SetTestId(string id) => Id = id;
    }

    [Fact]
    public void Id_ShouldBeDefaultByDefault_ForGuid()
    {
        TestSoftDeletableEntityWithGuid entity = new();

        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Id_ShouldBeDefaultByDefault_ForInt()
    {
        TestSoftDeletableEntityWithInt entity = new();

        entity.Id.Should().Be(default);
    }

    [Fact]
    public void Id_ShouldBeNullByDefault_ForString()
    {
        TestSoftDeletableEntityWithString entity = new();

        entity.Id.Should().BeNull();
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForGuid()
    {
        TestSoftDeletableEntityWithGuid entity = new();
        var expectedId = Guid.NewGuid();

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForInt()
    {
        TestSoftDeletableEntityWithInt entity = new();
        int expectedId = 42;

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void Id_ShouldBeSetCorrectly_ForString()
    {
        TestSoftDeletableEntityWithString entity = new();
        string expectedId = "test-id-123";

        entity.SetTestId(expectedId);

        entity.Id.Should().Be(expectedId);
    }

    [Fact]
    public void SoftDeletableEntityBaseTId_ShouldInheritSoftDeleteFunctionality()
    {
        TestSoftDeletableEntityWithGuid entity = new();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void MarkAsDeleted_ShouldWork()
    {
        TestSoftDeletableEntityWithGuid entity = new();
        DateTimeOffset deletedAt = DateTimeOffset.UtcNow;
        const string deletedBy = "TestUser";

        entity.MarkAsDeleted(deletedAt, deletedBy);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(deletedAt);
        entity.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Restore_ShouldWork()
    {
        TestSoftDeletableEntityWithGuid entity = new();
        entity.MarkAsDeleted(DateTimeOffset.UtcNow, "TestUser");

        entity.Restore();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void MarkAsHardDeleted_ShouldWork()
    {
        TestSoftDeletableEntityWithGuid entity = new();

        entity.MarkAsHardDeleted();

        entity.IsHardDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDeletableEntityBaseTId_ShouldInheritFromEntityBase()
    {
        TestSoftDeletableEntityWithGuid entity = new();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        const string createdBy = "TestUser";

        entity.SetCreatedInfo(createdAt, createdBy);

        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void SoftDeletableEntityBaseTId_ShouldSupportDomainEvents()
    {
        TestSoftDeletableEntityWithGuid entity = new();
        TestDomainEvent domainEvent = new() { Message = "Test" };

        entity.Raise(domainEvent);

        entity.DomainEvents.Should().HaveCount(1);
    }

    private sealed class TestDomainEvent : CSharpEssentials.Entity.Interfaces.IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }
}
