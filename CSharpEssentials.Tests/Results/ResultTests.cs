using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTests
{
    #region Success

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorsOrEmptyArray.Should().BeEmpty();
    }

    #endregion

    #region Failure

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        var error = Error.Failure("TEST", "Test error");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(error);
        result.FirstError.Should().Be(error);
        result.LastError.Should().Be(error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        Error[] errors = [Error.Failure("ERR1", "Error 1"), Error.Validation("ERR2", "Error 2")];
        var result = Result.Failure(errors);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region From

    [Fact]
    public void From_WithErrors_ShouldCreateFailureResult()
    {
        Error[] errors = [Error.Failure("TEST", "Test")];
        var result = Result.From(errors);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region And

    [Fact]
    public void And_WithAllSuccess_ShouldReturnSuccess()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var combined = Result.And(result1, result2);

        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void And_WithAnyFailure_ShouldReturnFailure()
    {
        var result1 = Result.Success();
        var result2 = Result.Failure(Error.Failure("TEST", "Error"));
        var combined = Result.And(result1, result2);

        combined.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Or

    [Fact]
    public void Or_WithAnySuccess_ShouldReturnSuccess()
    {
        var result1 = Result.Failure(Error.Failure("TEST", "Error"));
        var result2 = Result.Success();
        var combined = Result.Or(result1, result2);

        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Or_WithAllFailures_ShouldReturnFailure()
    {
        var result1 = Result.Failure(Error.Failure("ERR1", "Error 1"));
        var result2 = Result.Failure(Error.Validation("ERR2", "Error 2"));
        var combined = Result.Or(result1, result2);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        Result result = Error.Failure("TEST", "Error");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromErrorArray_ShouldCreateFailure()
    {
        Error[] errors = [Error.Failure("ERR1", "Error 1"), Error.Validation("ERR2", "Error 2")];
        Result result = errors;

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void ImplicitConversion_FromBool_ShouldWork()
    {
        Result success = true;
        Result failure = false;

        success.IsSuccess.Should().BeTrue();
        failure.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Failure(Error.Failure("TEST", "Error"));

        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }

    #endregion

    #region Serialization

    [Fact]
    public void JsonSerialization_ShouldWork()
    {
        var result = Result.Failure(Error.Failure("TEST", "Error"));
        string json = JsonSerializer.Serialize(result);
        Result deserialized = JsonSerializer.Deserialize<Result>(json);

        deserialized.IsFailure.Should().BeTrue();
        deserialized.Errors.Should().HaveCount(1);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_Success_ShouldReturnSuccess()
    {
        var result = Result.Success();

        result.ToString().Should().Be("Success");
    }

    [Fact]
    public void ToString_Failure_ShouldContainErrorInfo()
    {
        var result = Result.Failure(Error.Failure("TEST", "Test error"));

        result.ToString().Should().Contain("Failure");
    }

    #endregion
}

