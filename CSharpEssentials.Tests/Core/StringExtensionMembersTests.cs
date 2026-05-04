#if NET10_0_OR_GREATER
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class StringExtensionMembersTests
{
    [Fact]
    public void IsPalindrome_Should_Be_True_For_Empty()
    {
        string s = string.Empty;
        s.IsPalindrome.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_Should_Be_True_For_Single_Char()
    {
        string s = "a";
        s.IsPalindrome.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_Should_Be_True_For_Radar()
    {
        string s = "radar";
        s.IsPalindrome.Should().BeTrue();
    }

    [Fact]
    public void IsPalindrome_Should_Be_False_For_Hello()
    {
        string s = "hello";
        s.IsPalindrome.Should().BeFalse();
    }
}
#endif
