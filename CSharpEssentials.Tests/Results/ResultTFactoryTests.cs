using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTFactoryTests
{
    private static readonly int[] ExpectedAndValues = [1, 2, 3];

    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        var result = 42.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        var result = Result<int>.Failure(TestData.Errors.Failure);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        Error[] errors = [TestData.Errors.Failure, TestData.Errors.Validation];
        var result = Result<int>.Failure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void From_WithError_ShouldCreateFailureResult()
    {
        var result = Result<int>.From(TestData.Errors.Failure);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void From_WithErrors_ShouldCreateFailureResult()
    {
        Error[] errors = [TestData.Errors.Failure];
        var result = Result<int>.From(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void And_WithAllSuccess_ShouldReturnSuccessWithValues()
    {
        var result1 = 1.ToResult();
        var result2 = 2.ToResult();
        var result3 = 3.ToResult();
#pragma warning disable IDE0008
        var combined = Result<int>.And(result1, result2, result3);
#pragma warning restore IDE0008

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(ExpectedAndValues);
    }

    [Fact]
    public void And_WithAnyFailure_ShouldReturnFailure()
    {
        var result1 = 1.ToResult();
        var result2 = Result<int>.Failure(TestData.Errors.Failure);
        var result3 = 3.ToResult();
#pragma warning disable IDE0008
        var combined = Result<int>.And(result1, result2, result3);
#pragma warning restore IDE0008

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().Contain(TestData.Errors.Failure);
    }

    [Fact]
    public void Or_WithAnySuccess_ShouldReturnFirstSuccess()
    {
        var result1 = Result<int>.Failure(TestData.Errors.Failure);
        var result2 = 42.ToResult();
        var result3 = 100.ToResult();
        var combined = Result<int>.Or(result1, result2, result3);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(42);
    }

    [Fact]
    public void Or_WithAllFailures_ShouldReturnFailure()
    {
        var result1 = Result<int>.Failure(TestData.Errors.Failure);
        var result2 = Result<int>.Failure(TestData.Errors.Validation);
        var combined = Result<int>.Or(result1, result2);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }
}
