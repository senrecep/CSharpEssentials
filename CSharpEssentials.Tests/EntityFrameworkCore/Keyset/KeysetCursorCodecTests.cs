using System.Text;

using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

public sealed class KeysetCursorCodecTests
{
    private const string Fingerprint = "abcdefgh";

    public static TheoryData<object> SupportedValues => new()
    {
        42,
        -7L,
        (short)12,
        (byte)200,
        123.4567890123456789m,
        3.14159d,
        double.PositiveInfinity,
        double.NegativeInfinity,
        double.NaN,
        2.5f,
        float.NegativeInfinity,
        float.NaN,
        "héllo \"world\" / ünicode",
        string.Empty,
        Guid.Parse("6f9619ff-8b86-d011-b42d-00cf4fc964ff"),
        new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc).AddTicks(1234567),
        new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Unspecified).AddTicks(1),
        new DateTimeOffset(2024, 5, 6, 7, 8, 9, TimeSpan.FromHours(3)).AddTicks(7),
        TimeSpan.FromTicks(98765432101),
        new DateOnly(2024, 2, 29),
        new TimeOnly(23, 59, 59).Add(TimeSpan.FromTicks(9999)),
        KeysetRowStatus.Archived,
    };

    [Theory]
    [MemberData(nameof(SupportedValues))]
    public void Encode_Should_RoundTripValue_When_TypeIsSupported(object value)
    {
        Type[] types = [value.GetType()];

        string cursor = KeysetCursorCodec.Encode(KeysetCursorDirection.After, Fingerprint, types, [value], NoOpCursorProtector.Instance);
        bool decoded = KeysetCursorCodec.TryDecode(
            cursor, KeysetCursorDirection.After, Fingerprint, types, NoOpCursorProtector.Instance, out object[] values, out _);

        decoded.Should().BeTrue();
        values.Should().ContainSingle().Which.Should().Be(value);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Encode_Should_PreserveDateTimeKind_When_ValueIsDateTime(DateTimeKind kind)
    {
        DateTime value = new DateTime(2024, 5, 6, 7, 8, 9, kind).AddTicks(42);
        Type[] types = [typeof(DateTime)];

        string cursor = KeysetCursorCodec.Encode(KeysetCursorDirection.After, Fingerprint, types, [value], NoOpCursorProtector.Instance);
        KeysetCursorCodec.TryDecode(
            cursor, KeysetCursorDirection.After, Fingerprint, types, NoOpCursorProtector.Instance, out object[] values, out _);

        var decoded = (DateTime)values[0];
        decoded.Kind.Should().Be(kind);
        decoded.Ticks.Should().Be(value.Ticks);
    }

    [Fact]
    public void Encode_Should_ProduceUnpaddedBase64Url_When_PayloadIsEncoded()
    {
        Type[] types = [typeof(string), typeof(int)];

        string cursor = KeysetCursorCodec.Encode(
            KeysetCursorDirection.Before, Fingerprint, types, ["??>>~~", 1], NoOpCursorProtector.Instance);

        cursor.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        Decode(cursor).Should().Be("{\"v\":1,\"d\":\"b\",\"k\":\"abcdefgh\",\"p\":[\"??\\u003E\\u003E~~\",1]}");
    }

    [Fact]
    public void Encode_Should_RoundTripAllValues_When_KeyIsComposite()
    {
        Type[] types = [typeof(DateTime), typeof(string), typeof(int)];
        object[] input = [new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), "x", 5];

        string cursor = KeysetCursorCodec.Encode(KeysetCursorDirection.After, Fingerprint, types, input, NoOpCursorProtector.Instance);
        bool decoded = KeysetCursorCodec.TryDecode(
            cursor, KeysetCursorDirection.After, Fingerprint, types, NoOpCursorProtector.Instance, out object[] values, out _);

        decoded.Should().BeTrue();
        values.Should().Equal(input);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a cursor!")]
    [InlineData("a")]
    [InlineData("eyJ2IjoxfQ==")]
    [InlineData("eyJ2IjoxfQ+/")]
    [InlineData("bm90IGpzb24")]
    [InlineData("WzEsMl0")]
    [InlineData("eyJ2IjoxLCJkIjoiYSJ9")]
    public void TryDecode_Should_ReturnInvalidCursor_When_CursorIsGarbage(string cursor)
    {
        Error error = DecodeError(cursor, [typeof(int)]);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be(KeysetCursorErrors.InvalidCode);
        error.Metadata.Should().ContainKey(KeysetCursorErrors.ParameterKey).WhoseValue.Should().Be("after");
    }

    [Fact]
    public void TryDecode_Should_ReturnInvalidCursor_When_StringValueIsInvalidUtf8()
    {
        byte[] json = [.. Encoding.UTF8.GetBytes("{\"v\":1,\"d\":\"a\",\"k\":\"abcdefgh\",\"p\":[\""), 0xFF, .. Encoding.UTF8.GetBytes("\"]}")];

        Error error = DecodeError(Encode(json), [typeof(string)]);

        error.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnFailure_When_CursorIsTampered()
    {
        Type[] types = [typeof(int), typeof(string)];
        string cursor = KeysetCursorCodec.Encode(KeysetCursorDirection.After, Fingerprint, types, [5, "x"], NoOpCursorProtector.Instance);
        string tampered = Encode(Decode(cursor).Replace("\"x\"", "5", StringComparison.Ordinal));

        Error error = DecodeError(tampered, types);

        error.Type.Should().Be(ErrorType.Validation);
        error.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnUnsupportedVersion_When_VersionDiffers()
    {
        string cursor = Encode("{\"v\":2,\"d\":\"a\",\"k\":\"abcdefgh\",\"p\":[1]}");

        Error error = DecodeError(cursor, [typeof(int)]);

        error.Code.Should().Be(KeysetCursorErrors.UnsupportedVersionCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnDirectionMismatch_When_BeforeCursorIsUsedAsAfter()
    {
        string cursor = KeysetCursorCodec.Encode(
            KeysetCursorDirection.Before, Fingerprint, [typeof(int)], [1], NoOpCursorProtector.Instance);

        Error error = DecodeError(cursor, [typeof(int)]);

        error.Code.Should().Be(KeysetCursorErrors.DirectionMismatchCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnKeyMismatch_When_FingerprintDiffers()
    {
        string cursor = KeysetCursorCodec.Encode(
            KeysetCursorDirection.After, "otherkey", [typeof(int)], [1], NoOpCursorProtector.Instance);

        Error error = DecodeError(cursor, [typeof(int)]);

        error.Code.Should().Be(KeysetCursorErrors.KeyMismatchCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnKeyMismatch_When_ValueCountDiffers()
    {
        string cursor = Encode("{\"v\":1,\"d\":\"a\",\"k\":\"abcdefgh\",\"p\":[1,2]}");

        Error error = DecodeError(cursor, [typeof(int)]);

        error.Code.Should().Be(KeysetCursorErrors.KeyMismatchCode);
    }

    [Theory]
    [InlineData(typeof(int), "\"1\"")]
    [InlineData(typeof(int), "1.5")]
    [InlineData(typeof(int), "null")]
    [InlineData(typeof(byte), "300")]
    [InlineData(typeof(string), "1")]
    [InlineData(typeof(Guid), "\"not-a-guid\"")]
    [InlineData(typeof(DateTime), "\"yesterday\"")]
    [InlineData(typeof(DateOnly), "99999999")]
    [InlineData(typeof(TimeOnly), "-1")]
    [InlineData(typeof(KeysetRowStatus), "\"Active\"")]
    [InlineData(typeof(string), "\"\\uD800\"")]
    [InlineData(typeof(string), "\"a\\uDC00b\"")]
    [InlineData(typeof(double), "\"infinity\"")]
    [InlineData(typeof(float), "\"1.5\"")]
    public void TryDecode_Should_ReturnInvalidCursor_When_ValueDoesNotMatchKeyType(Type type, string json)
    {
        string cursor = Encode($"{{\"v\":1,\"d\":\"a\",\"k\":\"abcdefgh\",\"p\":[{json}]}}");

        Error error = DecodeError(cursor, [type]);

        error.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public void TryDecode_Should_ReturnInvalidCursor_When_ProtectorRejectsCursor()
    {
        var protector = new PrefixCursorProtector();
        string cursor = KeysetCursorCodec.Encode(KeysetCursorDirection.After, Fingerprint, [typeof(int)], [1], protector);

        bool decoded = KeysetCursorCodec.TryDecode(
            cursor[PrefixCursorProtector.Prefix.Length..], KeysetCursorDirection.After, Fingerprint, [typeof(int)], protector,
            out _, out Error error);

        cursor.Should().StartWith(PrefixCursorProtector.Prefix);
        decoded.Should().BeFalse();
        error.Code.Should().Be(KeysetCursorErrors.InvalidCode);
    }

    [Fact]
    public void TryDecode_Should_UseBeforeParameterName_When_DecodingBeforeCursor()
    {
        bool decoded = KeysetCursorCodec.TryDecode(
            "%%%", KeysetCursorDirection.Before, Fingerprint, [typeof(int)], NoOpCursorProtector.Instance, out _, out Error error);

        decoded.Should().BeFalse();
        error.Metadata.Should().ContainKey(KeysetCursorErrors.ParameterKey).WhoseValue.Should().Be("before");
    }

    private static Error DecodeError(string cursor, Type[] types)
    {
        bool decoded = KeysetCursorCodec.TryDecode(
            cursor, KeysetCursorDirection.After, Fingerprint, types, NoOpCursorProtector.Instance, out object[] values, out Error error);

        decoded.Should().BeFalse();
        values.Should().BeEmpty();
        return error;
    }

    private static string Decode(string cursor)
    {
        string base64 = cursor.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }

    private static string Encode(string json) => Encode(Encoding.UTF8.GetBytes(json));

    private static string Encode(byte[] json) =>
        Convert.ToBase64String(json).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
