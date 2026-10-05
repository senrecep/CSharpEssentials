using System.Text.Json.Serialization;
using CSharpEssentials.Core;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class EnumToStringConverterTests
{
    private enum TestStatus
    {
        Active,
        Inactive,
        PendingApproval,
        InProgress
    }

    private enum SimpleValue
    {
        One,
        Two,
        Three
    }

    private enum AcronymValue
    {
        HTTPStatus,
        IOError,
        Value1,
        PendingApproval
    }

    private enum CustomNamedValue
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
    public void ConvertToProviderExpression_Compiled_ShouldWriteStringEnumNamingName()
    {
        Func<AcronymValue, string> toProvider = new EnumToFormattedStringConverter<AcronymValue>().ConvertToProviderExpression.Compile();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
            toProvider(value).Should().Be(StringEnumNaming.GetName(value));
    }

    [Fact]
    public void ConvertFromProviderExpression_Compiled_ShouldReadStringEnumNamingName()
    {
        Func<string, AcronymValue> fromProvider = new EnumToFormattedStringConverter<AcronymValue>().ConvertFromProviderExpression.Compile();

        foreach (AcronymValue value in Enum.GetValues<AcronymValue>())
            fromProvider(StringEnumNaming.GetName(value)).Should().Be(value);
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

