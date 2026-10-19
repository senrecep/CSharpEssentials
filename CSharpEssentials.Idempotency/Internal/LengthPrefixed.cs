using System.Globalization;
using System.Text;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Writes a part as <c>{utf8ByteLength}:{value}</c>. The length tells a reader where the value ends, so the value may
/// contain any character, including the separators around it, and two different part lists never produce the same text.
/// </summary>
internal static class LengthPrefixed
{
    /// <summary>UTF-8 that throws on a lone surrogate instead of replacing it with U+FFFD.</summary>
    public static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <exception cref="ArgumentException"><paramref name="value"/> is not well-formed UTF-16.</exception>
    public static StringBuilder Append(StringBuilder builder, string value) =>
        builder
            .Append(StrictUtf8.GetByteCount(value).ToString(CultureInfo.InvariantCulture))
            .Append(':')
            .Append(value);
}
