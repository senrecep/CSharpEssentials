using CSharpEssentials.Clone;
using FluentAssertions;

namespace CSharpEssentials.Tests.Clone;

public class ICloneableTests
{
    private sealed class TestCloneable : ICloneable<TestCloneable>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public TestCloneable Clone()
        {
            return new TestCloneable
            {
                Id = Id,
                Name = Name
            };
        }
    }

    private sealed class NestedCloneable : ICloneable<NestedCloneable>
    {
        public int Id { get; set; }
        public TestCloneable Child { get; set; } = new();

        public NestedCloneable Clone()
        {
            return new NestedCloneable
            {
                Id = Id,
                Child = Child.Clone()
            };
        }
    }

    [Fact]
    public void Clone_ShouldCreateNewInstance()
    {
        TestCloneable original = new() { Id = 1, Name = "Test" };

        TestCloneable cloned = original.Clone();

        cloned.Should().NotBeSameAs(original);
        cloned.Id.Should().Be(original.Id);
        cloned.Name.Should().Be(original.Name);
    }

    [Fact]
    public void Clone_ShouldCreateIndependentCopy()
    {
        TestCloneable original = new() { Id = 1, Name = "Test" };
        TestCloneable cloned = original.Clone();

        cloned.Name = "Modified";

        original.Name.Should().Be("Test");
        cloned.Name.Should().Be("Modified");
    }

    [Fact]
    public void Clone_WithNestedProperties_ShouldCreateDeepCopy()
    {
        NestedCloneable original = new()
        {
            Id = 1,
            Child = new TestCloneable { Id = 10, Name = "Child" }
        };

        NestedCloneable cloned = original.Clone();

        cloned.Should().NotBeSameAs(original);
        cloned.Child.Should().NotBeSameAs(original.Child);
        cloned.Child.Id.Should().Be(original.Child.Id);
        cloned.Child.Name.Should().Be(original.Child.Name);
    }

    [Fact]
    public void Clone_WithNestedProperties_ShouldBeIndependent()
    {
        NestedCloneable original = new()
        {
            Id = 1,
            Child = new TestCloneable { Id = 10, Name = "Child" }
        };

        NestedCloneable cloned = original.Clone();
        cloned.Child.Name = "ModifiedChild";

        original.Child.Name.Should().Be("Child");
        cloned.Child.Name.Should().Be("ModifiedChild");
    }

    [Fact]
    public void Clone_WithNullNestedProperty_ShouldHandleGracefully()
    {
        NestedCloneable original = new()
        {
            Id = 1,
            Child = null!
        };

        Action act = () => original.Clone();

        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Clone_WithComplexHierarchy_ShouldCloneAllLevels()
    {
        var original = new DeepHierarchy
        {
            Level = 1,
            Child = new DeepHierarchy
            {
                Level = 2,
                Child = new DeepHierarchy
                {
                    Level = 3,
                    Child = null!
                }
            }
        };

        DeepHierarchy cloned = original.Clone();

        cloned.Should().NotBeSameAs(original);
        cloned.Level.Should().Be(1);
        cloned.Child.Should().NotBeSameAs(original.Child);
        cloned.Child!.Level.Should().Be(2);
        cloned.Child.Child.Should().NotBeSameAs(original.Child.Child);
        cloned.Child.Child!.Level.Should().Be(3);
    }

    private sealed class DeepHierarchy : ICloneable<DeepHierarchy>
    {
        public int Level { get; set; }
        public DeepHierarchy? Child { get; set; }

        public DeepHierarchy Clone()
        {
            return new DeepHierarchy
            {
                Level = Level,
                Child = Child?.Clone()
            };
        }
    }
}
