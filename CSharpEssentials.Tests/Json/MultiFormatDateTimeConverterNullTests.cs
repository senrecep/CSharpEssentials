using System.Text.Json;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public sealed class MultiFormatDateTimeConverterNullTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new MultiFormatDateTimeConverterFactory() }
    };

    [Fact]
    public void Deserialize_Null_Into_NonNullable_Should_Throw()
    {
        Action act = () => JsonSerializer.Deserialize<DateTime>("null", Options);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_Null_Into_Nullable_Should_Return_Null()
    {
        JsonSerializer.Deserialize<DateTime?>("null", Options).Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyString_Into_NonNullable_Should_Throw()
    {
        Action act = () => JsonSerializer.Deserialize<DateTime>("\"\"", Options);

        act.Should().Throw<JsonException>().WithMessage("*Empty or null string*");
    }

    [Fact]
    public void Deserialize_EmptyString_Into_Nullable_Should_Return_Null()
    {
        JsonSerializer.Deserialize<DateTime?>("\"\"", Options).Should().BeNull();
    }

    [Fact]
    public void Deserialize_UnknownText_Should_Throw_With_Supported_Formats()
    {
        Action act = () => JsonSerializer.Deserialize<DateTime>("\"not a date\"", Options);

        act.Should().Throw<JsonException>().WithMessage("*\"not a date\"*Supported formats:*yyyy-MM-dd*");
    }

    [Fact]
    public void Serialize_Null_Nullable_Should_Write_Null()
    {
        JsonSerializer.Serialize<DateTime?>(null, Options).Should().Be("null");
    }

    [Fact]
    public void Serialize_Nullable_With_Value_Should_Write_Default_Format()
    {
        DateTime? value = new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc);

        JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(value, Options)).Should().Be("2024-03-14T15:30:45.000000+00:00");
    }

    [Fact]
    public void CanConvert_Should_Accept_Only_DateTime_And_Nullable_DateTime()
    {
        var factory = new MultiFormatDateTimeConverterFactory();

        factory.CanConvert(typeof(DateTime)).Should().BeTrue();
        factory.CanConvert(typeof(DateTime?)).Should().BeTrue();
        factory.CanConvert(typeof(DateTimeOffset)).Should().BeFalse();
        factory.CanConvert(typeof(string)).Should().BeFalse();
    }
}
