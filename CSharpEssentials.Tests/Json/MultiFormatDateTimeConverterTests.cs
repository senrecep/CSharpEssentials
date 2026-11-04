using System.Globalization;
using System.Reflection;
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

    [Theory]
    [InlineData("2024W11")]
    [InlineData("2024-W11-4")]
    public void Deserialize_WithWeekText_ShouldThrowWithoutListingDeadFormats(string text)
    {
        JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>($"\"{text}\"", DefaultOptions));

        ex.Message.Should().NotContain("'W'ww").And.NotContain("XXX").And.NotContain(" G 'at' ");
    }

    [Theory]
    [InlineData("999999999999999")]
    [InlineData("-999999999999999")]
    public void Deserialize_WithOutOfRangeUnixSeconds_ShouldThrowJsonException(string text)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>($"\"{text}\"", DefaultOptions));
    }

    [Fact]
    public void Deserialize_WithMaxUnixSeconds_ShouldReturnLastSecond()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"253402300799\"", DefaultOptions);

        dateTime.Should().Be(new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc));
    }

    [Fact]
    public void Serialize_WithUnspecifiedKind_ShouldWriteAsUtc()
    {
        DateTime unspecified = new(2024, 3, 14, 15, 30, 45, DateTimeKind.Unspecified);

        string json = JsonSerializer.Serialize(unspecified, DefaultOptions);

        JsonSerializer.Deserialize<string>(json).Should().Be("2024-03-14T15:30:45.000000+00:00");
        JsonSerializer.Deserialize<DateTime>(json, DefaultOptions).Ticks.Should().Be(unspecified.Ticks);
    }

    public static TheoryData<string> IsoShapedTexts =>
    [
        "2024-03-14",
        "2024-03-14T15:30:45",
        "2024-03-14T15:30:45Z",
        "2024-03-14T15:30:45.123",
        "2024-03-14T15:30:45.123Z",
        "2024-03-14T15:30:45+03:00",
        "2024-03-14T15:30:45-05:30",
        "2024-03-14T15:30:45.123456",
        "2024-03-14T15:30:45.123+03",
        "2024-03-14T15:30:45.123-05",
        "2024-03-14T15:30:45.123456+03:00",
        "2024-03-14T15:30:45.123456-00:00",
        "0001-01-01T00:00:00Z",
        "0001-01-01T00:30:00+01:00",
        "9999-12-31T23:59:59Z",
        "9999-12-31T23:59:59-01:00",
        "2024-02-29T00:00:00Z",
        "2023-02-29T00:00:00Z",
        "2024-03-14T24:00:00Z",
        "2024-03-14T23:59:60Z",
        "2024-03-14T15:60:00Z",
        "2024-03-14T15:30:45+14:00",
        "2024-03-14T15:30:45+15:00",
        "2024-03-14T15:30:45+03:60",
        "2024-03-14T15:30:45.12Z",
        "2024-03-14T15:30:45.1234Z",
        "2024-03-14T15:30:45.12345Z",
        "2024-03-14T15:30:45.1+03:00",
        "2024-03-14T15:30:45.12345+3:00",
        "2024-03-14T15:30:4Z",
        "2024-03-14T15:30Z",
        "2024-03-14T15:30",
        "2024-03-14t15:30:45z",
        "2024-03-14 15:30:45",
        "2024-3-14",
        "2024-13-14",
        "2024-03-14T15:30:45.123z",
        "+2024-03-14",
    ];

    [Theory]
    [MemberData(nameof(IsoShapedTexts))]
    public void Deserialize_IsoShapedText_Should_Match_The_Format_List(string text)
    {
        DateTime? expected = ParseWithList(text);

        Func<DateTime> read = () => JsonSerializer.Deserialize<DateTime>($"\"{text}\"", DefaultOptions);

        if (expected is null)
        {
            read.Should().Throw<JsonException>();
            return;
        }

        DateTime actual = read();
        actual.Kind.Should().Be(DateTimeKind.Utc);
        actual.Ticks.Should().Be(expected.Value.Ticks);
    }

    [Fact]
    public void Deserialize_IsoText_With_Custom_List_Should_Use_Only_That_List()
    {
        JsonSerializerOptions options = new() { Converters = { new MultiFormatDateTimeConverter<DateTime>(false, ["dd.MM.yyyy"]) } };

        Action read = () => JsonSerializer.Deserialize<DateTime>("\"2024-03-14T15:30:45Z\"", options);

        read.Should().Throw<JsonException>();
    }

    [Fact]
    public void Deserialize_EscapedIsoText_Should_Read_The_Same_Value()
    {
        DateTime dateTime = JsonSerializer.Deserialize<DateTime>("\"2024-03-14T15:30:45\\u002B03:00\"", DefaultOptions);

        dateTime.Kind.Should().Be(DateTimeKind.Utc);
        dateTime.Ticks.Should().Be(new DateTime(2024, 3, 14, 12, 30, 45, DateTimeKind.Utc).Ticks);
    }

    private static DateTime? ParseWithList(string text)
    {
        string[] formats = (string[])typeof(MultiFormatDateTimeConverterFactory)
            .GetField("DefaultFormats", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        foreach (string format in formats)
        {
            if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime value))
                return value;
        }

        return null;
    }
}
