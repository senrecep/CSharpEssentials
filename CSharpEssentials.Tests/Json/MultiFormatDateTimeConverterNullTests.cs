using System.Text;
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
    public void Converter_Write_Null_For_Nullable_Should_Write_Null_Token()
    {
        MultiFormatDateTimeConverter<DateTime?> converter = new(true, ["yyyy-MM-dd"]);
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
            converter.Write(writer, null, Options);

        Encoding.UTF8.GetString(stream.ToArray()).Should().Be("null");
    }

}
