using CSharpEssentials.ResultPattern;
using FluentAssertions;


namespace CSharpEssentials.Tests.Meta;

public class StringExtensionsTests
{
    [Fact]
    public void TrimStart_WithValidPrefix_ShouldRemovePrefix()
    {
        const string input = "prefix_hello";
        const string prefix = "prefix_";

        Result<string> result = input.TrimStart(prefix);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void TrimStart_WithoutPrefix_ShouldReturnOriginal()
    {
        const string input = "hello";
        const string prefix = "prefix_";

        Result<string> result = input.TrimStart(prefix);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void TrimStart_WithEmptyInput_ShouldReturnError()
    {
        const string input = "";
        const string prefix = "prefix_";

        Result<string> result = input.TrimStart(prefix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimStart_WithNullInput_ShouldReturnError()
    {
        string? input = null;
        const string prefix = "prefix_";

        Result<string> result = input.TrimStart(prefix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimStart_WithEmptyPrefix_ShouldReturnError()
    {
        const string input = "hello";
        const string prefix = "";

        Result<string> result = input.TrimStart(prefix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("PrefixIsEmpty");
    }

    [Fact]
    public void TrimStart_WithNullPrefix_ShouldReturnError()
    {
        const string input = "hello";
        string? prefix = null;

        Result<string> result = input.TrimStart(prefix!);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("PrefixIsEmpty");
    }

    [Fact]
    public void TrimStart_WithCaseInsensitive_ShouldRemovePrefix()
    {
        const string input = "PREFIX_hello";
        const string prefix = "prefix_";

        Result<string> result = input.TrimStart(prefix, StringComparison.OrdinalIgnoreCase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void TrimEnd_WithValidSuffix_ShouldRemoveSuffix()
    {
        const string input = "hello_suffix";
        const string suffix = "_suffix";

        Result<string> result = input.TrimEnd(suffix);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void TrimEnd_WithoutSuffix_ShouldReturnOriginal()
    {
        const string input = "hello";
        const string suffix = "_suffix";

        Result<string> result = input.TrimEnd(suffix);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void TrimEnd_WithEmptyInput_ShouldReturnError()
    {
        const string input = "";
        const string suffix = "_suffix";

        Result<string> result = input.TrimEnd(suffix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimEnd_WithNullInput_ShouldReturnError()
    {
        string? input = null;
        const string suffix = "_suffix";

        Result<string> result = input.TrimEnd(suffix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("InputIsEmpty");
    }

    [Fact]
    public void TrimEnd_WithEmptySuffix_ShouldReturnError()
    {
        const string input = "hello";
        const string suffix = "";

        Result<string> result = input.TrimEnd(suffix);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("SuffixIsEmpty");
    }

    [Fact]
    public void TrimEnd_WithCaseInsensitive_ShouldRemoveSuffix()
    {
        const string input = "hello_SUFFIX";
        const string suffix = "_suffix";

        Result<string> result = input.TrimEnd(suffix, StringComparison.OrdinalIgnoreCase);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }
}

