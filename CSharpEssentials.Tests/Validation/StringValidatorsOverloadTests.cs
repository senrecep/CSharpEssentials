using System.Text.RegularExpressions;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class StringValidatorsOverloadTests
{
    private static readonly Error CustomError = Error.Validation("Custom.Code", "Custom description");

    public sealed record M(string? Value);

    private static async Task<Result<M>> Validate(string? value, Action<RuleChain<M, string?>> configure) =>
        await Validator.ValidateAsync(new M(value), (m, rules) => configure(rules.For(() => m.Value)));

    public static TheoryData<Action<RuleChain<M, string?>>> InvalidLengthArguments => new()
    {
        c => c.MaxLength(-1),
        c => c.MaxLength(-1, CustomError),
        c => c.MinLength(-1),
        c => c.MinLength(-1, CustomError),
        c => c.Length(-1, 5),
        c => c.Length(-1, 5, CustomError),
        c => c.Length(6, 5),
        c => c.Length(6, 5, CustomError),
    };

    [Theory]
    [MemberData(nameof(InvalidLengthArguments))]
    public async Task LengthRules_ShouldThrow_WhenArgumentsAreInvalid(Action<RuleChain<M, string?>> configure)
    {
        Func<Task> act = () => Validate("abc", configure);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task MaxLength_ShouldReportEnteredLength_InDefaultMessage()
    {
        Result<M> result = await Validate("abcdef", c => c.MaxLength(5));

        result.FirstError.Description.Should().Be("'Value' must be 5 characters or fewer. You entered 6 characters.");
    }

    [Fact]
    public async Task MinLength_Error_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.MinLength(3, CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcdef")]
    public async Task Length_Error_ShouldUseCustomError_WhenOutOfRange(string value)
    {
        Result<M> result = await Validate(value, c => c.Length(2, 5, CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("abc")]
    public async Task Length_Error_ShouldPass_WhenNullOrWithinRange(string? value)
    {
        Result<M> result = await Validate(value, c => c.Length(2, 5, CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Length_ShouldReportBoundsAndLength_InDefaultMessage()
    {
        Result<M> result = await Validate("a", c => c.Length(2, 5));

        result.FirstError.Code.Should().Be("Value.Length");
        result.FirstError.Description.Should().Be("'Value' must be between 2 and 5 characters. You entered 1 characters.");
    }

    [Fact]
    public async Task EmailAddress_ShouldFail_WhenMatchTimesOut()
    {
        string pathological = "a@" + new string('.', 200_000) + " ";

        Result<M> result = await Validate(pathological, c => c.EmailAddress());

        result.FirstError.Code.Should().Be("Value.EmailAddress");
    }

    [Fact]
    public async Task EmailAddress_Error_ShouldUseCustomError_WhenMatchTimesOut()
    {
        string pathological = "a@" + new string('.', 200_000) + " ";

        Result<M> result = await Validate(pathological, c => c.EmailAddress(CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Matches_Pattern_ShouldFail_WhenMatchTimesOut()
    {
        string pathological = new string('a', 64) + "!";

        Result<M> result = await Validate(pathological, c => c.Matches("^(a+)+$"));

        result.FirstError.Code.Should().Be("Value.Matches");
    }

    [Fact]
    public async Task Matches_PatternError_ShouldUseCustomError_WhenMatchTimesOut()
    {
        string pathological = new string('a', 64) + "!";

        Result<M> result = await Validate(pathological, c => c.Matches("^(a+)+$", CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Matches_Pattern_ShouldThrow_WhenPatternIsInvalid()
    {
        Func<Task> act = () => Validate("abc", c => c.Matches("(unclosed"));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Matches_Regex_ShouldFail_WhenValueDoesNotMatch()
    {
        Result<M> result = await Validate("abc", c => c.Matches(new Regex("^[0-9]+$"), "digits only"));

        result.FirstError.Code.Should().Be("Value.Matches");
        result.FirstError.Description.Should().Be("digits only");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("123")]
    public async Task Matches_Regex_ShouldPass_WhenNullOrMatching(string? value)
    {
        Result<M> result = await Validate(value, c => c.Matches(new Regex("^[0-9]+$")));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Matches_Regex_ShouldFail_WhenMatchTimesOut()
    {
        Regex regex = new("^(a+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(1));

        Result<M> result = await Validate(new string('a', 64) + "!", c => c.Matches(regex));

        result.FirstError.Code.Should().Be("Value.Matches");
    }

    [Fact]
    public async Task Matches_RegexError_ShouldUseCustomError_WhenValueDoesNotMatch()
    {
        Result<M> result = await Validate("abc", c => c.Matches(new Regex("^[0-9]+$"), CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Matches_RegexError_ShouldPass_WhenValueMatches()
    {
        Result<M> result = await Validate("123", c => c.Matches(new Regex("^[0-9]+$"), CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Matches_RegexError_ShouldUseCustomError_WhenMatchTimesOut()
    {
        Regex regex = new("^(a+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(1));

        Result<M> result = await Validate(new string('a', 64) + "!", c => c.Matches(regex, CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Equal_Error_ShouldUseCustomError_WhenDifferent()
    {
        Result<M> result = await Validate("abc", c => c.Equal("ABC", CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Equal_Error_ShouldPass_WhenOrdinallyEqual()
    {
        Result<M> result = await Validate("abc", c => c.Equal("abc", CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotEqual_ShouldFail_WhenOrdinallyEqual()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("abc"));

        result.FirstError.Code.Should().Be("Value.NotEqual");
        result.FirstError.Description.Should().Be("'Value' must not be equal to 'abc'.");
    }

    [Fact]
    public async Task NotEqual_Error_ShouldUseCustomError_WhenOrdinallyEqual()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("abc", CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task NotEqual_Error_ShouldPass_WhenCaseDiffers()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("ABC", CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Equal_WithComparison_ShouldPass_WhenEqualIgnoringCase()
    {
        Result<M> result = await Validate("abc", c => c.Equal("ABC", StringComparison.OrdinalIgnoreCase));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Equal_WithComparison_ShouldFail_WhenDifferent()
    {
        Result<M> result = await Validate("abc", c => c.Equal("xyz", StringComparison.OrdinalIgnoreCase));

        result.FirstError.Code.Should().Be("Value.Equal");
        result.FirstError.Description.Should().Be("'Value' must be equal to 'xyz'.");
    }

    [Fact]
    public async Task Equal_WithComparisonError_ShouldUseCustomError_WhenDifferent()
    {
        Result<M> result = await Validate("abc", c => c.Equal("xyz", StringComparison.OrdinalIgnoreCase, CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task Equal_WithComparisonError_ShouldPass_WhenEqualIgnoringCase()
    {
        Result<M> result = await Validate("abc", c => c.Equal("ABC", StringComparison.OrdinalIgnoreCase, CustomError));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotEqual_WithComparison_ShouldFail_WhenEqualIgnoringCase()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("ABC", StringComparison.OrdinalIgnoreCase));

        result.FirstError.Code.Should().Be("Value.NotEqual");
        result.FirstError.Description.Should().Be("'Value' must not be equal to 'ABC'.");
    }

    [Fact]
    public async Task NotEqual_WithComparison_ShouldPass_WhenDifferent()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("xyz", StringComparison.OrdinalIgnoreCase));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotEqual_WithComparisonError_ShouldUseCustomError_WhenEqualIgnoringCase()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("ABC", StringComparison.OrdinalIgnoreCase, CustomError));

        result.FirstError.Should().Be(CustomError);
    }

    [Fact]
    public async Task NotEqual_WithComparisonError_ShouldPass_WhenDifferent()
    {
        Result<M> result = await Validate("abc", c => c.NotEqual("xyz", StringComparison.OrdinalIgnoreCase, CustomError));

        result.IsSuccess.Should().BeTrue();
    }
}
