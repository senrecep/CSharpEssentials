using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;
using System.Text.Json;

namespace CSharpEssentials.Tests.Json;

[StringEnum]
internal enum TestStringEnumType
{
    FirstValue,
    SecondValue,
    ThirdValue
}

internal enum RegularEnumType
{
    First,
    Second
}

public class ConditionalStringEnumConverterTests
{
    private static readonly JsonSerializerOptions StringEnumOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions AllowIntegerOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter(allowIntegerValues: true) }
    };

    private static readonly JsonSerializerOptions DisallowIntegerOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter(allowIntegerValues: false) }
    };

    [Fact]
    public void Serialize_WithStringEnumAttribute_ShouldSerializeAsString()
    {
        string json = JsonSerializer.Serialize(TestStringEnumType.FirstValue, StringEnumOptions);

        json.Should().Be("\"first_value\"");
    }

    [Fact]
    public void Deserialize_WithStringEnumAttribute_ShouldDeserializeFromString()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("\"first_value\"", StringEnumOptions);

        value.Should().Be(TestStringEnumType.FirstValue);
    }

    [Fact]
    public void Serialize_WithoutStringEnumAttribute_ShouldSerializeAsInteger()
    {
        string json = JsonSerializer.Serialize(RegularEnumType.First, StringEnumOptions);

        json.Should().Be("0");
    }

    [Fact]
    public void Serialize_WithCustomNamingPolicy_ShouldUsePolicy()
    {
        string json = JsonSerializer.Serialize(TestStringEnumType.FirstValue, CamelCaseOptions);

        json.Should().Be("\"firstValue\"");
    }

    [Fact]
    public void Deserialize_WithIntegerValue_ShouldWorkWhenAllowed()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("0", AllowIntegerOptions);

        value.Should().Be(TestStringEnumType.FirstValue);
    }

    [Fact]
    public void Deserialize_WithIntegerValue_ShouldFailWhenNotAllowed()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestStringEnumType>("0", DisallowIntegerOptions));
    }

    [Fact]
    public void CanConvert_WithStringEnumAttribute_ShouldReturnTrue()
    {
        ConditionalStringEnumConverter converter = new();

        converter.CanConvert(typeof(TestStringEnumType)).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithoutStringEnumAttribute_ShouldReturnFalse()
    {
        ConditionalStringEnumConverter converter = new();

        converter.CanConvert(typeof(RegularEnumType)).Should().BeFalse();
    }

    [Fact]
    public void CanConvert_WithCustomPredicate_ShouldUsePredicate()
    {
        ConditionalStringEnumConverter converter = new(
            canConvert: type => type == typeof(RegularEnumType));

        converter.CanConvert(typeof(RegularEnumType)).Should().BeTrue();
        converter.CanConvert(typeof(TestStringEnumType)).Should().BeFalse();
    }
}
