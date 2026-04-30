using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultFactoryTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        var result = Result.Failure(TestData.Errors.Failure);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        Error[] errors = new[] { TestData.Errors.Failure, TestData.Errors.Validation };
        var result = Result.Failure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void From_WithErrors_ShouldCreateFailureResult()
    {
        Error[] errors = new[] { TestData.Errors.Failure };
        var result = Result.From(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void SuccessT_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void FailureT_WithSingleError_ShouldCreateFailureResult()
    {
        var result = Result.Failure<int>(TestData.Errors.Failure);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void FailureT_WithMultipleErrors_ShouldCreateFailureResult()
    {
        Error[] errors = new[] { TestData.Errors.Failure, TestData.Errors.Validation };
        var result = Result.Failure<int>(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void FromT_WithError_ShouldCreateFailureResult()
    {
        var result = Result.From<int>(TestData.Errors.Failure);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void FromT_WithErrors_ShouldCreateFailureResult()
    {
        Error[] errors = new[] { TestData.Errors.Failure };
        var result = Result.From<int>(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }
}

