using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorTypeExtensionsTests
{
    private const ErrorType Undefined = (ErrorType)99;

    public static TheoryData<ErrorType, string, string, string> Spellings => new()
    {
        { ErrorType.Failure, "Failure", "failure", "failure" },
        { ErrorType.Unexpected, "Unexpected", "unexpected", "unexpected" },
        { ErrorType.Validation, "Validation", "validation", "validation" },
        { ErrorType.Conflict, "Conflict", "conflict", "conflict" },
        { ErrorType.NotFound, "NotFound", "not_found", "not-found" },
        { ErrorType.Unauthorized, "Unauthorized", "unauthorized", "unauthorized" },
        { ErrorType.Forbidden, "Forbidden", "forbidden", "forbidden" },
        { ErrorType.Unknown, "Unknown", "unknown", "unknown" },
    };

    [Theory]
    [MemberData(nameof(Spellings))]
    public void CaseHelpers_ShouldReturnExpectedSpellings(ErrorType type, string name, string snake, string kebab)
    {
        type.ToWireName().Should().Be(snake);
        type.ToOptimizedString().Should().Be(name);
        type.ToKebabCase().Should().Be(kebab);
        type.ToLowerCase().Should().BeLowerCased().And.BeEquivalentTo(name);
        type.ToUpperCase().Should().BeUpperCased().And.BeEquivalentTo(name);
    }

    [Theory]
    [MemberData(nameof(Spellings))]
    public void ParseWire_ShouldAcceptWireNameAndMemberNameInAnyCasing(ErrorType type, string name, string snake, string kebab)
    {
        ErrorTypeExtensions.ParseWire(snake).Should().Be(type);
        ErrorTypeExtensions.ParseWire(name).Should().Be(type);
        ErrorTypeExtensions.ParseWire(name.ToUpperInvariant()).Should().Be(type);
        ErrorTypeExtensions.TryParseWire(snake, out ErrorType parsed).Should().BeTrue();
        parsed.Should().Be(type);
        ErrorTypeExtensions.TryParseWire(kebab, out _).Should().Be(kebab == snake);
    }

    [Fact]
    public void ParseWire_WithDefinedNumber_ShouldReturnMember()
    {
        ErrorTypeExtensions.ParseWire("4").Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("99")]
    [InlineData("")]
    public void TryParseWire_WithUnknownText_ShouldReturnFalse(string text)
    {
        ErrorTypeExtensions.TryParseWire(text, out _).Should().BeFalse();
    }

    [Fact]
    public void TryParseWire_WithNull_ShouldReturnFalseAndDefault()
    {
        ErrorTypeExtensions.TryParseWire(null, out ErrorType value).Should().BeFalse();
        value.Should().Be(default(ErrorType));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("missing")]
    [InlineData("99")]
    public void ParseWire_WithInvalidText_ShouldThrowEnumValueException(string? text)
    {
        Action act = () => ErrorTypeExtensions.ParseWire(text);

        act.Should().Throw<EnumValueException>();
    }

    [Fact]
    public void ToWireName_WithUndefinedValue_ShouldThrowEnumValueException()
    {
        Action act = () => Undefined.ToWireName();

        act.Should().Throw<EnumValueException>();
    }

    [Fact]
    public void CaseHelpers_WithUndefinedValue_ShouldFallBackToNumber()
    {
        Undefined.ToOptimizedString().Should().Be("99");
        Undefined.ToKebabCase().Should().Be("99");
        Undefined.ToLowerCase().Should().Be("99");
        Undefined.ToUpperCase().Should().Be("99");
    }

    [Fact]
    public void IsDefined_ShouldDistinguishMembersFromOtherValues()
    {
        ErrorType.Forbidden.IsDefined().Should().BeTrue();
        Undefined.IsDefined().Should().BeFalse();
        ErrorTypeExtensions.IsDefined("NotFound").Should().BeTrue();
        ErrorTypeExtensions.IsDefined("not_found").Should().BeFalse();
    }

    [Fact]
    public void GetNamesAndGetValues_ShouldListMembersInDeclarationOrder()
    {
        ErrorTypeExtensions.GetNames().Should().Equal(Enum.GetNames<ErrorType>());
        ErrorTypeExtensions.GetValues().Should().Equal(Enum.GetValues<ErrorType>());
    }

    [Fact]
    public void AsUnderlyingType_ShouldReturnNumericValue()
    {
        ErrorType.Unauthorized.AsUnderlyingType().Should().Be(5);
        Undefined.AsUnderlyingType().Should().Be(99);
    }

    [Fact]
    public void WireNameConstants_ShouldMatchToWireName()
    {
        ErrorTypeExtensions.NotFoundWireName.Should().Be(ErrorType.NotFound.ToWireName());
        ErrorTypeExtensions.NotFoundKebabCase.Should().Be(ErrorType.NotFound.ToKebabCase());
    }
}
