using System.Text.Json;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumNameConverterTests
{
    public static readonly TheoryData<string> Corpus =
    [
        "A", "Ab", "ABC", "Pending", "PendingApproval", "HTTPStatus", "HTTPResponse", "XMLHttpRequest", "IOStream",
        "URLValue", "Value1", "Value10Items", "V1Api", "Is64Bit", "HTML5Parser", "Item2B", "ABC123Def", "already_snake",
        "Mixed_Underscore", "myValue", "lowercase", "UPPERCASE", "ÇağrıMerkezi", "ÖzelDurum", "Id", "ID", "IDs", "OAuth2Token",
        "_Leading", "Trailing_", "A1B2C3",
    ];

    [Theory]
    [MemberData(nameof(Corpus))]
    public void Separated_Names_Should_Match_JsonNamingPolicy(string name)
    {
        EnumNameConverter.Convert(name, EnumNaming.SnakeCaseLower).Should().Be(JsonNamingPolicy.SnakeCaseLower.ConvertName(name));
        EnumNameConverter.Convert(name, EnumNaming.SnakeCaseUpper).Should().Be(JsonNamingPolicy.SnakeCaseUpper.ConvertName(name));
        EnumNameConverter.Convert(name, EnumNaming.KebabCaseLower).Should().Be(JsonNamingPolicy.KebabCaseLower.ConvertName(name));
        EnumNameConverter.Convert(name, EnumNaming.KebabCaseUpper).Should().Be(JsonNamingPolicy.KebabCaseUpper.ConvertName(name));
    }

    [Theory]
    [MemberData(nameof(Corpus))]
    public void CamelCase_Should_Match_JsonNamingPolicy(string name) =>
        EnumNameConverter.Convert(name, EnumNaming.CamelCase).Should().Be(JsonNamingPolicy.CamelCase.ConvertName(name));

    [Theory]
    [MemberData(nameof(Corpus))]
    public void PascalCase_Should_Return_Member_Name(string name) =>
        EnumNameConverter.Convert(name, EnumNaming.PascalCase).Should().Be(name);

    [Fact]
    public void Default_Should_Be_SnakeCaseLower() =>
        EnumNameConverter.Convert("HTTPStatus", EnumNaming.Default).Should().Be("http_status");

    [Theory]
    [InlineData("HTTPResponse", "httpresponse")]
    [InlineData("InProgress", "in_progress")]
    [InlineData("Value1", "value_1")]
    public void LegacySnakeCase_Should_Keep_The_4x_Output(string name, string expected) =>
        EnumNameConverter.ToLegacySnakeCase(name).Should().Be(expected);

    // Member names never hold spaces, but the converters are ports of JsonNamingPolicy and must stay in step with it.
    public static readonly TheoryData<string> SpacedNames =
    [
        "Foo Bar", "Foo bar", "foo bar", " Leading", "Trailing ", "Two  Spaces", "AB C", "ABC Def", "A B", "HTTP status", "Value 1", "",
    ];

    [Theory]
    [MemberData(nameof(SpacedNames))]
    public void Separated_Names_With_Spaces_Should_Match_JsonNamingPolicy(string name)
    {
        EnumNameConverter.Convert(name, EnumNaming.SnakeCaseLower).Should().Be(JsonNamingPolicy.SnakeCaseLower.ConvertName(name));
        EnumNameConverter.Convert(name, EnumNaming.KebabCaseUpper).Should().Be(JsonNamingPolicy.KebabCaseUpper.ConvertName(name));
    }

    [Theory]
    [MemberData(nameof(SpacedNames))]
    public void CamelCase_With_Spaces_Should_Match_JsonNamingPolicy(string name) =>
        EnumNameConverter.Convert(name, EnumNaming.CamelCase).Should().Be(JsonNamingPolicy.CamelCase.ConvertName(name));

    [Fact]
    public void Undefined_Naming_Should_Fall_Back_To_SnakeCaseLower() =>
        EnumNameConverter.Convert("PendingApproval", (EnumNaming)99).Should().Be("pending_approval");
}
