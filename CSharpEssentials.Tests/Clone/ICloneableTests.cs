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
}
