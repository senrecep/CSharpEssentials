using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTryGetTests
{
    [Fact]
    public void TryGet_WithSuccessResult_ShouldReturnTrueAndValue()
    {
        Result<int> result = 42;

        bool success = result.TryGet(out int value, out Error[]? errors);

        success.Should().BeTrue();
        value.Should().Be(42);
        errors.Should().BeNull();
    }

    [Fact]
    public void TryGet_WithFailureResult_ShouldReturnFalseAndErrors()
    {
        var error = Error.Failure("Test.Code", "Test error");
        Result<int> result = error;

        bool success = result.TryGet(out int value, out Error[]? errors);

        success.Should().BeFalse();
        value.Should().Be(0);
        errors.Should().NotBeNull();
        errors!.Should().ContainSingle();
        errors[0].Code.Should().Be("Test.Code");
    }

    [Fact]
    public void TryGet_NonGeneric_WithSuccessResult_ShouldReturnTrueAndNullErrors()
    {
        var result = Result.Success();

        bool success = result.TryGet(out Error[]? errors);

        success.Should().BeTrue();
        errors.Should().BeNull();
    }

    [Fact]
    public void TryGet_NonGeneric_WithFailureResult_ShouldReturnFalseAndErrors()
    {
        var error = Error.Failure("Test.Code", "Test error");
        Result result = error;

        bool success = result.TryGet(out Error[]? errors);

        success.Should().BeFalse();
        errors.Should().NotBeNull();
        errors!.Should().ContainSingle();
        errors[0].Code.Should().Be("Test.Code");
    }
}
