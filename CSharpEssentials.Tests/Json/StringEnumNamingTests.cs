using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class StringEnumNamingTests
{
    private static readonly JsonSerializerOptions SnakeCaseJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    [StringEnum]
    private enum NamingStatus
    {
        Active,
        Inactive,
        PendingApproval
    }

    [StringEnum]
    private enum AcronymKind
    {
        HTTPStatus,
        IOError,
        Value1,
        XMLHttpRequest
    }

    [StringEnum]
    private enum CustomNamed
    {
        [JsonStringEnumMemberName("custom")]
        Original,
        Plain
    }

    [Flags]
    [StringEnum]
    private enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }

    private enum PlainKind
    {
        First,
        Second
    }

    [Fact]
    public void DefaultPolicy_ShouldBeSnakeCaseLower()
    {
        StringEnumNaming.DefaultPolicy.Should().BeSameAs(JsonNamingPolicy.SnakeCaseLower);
    }

    [Fact]
    public void IsStringEnum_ShouldBeTrueOnlyForEnumsWithAttribute()
    {
        StringEnumNaming.IsStringEnum(typeof(NamingStatus)).Should().BeTrue();
        StringEnumNaming.IsStringEnum(typeof(PlainKind)).Should().BeFalse();
        StringEnumNaming.IsStringEnum(typeof(string)).Should().BeFalse();
        StringEnumNaming.IsStringEnum(typeof(int)).Should().BeFalse();
        StringEnumNaming.IsStringEnum(typeof(StringEnumNamingTests)).Should().BeFalse();
        StringEnumNaming.IsStringEnum(null!).Should().BeFalse();
    }

    [Fact]
    public void GetName_ShouldUseSnakeCaseByDefault()
    {
        StringEnumNaming.GetName(NamingStatus.Active).Should().Be("active");
        StringEnumNaming.GetName(NamingStatus.PendingApproval).Should().Be("pending_approval");
    }

    [Theory]
    [InlineData(nameof(AcronymKind.HTTPStatus))]
    [InlineData(nameof(AcronymKind.IOError))]
    [InlineData(nameof(AcronymKind.Value1))]
    [InlineData(nameof(AcronymKind.XMLHttpRequest))]
    public void GetName_ShouldMatchJsonNamingPolicySnakeCaseLower(string member)
    {
        AcronymKind value = Enum.Parse<AcronymKind>(member);

        StringEnumNaming.GetName(value).Should().Be(JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString()));
    }

    [Fact]
    public void GetName_ShouldHandleAcronymsAndDigits()
    {
        StringEnumNaming.GetName(AcronymKind.HTTPStatus).Should().Be("http_status");
        StringEnumNaming.GetName(AcronymKind.IOError).Should().Be("io_error");
        StringEnumNaming.GetName(AcronymKind.Value1).Should().Be("value1");
        StringEnumNaming.GetName(AcronymKind.XMLHttpRequest).Should().Be("xml_http_request");
    }

    [Fact]
    public void GetName_ShouldMatchJsonStringEnumConverterOutput()
    {
        foreach (AcronymKind value in Enum.GetValues<AcronymKind>())
        {
            JsonSerializer.Serialize(value, SnakeCaseJsonOptions).Should().Be($"\"{StringEnumNaming.GetName(value)}\"");
        }
    }

    [Fact]
    public void GetName_NonGeneric_ShouldMatchGeneric()
    {
        object boxed = AcronymKind.HTTPStatus;

        StringEnumNaming.GetName(boxed.GetType(), boxed).Should().Be("http_status");
    }

    [Fact]
    public void GetNames_ShouldReturnNamesInDeclarationOrder()
    {
        StringEnumNaming.GetNames<AcronymKind>().Should().Equal("http_status", "io_error", "value1", "xml_http_request");
        StringEnumNaming.GetNames(NamingStatus.Active.GetType()).Should().Equal("active", "inactive", "pending_approval");
    }

    [Fact]
    public void JsonStringEnumMemberName_ShouldOverridePolicy()
    {
        StringEnumNaming.GetName(CustomNamed.Original).Should().Be("custom");
        StringEnumNaming.GetName(CustomNamed.Plain).Should().Be("plain");
        StringEnumNaming.GetNames<CustomNamed>().Should().Equal("custom", "plain");
        StringEnumNaming.GetName(CustomNamed.Original, JsonNamingPolicy.CamelCase).Should().Be("custom");
    }

    [Fact]
    public void JsonStringEnumMemberName_ShouldBeParsableByCustomAndMemberName()
    {
        StringEnumNaming.TryParse("custom", out CustomNamed custom).Should().BeTrue();
        custom.Should().Be(CustomNamed.Original);
        StringEnumNaming.TryParse("Original", out CustomNamed original).Should().BeTrue();
        original.Should().Be(CustomNamed.Original);
    }

    [Fact]
    public void CustomPolicy_ShouldBeCachedSeparatelyFromDefault()
    {
        StringEnumNaming.GetNames<NamingStatus>().Should().Equal("active", "inactive", "pending_approval");
        StringEnumNaming.GetNames<NamingStatus>(JsonNamingPolicy.CamelCase).Should().Equal("active", "inactive", "pendingApproval");
        StringEnumNaming.GetName(NamingStatus.PendingApproval).Should().Be("pending_approval");
        StringEnumNaming.GetName(NamingStatus.PendingApproval, JsonNamingPolicy.CamelCase).Should().Be("pendingApproval");
    }

    [Fact]
    public void CustomPolicy_TryParse_ShouldAcceptPolicyName()
    {
        StringEnumNaming.TryParse("pendingApproval", out NamingStatus result, JsonNamingPolicy.CamelCase).Should().BeTrue();
        result.Should().Be(NamingStatus.PendingApproval);
    }

    [Fact]
    public void GetName_FlagsCombination_ShouldJoinWithCommaSpace()
    {
        StringEnumNaming.GetName(Permissions.Read | Permissions.Write).Should().Be("read, write");
        StringEnumNaming.GetName(Permissions.Read | Permissions.Write | Permissions.Execute).Should().Be("read, write, execute");
        StringEnumNaming.GetName(Permissions.None).Should().Be("none");
    }

    [Fact]
    public void GetName_FlagsCombination_ShouldMatchJsonStringEnumConverter()
    {
        JsonSerializer.Serialize(Permissions.Read | Permissions.Execute, SnakeCaseJsonOptions)
            .Should().Be($"\"{StringEnumNaming.GetName(Permissions.Read | Permissions.Execute)}\"");
    }

    [Fact]
    public void GetName_UndefinedValue_ShouldReturnNumber()
    {
        StringEnumNaming.GetName((NamingStatus)99).Should().Be("99");
        StringEnumNaming.GetName((Permissions)8).Should().Be("8");
        StringEnumNaming.GetName((Permissions)9).Should().Be("9");
    }

    [Theory]
    [InlineData("pending_approval", nameof(NamingStatus.PendingApproval))]
    [InlineData("PendingApproval", nameof(NamingStatus.PendingApproval))]
    [InlineData("ACTIVE", nameof(NamingStatus.Active))]
    [InlineData("active", nameof(NamingStatus.Active))]
    [InlineData("Active", nameof(NamingStatus.Active))]
    [InlineData("PENDING_APPROVAL", nameof(NamingStatus.PendingApproval))]
    [InlineData("  inactive  ", nameof(NamingStatus.Inactive))]
    [InlineData("1", nameof(NamingStatus.Inactive))]
    public void TryParse_ValidInput_ShouldSucceed(string input, string expected)
    {
        StringEnumNaming.TryParse(input, out NamingStatus result).Should().BeTrue();
        result.Should().Be(Enum.Parse<NamingStatus>(expected));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("99")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData(" 1x")]
    [InlineData("unknown")]
    [InlineData("active,inactive")]
    [InlineData("pending approval")]
    public void TryParse_InvalidInput_ShouldFail(string? input)
    {
        StringEnumNaming.TryParse(input, out NamingStatus result).Should().BeFalse();
        result.Should().Be(default(NamingStatus));
    }

    [Fact]
    public void TryParse_AllowIntegerValuesFalse_ShouldRejectNumbers()
    {
        StringEnumNaming.TryParse("1", out NamingStatus _, allowIntegerValues: false).Should().BeFalse();
        StringEnumNaming.TryParse("0", out NamingStatus _, allowIntegerValues: false).Should().BeFalse();
        StringEnumNaming.TryParse("active", out NamingStatus result, allowIntegerValues: false).Should().BeTrue();
        result.Should().Be(NamingStatus.Active);
    }

    [Theory]
    [InlineData("http_status", nameof(AcronymKind.HTTPStatus))]
    [InlineData("HTTPStatus", nameof(AcronymKind.HTTPStatus))]
    [InlineData("io_error", nameof(AcronymKind.IOError))]
    [InlineData("IOError", nameof(AcronymKind.IOError))]
    [InlineData("value1", nameof(AcronymKind.Value1))]
    [InlineData("Value1", nameof(AcronymKind.Value1))]
    public void TryParse_AcronymNames_ShouldSucceed(string input, string expected)
    {
        StringEnumNaming.TryParse(input, out AcronymKind result).Should().BeTrue();
        result.Should().Be(Enum.Parse<AcronymKind>(expected));
    }

    [Theory]
    [InlineData("read, write", 3)]
    [InlineData("Read,Write", 3)]
    [InlineData("read,write,execute", 7)]
    [InlineData(" read , EXECUTE ", 5)]
    [InlineData("read", 1)]
    [InlineData("none", 0)]
    [InlineData("3", 3)]
    [InlineData("7", 7)]
    [InlineData("1, write", 3)]
    public void TryParse_Flags_ShouldCombine(string input, int expected)
    {
        StringEnumNaming.TryParse(input, out Permissions result).Should().BeTrue();
        result.Should().Be((Permissions)expected);
    }

    [Theory]
    [InlineData("read,,write")]
    [InlineData("read,")]
    [InlineData(",read")]
    [InlineData("read, unknown")]
    [InlineData("8")]
    [InlineData("9")]
    public void TryParse_Flags_InvalidInput_ShouldFail(string input)
    {
        StringEnumNaming.TryParse(input, out Permissions _).Should().BeFalse();
    }

    [Fact]
    public void TryParse_NonGeneric_ShouldReturnBoxedEnum()
    {
        StringEnumNaming.TryParse(typeof(NamingStatus), "pending_approval", out object? result).Should().BeTrue();
        result.Should().Be(NamingStatus.PendingApproval);

        StringEnumNaming.TryParse(typeof(NamingStatus), "nope", out object? failed).Should().BeFalse();
        failed.Should().BeNull();
    }

    [Fact]
    public void GetTable_WithNonEnumType_ShouldThrowArgumentException()
    {
        Action getNames = () => StringEnumNaming.GetNames(typeof(string));
        Action getName = () => StringEnumNaming.GetName(typeof(int), 1);
        Action tryParse = () => StringEnumNaming.TryParse(typeof(StringEnumNamingTests), "x", out object? _);

        getNames.Should().Throw<ArgumentException>().WithParameterName("enumType");
        getName.Should().Throw<ArgumentException>().WithParameterName("enumType");
        tryParse.Should().Throw<ArgumentException>().WithParameterName("enumType");
    }

    [Fact]
    public void RoundTrip_GetNameThenTryParse_ShouldReturnOriginal()
    {
        AssertRoundTrip<NamingStatus>();
        AssertRoundTrip<AcronymKind>();
        AssertRoundTrip<CustomNamed>();
        AssertRoundTrip<Permissions>();
        AssertRoundTrip<PlainKind>();

        StringEnumNaming.TryParse(StringEnumNaming.GetName(Permissions.Read | Permissions.Execute), out Permissions flags).Should().BeTrue();
        flags.Should().Be(Permissions.Read | Permissions.Execute);
    }

    [Fact]
    public void RoundTrip_WithCamelCasePolicy_ShouldReturnOriginal()
    {
        foreach (AcronymKind value in Enum.GetValues<AcronymKind>())
        {
            string name = StringEnumNaming.GetName(value, JsonNamingPolicy.CamelCase);
            StringEnumNaming.TryParse(name, out AcronymKind parsed, JsonNamingPolicy.CamelCase).Should().BeTrue();
            parsed.Should().Be(value);
        }
    }

    private static void AssertRoundTrip<TEnum>() where TEnum : struct, Enum
    {
        foreach (TEnum value in Enum.GetValues<TEnum>())
        {
            string name = StringEnumNaming.GetName(value);
            StringEnumNaming.TryParse(name, out TEnum parsed, allowIntegerValues: false).Should().BeTrue($"'{name}' should parse back to {value}");
            parsed.Should().Be(value);
        }
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(99, false)]
    [InlineData(-1, false)]
    public void IsDefined_ShouldReportWhetherValueIsAMember(int value, bool expected)
    {
        StringEnumNaming.IsDefined((NamingStatus)value).Should().Be(expected);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(3, true)]
    [InlineData(7, true)]
    [InlineData(8, false)]
    [InlineData(9, false)]
    public void IsDefined_ShouldAcceptCombinationsOfDefinedFlags(int value, bool expected)
    {
        StringEnumNaming.IsDefined((Permissions)value).Should().Be(expected);
    }
}
