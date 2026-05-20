using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class NullableAndCollectionValidatorsTests
{
    // =========================================================================
    // Nullable validators
    // =========================================================================

    private sealed record NullableModel(int? Value);

    private async Task<Result<NullableModel>> ValidateNullable(int? value, Action<RuleChain<NullableModel, int?>> configure)
    {
        return await Validator.ValidateAsync(new NullableModel(value), (m, rules) =>
        {
            RuleChain<NullableModel, int?> chain = rules.For(() => m.Value);
            configure(chain);
        });
    }

    [Fact]
    public async Task NotNull_ShouldFail_WhenNullableHasNoValue()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.NotNull());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.NotNull");
    }

    [Fact]
    public async Task NotNull_ShouldPass_WhenNullableHasValue()
    {
        Result<NullableModel> result = await ValidateNullable(42, c => c.NotNull());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Null_ShouldFail_WhenNullableHasValue()
    {
        Result<NullableModel> result = await ValidateNullable(42, c => c.Null());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Null");
    }

    [Fact]
    public async Task Null_ShouldPass_WhenNullableHasNoValue()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.Null());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GreaterThan_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.GreaterThan(0));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GreaterThan_Nullable_ShouldFail_WhenValueBelowThreshold()
    {
        Result<NullableModel> result = await ValidateNullable(-1, c => c.GreaterThan(0));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GreaterThan_Nullable_ShouldPass_WhenValueAboveThreshold()
    {
        Result<NullableModel> result = await ValidateNullable(5, c => c.GreaterThan(0));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InclusiveBetween_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.InclusiveBetween(1, 10));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InclusiveBetween_Nullable_ShouldPass_WhenValueWithinRange()
    {
        Result<NullableModel> result = await ValidateNullable(5, c => c.InclusiveBetween(1, 10));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GreaterThanOrEqualTo_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.GreaterThanOrEqualTo(0));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LessThan_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.LessThan(100));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LessThanOrEqualTo_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.LessThanOrEqualTo(100));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExclusiveBetween_Nullable_ShouldSkip_WhenNull()
    {
        Result<NullableModel> result = await ValidateNullable(null, c => c.ExclusiveBetween(1, 10));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExclusiveBetween_Nullable_ShouldFail_WhenValueOutsideRange()
    {
        Result<NullableModel> result = await ValidateNullable(0, c => c.ExclusiveBetween(1, 10));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.ExclusiveBetween");
    }

    [Fact]
    public async Task ExclusiveBetween_Nullable_ShouldPass_WhenValueInsideRange()
    {
        Result<NullableModel> result = await ValidateNullable(5, c => c.ExclusiveBetween(1, 10));

        result.IsSuccess.Should().BeTrue();
    }

    // =========================================================================
    // Collection validators — IEnumerable<T>? (interface declared property)
    // =========================================================================

    private sealed record CollectionModel(IEnumerable<string>? Items);

    private async Task<Result<CollectionModel>> ValidateCollection(
        IEnumerable<string>? items,
        Action<RuleChain<CollectionModel, IEnumerable<string>?>> configure)
    {
        return await Validator.ValidateAsync(new CollectionModel(items), (m, rules) =>
        {
            RuleChain<CollectionModel, IEnumerable<string>?> chain = rules.For(() => m.Items);
            configure(chain);
        });
    }

    [Fact]
    public async Task NotEmpty_ShouldFail_WhenCollectionIsNull()
    {
        Result<CollectionModel> result = await ValidateCollection(null, c => c.NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Items.NotEmpty");
    }

    [Fact]
    public async Task NotEmpty_ShouldFail_WhenCollectionIsEmpty()
    {
        Result<CollectionModel> result = await ValidateCollection([], c => c.NotEmpty());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task NotEmpty_ShouldPass_WhenCollectionHasItems()
    {
        Result<CollectionModel> result = await ValidateCollection(["a", "b"], c => c.NotEmpty());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MinCount_ShouldFail_WhenBelowMinimum()
    {
        Result<CollectionModel> result = await ValidateCollection(["a"], c => c.MinCount(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Items.MinCount");
    }

    [Fact]
    public async Task MinCount_ShouldPass_WhenAtMinimum()
    {
        Result<CollectionModel> result = await ValidateCollection(["a", "b", "c"], c => c.MinCount(3));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MaxCount_ShouldFail_WhenAboveMaximum()
    {
        Result<CollectionModel> result = await ValidateCollection(["a", "b", "c", "d"], c => c.MaxCount(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Items.MaxCount");
    }

    [Fact]
    public async Task MaxCount_ShouldPass_WhenAtMaximum()
    {
        Result<CollectionModel> result = await ValidateCollection(["a", "b", "c"], c => c.MaxCount(3));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CountBetween_ShouldFail_WhenBelowRange()
    {
        Result<CollectionModel> result = await ValidateCollection(["a"], c => c.CountBetween(2, 5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Items.CountBetween");
    }

    [Fact]
    public async Task CountBetween_ShouldFail_WhenAboveRange()
    {
        Result<CollectionModel> result = await ValidateCollection(["a", "b", "c", "d", "e", "f"], c => c.CountBetween(2, 5));

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task CountBetween_ShouldPass_WhenCountWithinRange(int count)
    {
        IEnumerable<string> items = Enumerable.Repeat("x", count);

        Result<CollectionModel> result = await ValidateCollection(items, c => c.CountBetween(2, 5));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MinCount_ShouldSkip_WhenCollectionIsNull()
    {
        Result<CollectionModel> result = await ValidateCollection(null, c => c.MinCount(1));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MaxCount_ShouldSkip_WhenCollectionIsNull()
    {
        Result<CollectionModel> result = await ValidateCollection(null, c => c.MaxCount(5));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CountBetween_ShouldSkip_WhenCollectionIsNull()
    {
        Result<CollectionModel> result = await ValidateCollection(null, c => c.CountBetween(1, 5));

        result.IsSuccess.Should().BeTrue();
    }

    // =========================================================================
    // Collection validators — List<T>? (concrete collection type)
    // =========================================================================

    private sealed record ListModel(List<string>? Tags);
    private sealed record ArrayModel(string[]? Codes);
    private sealed record IListModel(IList<string>? Names);

    [Fact]
    public async Task NotEmpty_ShouldWork_WithListProperty()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(null),
            (m, rules) => rules.For(() => m.Tags).NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Tags.NotEmpty");
    }

    [Fact]
    public async Task NotEmpty_ShouldPass_WithNonEmptyList()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(["tag1", "tag2"]),
            (m, rules) => rules.For(() => m.Tags).NotEmpty());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MinCount_ShouldWork_WithListProperty()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(["a"]),
            (m, rules) => rules.For(() => m.Tags).MinCount(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Tags.MinCount");
    }

    [Fact]
    public async Task MaxCount_ShouldWork_WithListProperty()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(["a", "b", "c", "d"]),
            (m, rules) => rules.For(() => m.Tags).MaxCount(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Tags.MaxCount");
    }

    [Fact]
    public async Task CountBetween_ShouldWork_WithListProperty()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(["only-one"]),
            (m, rules) => rules.For(() => m.Tags).CountBetween(2, 5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Tags.CountBetween");
    }

    [Fact]
    public async Task NotEmpty_ShouldWork_WithArrayProperty()
    {
        Result<ArrayModel> result = await Validator.ValidateAsync(
            new ArrayModel([]),
            (m, rules) => rules.For(() => m.Codes).NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Codes.NotEmpty");
    }

    [Fact]
    public async Task NotEmpty_ShouldWork_WithIListProperty()
    {
        Result<IListModel> result = await Validator.ValidateAsync(
            new IListModel(null),
            (m, rules) => rules.For(() => m.Names).NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Names.NotEmpty");
    }

    [Fact]
    public async Task MinCount_ShouldSkip_WhenListIsNull()
    {
        Result<ListModel> result = await Validator.ValidateAsync(
            new ListModel(null),
            (m, rules) => rules.For(() => m.Tags).MinCount(1));

        result.IsSuccess.Should().BeTrue();
    }
}
