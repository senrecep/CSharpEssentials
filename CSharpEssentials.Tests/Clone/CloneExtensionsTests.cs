using CSharpEssentials.Clone;
using FluentAssertions;

namespace CSharpEssentials.Tests.Clone;

public class CloneExtensionsTests
{
    private sealed class TestCloneable : ICloneable<TestCloneable>
    {
        public int Value { get; init; }
        public string Name { get; init; } = string.Empty;

        public TestCloneable Clone() => new()
        {
            Value = Value,
            Name = Name
        };
    }

    [Fact]
    public void Clone_IEnumerable_ShouldCloneAllItems()
    {
        List<TestCloneable> source =
        [
            new() { Value = 1, Name = "Item1" },
            new() { Value = 2, Name = "Item2" },
            new() { Value = 3, Name = "Item3" }
        ];

        var cloned = source.Clone().ToList();

        cloned.Should().HaveCount(3);
        cloned[0].Value.Should().Be(1);
        cloned[0].Name.Should().Be("Item1");
        cloned[1].Value.Should().Be(2);
        cloned[2].Value.Should().Be(3);
    }

    [Fact]
    public void Clone_IEnumerable_ShouldCreateNewInstances()
    {
        List<TestCloneable> source =
        [
            new() { Value = 1, Name = "Item1" },
            new() { Value = 2, Name = "Item2" }
        ];

        var cloned = source.Clone().ToList();

        cloned[0].Should().NotBeSameAs(source[0]);
        cloned[1].Should().NotBeSameAs(source[1]);
    }

    [Fact]
    public void Clone_IEnumerable_WithEmptyCollection_ShouldReturnEmpty()
    {
        List<TestCloneable> source = [];

        var cloned = source.Clone().ToList();

        cloned.Should().BeEmpty();
    }

    [Fact]
    public void Clone_IQueryable_ShouldCloneAllItems()
    {
        List<TestCloneable> source =
        [
            new() { Value = 10, Name = "Query1" },
            new() { Value = 20, Name = "Query2" }
        ];

        var cloned = source.AsQueryable().Clone().ToList();

        cloned.Should().HaveCount(2);
        cloned[0].Value.Should().Be(10);
        cloned[0].Name.Should().Be("Query1");
        cloned[1].Value.Should().Be(20);
        cloned[1].Name.Should().Be("Query2");
    }

    [Fact]
    public void Clone_IQueryable_ShouldCreateNewInstances()
    {
        List<TestCloneable> source =
        [
            new() { Value = 1, Name = "Item1" }
        ];

        var cloned = source.AsQueryable().Clone().ToList();

        cloned[0].Should().NotBeSameAs(source[0]);
    }

    [Fact]
    public void Clone_IQueryable_WithEmptyCollection_ShouldReturnEmpty()
    {
        List<TestCloneable> source = [];

        var cloned = source.AsQueryable().Clone().ToList();

        cloned.Should().BeEmpty();
    }

    [Fact]
    public void Clone_IQueryable_ShouldReturnIQueryable()
    {
        List<TestCloneable> source =
        [
            new() { Value = 1, Name = "Item1" },
            new() { Value = 2, Name = "Item2" }
        ];

        IQueryable<TestCloneable> cloned = source.AsQueryable().Clone();

        cloned.Should().BeAssignableTo<IQueryable<TestCloneable>>();
    }

    [Fact]
    public void Clone_IEnumerable_ShouldPreserveOrder()
    {
        List<TestCloneable> source =
        [
            new() { Value = 3, Name = "Third" },
            new() { Value = 1, Name = "First" },
            new() { Value = 2, Name = "Second" }
        ];

        var cloned = source.Clone().ToList();

        cloned[0].Value.Should().Be(3);
        cloned[1].Value.Should().Be(1);
        cloned[2].Value.Should().Be(2);
    }
}

