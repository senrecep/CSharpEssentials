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

    [Fact]
    public void ToSnakeCase_Should_Not_Overflow_The_Stack_When_Input_Is_Very_Large()
    {
        ChildProcessScenario.Run(OversizedInputScenarios.ToSnakeCase);
    }

    [Theory]
    [InlineData("用户名", "用户名")]
    [InlineData("user用户Name", "user用户_name")]
    [InlineData("cafe\u0301", "cafe\u0301")]
    [InlineData("foo\u01C5emal", "foo_\u01C6emal")]
    [InlineData("עברית שלום", "עברית_שלום")]
    [InlineData("สวัสดีWorld", "สวัสดี_world")]
    public void ToSnakeCase_KeepsLettersWithoutCaseTitlecaseLettersAndMarks(string input, string expected)
    {
        input.ToSnakeCase().Should().Be(expected);
    }

    [Fact]
    public void ToKebabCase_KeepsArabicLetters()
    {
        "مرحبا بالعالم".ToKebabCase().Should().Be("مرحبا-بالعالم");
    }

    [Fact]
    public void ToPascalCase_KeepsLettersWithoutCase()
    {
        "user_用户_name".ToPascalCase().Should().Be("User用户Name");
    }

    [Fact]
    public void ToSnakeCase_KeepsLettersWithoutCase_When_Input_Uses_The_Pooled_Buffer()
    {
        string input = string.Concat(Enumerable.Repeat("用户Name", 64));

        input.ToSnakeCase().Should().Be(string.Concat(Enumerable.Repeat("用户_name", 64)));
    }

    [Theory]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(129)]
    [InlineData(256)]
    [InlineData(257)]
    [InlineData(600)]
    public void ToSnakeCase_Fits_When_Every_Character_Needs_A_Separator(int pairs)
    {
        string input = string.Concat(Enumerable.Repeat("A1", pairs));

        input.ToSnakeCase().Should().Be(string.Join("_", Enumerable.Repeat("a_1", pairs)));
        input.ToTitleCase().Should().Be(string.Join(" ", Enumerable.Repeat("A 1", pairs)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(511)]
    [InlineData(512)]
    [InlineData(600)]
    public void ToUnderscoreCamelCase_Fits_When_Input_Has_No_Separators(int length)
    {
        string input = new('a', length);

        input.ToUnderscoreCamelCase().Should().Be("_" + input);
        input.ToCamelCase().Should().Be(input);
        input.ToPascalCase().Should().Be("A" + input[1..]);
    }

    [Fact]
    public void ToSnakeCase_Handles_Mixed_Ascii_And_Non_Ascii_Characters()
    {
        "Über_straßeName用户ID9".ToSnakeCase().Should().Be("über_straße_name用户_id_9");
        "Über_straßeName用户ID9".ToMacroCase().Should().Be("ÜBER_STRAßE_NAME用户_ID_9");
    }

    [Fact]
    public void ToMacroCase_WithTurkishCulture_Uses_The_Culture_For_Ascii_Letters()
    {
        "isim".ToMacroCase(CultureInfo.GetCultureInfo("tr-TR")).Should().Be("İSİM");
        "isim".ToMacroCase().Should().Be("ISIM");
    }
}
