using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

public class FailureResponseTests
{
    private sealed record Dto(int Id);

    [Fact]
    public void Factory_Should_Be_Set_For_Result_Types()
    {
        FailureResponse<Result>.Factory.Should().NotBeNull();
        FailureResponse<Result<int>>.Factory.Should().NotBeNull();
        FailureResponse<Result<string>>.Factory.Should().NotBeNull();
    }

    [Fact]
    public void Factory_Should_Be_Null_For_Other_Types()
    {
        FailureResponse<string>.Factory.Should().BeNull();
        FailureResponse<int>.Factory.Should().BeNull();
        FailureResponse<Dto>.Factory.Should().BeNull();
        FailureResponse<IResult<int>>.Factory.Should().BeNull();
        FailureResponse<Result<int>?>.Factory.Should().BeNull();
        FailureResponse<object>.Factory.Should().BeNull();
    }

    [Fact]
    public void CreateFailure_Should_Use_The_Given_Array_For_Generic_Result()
    {
        Error[] errors = [Error.Validation("Code", "Message")];

        Result<int> result = FailureResponse<Result<int>>.Factory!.CreateFailure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void CreateFailure_Should_Use_The_Given_Array_For_Result()
    {
        Error[] errors = [Error.Validation("Code", "Message")];

        Result result = FailureResponse<Result>.Factory!.CreateFailure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void CreateFailure_Should_Reject_Empty_And_Null_Arrays()
    {
        IFailureFactory<Result<int>> factory = FailureResponse<Result<int>>.Factory!;

        Action empty = () => factory.CreateFailure([]);
        Action missing = () => factory.CreateFailure(null!);

        empty.Should().Throw<ArgumentException>();
        missing.Should().Throw<ArgumentNullException>();
    }
}
