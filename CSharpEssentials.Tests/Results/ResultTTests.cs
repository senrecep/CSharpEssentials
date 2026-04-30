using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using System.Text.Json;

namespace CSharpEssentials.Tests.Results;

public class ResultTTests
{
    private static readonly int[] ExpectedAndValues = [1, 2, 3];

    #region Success

    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Success_WithReferenceType_ShouldCreateSuccessfulResult()
    {
        var result = Result<string>.Success("test");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    #endregion

    #region Failure

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        var result = Result<int>.Failure(Error.Failure("TEST", "Error"));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        Error[] errors = [Error.Failure("ERR1", "Error 1"), Error.Validation("ERR2", "Error 2")];
        var result = Result<int>.Failure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        Result<int> result = Error.Failure("TEST", "Error");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromErrorArray_ShouldCreateFailure()
    {
        Error[] errors = [Error.Failure("ERR1", "Error 1")];
        Result<int> result = errors;

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region And

    [Fact]
    public void And_WithAllSuccess_ShouldReturnSuccessWithValues()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);
#pragma warning disable IDE0008
        var combined = Result<int>.And(result1, result2, result3);
#pragma warning restore IDE0008

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(ExpectedAndValues);
    }

    [Fact]
    public void And_WithAnyFailure_ShouldReturnFailure()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Failure(Error.Failure("TEST", "Error"));
        var result3 = Result<int>.Success(3);
#pragma warning disable IDE0008
        var combined = Result<int>.And(result1, result2, result3);
#pragma warning restore IDE0008

        combined.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Or

    [Fact]
    public void Or_WithAnySuccess_ShouldReturnFirstSuccess()
    {
        var result1 = Result<int>.Failure(Error.Failure("TEST", "Error"));
        var result2 = Result<int>.Success(42);
        var result3 = Result<int>.Success(100);
#pragma warning disable IDE0008
        var combined = Result<int>.Or(result1, result2, result3);
#pragma warning restore IDE0008

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(42);
    }

    [Fact]
    public void Or_WithAllFailures_ShouldReturnFailure()
    {
        var result1 = Result<int>.Failure(Error.Failure("ERR1", "Error 1"));
        var result2 = Result<int>.Failure(Error.Validation("ERR2", "Error 2"));
#pragma warning disable IDE0008
        var combined = Result<int>.Or(result1, result2);
#pragma warning restore IDE0008

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Value Access

    [Fact]
    public void Value_OnFailure_ShouldReturnDefault()
    {
        var result = Result<int>.Failure(Error.Failure("TEST", "Error"));

        // Value returns default when result is failure
        result.Value.Should().Be(default);
    }

    [Fact]
    public void Value_OnSuccess_ShouldReturnValue()
    {
        var result = Result<int>.Success(42);

        result.Value.Should().Be(42);
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_SameSuccessValues_ShouldBeEqual()
    {
        var result1 = Result<int>.Success(42);
        var result2 = Result<int>.Success(42);

        result1.Should().Be(result2);
    }

    [Fact]
    public void Equality_DifferentSuccessValues_ShouldNotBeEqual()
    {
        var result1 = Result<int>.Success(42);
        var result2 = Result<int>.Success(43);

        result1.Should().NotBe(result2);
    }

    #endregion

    #region Serialization

    [Fact]
    public void JsonSerialization_Success_ShouldWork()
    {
        var result = Result<int>.Success(42);
        string json = JsonSerializer.Serialize(result);
        Result<int> deserialized = JsonSerializer.Deserialize<Result<int>>(json);

        deserialized.IsSuccess.Should().BeTrue();
        deserialized.Value.Should().Be(42);
    }

    [Fact]
    public void JsonSerialization_Failure_ShouldWork()
    {
        var result = Result<int>.Failure(Error.Failure("TEST", "Error"));
        string json = JsonSerializer.Serialize(result);
        Result<int> deserialized = JsonSerializer.Deserialize<Result<int>>(json);

        deserialized.IsFailure.Should().BeTrue();
    }

    #endregion
}
