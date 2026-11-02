using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class NullableValidatorsOverloadTests
{
    private static readonly Error CustomError = Error.Validation("Custom.Code", "Custom description");

    public sealed record M(int? Value);

    private static async Task<Result<M>> Validate(int? value, Action<RuleChain<M, int?>> configure) =>
        await Validator.ValidateAsync(new M(value), (m, rules) => configure(rules.For(() => m.Value)));

    public static TheoryData<Action<RuleChain<M, int?>>> AllRules => new()
    {
        c => c.GreaterThan(500),
        c => c.GreaterThan(500, CustomError),
        c => c.GreaterThanOrEqualTo(500),
        c => c.GreaterThanOrEqualTo(500, CustomError),
        c => c.LessThan(5),
        c => c.LessThan(5, CustomError),
        c => c.LessThanOrEqualTo(5),
        c => c.LessThanOrEqualTo(5, CustomError),
        c => c.InclusiveBetween(1, 5),
        c => c.InclusiveBetween(1, 5, CustomError),
        c => c.ExclusiveBetween(1, 5),
        c => c.ExclusiveBetween(1, 5, CustomError),
    };

    [Theory]
    [MemberData(nameof(AllRules))]
    public async Task ComparisonRules_ShouldSkip_WhenValueIsNull(Action<RuleChain<M, int?>> configure)
    {
        Result<M> result = await Validate(null, configure);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(AllRules))]
    public async Task ComparisonRules_ShouldSkip_WhenChainAlreadyFailed(Action<RuleChain<M, int?>> configure)
    {
        Result<M> result = await Validate(100, c => configure(c.Null()));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Value.Null");
    }

    [Theory]
    [InlineData(6, true)]
    [InlineData(5, false)]
    public async Task GreaterThan_Error_ShouldUseCustomError_WhenAtOrBelowThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.GreaterThan(5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(4, false)]
    public async Task GreaterThanOrEqualTo_ShouldFailOnlyBelowThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.GreaterThanOrEqualTo(5));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
        {
            result.FirstError.Code.Should().Be("Value.GreaterThanOrEqualTo");
            result.FirstError.Description.Should().Be("'Value' must be greater than or equal to 5.");
        }
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(4, false)]
    public async Task GreaterThanOrEqualTo_Error_ShouldUseCustomError_WhenBelowThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.GreaterThanOrEqualTo(5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public async Task LessThan_ShouldFailAtOrAboveThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.LessThan(5));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
        {
            result.FirstError.Code.Should().Be("Value.LessThan");
            result.FirstError.Description.Should().Be("'Value' must be less than 5.");
        }
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public async Task LessThan_Error_ShouldUseCustomError_WhenAtOrAboveThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.LessThan(5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public async Task LessThanOrEqualTo_ShouldFailOnlyAboveThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.LessThanOrEqualTo(5));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
        {
            result.FirstError.Code.Should().Be("Value.LessThanOrEqualTo");
            result.FirstError.Description.Should().Be("'Value' must be less than or equal to 5.");
        }
    }

    [Theory]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public async Task LessThanOrEqualTo_Error_ShouldUseCustomError_WhenAboveThreshold(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.LessThanOrEqualTo(5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task InclusiveBetween_ShouldFail_WhenOutsideRange(int value)
    {
        Result<M> result = await Validate(value, c => c.InclusiveBetween(1, 5));

        result.FirstError.Code.Should().Be("Value.InclusiveBetween");
        result.FirstError.Description.Should().Be("'Value' must be between 1 and 5 (inclusive).");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task ExclusiveBetween_ShouldFail_WhenOnBounds(int value)
    {
        Result<M> result = await Validate(value, c => c.ExclusiveBetween(1, 5));

        result.FirstError.Code.Should().Be("Value.ExclusiveBetween");
        result.FirstError.Description.Should().Be("'Value' must be between 1 and 5 (exclusive).");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    public async Task InclusiveBetween_Error_ShouldUseCustomError_WhenOutsideRange(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.InclusiveBetween(1, 5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public async Task ExclusiveBetween_Error_ShouldUseCustomError_WhenOnOrOutsideBounds(int value, bool expectedSuccess)
    {
        Result<M> result = await Validate(value, c => c.ExclusiveBetween(1, 5, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    public static TheoryData<Action<RuleChain<M, int?>>> InvalidRanges => new()
    {
        c => c.InclusiveBetween(5, 1),
        c => c.InclusiveBetween(5, 1, CustomError),
        c => c.ExclusiveBetween(5, 5),
        c => c.ExclusiveBetween(5, 5, CustomError),
    };

    [Theory]
    [MemberData(nameof(InvalidRanges))]
    public async Task RangeRules_ShouldThrow_WhenBoundsAreInvalid(Action<RuleChain<M, int?>> configure)
    {
        Func<Task> act = () => Validate(3, configure);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Null_Error_ShouldUseCustomError_WhenValuePresent()
    {
        Result<M> result = await Validate(1, c => c.Null(CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task NotNull_Error_ShouldPass_WhenValuePresent()
    {
        Result<M> result = await Validate(1, c => c.NotNull(CustomError));

        result.IsSuccess.Should().BeTrue();
    }
}
