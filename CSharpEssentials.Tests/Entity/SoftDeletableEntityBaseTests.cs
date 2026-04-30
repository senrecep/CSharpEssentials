using CSharpEssentials.Entity;
using FluentAssertions;
using static CSharpEssentials.Tests.TestData;

namespace CSharpEssentials.Tests.Entity;

public class SoftDeletableEntityBaseTests
{
    private sealed class TestSoftDeletableEntity : SoftDeletableEntityBase
    {
    }

    [Fact]
    public void IsDeleted_ShouldBeFalse_Initially()
    {
        TestSoftDeletableEntity entity = new();

        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void DeletedAt_ShouldBeNull_Initially()
    {
        TestSoftDeletableEntity entity = new();

        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void DeletedBy_ShouldBeNull_Initially()
    {
        TestSoftDeletableEntity entity = new();

        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void IsHardDeleted_ShouldBeFalse_Initially()
    {
        TestSoftDeletableEntity entity = new();

        entity.IsHardDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetDeletedProperties()
    {
        TestSoftDeletableEntity entity = new();
        DateTimeOffset deletedAt = Dates.UtcNow;
        const string deletedBy = "TestUser";

        entity.MarkAsDeleted(deletedAt, deletedBy);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(deletedAt);
        entity.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Restore_ShouldResetDeletedProperties()
    {
        TestSoftDeletableEntity entity = new();
        DateTimeOffset deletedAt = Dates.UtcNow;
        const string deletedBy = "TestUser";

        entity.MarkAsDeleted(deletedAt, deletedBy);
        entity.Restore();

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAt.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
        entity.IsHardDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkAsHardDeleted_ShouldSetIsHardDeleted()
    {
        TestSoftDeletableEntity entity = new();

        entity.MarkAsHardDeleted();

        entity.IsHardDeleted.Should().BeTrue();
    }

    [Fact]
    public void Restore_ShouldResetHardDeletedFlag()
    {
        TestSoftDeletableEntity entity = new();

        entity.MarkAsHardDeleted();
        entity.Restore();

        entity.IsHardDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkAsDeleted_ThenRestore_ThenMarkAsDeletedAgain_ShouldWork()
    {
        TestSoftDeletableEntity entity = new();
        DateTimeOffset firstDeletedAt = Dates.PastDate;
        DateTimeOffset secondDeletedAt = Dates.UtcNow;
        const string deletedBy = "TestUser";

        entity.MarkAsDeleted(firstDeletedAt, deletedBy);
        entity.Restore();
        entity.MarkAsDeleted(secondDeletedAt, deletedBy);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(secondDeletedAt);
        entity.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void SoftDeletableEntity_ShouldInheritFromEntityBase()
    {
        TestSoftDeletableEntity entity = new();
        DateTimeOffset createdAt = Dates.UtcNow;
        const string createdBy = "TestUser";

        entity.SetCreatedInfo(createdAt, createdBy);

        entity.CreatedAt.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(createdBy);
    }
}
