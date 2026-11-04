using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

[Flags]
public enum JsonWriteAccess
{
    None = 0,
    Read = 1,
    Write = 2,
    Exec = 8,
    Plus = 16,
    Accent = 32,
}

public enum JsonWriteKind
{
    Plain = 0,
    Plus = 1,
    Accent = 2,
}

/// <summary>
/// The pre-encoded write paths must give the bytes the plain string paths gave, for every encoder.
/// </summary>
public class EnumConverterWriteTests
{
    static EnumConverterWriteTests()
    {
        EnumMetadata.Register(new EnumInfo<JsonWriteAccess>(
            [
                new(JsonWriteAccess.None, 0, "None", "none"),
                new(JsonWriteAccess.Read, 1, "Read", "read"),
                new(JsonWriteAccess.Read, 1, "ReadAlias", "read_alias"),
                new(JsonWriteAccess.Write, 2, "Write", "write"),
                new(JsonWriteAccess.Exec, 8, "Exec", "exec"),
                new((JsonWriteAccess)12, 12, "Mixed", "mixed"),
                new(JsonWriteAccess.Plus, 16, "Plus", "a+b"),
                new(JsonWriteAccess.Accent, 32, "Accent", "café"),
            ],
            static v => unchecked((ulong)(long)v),
            static r => unchecked((JsonWriteAccess)(long)r),
            isFlags: true,
            EnumStorage.String));
        EnumMetadata.Register(new EnumInfo<JsonWriteKind>(
            [
                new(JsonWriteKind.Plain, 0, "Plain", "plain"),
                new(JsonWriteKind.Plain, 0, "PlainAlias", "plain_alias"),
                new(JsonWriteKind.Plus, 1, "Plus", "a+b"),
                new(JsonWriteKind.Accent, 2, "Accent", "café"),
            ],
            static v => unchecked((ulong)(long)v),
            static r => unchecked((JsonWriteKind)(long)r),
            isFlags: false,
            EnumStorage.String));
    }

    public static TheoryData<string> Encoders => ["none", "default", "relaxed", "latin"];

    private static JavaScriptEncoder? Encoder(string name) => name switch
    {
        "default" => JavaScriptEncoder.Default,
        "relaxed" => JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        "latin" => JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
        _ => null,
    };

    private static JsonSerializerOptions Options(string encoder, EnumWireFormat writeAs = EnumWireFormat.String) =>
        new JsonSerializerOptions { Encoder = Encoder(encoder) }.AddEnumConventions(EnumConventions.Default, EnumReadMode.Data, writeAs);

    [Theory]
    [MemberData(nameof(Encoders))]
    public void Write_Flags_Should_Match_FormatFlags_For_Every_Value(string encoder)
    {
        JsonSerializerOptions options = Options(encoder);
        EnumInfo<JsonWriteAccess> info = EnumMetadata.Get<JsonWriteAccess>();

        for (int raw = 0; raw < 128; raw++)
        {
            var value = (JsonWriteAccess)raw;
            Func<string> write = () => JsonSerializer.Serialize(value, options);
            if (!info.IsDefined(value))
            {
                write().Should().Be(raw.ToString(System.Globalization.CultureInfo.InvariantCulture));
                continue;
            }

            List<string> names = [];
            try
            {
                info.FormatFlags(value, names);
            }
            catch (EnumValueException)
            {
                write.Should().Throw<EnumValueException>("raw {0}", raw);
                continue;
            }

            write().Should().Be(JsonSerializer.Serialize(names, options), "raw {0}", raw);
        }
    }

    [Fact]
    public void Write_Flags_With_Bits_Only_A_Composite_Partly_Covers_Should_Throw_Before_Writing()
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);

        Action write = () => JsonSerializer.Serialize(writer, (JsonWriteAccess)4, Options("none"));

        write.Should().Throw<EnumValueException>();
        writer.Flush();
        stream.Length.Should().Be(0);
    }

    [Theory]
    [InlineData("none", JsonWriteAccess.Read | JsonWriteAccess.Exec, "[\"read\",\"exec\"]")]
    [InlineData("none", JsonWriteAccess.Exec | (JsonWriteAccess)4, "[\"exec\",\"mixed\"]")]
    [InlineData("none", JsonWriteAccess.Plus | JsonWriteAccess.Accent, "[\"a\\u002Bb\",\"caf\\u00E9\"]")]
    [InlineData("relaxed", JsonWriteAccess.Plus | JsonWriteAccess.Accent, "[\"a+b\",\"café\"]")]
    [InlineData("none", JsonWriteAccess.None, "[]")]
    public void Write_Flags_Should_Write_Single_Flags_Then_Composites(string encoder, JsonWriteAccess value, string expected)
    {
        JsonSerializer.Serialize(value, Options(encoder)).Should().Be(expected);
    }

    [Theory]
    [InlineData("none", JsonWriteKind.Plain, "\"plain\"")]
    [InlineData("none", JsonWriteKind.Plus, "\"a\\u002Bb\"")]
    [InlineData("none", JsonWriteKind.Accent, "\"caf\\u00E9\"")]
    [InlineData("relaxed", JsonWriteKind.Plus, "\"a+b\"")]
    [InlineData("relaxed", JsonWriteKind.Accent, "\"café\"")]
    [InlineData("latin", JsonWriteKind.Accent, "\"café\"")]
    [InlineData("latin", JsonWriteKind.Plain, "\"plain\"")]
    [InlineData("none", (JsonWriteKind)7, "7")]
    public void Write_Should_Use_The_First_Member_And_The_Writer_Encoder(string encoder, JsonWriteKind value, string expected)
    {
        JsonSerializer.Serialize(value, Options(encoder)).Should().Be(expected);
    }

    [Theory]
    [InlineData(JsonWriteAccess.Read | JsonWriteAccess.Exec, "9")]
    [InlineData((JsonWriteAccess)64, "64")]
    public void Write_Flags_As_Number_Should_Write_The_Integer(JsonWriteAccess value, string expected)
    {
        JsonSerializer.Serialize(value, Options("none", EnumWireFormat.Number)).Should().Be(expected);
    }
}
