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

    [Theory]
    [InlineData("2024-03-14")]
    [InlineData("14.03.2024")]
    [InlineData("20240314")]
    [InlineData("March 14, 2024")]
    public void Deserialize_WithDateOnlyText_ShouldReturnMidnightUtc(string text)
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>($"\"{text}\"", DefaultOptions);

        dateTime.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Ticks.Should().Be(new DateTime(2024, 3, 14, 0, 0, 0, DateTimeKind.Utc).Ticks);
    }

    [Theory]
    [InlineData("2024-03-14")]
    [InlineData("14.03.2024")]
    [InlineData("20240314")]
    [InlineData("March 14, 2024")]
    public void Deserialize_WithDateOnlyText_ShouldReturnMidnightUtcForNullable(string text)
    {
        DateTime? dateTime = JsonSerializer.Deserialize<DateTime?>($"\"{text}\"", DefaultOptions);

        dateTime.Should().NotBeNull();
        dateTime!.Value.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Value.Ticks.Should().Be(new DateTime(2024, 3, 14, 0, 0, 0, DateTimeKind.Utc).Ticks);
    }

    [Fact]
    public void Deserialize_WithUnixSecondsText_ShouldReturnUtc()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"1710430245\"", DefaultOptions);

        dateTime.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Ticks.Should().Be(new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc).Ticks);
    }

    [Fact]
    public void Deserialize_WithUnixSecondsText_ShouldReturnUtcForNullable()
    {
        DateTime? dateTime = JsonSerializer.Deserialize<DateTime?>("\"1710430245\"", DefaultOptions);

        dateTime.Should().NotBeNull();
        dateTime!.Value.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Value.Ticks.Should().Be(new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc).Ticks);
    }

    [Theory]
    [InlineData("2024-03-14T15:30:45")]
    [InlineData("2024-03-14T15:30:45Z")]
    [InlineData("2024-03-14T15:30:45+00:00")]
    public void Deserialize_WithTimeText_ShouldReturnUtc(string text)
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>($"\"{text}\"", DefaultOptions);

        dateTime.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Ticks.Should().Be(new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc).Ticks);
    }

    [Fact]
    public void Deserialize_WithOffsetText_ShouldConvertToUtc()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"2024-03-14T15:30:45.000000+03:00\"", DefaultOptions);

        dateTime.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Ticks.Should().Be(new DateTime(2024, 3, 14, 12, 30, 45, DateTimeKind.Utc).Ticks);
    }

    [Fact]
    public void Serialize_WithUtcValue_ShouldWriteZeroOffset()
    {
        DateTime dateTime = new(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc);

        string json = JsonSerializer.Serialize(dateTime, DefaultOptions);

        JsonSerializer.Deserialize<string>(json).Should().Be("2024-03-14T15:30:45.000000+00:00");
    }

    [Fact]
    public void Serialize_WithNullableNull_ShouldWriteNull()
    {
        string json = JsonSerializer.Serialize<DateTime?>(null, DefaultOptions);

        json.Should().Be("null");
    }

    [Fact]
    public void RoundTrip_WithUtcValue_ShouldBeLossless()
    {
        DateTime original = new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc).AddTicks(1230);

        string json = JsonSerializer.Serialize(original, DefaultOptions);
        DateTime restored = JsonSerializer.Deserialize<DateTime>(json, DefaultOptions);

        restored.Kind.Should().Be(DateTimeKind.Utc);
        restored.Ticks.Should().Be(original.Ticks - original.Ticks % 10);
    }

    [Fact]
    public void RoundTrip_WithNullableUtcValue_ShouldBeLossless()
    {
        DateTime? original = new DateTime(2024, 3, 14, 15, 30, 45, DateTimeKind.Utc);

        string json = JsonSerializer.Serialize(original, DefaultOptions);
        DateTime? restored = JsonSerializer.Deserialize<DateTime?>(json, DefaultOptions);

        restored.Should().NotBeNull();
        restored!.Value.Kind.Should().Be(DateTimeKind.Utc);
        restored.Value.Ticks.Should().Be(original.Value.Ticks);
    }

    [Fact]
    public void RoundTrip_WithLocalValue_ShouldPreserveInstant()
    {
        DateTime original = new(2024, 3, 14, 15, 30, 45, DateTimeKind.Local);

        string json = JsonSerializer.Serialize(original, DefaultOptions);
        DateTime restored = JsonSerializer.Deserialize<DateTime>(json, DefaultOptions);

        restored.Kind.Should().Be(DateTimeKind.Utc);
        restored.Ticks.Should().Be(original.ToUniversalTime().Ticks);
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
