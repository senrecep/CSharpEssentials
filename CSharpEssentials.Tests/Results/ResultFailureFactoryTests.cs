using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;
using Xunit;

namespace CSharpEssentials.Tests.Results;

public class ResultFailureFactoryTests
{
    private static IFailureFactory<T>? FactoryOf<T>() => default(T) as IFailureFactory<T>;

    [Fact]
    public void Factory_ShouldExistForResultTypes()
    {
        FactoryOf<Result>().Should().NotBeNull();
        FactoryOf<Result<int>>().Should().NotBeNull();
        FactoryOf<Result<string>>().Should().NotBeNull();
    }

    [Fact]
    public void Factory_ShouldNotExistForOtherTypes()
    {
        FactoryOf<string>().Should().BeNull();
        FactoryOf<int>().Should().BeNull();
        FactoryOf<IResult<int>>().Should().BeNull();
        FactoryOf<Result<int>?>().Should().BeNull();
    }

    [Fact]
    public void CreateFailure_Result_ShouldUseArrayWithoutCopy()
    {
        Error[] errors = [Error.Validation("Code", "Message")];

        Result result = FactoryOf<Result>()!.CreateFailure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void CreateFailure_ResultT_ShouldUseArrayWithoutCopy()
    {
        Error[] errors = [Error.Validation("Code", "Message")];

        Result<int> result = FactoryOf<Result<int>>()!.CreateFailure(errors);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void CreateFailure_EmptyArray_ShouldThrow()
    {
        FluentActions.Invoking(() => FactoryOf<Result>()!.CreateFailure([])).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => FactoryOf<Result<int>>()!.CreateFailure([])).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateFailure_NullArray_ShouldThrow()
    {
        FluentActions.Invoking(() => FactoryOf<Result>()!.CreateFailure(null!)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => FactoryOf<Result<int>>()!.CreateFailure(null!)).Should().Throw<ArgumentNullException>();
    }
}
