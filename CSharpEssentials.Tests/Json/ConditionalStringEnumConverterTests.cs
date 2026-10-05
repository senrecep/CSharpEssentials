using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;

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

[StringEnum]
internal enum ConverterAcronymKind
{
    HTTPStatus,
    IOError,
    Value1,
    PendingApproval
}

[StringEnum]
[Flags]
internal enum ConverterFlagsKind
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4
}

public class ConditionalStringEnumConverterTests
{
    private static JsonSerializerOptions Strict(bool allowIntegerValues = true) => new()
    {
        Converters = { new ConditionalStringEnumConverter(allowIntegerValues: allowIntegerValues) { AllowUndefinedValues = false } }
    };

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

    [Fact]
    public void Serialize_ShouldWriteSameNamesAsStringEnumNaming()
    {
        foreach (ConverterAcronymKind value in Enum.GetValues<ConverterAcronymKind>())
        {
            string json = JsonSerializer.Serialize(value, StringEnumOptions);

            json.Should().Be($"\"{StringEnumNaming.GetName(value)}\"");
        }
    }

    [Fact]
    public void Serialize_AcronymMember_ShouldWriteSnakeCaseName()
    {
        string json = JsonSerializer.Serialize(ConverterAcronymKind.HTTPStatus, StringEnumOptions);

        json.Should().Be("\"http_status\"");
    }

    [Fact]
    public void Serialize_WithCamelCasePolicy_ShouldMatchStringEnumNamingWithSamePolicy()
    {
        string json = JsonSerializer.Serialize(ConverterAcronymKind.PendingApproval, CamelCaseOptions);

        json.Should().Be($"\"{StringEnumNaming.GetName(ConverterAcronymKind.PendingApproval, JsonNamingPolicy.CamelCase)}\"");
    }

    [Theory]
    [InlineData("\"http_status\"", nameof(ConverterAcronymKind.HTTPStatus))]
    [InlineData("\"HTTPStatus\"", nameof(ConverterAcronymKind.HTTPStatus))]
    [InlineData("\"io_error\"", nameof(ConverterAcronymKind.IOError))]
    [InlineData("\"pending_approval\"", nameof(ConverterAcronymKind.PendingApproval))]
    [InlineData("\"PendingApproval\"", nameof(ConverterAcronymKind.PendingApproval))]
    public void Deserialize_ShouldReadSnakeCaseAndPascalCase(string json, string expected)
    {
        ConverterAcronymKind value = JsonSerializer.Deserialize<ConverterAcronymKind>(json, StringEnumOptions);

        value.Should().Be(Enum.Parse<ConverterAcronymKind>(expected));
    }

    [Fact]
    public void Deserialize_ShouldReadEveryNameProducedByStringEnumNaming()
    {
        foreach (ConverterAcronymKind expected in Enum.GetValues<ConverterAcronymKind>())
        {
            string json = $"\"{StringEnumNaming.GetName(expected)}\"";

            JsonSerializer.Deserialize<ConverterAcronymKind>(json, StringEnumOptions).Should().Be(expected);
        }
    }

    [Fact]
    public void Deserialize_ShouldAcceptUndefinedNumber_ByDefault()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("999", AllowIntegerOptions);

        ((int)value).Should().Be(999);
    }

    [Fact]
    public void Serialize_ShouldRoundTripUndefinedNumber_ByDefault()
    {
        string json = JsonSerializer.Serialize((TestStringEnumType)999, StringEnumOptions);
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>(json, StringEnumOptions);

        ((int)value).Should().Be(999);
    }

    [Fact]
    public void AllowUndefinedValues_ShouldDefaultToTrue()
    {
        new ConditionalStringEnumConverter().AllowUndefinedValues.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenUndefinedNumberAndUndefinedValuesAreDisallowed()
    {
        Action act = () => JsonSerializer.Deserialize<TestStringEnumType>("999", Strict());

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_ShouldAcceptDefinedNumber_WhenUndefinedValuesAreDisallowed()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("1", Strict());

        value.Should().Be(TestStringEnumType.SecondValue);
    }

    [Fact]
    public void Deserialize_ShouldAcceptName_WhenUndefinedValuesAreDisallowed()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("\"third_value\"", Strict());

        value.Should().Be(TestStringEnumType.ThirdValue);
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenIntegerValuesAreDisallowed_AndNumberIsDefined()
    {
        Action act = () => JsonSerializer.Deserialize<TestStringEnumType>("1", Strict(allowIntegerValues: false));

        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("3", 3)]
    [InlineData("7", 7)]
    [InlineData("\"read, delete\"", 5)]
    public void Deserialize_ShouldAcceptCombinationOfDefinedFlags_WhenUndefinedValuesAreDisallowed(string json, int expected)
    {
        ConverterFlagsKind value = JsonSerializer.Deserialize<ConverterFlagsKind>(json, Strict());

        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenFlagsNumberContainsUndefinedBit()
    {
        Action act = () => JsonSerializer.Deserialize<ConverterFlagsKind>("8", Strict());

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_ShouldReadDictionaryKeys_WhenKeysAreDefined()
    {
        Dictionary<TestStringEnumType, int>? value = JsonSerializer.Deserialize<Dictionary<TestStringEnumType, int>>(
            "{\"first_value\":1,\"2\":2}", Strict());

        value.Should().Equal(new Dictionary<TestStringEnumType, int>
        {
            [TestStringEnumType.FirstValue] = 1,
            [TestStringEnumType.ThirdValue] = 2
        });
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenDictionaryKeyIsUndefinedNumber()
    {
        Action act = () => JsonSerializer.Deserialize<Dictionary<TestStringEnumType, int>>("{\"999\":1}", Strict());

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_ShouldWriteUndefinedNumber_WhenUndefinedValuesAreDisallowed()
    {
        string json = JsonSerializer.Serialize((TestStringEnumType)999, Strict());

        json.Should().Be("999");
    }

    [Fact]
    public void Serialize_ShouldWriteName_WhenUndefinedValuesAreDisallowed()
    {
        string json = JsonSerializer.Serialize(TestStringEnumType.SecondValue, Strict());

        json.Should().Be("\"second_value\"");
    }

    [Fact]
    public void Serialize_ShouldWriteDictionaryKeyName_WhenUndefinedValuesAreDisallowed()
    {
        string json = JsonSerializer.Serialize(new Dictionary<TestStringEnumType, int> { [TestStringEnumType.SecondValue] = 1 }, Strict());

        json.Should().Be("{\"second_value\":1}");
    }

    [Fact]
    public void Deserialize_ShouldIgnoreNonStringEnums_WhenUndefinedValuesAreDisallowed()
    {
        RegularEnumType value = JsonSerializer.Deserialize<RegularEnumType>("999", Strict());

        ((int)value).Should().Be(999);
    }
}
