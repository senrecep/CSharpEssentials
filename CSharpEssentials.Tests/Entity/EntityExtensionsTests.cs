using CSharpEssentials.Entity;
using FluentAssertions;

namespace CSharpEssentials.Tests.Entity;

public class EntityExtensionsTests
{
    private sealed class TestSoftDeletableEntity : SoftDeletableEntityBase
    {
    }

    [Fact]
    public void HardDelete_WithSingleEntity_ShouldMarkAsHardDeleted()
    {
        TestSoftDeletableEntity entity = new();
        TestSoftDeletableEntity[] entities = [entity];

        entities.HardDelete();

        entity.IsHardDeleted.Should().BeTrue();
    }

    [Fact]
    public void HardDelete_WithMultipleEntities_ShouldMarkAllAsHardDeleted()
    {
        TestSoftDeletableEntity entity1 = new();
        TestSoftDeletableEntity entity2 = new();
        TestSoftDeletableEntity entity3 = new();
        TestSoftDeletableEntity[] entities = [entity1, entity2, entity3];

        entities.HardDelete();

        entity1.IsHardDeleted.Should().BeTrue();
        entity2.IsHardDeleted.Should().BeTrue();
        entity3.IsHardDeleted.Should().BeTrue();
    }

    [Fact]
    public void HardDelete_WithEmptyCollection_ShouldNotThrow()
    {
        TestSoftDeletableEntity[] entities = [];

        Action action = () => entities.HardDelete();

        action.Should().NotThrow();
    }

    [Fact]
    public void HardDelete_WithList_ShouldWork()
    {
        TestSoftDeletableEntity entity1 = new();
        TestSoftDeletableEntity entity2 = new();
        List<TestSoftDeletableEntity> entities = [entity1, entity2];

        entities.HardDelete();

        entity1.IsHardDeleted.Should().BeTrue();
        entity2.IsHardDeleted.Should().BeTrue();
    }
}
