#if NET10_0_OR_GREATER
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class EnumerableExtensionMembersTests
{
    [Fact]
    public void IsEmpty_Should_Be_True_For_Empty_Collection()
    {
        int[] arr = [];
        arr.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_Should_Be_False_For_NonEmpty_Collection()
    {
        int[] arr = [1, 2, 3];
        arr.IsEmpty.Should().BeFalse();
    }
}
#endif
