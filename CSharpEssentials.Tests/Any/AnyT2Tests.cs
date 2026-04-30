using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyT2Tests
{
    [Fact]
    public void Create_WithAllTypes_ShouldWork()
    {
        var any1 = CSharpEssentials.Any.Any.Create<int, string, bool>(42);
        var any2 = CSharpEssentials.Any.Any.Create<int, string, bool>("test");
        var any3 = CSharpEssentials.Any.Any.Create<int, string, bool>(true);

        any1.Index.Should().Be(0);
        any2.Index.Should().Be(1);
        any3.Index.Should().Be(2);
    }

    [Fact]
    public void Switch_WithAllTypes_ShouldWork()
    {
        var any1 = CSharpEssentials.Any.Any.Create<int, string, bool>(42);
        var any2 = CSharpEssentials.Any.Any.Create<int, string, bool>("test");
        var any3 = CSharpEssentials.Any.Any.Create<int, string, bool>(true);

        bool executed1 = false;
        bool executed2 = false;
        bool executed3 = false;

        any1.Switch(first: _ => executed1 = true);
        any2.Switch(second: _ => executed2 = true);
        any3.Switch(third: _ => executed3 = true);

        executed1.Should().BeTrue();
        executed2.Should().BeTrue();
        executed3.Should().BeTrue();
    }

    [Fact]
    public void Match_WithAllTypes_ShouldWork()
    {
        var any1 = CSharpEssentials.Any.Any.Create<int, string, bool>(42);
        var any2 = CSharpEssentials.Any.Any.Create<int, string, bool>("test");
        var any3 = CSharpEssentials.Any.Any.Create<int, string, bool>(true);

        AnyActionResult<int> result1 = any1.Match(first: x => x * 2);
        AnyActionResult<int> result2 = any2.Match(second: x => x.Length);
        AnyActionResult<int> result3 = any3.Match(third: x => x ? 1 : 0);

        result1.Result.Should().Be(84);
        result2.Result.Should().Be(4);
        result3.Result.Should().Be(1);
    }
}

