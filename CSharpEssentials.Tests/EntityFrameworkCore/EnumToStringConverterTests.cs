using CSharpEssentials.EntityFrameworkCore.Converters;
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
}

