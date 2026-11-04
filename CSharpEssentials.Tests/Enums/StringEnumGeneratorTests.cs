using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public void Names_And_Values_Should_Match_The_Array_Methods_And_Be_Cached()
    {
        ColorExtensions.Names.Should().Equal(ColorExtensions.GetNames());
        ColorExtensions.Values.Should().Equal(ColorExtensions.GetValues());
        GeneratedPermissionsExtensions.Names.Should().Equal(GeneratedPermissionsExtensions.GetNames());
        GeneratedPermissionsExtensions.Values.Should().Equal(GeneratedPermissionsExtensions.GetValues());
        ColorExtensions.Names.Should().BeSameAs(ColorExtensions.Names);
        ColorExtensions.Values.Should().BeSameAs(ColorExtensions.Values);
        ColorExtensions.GetNames().Should().NotBeSameAs(ColorExtensions.GetNames());
        ColorExtensions.Names.Should().NotBeAssignableTo<string[]>();
        ColorExtensions.Values.Should().NotBeAssignableTo<Color[]>();
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
    public void Constants_Should_Keep_Letters_Without_Case_And_Marks()
    {
        ScriptStatus.用户Name.ToString().Should().Be("用户Name");
        ScriptStatusExtensions.用户NameSnakeCase.Should().Be("用户_name");
        ScriptStatusExtensions.用户NameKebabCase.Should().Be("用户-name");
        ScriptStatusExtensions.Cafe\u0301SnakeCase.Should().Be("cafe\u0301");
        ScriptStatusExtensions.用户NameSnakeCase.Should().Be(CSharpEssentials.Core.StringExtensions.ToSnakeCase(nameof(ScriptStatus.用户Name)));
    }

    [Fact]
    [Obsolete("Pins the generated fallback of an obsolete generated helper.")]
    public void Fallback_Should_Keep_Letters_Without_Case()
    {
        (ScriptParts.用户 | ScriptParts.名).ToSnakeCase().Should().Be("用户_名");
        (ScriptParts.用户 | ScriptParts.名).ToKebabCase().Should().Be("用户-名");
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
    public void ParseWire_Should_Report_The_Same_Error_As_The_Data_Mode_Parser()
    {
        string longText = new('x', 80);
        EnumConventions reject = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };
        EnumMetadata.Get<Status>().TryParse(longText, EnumReadMode.Data, reject, out _, out EnumValueError? expected).Should().BeFalse();

        Action parse = () => StatusExtensions.ParseWire(longText);

        EnumValueError error = parse.Should().Throw<EnumValueException>().Which.Error;
        error.Value.Should().Be(expected!.Value).And.Be(new string('x', 64) + "…");
        error.AllowedValues.Should().Equal(expected.AllowedValues);
        error.EnumType.Should().Be(expected.EnumType);
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
    public void Generated_IsDefined_And_ToDescription_Should_Match_The_Runtime_Metadata()
    {
        AssertMatchesMetadata(Range(-2, 5).Select(static r => (Color)(int)r), static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(Range(-2, 260).Select(static r => (GeneratedPermissions)(byte)r).Distinct(), static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(Range(-2, 102).Select(static r => (GeneratedAttributes)(int)r), static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(Range(-7, 9).Concat([long.MinValue, long.MinValue + 1, long.MaxValue]).Select(static r => (GeneratedSigned)r),
            static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(new ulong[] { 0, 1, 2, ulong.MaxValue, ulong.MaxValue - 1 }.Select(static r => (GeneratedHuge)r),
            static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(Range(-5, 1002).Concat([int.MinValue, int.MaxValue]).Select(static r => (GeneratedSparse)(int)r),
            static v => v.IsDefined(), static v => v.ToDescription());
        AssertMatchesMetadata(Range(-2, 20).Select(static r => (GeneratedSignBit)(int)r).Concat([(GeneratedSignBit)int.MinValue]),
            static v => v.IsDefined(), static v => v.ToDescription());
    }

    [Fact]
    public void Flags_ToOptimizedString_Should_Match_Enum_ToString()
    {
        AssertFlagsText(Range(0, 255).Select(static r => (GeneratedPermissions)(byte)r), static v => v.ToOptimizedString(),
            static v => v.ToLowerCase(), static v => v.ToUpperCase());
        AssertFlagsText(Range(0, 1023).Select(static r => (GeneratedWidePermissions)(int)r).Concat([(GeneratedWidePermissions)(-1), (GeneratedWidePermissions)int.MinValue]),
            static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
        AssertFlagsText(Range(0, 40).Select(static r => (GeneratedFlagsNoZero)(uint)r).Concat([(GeneratedFlagsNoZero)uint.MaxValue]),
            static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
        AssertFlagsText(new ulong[] { 0, 1, 2, 3, 4, 1UL << 63, (1UL << 63) | 1, (1UL << 63) | 2, ulong.MaxValue }.Select(static r => (GeneratedHugeBits)r),
            static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
        AssertFlagsText(Range(0, 16).Select(static r => (GeneratedFlagsDuplicate)(int)r).Where(static v => !Enum.IsDefined(v)), static v => v.ToOptimizedString(),
            static v => v.ToLowerCase(), static v => v.ToUpperCase());
        AssertFlagsText(Range(-2, 20).Select(static r => (GeneratedSignBit)(int)r).Concat([(GeneratedSignBit)int.MinValue, (GeneratedSignBit)(int.MinValue | 5)]),
            static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
    }

    [Fact]
    public void Flags_ToOptimizedString_Should_Use_Composite_Members_Like_Enum_ToString()
    {
        (GeneratedPermissions.Read | GeneratedPermissions.Write | GeneratedPermissions.Delete).ToOptimizedString().Should().Be("ReadWrite, Delete");
        (GeneratedPermissions.Write | GeneratedPermissions.Delete).ToOptimizedString().Should().Be("Write, Delete");
        ((GeneratedPermissions)8).ToOptimizedString().Should().Be("8");
        ((GeneratedPermissions)9).ToOptimizedString().Should().Be("9");
        ((GeneratedFlagsNoZero)0).ToOptimizedString().Should().Be("0");
    }

    [Fact]
    public void ToDescription_Should_Return_The_Description_Of_The_First_Member_With_The_Value()
    {
        GeneratedAttributes.Pending.ToDescription().Should().Be("Waiting for work");
        GeneratedAttributes.Completed.ToDescription().Should().BeNull();
        ((GeneratedAttributes)42).ToDescription().Should().BeNull();
        GeneratedSparse.Low.ToDescription().Should().Be("Low end");
        GeneratedSparse.Shared.ToDescription().Should().BeNull();
        GeneratedSparse.SharedAlias.ToDescription().Should().BeNull();
        Color.Red.ToDescription().Should().BeNull();
    }

    [Fact]
    public void IsDefined_Should_Cover_Sparse_Negative_And_Extreme_Values()
    {
        ((GeneratedSparse)(-3)).IsDefined().Should().BeTrue();
        ((GeneratedSparse)(-2)).IsDefined().Should().BeFalse();
        ((GeneratedSparse)1000).IsDefined().Should().BeTrue();
        GeneratedSigned.Min.IsDefined().Should().BeTrue();
        ((GeneratedSigned)long.MaxValue).IsDefined().Should().BeFalse();
        GeneratedHuge.Max.IsDefined().Should().BeTrue();
        ((GeneratedHuge)1).IsDefined().Should().BeFalse();
        ((GeneratedPermissions)(1 | 2 | 4)).IsDefined().Should().BeTrue();
        ((GeneratedPermissions)255).IsDefined().Should().BeFalse();
        ((GeneratedSignBit)int.MinValue).IsDefined().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("en-US")]
    [InlineData("sv-SE")]
    [InlineData("tilde")]
    public void Undefined_Values_Should_Format_Like_The_Bcl_Under_Any_Culture(string cultureName)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo culture;
            if (cultureName == "tilde")
            {
                culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                culture.NumberFormat.NegativeSign = "~";
            }
            else
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }

            CultureInfo.CurrentCulture = culture;

            AssertBclText((Color)99, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((Color)(-1), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((Color)int.MinValue, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedSigned)(-1), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedSigned)long.MaxValue, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedHuge)(ulong.MaxValue - 1), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedSparse)0, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedSparse)(-4), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((NoZeroMember)0, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedPermissions)8, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText(GeneratedPermissions.Read | GeneratedPermissions.Delete, static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedPermissions)(1 | 8), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
            AssertBclText((GeneratedSignBit)(-3), static v => v.ToOptimizedString(), static v => v.ToLowerCase(), static v => v.ToUpperCase());
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Undefined_Non_Flags_Values_Should_Format_Without_Allocating_For_Small_Numbers()
    {
        Color undefined = (Color)99;
        _ = undefined.ToOptimizedString();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            _ = undefined.ToOptimizedString();

        (GC.GetAllocatedBytesForCurrentThread() - before).Should().Be(0);
    }

    private static void AssertBclText<T>(T value, Func<T, string> optimized, Func<T, string> lower, Func<T, string> upper)
        where T : struct, Enum
    {
        string expected = value.ToString();
        optimized(value).Should().Be(expected);
        lower(value).Should().Be(CultureInfo.InvariantCulture.TextInfo.ToLower(expected));
        upper(value).Should().Be(CultureInfo.InvariantCulture.TextInfo.ToUpper(expected));
    }

    [SuppressMessage("Globalization", "CA1308", Justification = "ToLowerCase is the lower invariant form of the Enum.ToString() text.")]
    private static void AssertFlagsText<T>(IEnumerable<T> values, Func<T, string> optimized, Func<T, string> lower, Func<T, string> upper)
        where T : struct, Enum
    {
        foreach (T value in values)
        {
            string expected = value.ToString();
            optimized(value).Should().Be(expected, "{0}", expected);
            lower(value).Should().Be(expected.ToLowerInvariant(), "{0}", expected);
            upper(value).Should().Be(expected.ToUpperInvariant(), "{0}", expected);
        }
    }

    private static IEnumerable<long> Range(long from, long to)
    {
        for (long r = from; r <= to; r++)
            yield return r;
    }

    private static void AssertMatchesMetadata<T>(IEnumerable<T> values, Func<T, bool> isDefined, Func<T, string?> description)
        where T : struct, Enum
    {
        EnumInfo<T> info = EnumMetadata.Get<T>();
        foreach (T value in values)
        {
            isDefined(value).Should().Be(info.IsDefined(value), "IsDefined({0})", value);
            description(value).Should().Be(info.TryGetMember(value, out EnumMemberInfo<T>? member) ? member.Description : null, "ToDescription({0})", value);
        }
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

    [Fact]
    public void Generated_Code_Should_Escape_Keyword_Member_Names()
    {
        GeneratedKeywords.@class.ToWireName().Should().Be("class");
        GeneratedKeywords.@event.ToWireName().Should().Be("event");
        GeneratedKeywordsExtensions.ParseWire("event").Should().Be(GeneratedKeywords.@event);
        EnumMetadata.Get<GeneratedKeywords>().TypedMembers.Select(static m => m.MemberName).Should().Equal("class", "event");
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

[StringEnum]
internal enum ScriptStatus
{
    用户Name,
    Cafe\u0301,
}

[StringEnum, Flags]
internal enum ScriptParts
{
    None = 0,
    用户 = 1,
    名 = 2,
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
    Pending = 0,

    [JsonStringEnumMemberName("done")]
    [EnumMember(Value = "finished")]
    Completed = 1,

    [EnumMember(Value = "cancelled_by_user")]
    Cancelled = 2,

    HTTPStatus = 3,

    [Obsolete("Use Cancelled")]
    Aborted = 4,

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

[StringEnum]
internal enum GeneratedKeywords
{
    @class,
    @event,
}

[StringEnum]
internal enum GeneratedSparse
{
    [Description("Low end")]
    Low = -3,

    Zero = 0,

    Shared = 7,

    [Description("Second name of seven")]
    [EnumAlias("shared_alias_value")]
    SharedAlias = Shared,

    [Description("High end")]
    High = 1000,
}

[StringEnum, Flags]
[SuppressMessage("Usage", "CSE0007", Justification = "Fixture for a flag in the sign bit, which sign-extends to 64 bits.")]
internal enum GeneratedSignBit
{
    None = 0,
    One = 1,
    Four = 4,
    Sign = int.MinValue,
}

[StringEnum, Flags]
[SuppressMessage("Usage", "CSE0007", Justification = "Fixture for composites over bits without a single flag member.")]
[SuppressMessage("Major Code Smell", "S4070", Justification = "Fixture for composites over bits without a single flag member.")]
internal enum GeneratedWidePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    Delete = 8,
    Share = 16,
    Archive = 32,
    Audit = 64,
    Admin = 128,
    ReadWrite = Read | Write,
    Owner = 256 | 512,
    Everything = Admin | Audit | Archive | Share | Delete | Execute | ReadWrite,
}

[StringEnum, Flags]
[SuppressMessage("Usage", "CSE0006", Justification = "Fixture for formatting zero without a zero member.")]
internal enum GeneratedFlagsNoZero : uint
{
    One = 1,
    Two = 2,
    Eight = 8,
    Top = 0x80000000,
}

[StringEnum, Flags]
internal enum GeneratedHugeBits : ulong
{
    None = 0,
    Low = 1,
    Next = 2,
    Top = 1UL << 63,
}

[StringEnum, Flags]
[SuppressMessage("Design", "CA1069", Justification = "Fixture for two flag members sharing a value.")]
internal enum GeneratedFlagsDuplicate
{
    None = 0,
    Read = 1,
    Write = 2,
    Modify = 2,
    Delete = 4,
}

[StringEnum]
internal enum NoZeroMember
{
    One = 1,
    Two = 2,
}
