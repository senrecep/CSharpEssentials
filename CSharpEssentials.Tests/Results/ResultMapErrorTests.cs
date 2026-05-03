using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMapErrorTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error MappedError = Error.Failure("Mapped.Code", "Mapped message");

    #region Result.MapError

    [Fact]
    public void Result_MapError_ArrayMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result mapped = result.MapError(errors => new[] { MappedError });

        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_MapError_ArrayMapper_WithFailure_ShouldMapErrors()
    {
        var result = Result.Failure(TestError);

        Result mapped = result.MapError(errors => new[] { MappedError });

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Should().ContainSingle().Which.Should().Be(MappedError);
    }

    [Fact]
    public void Result_MapError_FirstErrorMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result mapped = result.MapError(error => MappedError);

        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_MapError_FirstErrorMapper_WithFailure_ShouldMapFirstError()
    {
        var result = Result.Failure(TestError);

        Result mapped = result.MapError(error => MappedError);

        mapped.IsFailure.Should().BeTrue();
        mapped.FirstError.Should().Be(MappedError);
    }

    #endregion

    #region Result<T>.MapError

    [Fact]
    public void ResultT_MapError_ArrayMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> mapped = result.MapError(errors => new[] { MappedError });

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_MapError_ArrayMapper_WithFailure_ShouldMapErrors()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> mapped = result.MapError(errors => new[] { MappedError });

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Should().ContainSingle().Which.Should().Be(MappedError);
    }

    [Fact]
    public void ResultT_MapError_FirstErrorMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result<int>.Success(42);

        Result<int> mapped = result.MapError(error => MappedError);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_MapError_FirstErrorMapper_WithFailure_ShouldMapFirstError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> mapped = result.MapError(error => MappedError);

        mapped.IsFailure.Should().BeTrue();
        mapped.FirstError.Should().Be(MappedError);
    }

    #endregion
}
