using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMapErrorTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");
    private static readonly Error MappedError = Error.Failure("Mapped.Code", "Mapped message");
    private static readonly Error[] ThreeErrors =
    [
        Error.Validation("First.Code", "First"),
        Error.NotFound("Second.Code", "Second"),
        Error.Conflict("Third.Code", "Third")
    ];

    private static Error Prefix(Error error) => Error.Failure($"Mapped.{error.Code}", error.Description);

    private static readonly string[] MappedCodes = ["Mapped.First.Code", "Mapped.Second.Code", "Mapped.Third.Code"];

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
    public void Result_MapError_ErrorMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = Result.Success();

        Result mapped = result.MapError(error => MappedError);

        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_MapError_ErrorMapper_WithFailure_ShouldMapSingleError()
    {
        var result = Result.Failure(TestError);

        Result mapped = result.MapError(error => MappedError);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Should().ContainSingle().Which.Should().Be(MappedError);
    }

    [Fact]
    public void Result_MapError_ErrorMapper_WithMultipleErrors_ShouldMapEveryErrorInOrder()
    {
        var result = Result.Failure(ThreeErrors);

        Result mapped = result.MapError(Prefix);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
    }

    [Fact]
    public void Result_MapError_ErrorMapper_WithSuccess_ShouldNotInvokeMapper()
    {
        var result = Result.Success();
        int calls = 0;

        result.MapError(error => { calls++; return MappedError; });

        calls.Should().Be(0);
    }

    #endregion

    #region Result<T>.MapError

    [Fact]
    public void ResultT_MapError_ArrayMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

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
    public void ResultT_MapError_ErrorMapper_WithSuccess_ShouldReturnOriginal()
    {
        var result = 42.ToResult();

        Result<int> mapped = result.MapError(error => MappedError);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void ResultT_MapError_ErrorMapper_WithFailure_ShouldMapSingleError()
    {
        var result = Result<int>.Failure(TestError);

        Result<int> mapped = result.MapError(error => MappedError);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Should().ContainSingle().Which.Should().Be(MappedError);
    }

    [Fact]
    public void ResultT_MapError_ErrorMapper_WithMultipleErrors_ShouldMapEveryErrorInOrder()
    {
        var result = Result<int>.Failure(ThreeErrors);

        Result<int> mapped = result.MapError(Prefix);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
    }

    [Fact]
    public void ResultT_MapError_ErrorMapper_WithSuccess_ShouldNotInvokeMapper()
    {
        var result = 42.ToResult();
        int calls = 0;

        result.MapError(error => { calls++; return MappedError; });

        calls.Should().Be(0);
    }

    #endregion
}
