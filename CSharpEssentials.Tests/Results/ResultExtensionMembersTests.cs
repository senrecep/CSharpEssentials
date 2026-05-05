#if NET10_0_OR_GREATER
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public sealed class ResultExtensionMembersTests
{
    [Fact]
    public void ValueOrDefault_Should_Return_Value_When_Success()
    {
        var result = 42;
        result.ValueOrDefault.Should().Be(42);
    }

    [Fact]
    public void ValueOrDefault_Should_Return_Default_When_Failure()
    {
        var result = Result<int>.Failure(Error.Failure("E1"));
        result.ValueOrDefault.Should().Be(0);
    }

    [Fact]
    public void ValueOrDefault_Should_Return_Null_For_Reference_Type_When_Failure()
    {
        var result = Result<string>.Failure(Error.Failure("E1"));
        result.ValueOrDefault.Should().BeNull();
    }

    [Fact]
    public void OrOperator_Should_Return_Left_When_Success()
    {
        var left = 1;
        var right = 2;
        (left | right).Value.Should().Be(1);
    }

    [Fact]
    public void OrOperator_Should_Return_Right_When_Left_Fails()
    {
        var left = Result<int>.Failure(Error.Failure("E1"));
        var right = 2;
        (left | right).Value.Should().Be(2);
    }

    [Fact]
    public void OrOperator_Should_Return_Right_When_Both_Fail()
    {
        var left = Result<int>.Failure(Error.Failure("E1"));
        var right = Result<int>.Failure(Error.Failure("E2"));
        (left | right).IsFailure.Should().BeTrue();
    }
}
#endif
