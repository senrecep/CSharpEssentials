using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

public class StringEnumGeneratorTests
{
    [Fact]
    public void ToOptimizedString_Should_Return_Name()
    {
        Color c = Color.Red;
        c.ToOptimizedString().Should().Be("Red");
    }

    [Fact]
    public void ToOptimizedString_Should_Return_Name_For_Last_Member()
    {
        Color c = Color.Blue;
        c.ToOptimizedString().Should().Be("Blue");
    }

    [Fact]
    public void ToOptimizedString_Should_Return_Name_For_CamelCase_Member()
    {
        Status s = Status.InProgress;
        s.ToOptimizedString().Should().Be("InProgress");
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void ToSnakeCase_Should_Return_SnakeCase()
    {
        Status s = Status.InProgress;
        s.ToSnakeCase().Should().Be("in_progress");
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void ToSnakeCase_Should_Return_SnakeCase_For_Simple_Name()
    {
        Color c = Color.Red;
        c.ToSnakeCase().Should().Be("red");
    }

    [Fact]
    public void ToKebabCase_Should_Return_KebabCase()
    {
        Status s = Status.InProgress;
        s.ToKebabCase().Should().Be("in-progress");
    }

    [Fact]
    public void ToLowerCase_Should_Return_LowerCase()
    {
        Color c = Color.Red;
        c.ToLowerCase().Should().Be("red");
    }

    [Fact]
    public void ToUpperCase_Should_Return_UpperCase()
    {
        Color c = Color.Red;
        c.ToUpperCase().Should().Be("RED");
    }

    [Fact]
    public void IsDefined_Should_Return_True_For_Known()
    {
        ColorExtensions.IsDefined("Green").Should().BeTrue();
    }

    [Fact]
    public void IsDefined_Should_Return_False_For_Unknown()
    {
        ColorExtensions.IsDefined("Purple").Should().BeFalse();
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void TryParse_Should_Return_True_And_Value_For_Valid_Name()
    {
        bool result = ColorExtensions.TryParse("Green", out Color value);
        result.Should().BeTrue();
        value.Should().Be(Color.Green);
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void TryParse_Should_Return_False_For_Invalid_Name()
    {
        bool result = ColorExtensions.TryParse("Purple", out Color value);
        result.Should().BeFalse();
        value.Should().Be(Color.Red); // default(int) cast to enum = first value (0)
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void TryParse_Should_Parse_Numeric_Value()
    {
        bool result = ColorExtensions.TryParse("2", out Color value);
        result.Should().BeTrue();
        value.Should().Be(Color.Blue);
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void Parse_Should_Return_Value_For_Valid_Name()
    {
        Color value = ColorExtensions.Parse("Green");
        value.Should().Be(Color.Green);
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void Parse_Should_Throw_For_Invalid_Name()
    {
        Action act = () => ColorExtensions.Parse("Purple");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetNames_Should_Return_All_Names()
    {
        string[] names = ColorExtensions.GetNames();
        names.Should().ContainInOrder("Red", "Green", "Blue");
    }

    [Fact]
    public void GetValues_Should_Return_All_Values()
    {
        Color[] values = ColorExtensions.GetValues();
        values.Should().ContainInOrder(Color.Red, Color.Green, Color.Blue);
    }

    [Fact]
    public void AsUnderlyingType_Should_Return_Numeric_Value()
    {
        Color c = Color.Blue;
        c.AsUnderlyingType().Should().Be(2);
    }

    [Fact]
    public void Constants_Should_Have_Correct_SnakeCase()
    {
        StatusExtensions.InProgressSnakeCase.Should().Be("in_progress");
        StatusExtensions.NotStartedSnakeCase.Should().Be("not_started");
    }

    [Fact]
    public void Constants_Should_Have_Correct_KebabCase()
    {
        StatusExtensions.InProgressKebabCase.Should().Be("in-progress");
        StatusExtensions.NotStartedKebabCase.Should().Be("not-started");
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void ToSnakeCase_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatus h = HttpStatus.HTTPResponse;
        h.ToSnakeCase().Should().Be("httpresponse");
    }

    [Fact]
    public void ToKebabCase_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatus h = HttpStatus.HTTPResponse;
        h.ToKebabCase().Should().Be("httpresponse");
    }

    [Fact]
    [Obsolete("Pins the unchanged 4.x behavior of an obsolete generated helper.")]
    public void Fallback_Should_Handle_Unknown_Value()
    {
        var unknown = (HttpStatus)999;
        unknown.ToSnakeCase().Should().Be("999");
        unknown.ToKebabCase().Should().Be("999");
    }

    [Fact]
    public void Constants_Should_Handle_Consecutive_Uppercase()
    {
        HttpStatusExtensions.OKSnakeCase.Should().Be("ok");
        HttpStatusExtensions.NotFoundSnakeCase.Should().Be("not_found");
        HttpStatusExtensions.HTTPResponseSnakeCase.Should().Be("httpresponse");
        HttpStatusExtensions.HTTPResponseKebabCase.Should().Be("httpresponse");
    }

    [Fact]
    public void Nested_Enums_Should_Get_A_Namespace_Level_Extensions_Class()
    {
        NestedEnumContainer.NestedClassStatus.Completed.ToWireName().Should().Be("completed");
        NestedEnumStructContainer.NestedStructStatus.Pending.ToWireName().Should().Be("pending");
        NestedEnumContainer_NestedClassStatusExtensions.CompletedWireName.Should().Be("completed");
        EnumMetadata.IsRegistered(typeof(NestedEnumStructContainer.NestedStructStatus)).Should().BeTrue();
    }

    [Fact]
    public void ToWireName_Should_Use_The_Separator_Algorithm_Of_System_Text_Json()
    {
        HttpStatus.HTTPResponse.ToWireName().Should().Be("http_response");
        HttpStatusExtensions.HTTPResponseWireName.Should().Be("http_response");
        HttpStatusExtensions.OKWireName.Should().Be("ok");
        Status.InProgress.ToWireName().Should().Be(StatusExtensions.InProgressWireName);
    }

    [Fact]
    public void ToWireName_Should_Throw_For_Undefined_Values()
    {
        Action act = () => ((Color)42).ToWireName();

        act.Should().Throw<EnumValueException>().Which.Error.EnumType.Should().Be<Color>();
    }

    [Fact]
    public void ToWireName_Should_Write_Single_Flags_For_Flags_Values()
    {
        (GeneratedPermissions.Read | GeneratedPermissions.Write).ToWireName().Should().Be("read,write");
        GeneratedPermissions.ReadWrite.ToWireName().Should().Be("read,write");
        GeneratedPermissions.None.ToWireName().Should().Be("none");
    }

    [Theory]
    [InlineData("http_response")]
    [InlineData("HTTPResponse")]
    [InlineData("HTTP_RESPONSE")]
    [InlineData("httpresponse")]
    [InlineData("2")]
    public void TryParseWire_Should_Accept_Every_Known_Spelling(string text)
    {
        HttpStatusExtensions.TryParseWire(text, out HttpStatus value).Should().BeTrue();
        value.Should().Be(HttpStatus.HTTPResponse);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ok")]
    [InlineData("99")]
    [InlineData("bogus")]
    [InlineData("unknown")]
    public void TryParseWire_Should_Reject_Unknown_Values_Without_Using_The_Fallback(string? text)
    {
        bool known = text == "unknown";

        GeneratedAttributesExtensions.TryParseWire(text, out GeneratedAttributes value).Should().Be(known);
        if (known)
            value.Should().Be(GeneratedAttributes.Unknown);
    }

    [Fact]
    public void ParseWire_Should_Return_The_Value_Or_Throw_EnumValueException()
    {
        StatusExtensions.ParseWire("not_started").Should().Be(Status.NotStarted);

        Action unknown = () => StatusExtensions.ParseWire("bogus");
        Action missing = () => StatusExtensions.ParseWire(null);

        unknown.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("bogus");
        missing.Should().Throw<EnumValueException>().Which.Error.Value.Should().BeNull();
    }

    [Fact]
    public void IsDefined_Extension_Should_Be_Flags_Aware()
    {
        Color.Blue.IsDefined().Should().BeTrue();
        ((Color)42).IsDefined().Should().BeFalse();
        (GeneratedPermissions.Read | GeneratedPermissions.Delete).IsDefined().Should().BeTrue();
        ((GeneratedPermissions)8).IsDefined().Should().BeFalse();
    }

    [Fact]
    public void Generated_Metadata_Should_Carry_Member_Attributes()
    {
        EnumMetadata.IsRegistered(typeof(GeneratedAttributes)).Should().BeTrue();
        EnumInfo<GeneratedAttributes> info = EnumMetadata.Get<GeneratedAttributes>();

        info.WireNames.Should().Equal("waiting", "done", "cancelled_by_user", "http_status", "aborted", "unknown");
        info.Fallback!.Value.Should().Be(GeneratedAttributes.Unknown);
        info.Storage.Should().Be(EnumStorage.Integer);
        info.TypedMembers[0].Description.Should().Be("Waiting for work");
        info.TypedMembers[0].Aliases.Should().Equal("Started", "pending");
        info.TypedMembers[3].Aliases.Should().Equal("httpstatus");
        info.TypedMembers[4].IsObsolete.Should().BeTrue();
        info.TypedMembers[4].Value.Should().Be((GeneratedAttributes)4);
    }

    [Fact]
    public void Generated_Metadata_Should_Use_The_Enum_Naming()
    {
        KebabNamed.FirstValue.ToWireName().Should().Be("first-value");
        EnumMetadata.Get<KebabNamed>().WireNames.Should().Equal("first-value", "SECOND");
    }

    [Fact]
    public void Generated_Metadata_Should_Round_Trip_Extreme_Underlying_Values()
    {
        EnumInfo<GeneratedSigned> signed = EnumMetadata.Get<GeneratedSigned>();
        EnumInfo<GeneratedHuge> huge = EnumMetadata.Get<GeneratedHuge>();

        signed.TypedMembers.Select(static m => m.NumericText).Should().Equal("-5", "-9223372036854775808", "7");
        GeneratedSigned.Min.ToWireName().Should().Be("min");
        GeneratedSignedExtensions.ParseWire("-5").Should().Be(GeneratedSigned.Negative);
        huge.TypedMembers.Select(static m => m.NumericText).Should().Equal("0", "18446744073709551615");
        GeneratedHugeExtensions.ParseWire("18446744073709551615").Should().Be(GeneratedHuge.Max);
    }
}

[StringEnum]
internal enum Color
{
    Red,
    Green,
    Blue
}

[StringEnum]
internal enum Status
{
    NotStarted,
    InProgress,
    Completed
}

[StringEnum]
internal enum HttpStatus
{
    OK,
    NotFound,
    HTTPResponse
}

internal static class NestedEnumContainer
{
    [StringEnum]
    internal enum NestedClassStatus
    {
        Pending,
        Completed
    }
}

internal struct NestedEnumStructContainer
{
    [StringEnum]
    internal enum NestedStructStatus
    {
        Pending,
        Completed
    }
}

[StringEnum, Flags]
internal enum GeneratedPermissions : byte
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}

[StringEnum(Storage = EnumStorage.Integer)]
internal enum GeneratedAttributes
{
    [Description("Waiting for work")]
    [EnumAlias("Started")]
    [JsonStringEnumMemberName("waiting")]
    Pending,

    [JsonStringEnumMemberName("done")]
    [EnumMember(Value = "finished")]
    Completed,

    [EnumMember(Value = "cancelled_by_user")]
    Cancelled,

    HTTPStatus,

    [Obsolete("Use Cancelled")]
    Aborted,

    [EnumFallback]
    Unknown = 100,
}

[StringEnum(Naming = EnumNaming.KebabCaseLower)]
internal enum KebabNamed
{
    FirstValue,

    [JsonStringEnumMemberName("SECOND")]
    SecondValue,
}

[StringEnum]
internal enum GeneratedSigned : long
{
    Negative = -5,
    Min = long.MinValue,
    Positive = 7,
}

[StringEnum]
internal enum GeneratedHuge : ulong
{
    Zero = 0,
    Max = ulong.MaxValue,
}
