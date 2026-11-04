using System.Text.Json.Serialization;
using CSharpEssentials.Core;
using CSharpEssentials.Enums;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class EnumToStringConverterTests
{
    [StringEnum]
    internal enum TestStatus
    {
        Active,
        Inactive,
        PendingApproval,
        InProgress
    }

    [StringEnum]
    internal enum SimpleValue
    {
        One,
        Two,
        Three
    }

    [StringEnum]
    internal enum AcronymValue
    {
        HTTPStatus,
        IOError,
        Value1,
        PendingApproval
    }

    [StringEnum]
    internal enum ScriptValue
    {
        用户Name,
        Other
    }

    [StringEnum]
    internal enum CustomNamedValue
    {
        [JsonStringEnumMemberName("custom_name")]
        Original,
        Other
    }

    [Fact]
    public void Converter_ShouldConvertEnumToSnakeCase()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        string? result = valueConverter.ConvertToProvider(TestStatus.PendingApproval) as string;

        result.Should().Be("pending_approval");
    }

    [Fact]
    public void Converter_ShouldConvertSimpleEnumToSnakeCase()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        string? result = valueConverter.ConvertToProvider(TestStatus.Active) as string;

        result.Should().Be("active");
    }

    [Fact]
    public void Converter_ShouldConvertSnakeCaseToEnum()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        var result = valueConverter.ConvertFromProvider("pending_approval") as TestStatus?;

        result.Should().Be(TestStatus.PendingApproval);
    }

    [Fact]
    public void Converter_ShouldConvertSimpleSnakeCaseToEnum()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        var result = valueConverter.ConvertFromProvider("active") as TestStatus?;

        result.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void Converter_ShouldHandleInProgressValue()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        string? toProvider = valueConverter.ConvertToProvider(TestStatus.InProgress) as string;
        var fromProvider = valueConverter.ConvertFromProvider("in_progress") as TestStatus?;

        toProvider.Should().Be("in_progress");
        fromProvider.Should().Be(TestStatus.InProgress);
    }

    [Fact]
    public void Converter_ShouldBeRoundTrippable()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();
        ValueConverter<TestStatus, string> valueConverter = converter;

        foreach (TestStatus status in Enum.GetValues<TestStatus>())
        {
            string? snakeCase = valueConverter.ConvertToProvider(status) as string;
            var roundTripped = valueConverter.ConvertFromProvider(snakeCase!) as TestStatus?;

            roundTripped.Should().Be(status);
        }
    }

    [Fact]
    public void Converter_WithSimpleValue_ShouldConvertCorrectly()
    {
        EnumToFormattedStringConverter<SimpleValue> converter = new();
        ValueConverter<SimpleValue, string> valueConverter = converter;

        string? one = valueConverter.ConvertToProvider(SimpleValue.One) as string;
        string? two = valueConverter.ConvertToProvider(SimpleValue.Two) as string;
        string? three = valueConverter.ConvertToProvider(SimpleValue.Three) as string;

        one.Should().Be("one");
        two.Should().Be("two");
        three.Should().Be("three");
    }

    [Fact]
    public void Converter_ShouldBeValueConverter()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();

        converter.Should().BeAssignableTo<ValueConverter<TestStatus, string>>();
    }

    [Fact]
    public void Converter_ShouldHaveCorrectModelClrType()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();

        converter.ModelClrType.Should().Be<TestStatus>();
    }

    [Fact]
    public void Converter_ShouldHaveCorrectProviderClrType()
    {
        EnumToFormattedStringConverter<TestStatus> converter = new();

        converter.ProviderClrType.Should().Be<string>();
    }

    [Fact]
    public void ConvertToProviderExpression_Compiled_ShouldWriteWireName()
    {
        Func<AcronymValue, string> toProvider = new EnumToFormattedStringConverter<AcronymValue>().ConvertToProviderExpression.Compile();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
            toProvider(value).Should().Be(EnumValueFormatter.Format(value, EnumWireFormat.String));
    }

    [Fact]
    public void ConvertFromProviderExpression_Compiled_ShouldReadWireName()
    {
        Func<string, AcronymValue> fromProvider = new EnumToFormattedStringConverter<AcronymValue>().ConvertFromProviderExpression.Compile();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
            fromProvider(EnumValueFormatter.Format(value, EnumWireFormat.String)).Should().Be(value);
    }

    [Theory]
    [InlineData(nameof(AcronymValue.HTTPStatus), "http_status")]
    [InlineData(nameof(AcronymValue.IOError), "io_error")]
    [InlineData(nameof(AcronymValue.Value1), "value1")]
    [InlineData(nameof(AcronymValue.PendingApproval), "pending_approval")]
    public void Converter_AcronymEnum_ShouldWriteJsonName(string member, string expected)
    {
        AcronymValue value = Enum.Parse<AcronymValue>(member);
        ValueConverter<AcronymValue, string> converter = new EnumToFormattedStringConverter<AcronymValue>();

        string? result = converter.ConvertToProvider(value) as string;

        result.Should().Be(expected);
    }

    [Fact]
    public void Converter_ShouldHonorJsonStringEnumMemberName()
    {
        ValueConverter<CustomNamedValue, string> converter = new EnumToFormattedStringConverter<CustomNamedValue>();

        converter.ConvertToProvider(CustomNamedValue.Original).Should().Be("custom_name");
        converter.ConvertFromProvider("custom_name").Should().Be(CustomNamedValue.Original);
    }

    [Theory]
    [InlineData(nameof(AcronymValue.HTTPStatus), "httpstatus")]
    [InlineData(nameof(AcronymValue.IOError), "ioerror")]
    [InlineData(nameof(AcronymValue.Value1), "value_1")]
    [InlineData(nameof(AcronymValue.PendingApproval), "pending_approval")]
    public void LegacyConverter_ShouldWriteCoreSnakeCase(string member, string expected)
    {
        AcronymValue value = Enum.Parse<AcronymValue>(member);
        ValueConverter<AcronymValue, string> converter = new LegacySnakeCaseEnumConverter<AcronymValue>();

        string? result = converter.ConvertToProvider(value) as string;

        result.Should().Be(expected);
        result.Should().Be(value.ToString().ToSnakeCase());
    }

    [Fact]
    public void LegacyConverter_ShouldRoundTripMembersWithLettersWithoutCase()
    {
        ValueConverter<ScriptValue, string> converter = new LegacySnakeCaseEnumConverter<ScriptValue>();

        string? stored = converter.ConvertToProvider(ScriptValue.用户Name) as string;

        stored.Should().Be("用户_name");
        converter.ConvertFromProvider(stored).Should().Be(ScriptValue.用户Name);
    }

    [Theory]
    [InlineData(nameof(AcronymValue.HTTPStatus))]
    [InlineData(nameof(AcronymValue.IOError))]
    [InlineData(nameof(AcronymValue.Value1))]
    public void CanonicalAndLegacy_ShouldDifferForAcronymsAndDigits(string member)
    {
        AcronymValue value = Enum.Parse<AcronymValue>(member);
        ValueConverter<AcronymValue, string> canonical = new EnumToFormattedStringConverter<AcronymValue>();
        ValueConverter<AcronymValue, string> legacy = new LegacySnakeCaseEnumConverter<AcronymValue>();

        string? canonicalName = canonical.ConvertToProvider(value) as string;
        string? legacyName = legacy.ConvertToProvider(value) as string;

        canonicalName.Should().NotBe(legacyName);
    }

    [Fact]
    public void CanonicalConverter_ShouldReadLegacyFormatRows()
    {
        ValueConverter<AcronymValue, string> canonical = new EnumToFormattedStringConverter<AcronymValue>();
        ValueConverter<AcronymValue, string> legacy = new LegacySnakeCaseEnumConverter<AcronymValue>();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
        {
            string legacyName = (string)legacy.ConvertToProvider(value)!;

            canonical.ConvertFromProvider(legacyName).Should().Be(value);
        }
    }

    [Flags]
    internal enum LegacyAccess
    {
        None = 0,
        ReadHTTP = 1,
        Write2 = 2,
        Alias = Write2,
        All = ReadHTTP | Write2
    }

    private static string ReferenceWrite<TEnum>(TEnum value) where TEnum : struct, Enum => value.ToString().ToSnakeCase();

    private static TEnum ReferenceRead<TEnum>(string value) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(value.ToPascalCase(), true);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(-1)]
    public void LegacyConverter_ShouldMatchUncachedFormat_ForMembersCombinationsAndUndefinedValues(int raw)
    {
        LegacyAccess value = (LegacyAccess)raw;
        ValueConverter<LegacyAccess, string> legacy = new LegacySnakeCaseEnumConverter<LegacyAccess>();

        string written = (string)legacy.ConvertToProvider(value)!;

        written.Should().Be(ReferenceWrite(value));
        legacy.ConvertFromProvider(written).Should().Be(ReferenceRead<LegacyAccess>(written));
    }

    [Theory]
    [InlineData("read_http")]
    [InlineData("READHTTP")]
    [InlineData("Write2")]
    [InlineData("write_2")]
    [InlineData("alias")]
    [InlineData("all")]
    [InlineData("3")]
    public void LegacyConverter_ShouldMatchUncachedRead_ForAnyCasingAndFormat(string stored)
    {
        ValueConverter<LegacyAccess, string> legacy = new LegacySnakeCaseEnumConverter<LegacyAccess>();

        legacy.ConvertFromProvider(stored).Should().Be(ReferenceRead<LegacyAccess>(stored));
    }

    [Fact]
    public void LegacyConverter_ShouldThrowLikeUncachedRead_ForUnknownName()
    {
        ValueConverter<LegacyAccess, string> legacy = new LegacySnakeCaseEnumConverter<LegacyAccess>();

        Action act = () => legacy.ConvertFromProvider("missing");

        act.Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => ReferenceRead<LegacyAccess>("missing")).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void LegacyConverter_ShouldReuseTheStoredName_ForDefinedMembers()
    {
        ValueConverter<AcronymValue, string> legacy = new LegacySnakeCaseEnumConverter<AcronymValue>();

        object? first = legacy.ConvertToProvider(AcronymValue.HTTPStatus);
        object? second = legacy.ConvertToProvider(AcronymValue.HTTPStatus);

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void LegacyConverter_ShouldRoundTrip()
    {
        ValueConverter<AcronymValue, string> legacy = new LegacySnakeCaseEnumConverter<AcronymValue>();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
            legacy.ConvertFromProvider(legacy.ConvertToProvider(value)).Should().Be(value);
    }

    [Fact]
    public void CanonicalConverter_ShouldReadPascalCaseAndUpperCase()
    {
        ValueConverter<AcronymValue, string> converter = new EnumToFormattedStringConverter<AcronymValue>();

        converter.ConvertFromProvider("HTTPStatus").Should().Be(AcronymValue.HTTPStatus);
        converter.ConvertFromProvider("IO_ERROR").Should().Be(AcronymValue.IOError);
    }

    [Fact]
    public void CanonicalConverter_ShouldThrowForUnknownValue()
    {
        Func<AcronymValue> fromProvider = () => (AcronymValue)new EnumToFormattedStringConverter<AcronymValue>().ConvertFromProvider("not_a_member")!;

        fromProvider.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("99")]
    [InlineData("-5")]
    [InlineData("http status")]
    [InlineData("Http-Status")]
    public void CanonicalConverter_ShouldThrow_When_ValueIsNumericOrLooselyFormatted(string value)
    {
        ValueConverter<AcronymValue, string> converter = new EnumToFormattedStringConverter<AcronymValue>();

        Action read = () => converter.ConvertFromProvider(value);

        read.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("httpstatus", nameof(AcronymValue.HTTPStatus))]
    [InlineData("HTTPSTATUS", nameof(AcronymValue.HTTPStatus))]
    [InlineData("value_1", nameof(AcronymValue.Value1))]
    [InlineData("ioerror", nameof(AcronymValue.IOError))]
    public void CanonicalConverter_ShouldRead_When_ValueIsLegacyFormat(string value, string expected)
    {
        ValueConverter<AcronymValue, string> converter = new EnumToFormattedStringConverter<AcronymValue>();

        object? result = converter.ConvertFromProvider(value);

        result.Should().Be(Enum.Parse<AcronymValue>(expected));
    }
}

