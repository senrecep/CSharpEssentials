using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Testing;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results.Testing;

public class ResultAssertionExtensionsTests
{
    private static readonly Error NameRequired = Error.Validation("Name.Required", "Name is required.", new ErrorMetadata { ["field"] = "Name" });
    private static readonly Error UserMissing = Error.NotFound("User.NotFound", "User 7 was not found.");

    [Fact]
    public void ShouldBeSuccess_Should_Pass_When_Result_Is_Success()
    {
        Action act = () => Result.Success().ShouldBeSuccess();

        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeSuccess_Should_List_Every_Error_When_Result_Is_Failure()
    {
        Result result = Result.Failure([NameRequired, UserMissing]);

        Action act = () => result.ShouldBeSuccess();

        act.Should().Throw<ResultAssertionException>()
            .Which.Message.Should().Contain("2 errors")
            .And.Contain("[0] Validation Name.Required: Name is required.")
            .And.Contain("metadata [{ field: Name }]")
            .And.Contain("[1] NotFound User.NotFound: User 7 was not found.");
    }

    [Fact]
    public void ShouldBeSuccess_Should_Return_Value_When_ResultT_Is_Success()
    {
        int value = 42.ToResult().ShouldBeSuccess();

        value.Should().Be(42);
    }

    [Fact]
    public void ShouldBeSuccess_Should_Throw_When_ResultT_Is_Failure()
    {
        Result<int> result = UserMissing;

        Action act = () => result.ShouldBeSuccess();

        act.Should().Throw<ResultAssertionException>().WithMessage("*User.NotFound*");
    }

    [Fact]
    public void ShouldBeFailure_Should_Return_Errors_When_Result_Is_Failure()
    {
        Result<int> result = Result<int>.Failure([NameRequired, UserMissing]);

        Error[] errors = result.ShouldBeFailure();

        errors.Should().Equal(NameRequired, UserMissing);
    }

    [Fact]
    public void ShouldBeFailure_Should_Throw_When_Result_Is_Success()
    {
        Action act = () => Result.Success().ShouldBeFailure();

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected a failure, but the result succeeded.");
    }

    [Fact]
    public void ShouldBeFailure_Should_Return_Default_Error_When_Result_Is_Default()
    {
        Error[] errors = default(Result<int>).ShouldBeFailure();

        errors.Should().ContainSingle().Which.Code.Should().Be("Result.Default");
    }

    [Fact]
    public void ShouldHaveError_Should_Return_Error_With_Code()
    {
        Result result = Result.Failure([NameRequired, UserMissing]);

        Error error = result.ShouldHaveError("User.NotFound");

        error.Should().Be(UserMissing);
    }

    [Fact]
    public void ShouldHaveError_Should_Return_Error_Of_Type()
    {
        Result result = Result.Failure([NameRequired, UserMissing]);

        Error error = result.ShouldHaveError(ErrorType.Validation);

        error.Should().Be(NameRequired);
    }

    [Fact]
    public void ShouldHaveError_Should_Match_Type_And_Code_Together()
    {
        Result result = Result.Failure([NameRequired, UserMissing]);

        Action act = () => result.ShouldHaveError(ErrorType.Validation, "User.NotFound");

        act.Should().Throw<ResultAssertionException>()
            .WithMessage("Expected an error with type Validation and code User.NotFound, but the result failed with 2 errors:*");
    }

    [Fact]
    public void ShouldHaveError_Should_Throw_When_Result_Is_Success()
    {
        Action act = () => 1.ToResult().ShouldHaveError("User.NotFound");

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected a failure*");
    }

    [Fact]
    public void ShouldBeFailure_Should_Show_Value_When_ResultT_Is_Success()
    {
        Action act = () => 42.ToResult().ShouldBeFailure();

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected a failure, but the result succeeded with Int32 42.");
    }
}
