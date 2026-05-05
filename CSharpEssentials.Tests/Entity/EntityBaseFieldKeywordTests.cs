using CSharpEssentials.Entity;
using FluentAssertions;

namespace CSharpEssentials.Tests.Entity;

public sealed class EntityBaseFieldKeywordTests
{
    private sealed class TestEntity : EntityBase;

    [Fact]
    public void SetCreatedInfo_With_MinValue_Should_Throw_ArgumentOutOfRangeException()
    {
        var entity = new TestEntity();
        Action act = () => entity.SetCreatedInfo(DateTimeOffset.MinValue, "user");
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("value");
    }

    [Fact]
    public void SetCreatedInfo_With_Valid_Date_Should_Set_CreatedAt()
    {
        var entity = new TestEntity();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        entity.SetCreatedInfo(now, "user");
        entity.CreatedAt.Should().Be(now);
    }
}
