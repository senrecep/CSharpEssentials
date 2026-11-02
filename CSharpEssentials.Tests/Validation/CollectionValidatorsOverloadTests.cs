using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class CollectionValidatorsOverloadTests
{
    private static readonly Error CustomError = Error.Validation("Custom.Code", "Custom description");

    public sealed record M(IEnumerable<int>? Items);

    private static IEnumerable<int> Lazy(int count)
    {
        for (int i = 0; i < count; i++)
            yield return i;
    }

    private static async Task<Result<M>> Validate(IEnumerable<int>? items, Action<RuleChain<M, IEnumerable<int>?>> configure) =>
        await Validator.ValidateAsync(new M(items), (m, rules) => configure(rules.For(() => m.Items)));

    [Fact]
    public async Task NotEmpty_ShouldFail_WhenLazySequenceIsEmpty()
    {
        Result<M> result = await Validate(Lazy(0), c => c.NotEmpty());

        result.FirstError.Code.Should().Be("Items.NotEmpty");
    }

    [Fact]
    public async Task NotEmpty_ShouldPass_WhenLazySequenceHasItems()
    {
        Result<M> result = await Validate(Lazy(1), c => c.NotEmpty());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotNull_ShouldFail_WhenCollectionIsNull()
    {
        Result<M> result = await Validate(null, c => c.NotNull());

        result.FirstError.Code.Should().Be("Items.NotNull");
        result.FirstError.Description.Should().Be("'Items' must not be null.");
    }

    [Fact]
    public async Task NotNull_ShouldPass_WhenCollectionIsEmpty()
    {
        Result<M> result = await Validate([], c => c.NotNull());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MinCount_ShouldCountLazySequence_InDefaultMessage()
    {
        Result<M> result = await Validate(Lazy(2), c => c.MinCount(3));

        result.FirstError.Code.Should().Be("Items.MinCount");
        result.FirstError.Description.Should().Be("'Items' must contain at least 3 item(s). It contains 2.");
    }

    [Fact]
    public async Task MaxCount_ShouldCountLazySequence_InDefaultMessage()
    {
        Result<M> result = await Validate(Lazy(4), c => c.MaxCount(3));

        result.FirstError.Code.Should().Be("Items.MaxCount");
        result.FirstError.Description.Should().Be("'Items' must contain at most 3 item(s). It contains 4.");
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public async Task MinCount_Error_ShouldUseCustomError_WhenTooFew(int count, bool expectedSuccess)
    {
        Result<M> result = await Validate(Lazy(count), c => c.MinCount(3, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(3, true)]
    public async Task MaxCount_Error_ShouldUseCustomError_WhenTooMany(int count, bool expectedSuccess)
    {
        Result<M> result = await Validate(Lazy(count), c => c.MaxCount(3, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public async Task CountBetween_Error_ShouldUseCustomError_WhenOutsideRange(int count, bool expectedSuccess)
    {
        Result<M> result = await Validate(Lazy(count), c => c.CountBetween(1, 3, CustomError));

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.FirstError.Should().Be(CustomError);
    }

    public static TheoryData<Action<RuleChain<M, IEnumerable<int>?>>> CountRules => new()
    {
        c => c.MinCount(4),
        c => c.MinCount(4, CustomError),
        c => c.MaxCount(0),
        c => c.MaxCount(0, CustomError),
        c => c.CountBetween(1, 2),
        c => c.CountBetween(1, 2, CustomError),
    };

    [Theory]
    [MemberData(nameof(CountRules))]
    public async Task CountRules_ShouldSkip_WhenCollectionIsNull(Action<RuleChain<M, IEnumerable<int>?>> configure)
    {
        Result<M> result = await Validate(null, configure);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(CountRules))]
    public async Task CountRules_ShouldSkip_WhenChainAlreadyFailed(Action<RuleChain<M, IEnumerable<int>?>> configure)
    {
        Result<M> result = await Validate([7, 8, 9], c => configure(c.Must(_ => false)));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Items.Must");
    }

    public static TheoryData<Action<RuleChain<M, IEnumerable<int>?>>> InvalidCountArguments => new()
    {
        c => c.MinCount(-1),
        c => c.MinCount(-1, CustomError),
        c => c.MaxCount(-1),
        c => c.MaxCount(-1, CustomError),
        c => c.CountBetween(-1, 2),
        c => c.CountBetween(-1, 2, CustomError),
        c => c.CountBetween(3, 2),
        c => c.CountBetween(3, 2, CustomError),
    };

    [Theory]
    [MemberData(nameof(InvalidCountArguments))]
    public async Task CountRules_ShouldThrow_WhenArgumentsAreInvalid(Action<RuleChain<M, IEnumerable<int>?>> configure)
    {
        Func<Task> act = () => Validate([1], configure);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
