using System.Globalization;
using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("HelloWorld", "HelloWorld")]
    [InlineData("helloworld", "Helloworld")]
    [InlineData("HELLOWORLD", "Helloworld")]
    [InlineData("hello world", "HelloWorld")]
    public void ToPascalCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToPascalCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("helloworld", "helloworld")]
    [InlineData("HELLOWORLD", "helloworld")]
    [InlineData("hello world", "helloWorld")]
    public void ToCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToCamelCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("Hello123World", "hello_123_world")]
    [InlineData("hello world", "hello_world")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToSnakeCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("hello world", "hello-world")]
    public void ToKebabCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToKebabCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("hello world", "Hello World")]
    public void ToTitleCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToTitleCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "HELLO_WORLD")]
    [InlineData("hello world", "HELLO_WORLD")]
    public void ToMacroCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToMacroCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "Hello-World")]
    [InlineData("hello world", "Hello-World")]
    public void ToTrainCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToTrainCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("HelloWorld", "_helloWorld")]
    [InlineData("hello world", "_helloWorld")]
    public void ToUnderscoreCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        input.ToUnderscoreCamelCase().Should().Be(expected);
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueForEmptyString()
    {
        string.Empty.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_ShouldReturnFalseForNonEmptyString()
    {
        "hello".IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotEmpty_ShouldReturnFalseForEmptyString()
    {
        string.Empty.IsNotEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotEmpty_ShouldReturnTrueForNonEmptyString()
    {
        "hello".IsNotEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNull_ShouldReturnTrueForNull()
    {
        string? nullString = null;
        nullString.IsNull().Should().BeTrue();
    }

    [Fact]
    public void IsNull_ShouldReturnFalseForNonNull()
    {
        "hello".IsNull().Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_ShouldReturnFalseForNull()
    {
        string? nullString = null;
        nullString.IsNotNull().Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_ShouldReturnTrueForNonNull()
    {
        "hello".IsNotNull().Should().BeTrue();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "A")]
    [InlineData("A", "A")]
    [InlineData("hello_world-test", "HelloWorldTest")]
    [InlineData("_hello", "Hello")]
    [InlineData("hello_", "Hello")]
    [InlineData("---hello", "Hello")]
    [InlineData("hello---world", "HelloWorld")]
    public void CaseConversion_EdgeCases_PascalCase(string input, string expected)
    {
        input.ToPascalCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("a", "a")]
    [InlineData("hello_world-test", "hello-world-test")]
    [InlineData("---hello", "hello")]
    [InlineData("hello---world", "hello-world")]
    public void CaseConversion_EdgeCases_KebabCase(string input, string expected)
    {
        input.ToKebabCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("hello_world-test", "hello_world_test")]
    [InlineData("---hello", "hello")]
    public void CaseConversion_EdgeCases_SnakeCase(string input, string expected)
    {
        input.ToSnakeCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("İstanbul Ankara", "İstanbulAnkara")]
    [InlineData("çok güzel", "ÇokGüzel")]
    public void ToPascalCase_WithTurkishCulture_ShouldConvertCorrectly(string input, string expected)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        input.ToPascalCase(culture).Should().Be(expected);
    }

    [Theory]
    [InlineData("İstanbul Ankara", "istanbulAnkara")]
    [InlineData("çok güzel", "çokGüzel")]
    public void ToCamelCase_WithTurkishCulture_ShouldConvertCorrectly(string input, string expected)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        input.ToCamelCase(culture).Should().Be(expected);
    }

    [Theory]
    [InlineData("İstanbul Ankara", "istanbul_ankara")]
    [InlineData("İSTANBUL", "istanbul")]
    public void ToSnakeCase_WithTurkishCulture_ShouldConvertCorrectly(string input, string expected)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        input.ToSnakeCase(culture).Should().Be(expected);
    }

    [Theory]
    [InlineData("İstanbul Ankara", "istanbul-ankara")]
    public void ToKebabCase_WithTurkishCulture_ShouldConvertCorrectly(string input, string expected)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        input.ToKebabCase(culture).Should().Be(expected);
    }

    [Theory]
    [InlineData("İstanbul Ankara", "İstanbul Ankara")]
    public void ToTitleCase_WithTurkishCulture_ShouldConvertCorrectly(string input, string expected)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        input.ToTitleCase(culture).Should().Be(expected);
    }
}
