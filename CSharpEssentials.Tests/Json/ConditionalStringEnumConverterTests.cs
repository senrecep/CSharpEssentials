using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.EnumsContracts;
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

/// <summary>
/// The 4.x converter as an obsolete <see cref="EnumConverterFactory"/>: snake_case wire names from enum metadata,
/// <see cref="EnumReadMode.Input"/> reads and undefined values rejected in both directions.
/// </summary>
[Obsolete("Tests the obsolete 4.x converter.")]
public class ConditionalStringEnumConverterTests
{
    private static readonly JsonSerializerOptions StringEnumOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions DisallowIntegerOptions = new()
    {
        Converters = { new ConditionalStringEnumConverter(allowIntegerValues: false) }
    };

    [Fact]
    public void Constructor_Should_Read_In_Input_Mode_With_The_Default_Conventions()
    {
        ConditionalStringEnumConverter converter = new();

        converter.Mode.Should().Be(EnumReadMode.Input);
        converter.WriteAs.Should().Be(EnumWireFormat.String);
        converter.Conventions.AcceptNumbers.Should().BeTrue();
    }

    [Fact]
    public void Constructor_Should_Accept_The_SnakeCaseLower_Policy()
    {
        ConditionalStringEnumConverter converter = new(JsonNamingPolicy.SnakeCaseLower);

        converter.CanConvert(typeof(TestStringEnumType)).Should().BeTrue();
    }

    [Fact]
    public void Constructor_Should_Throw_NotSupportedException_For_Another_Naming_Policy()
    {
        Action act = () => _ = new ConditionalStringEnumConverter(JsonNamingPolicy.CamelCase);

        act.Should().Throw<NotSupportedException>().WithMessage("*CSharpEssentialsEnumNaming*");
    }

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
    public void Deserialize_WithIntegerValue_ShouldWorkWhenAllowed()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("1", StringEnumOptions);

        value.Should().Be(TestStringEnumType.SecondValue);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("\"0\"")]
    public void Deserialize_WithIntegerValue_ShouldFailWhenNotAllowed(string json)
    {
        Action act = () => JsonSerializer.Deserialize<TestStringEnumType>(json, DisallowIntegerOptions);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Fact]
    public void Deserialize_WithName_ShouldWorkWhenIntegersAreNotAllowed()
    {
        TestStringEnumType value = JsonSerializer.Deserialize<TestStringEnumType>("\"third_value\"", DisallowIntegerOptions);

        value.Should().Be(TestStringEnumType.ThirdValue);
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
        ConditionalStringEnumConverter converter = new(canConvert: type => type == typeof(ConverterAcronymKind));

        converter.CanConvert(typeof(ConverterAcronymKind)).Should().BeTrue();
        converter.CanConvert(typeof(TestStringEnumType)).Should().BeFalse();
    }

    [Fact]
    public void Serialize_WithCustomPredicate_ShouldNotUseReflectionForEnumsWithoutGeneratedMetadata()
    {
        JsonSerializerOptions options = new() { Converters = { new ConditionalStringEnumConverter(canConvert: type => type == typeof(RegularEnumType)) } };

        string json = JsonSerializer.Serialize(RegularEnumType.Second, options);

        new ConditionalStringEnumConverter(canConvert: _ => true).CanConvert(typeof(RegularEnumType)).Should().BeFalse();
        json.Should().Be("1");
    }

    [Fact]
    public void Serialize_StringEnumWithoutGeneratedMetadata_ShouldFailLoud()
    {
        Action serialize = () => JsonSerializer.Serialize(UnreachableHolder<int>.Status.SecondValue, StringEnumOptions);

        new ConditionalStringEnumConverter().CanConvert(typeof(UnreachableHolder<int>.Status)).Should().BeTrue();
        serialize.Should().Throw<InvalidOperationException>().WithMessage("*UnreachableHolder*Status*no generated metadata*");
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

    [Theory]
    [InlineData("999")]
    [InlineData("\"999\"")]
    [InlineData("\"bogus\"")]
    public void Deserialize_ShouldRejectUndefinedValues(string json)
    {
        Action act = () => JsonSerializer.Deserialize<TestStringEnumType>(json, StringEnumOptions);

        act.Should().Throw<EnumValueJsonException>();
    }

    [Fact]
    public void Serialize_ShouldThrow_ForUndefinedValue()
    {
        Action act = () => JsonSerializer.Serialize((TestStringEnumType)999, StringEnumOptions);

        act.Should().Throw<EnumValueException>();
    }

    [Theory]
    [InlineData("3", 3)]
    [InlineData("7", 7)]
    [InlineData("\"read, delete\"", 5)]
    [InlineData("[\"read\",\"delete\"]", 5)]
    public void Deserialize_ShouldAcceptCombinationOfDefinedFlags(string json, int expected)
    {
        ConverterFlagsKind value = JsonSerializer.Deserialize<ConverterFlagsKind>(json, StringEnumOptions);

        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenFlagsNumberContainsUndefinedBit()
    {
        Action act = () => JsonSerializer.Deserialize<ConverterFlagsKind>("8", StringEnumOptions);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_ShouldWriteFlagsAsArray()
    {
        string json = JsonSerializer.Serialize(ConverterFlagsKind.Read | ConverterFlagsKind.Delete, StringEnumOptions);

        json.Should().Be("[\"read\",\"delete\"]");
    }

    [Fact]
    public void Deserialize_ShouldReadDictionaryKeys_WhenKeysAreDefined()
    {
        Dictionary<TestStringEnumType, int>? value = JsonSerializer.Deserialize<Dictionary<TestStringEnumType, int>>(
            "{\"first_value\":1,\"2\":2}", StringEnumOptions);

        value.Should().Equal(new Dictionary<TestStringEnumType, int>
        {
            [TestStringEnumType.FirstValue] = 1,
            [TestStringEnumType.ThirdValue] = 2
        });
    }

    [Fact]
    public void Deserialize_ShouldThrowJsonException_WhenDictionaryKeyIsUndefinedNumber()
    {
        Action act = () => JsonSerializer.Deserialize<Dictionary<TestStringEnumType, int>>("{\"999\":1}", StringEnumOptions);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_ShouldWriteDictionaryKeyName()
    {
        string json = JsonSerializer.Serialize(new Dictionary<TestStringEnumType, int> { [TestStringEnumType.SecondValue] = 1 }, StringEnumOptions);

        json.Should().Be("{\"second_value\":1}");
    }

    [Fact]
    public void Deserialize_ShouldIgnoreNonStringEnums()
    {
        RegularEnumType value = JsonSerializer.Deserialize<RegularEnumType>("999", StringEnumOptions);

        ((int)value).Should().Be(999);
    }
}
