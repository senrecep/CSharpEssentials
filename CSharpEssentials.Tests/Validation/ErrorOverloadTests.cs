using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

/// <summary>
/// Verifies that every validator method that accepts an <see cref="Error"/> parameter
/// uses that error verbatim instead of constructing a default one.
/// Logic is already covered by the message-overload tests; these tests focus only on
/// the "custom error is propagated unchanged" contract.
/// </summary>
public class ErrorOverloadTests
{
    private static readonly Error CustomError =
        Error.Validation("Custom.Code", "Custom description");

    // =========================================================================
    // StringValidators
    // =========================================================================

    private sealed record Str(string? Value);

    private async Task<Result<Str>> ValidateStr(string? value, Action<RuleChain<Str, string?>> configure) =>
        await Validator.ValidateAsync(new Str(value), (m, rules) => configure(rules.For(() => m.Value)));

    [Fact]
    public async Task String_NotEmpty_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr(null, c => c.NotEmpty(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_NotEmpty_Error_ShouldPass_WhenValid()
    {
        Result<Str> result = await ValidateStr("hello", c => c.NotEmpty(CustomError));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task String_NotNull_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr(null, c => c.NotNull(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_MaxLength_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("toolong", c => c.MaxLength(3, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_MinLength_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("ab", c => c.MinLength(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_Length_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("x", c => c.Length(3, 10, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_EmailAddress_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("notanemail", c => c.EmailAddress(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_Matches_Pattern_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("abc", c => c.Matches(@"^\d+$", CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_Equal_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("foo", c => c.Equal("bar", StringComparison.Ordinal, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_NotEqual_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("foo", c => c.NotEqual("foo", StringComparison.Ordinal, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_Contains_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("hello", c => c.Contains("xyz", CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_StartsWith_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("hello", c => c.StartsWith("world", CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task String_EndsWith_Error_ShouldUseCustomError()
    {
        Result<Str> result = await ValidateStr("hello", c => c.EndsWith("world", CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    // =========================================================================
    // ComparableValidators
    // =========================================================================

    private sealed record Num(int Value);

    private async Task<Result<Num>> ValidateNum(int value, Action<RuleChain<Num, int>> configure) =>
        await Validator.ValidateAsync(new Num(value), (m, rules) => configure(rules.For(() => m.Value)));

    [Fact]
    public async Task Comparable_GreaterThan_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(3, c => c.GreaterThan(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_GreaterThanOrEqualTo_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(3, c => c.GreaterThanOrEqualTo(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_LessThan_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(10, c => c.LessThan(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_LessThanOrEqualTo_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(10, c => c.LessThanOrEqualTo(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_InclusiveBetween_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(20, c => c.InclusiveBetween(1, 10, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_ExclusiveBetween_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(1, c => c.ExclusiveBetween(1, 10, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_Equal_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(3, c => c.Equal(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_NotEqual_Error_ShouldUseCustomError()
    {
        Result<Num> result = await ValidateNum(5, c => c.NotEqual(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Comparable_GreaterThan_Error_ShouldPass_WhenValid()
    {
        Result<Num> result = await ValidateNum(10, c => c.GreaterThan(5, CustomError));
        result.IsSuccess.Should().BeTrue();
    }

    // =========================================================================
    // NullableValidators
    // =========================================================================

    private sealed record NullNum(int? Value);

    private async Task<Result<NullNum>> ValidateNullNum(int? value, Action<RuleChain<NullNum, int?>> configure) =>
        await Validator.ValidateAsync(new NullNum(value), (m, rules) => configure(rules.For(() => m.Value)));

    [Fact]
    public async Task Nullable_NotNull_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(null, c => c.NotNull(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_Null_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(5, c => c.Null(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_GreaterThan_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(3, c => c.GreaterThan(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_GreaterThanOrEqualTo_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(3, c => c.GreaterThanOrEqualTo(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_LessThan_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(10, c => c.LessThan(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_LessThanOrEqualTo_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(10, c => c.LessThanOrEqualTo(5, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_InclusiveBetween_Error_ShouldUseCustomError()
    {
        Result<NullNum> result = await ValidateNullNum(20, c => c.InclusiveBetween(1, 10, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Nullable_NotNull_Error_ShouldPass_WhenHasValue()
    {
        Result<NullNum> result = await ValidateNullNum(5, c => c.NotNull(CustomError));
        result.IsSuccess.Should().BeTrue();
    }

    // =========================================================================
    // CollectionValidators
    // =========================================================================

    private sealed record Col(IEnumerable<string>? Items);

    private async Task<Result<Col>> ValidateCol(IEnumerable<string>? items, Action<RuleChain<Col, IEnumerable<string>?>> configure) =>
        await Validator.ValidateAsync(new Col(items), (m, rules) => configure(rules.For(() => m.Items)));

    [Fact]
    public async Task Collection_NotEmpty_Error_ShouldUseCustomError()
    {
        Result<Col> result = await ValidateCol(null, c => c.NotEmpty(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Collection_NotNull_Error_ShouldUseCustomError()
    {
        Result<Col> result = await ValidateCol(null, c => c.NotNull(CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Collection_MinCount_Error_ShouldUseCustomError()
    {
        Result<Col> result = await ValidateCol(["a"], c => c.MinCount(3, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Collection_MaxCount_Error_ShouldUseCustomError()
    {
        Result<Col> result = await ValidateCol(["a", "b", "c", "d"], c => c.MaxCount(2, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Collection_CountBetween_Error_ShouldUseCustomError()
    {
        Result<Col> result = await ValidateCol(["a", "b", "c", "d", "e"], c => c.CountBetween(1, 3, CustomError));
        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Collection_NotEmpty_Error_ShouldPass_WhenValid()
    {
        Result<Col> result = await ValidateCol(["item"], c => c.NotEmpty(CustomError));
        result.IsSuccess.Should().BeTrue();
    }
}
