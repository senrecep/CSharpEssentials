using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class StringValidatorsTests
{
    private sealed record M(string? Value);

    private async Task<Result<M>> Validate(string? value, Action<RuleChain<M, string?>> configure)
    {
        return await Validator.ValidateAsync(new M(value), (m, rules) =>
        {
            RuleChain<M, string?> chain = rules.For(() => m.Value);
            configure(chain);
        });
    }

    // -------------------------------------------------------------------------
    // NotEmpty
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task NotEmpty_ShouldFail_WhenValueIsNullOrWhitespace(string? value)
    {
        Result<M> result = await Validate(value, c => c.NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.NotEmpty");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("hello")]
    [InlineData("  x  ")]
    public async Task NotEmpty_ShouldPass_WhenValueHasContent(string value)
    {
        Result<M> result = await Validate(value, c => c.NotEmpty());

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // NotNull
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NotNull_ShouldFail_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.NotNull());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.NotNull");
    }

    [Fact]
    public async Task NotNull_ShouldPass_WhenValueIsEmptyString()
    {
        Result<M> result = await Validate("", c => c.NotNull());

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // MaxLength
    // -------------------------------------------------------------------------

    [Fact]
    public async Task MaxLength_ShouldFail_WhenValueExceedsLimit()
    {
        Result<M> result = await Validate("abcdef", c => c.MaxLength(5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.MaxLength");
    }

    [Theory]
    [InlineData("abcde")]
    [InlineData("abc")]
    [InlineData("")]
    public async Task MaxLength_ShouldPass_WhenValueIsWithinLimit(string value)
    {
        Result<M> result = await Validate(value, c => c.MaxLength(5));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MaxLength_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.MaxLength(5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // MinLength
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("ab")]
    [InlineData("")]
    public async Task MinLength_ShouldFail_WhenValueIsTooShort(string? value)
    {
        Result<M> result = await Validate(value, c => c.MinLength(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.MinLength");
    }

    [Fact]
    public async Task MinLength_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.MinLength(3));

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abcdef")]
    public async Task MinLength_ShouldPass_WhenValueMeetsMinimum(string value)
    {
        Result<M> result = await Validate(value, c => c.MinLength(3));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Length (range)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdef")]
    public async Task Length_ShouldFail_WhenValueOutsideRange(string value)
    {
        Result<M> result = await Validate(value, c => c.Length(3, 5));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Length");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("abcde")]
    public async Task Length_ShouldPass_WhenValueWithinRange(string value)
    {
        Result<M> result = await Validate(value, c => c.Length(3, 5));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Length_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.Length(3, 5));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // EmailAddress
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public async Task EmailAddress_ShouldFail_WhenValueIsInvalid(string? value)
    {
        Result<M> result = await Validate(value, c => c.EmailAddress());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.EmailAddress");
    }

    [Fact]
    public async Task EmailAddress_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.EmailAddress());

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("user+tag@domain.co.uk")]
    public async Task EmailAddress_ShouldPass_WhenValueIsValid(string value)
    {
        Result<M> result = await Validate(value, c => c.EmailAddress());

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Matches
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Matches_ShouldFail_WhenPatternDoesNotMatch()
    {
        Result<M> result = await Validate("abc123", c => c.Matches(@"^\d+$"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Matches");
    }

    [Fact]
    public async Task Matches_ShouldPass_WhenPatternMatches()
    {
        Result<M> result = await Validate("12345", c => c.Matches(@"^\d+$"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Matches_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.Matches(@"^\d+$"));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Equal / NotEqual
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Equal_ShouldFail_WhenValueDiffers()
    {
        Result<M> result = await Validate("hello", c => c.Equal("world"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Equal");
    }

    [Fact]
    public async Task Equal_ShouldPass_WhenValueMatches()
    {
        Result<M> result = await Validate("hello", c => c.Equal("hello"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotEqual_ShouldFail_WhenValueMatches()
    {
        Result<M> result = await Validate("forbidden", c => c.NotEqual("forbidden"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.NotEqual");
    }

    [Fact]
    public async Task NotEqual_ShouldPass_WhenValueDiffers()
    {
        Result<M> result = await Validate("allowed", c => c.NotEqual("forbidden"));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Contains / StartsWith / EndsWith
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Contains_ShouldFail_WhenSubstringAbsent()
    {
        Result<M> result = await Validate("hello world", c => c.Contains("xyz"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Value.Contains");
    }

    [Fact]
    public async Task Contains_ShouldPass_WhenSubstringPresent()
    {
        Result<M> result = await Validate("hello world", c => c.Contains("world"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StartsWith_ShouldFail_WhenPrefixAbsent()
    {
        Result<M> result = await Validate("hello", c => c.StartsWith("world"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartsWith_ShouldPass_WhenPrefixPresent()
    {
        Result<M> result = await Validate("hello world", c => c.StartsWith("hello"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EndsWith_ShouldFail_WhenSuffixAbsent()
    {
        Result<M> result = await Validate("hello", c => c.EndsWith("world"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EndsWith_ShouldPass_WhenSuffixPresent()
    {
        Result<M> result = await Validate("hello world", c => c.EndsWith("world"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Contains_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.Contains("x"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StartsWith_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.StartsWith("x"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EndsWith_ShouldSkip_WhenValueIsNull()
    {
        Result<M> result = await Validate(null, c => c.EndsWith("x"));

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Custom error message
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NotEmpty_ShouldUseCustomMessage_WhenProvided()
    {
        Result<M> result = await Validate(null, c => c.NotEmpty("Custom message."));

        result.FirstError.Description.Should().Be("Custom message.");
    }
}
