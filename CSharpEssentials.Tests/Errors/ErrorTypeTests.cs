using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_Values_ShouldBeCorrect()
    {
        ErrorType.Failure.Should().Be((ErrorType)0);
        ErrorType.Unexpected.Should().Be((ErrorType)1);
        ErrorType.Validation.Should().Be((ErrorType)2);
        ErrorType.Conflict.Should().Be((ErrorType)3);
        ErrorType.NotFound.Should().Be((ErrorType)4);
        ErrorType.Unauthorized.Should().Be((ErrorType)5);
        ErrorType.Forbidden.Should().Be((ErrorType)6);
        ErrorType.Unknown.Should().Be((ErrorType)7);
    }

    [Fact]
    public void ErrorType_ToIntType_ShouldConvertCorrectly()
    {
        ErrorType.Failure.ToIntType().Should().Be(0);
        ErrorType.Unexpected.ToIntType().Should().Be(1);
        ErrorType.Validation.ToIntType().Should().Be(2);
        ErrorType.Conflict.ToIntType().Should().Be(3);
        ErrorType.NotFound.ToIntType().Should().Be(4);
        ErrorType.Unauthorized.ToIntType().Should().Be(5);
        ErrorType.Forbidden.ToIntType().Should().Be(6);
        ErrorType.Unknown.ToIntType().Should().Be(7);
    }
}

