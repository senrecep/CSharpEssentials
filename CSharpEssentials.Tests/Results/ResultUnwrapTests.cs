using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultUnwrapTests
{
    [Fact]
    public void Unwrap_SuccessResult_ShouldReturnValue()
    {
        Result<int> result = 42;

        int value = result.Unwrap();

        value.Should().Be(42);
    }

    [Fact]
    public void Unwrap_FailureResult_ShouldThrowResultUnwrapException()
    {
        var error = Error.Failure("Test.Code", "Test error");
        Result<int> result = error;

        Action act = () => result.Unwrap();

        act.Should().Throw<ResultUnwrapException>()
            .Where(ex => ex.Errors.Length == 1 && ex.Errors[0].Code == "Test.Code");
    }

    [Fact]
    public void UnwrapOrDefault_SuccessResult_ShouldReturnValue()
    {
        Result<int> result = 42;

        int value = result.UnwrapOrDefault(0);

        value.Should().Be(42);
    }

    [Fact]
    public void UnwrapOrDefault_FailureResult_ShouldReturnDefault()
    {
        Result<int> result = Error.Failure("Test.Code", "Test error");

        int value = result.UnwrapOrDefault(99);

        value.Should().Be(99);
    }
}
