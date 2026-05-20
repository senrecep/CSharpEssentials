using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class ComparableValidatorsTests
{
    private sealed record M(int Value);

    private async Task<Result<M>> Validate(int value, Action<RuleChain<M, int>> configure)
    {
        return await Validator.ValidateAsync(new M(value), (m, rules) =>
        {
            RuleChain<M, int> chain = rules.For(() => m.Value);
            configure(chain);
        });
    }

    // -------------------------------------------------------------------------
    // GreaterThan
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5)]
    public async Task GreaterThan_ShouldFail_WhenValueIsLessOrEqual(int value)
    {
        Result<M> result = await Validate(value, c => c.GreaterThan(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.GreaterThan");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(100)]
    public async Task GreaterThan_ShouldPass_WhenValueIsGreater(int value)
    {
        Result<M> result = await Validate(value, c => c.GreaterThan(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // GreaterThanOrEqualTo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GreaterThanOrEqualTo_ShouldFail_WhenValueIsLess()
    {
        Result<M> result = await Validate(4, c => c.GreaterThanOrEqualTo(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.GreaterThanOrEqualTo");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public async Task GreaterThanOrEqualTo_ShouldPass_WhenValueMeetsThreshold(int value)
    {
        Result<M> result = await Validate(value, c => c.GreaterThanOrEqualTo(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // LessThan
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public async Task LessThan_ShouldFail_WhenValueIsGreaterOrEqual(int value)
    {
        Result<M> result = await Validate(value, c => c.LessThan(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.LessThan");
    }

    [Fact]
    public async Task LessThan_ShouldPass_WhenValueIsLess()
    {
        Result<M> result = await Validate(4, c => c.LessThan(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // LessThanOrEqualTo
    // -------------------------------------------------------------------------

    [Fact]
    public async Task LessThanOrEqualTo_ShouldFail_WhenValueExceedsThreshold()
    {
        Result<M> result = await Validate(6, c => c.LessThanOrEqualTo(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.LessThanOrEqualTo");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(3)]
    public async Task LessThanOrEqualTo_ShouldPass_WhenValueMeetsThreshold(int value)
    {
        Result<M> result = await Validate(value, c => c.LessThanOrEqualTo(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // InclusiveBetween
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]
    [InlineData(11)]
    public async Task InclusiveBetween_ShouldFail_WhenValueOutsideRange(int value)
    {
        Result<M> result = await Validate(value, c => c.InclusiveBetween(2, 10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.InclusiveBetween");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task InclusiveBetween_ShouldPass_WhenValueWithinBounds(int value)
    {
        Result<M> result = await Validate(value, c => c.InclusiveBetween(2, 10));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // ExclusiveBetween
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(0)]
    public async Task ExclusiveBetween_ShouldFail_WhenValueOnBoundsOrOutside(int value)
    {
        Result<M> result = await Validate(value, c => c.ExclusiveBetween(2, 10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.ExclusiveBetween");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(9)]
    public async Task ExclusiveBetween_ShouldPass_WhenValueStrictlyInsideBounds(int value)
    {
        Result<M> result = await Validate(value, c => c.ExclusiveBetween(2, 10));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Equal / NotEqual
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Equal_ShouldFail_WhenValueDiffers()
    {
        Result<M> result = await Validate(3, c => c.Equal(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Equal");
    }

    [Fact]
    public async Task Equal_ShouldPass_WhenValueMatches()
    {
        Result<M> result = await Validate(5, c => c.Equal(5));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotEqual_ShouldFail_WhenValueMatches()
    {
        Result<M> result = await Validate(5, c => c.NotEqual(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.NotEqual");
    }

    [Fact]
    public async Task NotEqual_ShouldPass_WhenValueDiffers()
    {
        Result<M> result = await Validate(3, c => c.NotEqual(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Works with DateOnly (non-int comparable)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GreaterThan_ShouldWork_WithDateOnly()
    {
        DateOnly value = new(2020, 1, 1);
        DateOnly threshold = new(2024, 1, 1);

        Result<DateOnly> result = await Validator.ValidateAsync(value, (v, rules) =>
            rules.For(() => v).GreaterThan(threshold));

        result.IsFailure.Should().BeTrue();
    }
}
