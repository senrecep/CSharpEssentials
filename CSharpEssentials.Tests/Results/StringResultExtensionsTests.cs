using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class StringResultExtensionsTests
{
    #region TrimStart

    [Fact]
    public void TrimStart_Should_ReturnTrimmedString_When_InputStartsWithPrefix()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimStart("Hello, ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("World!");
    }

    [Fact]
    public void TrimStart_Should_ReturnOriginalString_When_InputDoesNotStartWithPrefix()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimStart("Goodbye, ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void TrimStart_Should_ReturnFailure_When_InputIsNull()
    {
        string? input = null;

        Result<string> result = input.TrimStart("prefix");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimStart_Should_ReturnFailure_When_InputIsEmpty()
    {
        string input = string.Empty;

        Result<string> result = input.TrimStart("prefix");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimStart_Should_ReturnFailure_When_PrefixIsEmpty()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimStart(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("PrefixIsEmpty");
    }

    [Fact]
    public void TrimStart_Should_RespectComparisonType_When_CaseSensitive()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimStart("hello, ", StringComparison.Ordinal);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void TrimStart_Should_RespectComparisonType_When_CaseInsensitive()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimStart("hello, ", StringComparison.OrdinalIgnoreCase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("World!");
    }

    [Fact]
    public void TrimStart_Should_ReturnEmptyString_When_InputEqualsPrefix()
    {
        string input = "prefix";

        Result<string> result = input.TrimStart("prefix");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(string.Empty);
    }

    #endregion

    #region TrimEnd

    [Fact]
    public void TrimEnd_Should_ReturnTrimmedString_When_InputEndsWithSuffix()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimEnd(", World!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public void TrimEnd_Should_ReturnOriginalString_When_InputDoesNotEndWithSuffix()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimEnd(", Universe!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void TrimEnd_Should_ReturnFailure_When_InputIsNull()
    {
        string? input = null;

        Result<string> result = input.TrimEnd("suffix");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimEnd_Should_ReturnFailure_When_InputIsEmpty()
    {
        string input = string.Empty;

        Result<string> result = input.TrimEnd("suffix");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimEnd_Should_ReturnFailure_When_SuffixIsEmpty()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimEnd(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("SuffixIsEmpty");
    }

    [Fact]
    public void TrimEnd_Should_RespectComparisonType_When_CaseSensitive()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimEnd(", WORLD!", StringComparison.Ordinal);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello, World!");
    }

    [Fact]
    public void TrimEnd_Should_RespectComparisonType_When_CaseInsensitive()
    {
        string input = "Hello, World!";

        Result<string> result = input.TrimEnd(", WORLD!", StringComparison.OrdinalIgnoreCase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello");
    }

    [Fact]
    public void TrimEnd_Should_ReturnEmptyString_When_InputEqualsSuffix()
    {
        string input = "suffix";

        Result<string> result = input.TrimEnd("suffix");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(string.Empty);
    }

    #endregion
}
