using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultRecoverTests
{
    [Fact]
    public void Recover_ErrorTypeMatch_ShouldReturnRecoveredResult()
    {
        Result<int> result = Error.NotFound("Test.NotFound", "Not found");

        Result<int> recovered = result.Recover(ErrorType.NotFound, _ => 42);

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_ErrorTypeMismatch_ShouldReturnOriginalFailure()
    {
        Result<int> result = Error.Validation("Test.Validation", "Invalid");

        Result<int> recovered = result.Recover(ErrorType.NotFound, _ => 42);

        recovered.IsFailure.Should().BeTrue();
        recovered.FirstError.Code.Should().Be("Test.Validation");
    }

    [Fact]
    public void Recover_SuccessResult_ShouldReturnOriginalSuccess()
    {
        Result<int> result = 100;

        Result<int> recovered = result.Recover(ErrorType.NotFound, _ => 42);

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(100);
    }

    [Fact]
    public void Recover_WithResultRecovery_ShouldReturnRecoveredResult()
    {
        Result<int> result = Error.NotFound("Test.NotFound", "Not found");

        Result<int> recovered = result.Recover(ErrorType.NotFound, _ => Result.Success(42));

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void RecoverFirst_ErrorTypeMatch_ShouldReturnRecoveredResult()
    {
        Result<int> result = Error.NotFound("Test.NotFound", "Not found");

        Result<int> recovered = result.RecoverFirst(ErrorType.NotFound, _ => 42);

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void RecoverFirst_ErrorTypeMismatch_ShouldReturnOriginalFailure()
    {
        Result<int> result = Error.Validation("Test.Validation", "Invalid");

        Result<int> recovered = result.RecoverFirst(ErrorType.NotFound, _ => 42);

        recovered.IsFailure.Should().BeTrue();
        recovered.FirstError.Code.Should().Be("Test.Validation");
    }

    [Fact]
    public void Recover_PredicateMatch_ShouldReturnRecoveredResult()
    {
        Result<int> result = Error.NotFound("Test.NotFound", "Not found");

        Result<int> recovered = result.Recover(e => e.Type == ErrorType.NotFound, _ => 42);

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_PredicateMismatch_ShouldReturnOriginalFailure()
    {
        Result<int> result = Error.Validation("Test.Validation", "Invalid");

        Result<int> recovered = result.Recover(e => e.Type == ErrorType.NotFound, _ => 42);

        recovered.IsFailure.Should().BeTrue();
        recovered.FirstError.Code.Should().Be("Test.Validation");
    }
}
