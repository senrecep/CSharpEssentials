using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyKeyHeaderTests
{
    [Theory]
    [InlineData("key-1", "key-1")]
    [InlineData("\"key-1\"", "key-1")]
    [InlineData("\"k\"", "k")]
    [InlineData("k", "k")]
    public void TryParse_Should_Return_The_Unquoted_Key(string value, string expected)
    {
        bool parsed = IdempotencyKeyHeader.TryParse(value, out string? key);

        parsed.Should().BeTrue();
        key.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("\"")]
    [InlineData("\"\"")]
    [InlineData("\"abc")]
    [InlineData("abc\"")]
    [InlineData("\"ab\"c\"")]
    [InlineData("\"ab\\\"c\"")]
    [InlineData("\"ab\\c\"")]
    [InlineData("a\\b")]
    [InlineData("key 1")]
    [InlineData("\"key 1\"")]
    [InlineData("anahtar-ğ")]
    [InlineData(" key")]
    [InlineData("key ")]
    [InlineData(" \"key\" ")]
    [InlineData("k\uD800")]
    [InlineData("\"\uDC00k\"")]
    public void TryParse_Should_Reject_An_Invalid_Value(string? value)
    {
        bool parsed = IdempotencyKeyHeader.TryParse(value, out string? key);

        parsed.Should().BeFalse();
        key.Should().BeNull();
    }

    [Fact]
    public void TryParse_Should_Apply_The_Default_Max_Length_To_The_Unquoted_Key()
    {
        string longest = new('k', IdempotencyKeys.MaxKeyLength);
        string tooLong = new('k', IdempotencyKeys.MaxKeyLength + 1);

        IdempotencyKeyHeader.TryParse(longest, out _).Should().BeTrue();
        IdempotencyKeyHeader.TryParse($"\"{longest}\"", out string? quoted).Should().BeTrue();
        quoted.Should().Be(longest);
        IdempotencyKeyHeader.TryParse(tooLong, out _).Should().BeFalse();
        IdempotencyKeyHeader.TryParse($"\"{tooLong}\"", out _).Should().BeFalse();
    }

    [Fact]
    public void TryParse_Should_Apply_A_Custom_Max_Length_To_The_Unquoted_Key()
    {
        IdempotencyKeyHeader.TryParse("\"12345\"", 5, out string? key).Should().BeTrue();
        key.Should().Be("12345");
        IdempotencyKeyHeader.TryParse("\"123456\"", 5, out _).Should().BeFalse();
        IdempotencyKeyHeader.TryParse("123456", 5, out _).Should().BeFalse();
    }
}
