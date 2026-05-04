#if NET10_0_OR_GREATER
using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public sealed class MaybeExtensionMembersTests
{
    [Fact]
    public void IsNone_Should_Be_True_When_Empty()
    {
        var maybe = Maybe<int>.None;
        maybe.IsNone.Should().BeTrue();
    }

    [Fact]
    public void IsNone_Should_Be_False_When_Has_Value()
    {
        var maybe = Maybe<int>.From(42);
        maybe.IsNone.Should().BeFalse();
    }
}
#endif
