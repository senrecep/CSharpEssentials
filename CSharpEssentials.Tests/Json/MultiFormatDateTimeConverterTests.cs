using System.Text.Json;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class MultiFormatDateTimeConverterTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Converters = { new MultiFormatDateTimeConverterFactory() }
    };

    private static readonly JsonSerializerOptions CustomFormatOptions = new()
    {
        Converters = { new MultiFormatDateTimeConverterFactory("dd/MM/yyyy") }
    };

    private static readonly JsonSerializerOptions MultiFormatOptions = new()
    {
        Converters = { new MultiFormatDateTimeConverterFactory("dd-MM-yyyy", "yyyy-MM-dd") }
    };

    [Fact]
    public void Serialize_ShouldUseDefaultFormat()
    {
        DateTime dateTime = new(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc);
        string json = JsonSerializer.Serialize(dateTime, DefaultOptions);

        json.Should().Contain("2024-03-14");
    }

    [Fact]
    public void Deserialize_WithIso8601Format_ShouldWork()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"2024-03-14T15:30:45\"", DefaultOptions);

        dateTime.Year.Should().Be(2024);
        dateTime.Month.Should().Be(3);
        dateTime.Day.Should().Be(14);
    }

    [Fact]
    public void Deserialize_WithCustomFormat_ShouldWork()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"14/03/2024\"", CustomFormatOptions);

        dateTime.Year.Should().Be(2024);
        dateTime.Month.Should().Be(3);
        dateTime.Day.Should().Be(14);
    }

    [Fact]
    public void Deserialize_WithMultipleFormats_ShouldTryAll()
    {
        DateTime dateTime1 = JsonSerializer.Deserialize<DateTime>("\"14-03-2024\"", MultiFormatOptions);
        DateTime dateTime2 = JsonSerializer.Deserialize<DateTime>("\"2024-03-14\"", MultiFormatOptions);

        dateTime1.Day.Should().Be(14);
        dateTime2.Day.Should().Be(14);
    }

    [Fact]
    public void Deserialize_WithInvalidFormat_ShouldThrow()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"invalid\"", DefaultOptions));
    }

    [Fact]
    public void Deserialize_WithNull_ShouldReturnDefaultForNullable()
    {
        DateTime? dateTime = JsonSerializer.Deserialize<DateTime?>("null", DefaultOptions);

        dateTime.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithNull_ShouldThrowForNonNullable()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("null", DefaultOptions));
    }

    [Fact]
    public void CanConvert_WithDateTime_ShouldReturnTrue()
    {
        MultiFormatDateTimeConverterFactory factory = new();

        factory.CanConvert(typeof(DateTime)).Should().BeTrue();
        factory.CanConvert(typeof(DateTime?)).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithOtherTypes_ShouldReturnFalse()
    {
        MultiFormatDateTimeConverterFactory factory = new();

        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
    }
}
