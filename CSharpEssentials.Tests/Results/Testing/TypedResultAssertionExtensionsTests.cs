using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Testing;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results.Testing;

public class TypedResultAssertionExtensionsTests
{
    private static readonly NotFoundError Missing = new("Order.NotFound", "Order 3 was not found.");
    private static readonly ConflictError Taken = new("Order.Taken", "Order 3 is already taken.");

    [Fact]
    public void ShouldBeSuccess_Should_Return_Value_When_Typed_Result_Is_Success()
    {
        Result<int, NotFoundError> result = 5;

        result.ShouldBeSuccess().Should().Be(5);
    }

    [Fact]
    public void ShouldBeSuccess_Should_Show_Typed_Error_When_Result_Holds_Error()
    {
        Result<int, NotFoundError, ConflictError> result = Taken;

        Action act = () => result.ShouldBeSuccess();

        act.Should().Throw<ResultAssertionException>()
            .WithMessage("Expected a success, but the result holds the error ConflictError*Order.Taken*");
    }

    [Fact]
    public void ShouldBeFailureOf_Should_Return_Error_Of_Slot()
    {
        Result<int, NotFoundError, ConflictError, ValidationError> result = Taken;

        ConflictError error = result.ShouldBeFailureOf<ConflictError>();

        error.Should().Be(Taken);
    }

    [Fact]
    public void ShouldBeFailureOf_Should_Throw_When_Result_Holds_Other_Slot()
    {
        Result<int, NotFoundError, ConflictError> result = Taken;

        Action act = () => result.ShouldBeFailureOf<NotFoundError>();

        act.Should().Throw<ResultAssertionException>()
            .WithMessage("Expected an error of type NotFoundError, but the result holds the error ConflictError*");
    }

    [Fact]
    public void ShouldBeFailureOf_Should_Throw_When_Result_Is_Success()
    {
        Result<int, NotFoundError> result = 9;

        Action act = () => result.ShouldBeFailureOf<NotFoundError>();

        act.Should().Throw<ResultAssertionException>().WithMessage("*succeeded with Int32 9.");
    }

    [Fact]
    public void ShouldBeFailureOf_Should_Throw_When_Result_Is_Default()
    {
        Action act = () => default(Result<int, NotFoundError>).ShouldBeFailureOf<NotFoundError>();

        act.Should().Throw<ResultAssertionException>().WithMessage("*default and holds no case.");
    }

    [Fact]
    public void ShouldHaveError_Should_Work_On_Typed_Result()
    {
        Result<int, NotFoundError> result = Missing;

        Error error = result.ShouldHaveError(ErrorType.NotFound, "Order.NotFound");

        error.Description.Should().Be("Order 3 was not found.");
    }

    [Fact]
    public void ShouldBeFailure_Should_Show_Value_When_Typed_Result_Is_Success()
    {
        Result<int, NotFoundError, ConflictError> result = 7;

        Action act = () => result.ShouldBeFailure();

        act.Should().Throw<ResultAssertionException>().WithMessage("Expected a failure, but the result succeeded with Int32 7.");
    }

    [Fact]
    public void ShouldBeFailure_Should_Return_Converted_Error_Of_Typed_Result()
    {
        Result<int, NotFoundError> result = Missing;

        Error[] errors = result.ShouldBeFailure();

        errors.Should().ContainSingle().Which.Code.Should().Be("Order.NotFound");
    }
}
