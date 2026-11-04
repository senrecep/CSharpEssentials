using System.Buffers.Text;
#if !NETSTANDARD
using System.Runtime.InteropServices;
#endif

namespace CSharpEssentials.Core;

public static class Guider
{
    private const char _equal = '=', _hyphen = '-', _plus = '+', _slash = '/', _underscore = '_';
    private const byte _slashByte = (byte)_slash, _plusByte = (byte)_plus;
    private const short _byteCount = 16, _encodedLength = 22, _inputLength = 24;

    /// <summary>
    /// Converts a GUID to a 22-character RFC 4648 base64url string without padding.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string ToStringFromGuid(Guid id)
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        Span<byte> span = stackalloc byte[_inputLength];
#if NETSTANDARD
        byte[] guidBytes = id.ToByteArray();
        guidBytes.CopyTo(bytes);
#else
        MemoryMarshal.TryWrite(bytes, in id);
#endif
        Base64.EncodeToUtf8(bytes, span, out _, out _);
        Span<char> chars = stackalloc char[_encodedLength];
        for (int i = default; i < _encodedLength; i++)
            chars[i] = span[i] switch
            {
                _plusByte => _hyphen,
                _slashByte => _underscore,
                _ => (char)span[i]
            };
#if NETSTANDARD2_0
        return new string(chars.ToArray());
#else
        return new string(chars);
#endif
    }

    /// <summary>
    /// Converts a string produced by <see cref="ToStringFromGuid(Guid)"/> back to a GUID.
    /// </summary>
    /// <param name="id">The 22-character URL-safe string.</param>
    /// <returns>The decoded GUID.</returns>
    /// <exception cref="FormatException">The input is not exactly 22 characters long or contains a character outside A-Z, a-z, 0-9, '-' and '_'.</exception>
    public static Guid ToGuidFromString(ReadOnlySpan<char> id)
    {
        if (id.Length != _encodedLength)
            throw new FormatException($"The value must be exactly {_encodedLength} characters long.");

        Span<char> span = stackalloc char[_inputLength];
        for (int i = default; i < _encodedLength; i++)
        {
            char c = id[i];
            span[i] = c switch
            {
                _hyphen => _plus,
                _underscore => _slash,
                _ when IsBase64Alphanumeric(c) => c,
                _ => throw new FormatException("The value contains a character that is not part of the URL-safe alphabet.")
            };
        }
        span[_encodedLength] = span[_encodedLength + 1] = _equal;
#if NETSTANDARD2_0
        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64CharArray(span.ToArray(), 0, span.Length);
        }
        catch (FormatException)
        {
            throw new FormatException("The value is not a valid URL-safe encoded GUID.");
        }
        return new Guid(decoded);
#else
        Span<byte> bytes = stackalloc byte[_byteCount];
        if (!Convert.TryFromBase64Chars(span, bytes, out int written) || written != _byteCount)
            throw new FormatException("The value is not a valid URL-safe encoded GUID.");
        return new Guid(bytes);
#endif
    }

    private static bool IsBase64Alphanumeric(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';

    /// <summary>
    /// Creates a new RFC 9562 version 7 GUID: a 48-bit Unix millisecond timestamp followed by random bits.
    /// IDs from different milliseconds sort by creation time while the system clock does not move backwards; IDs created in the same millisecond have no set order.
    /// </summary>
    /// <remarks>
    /// The random bits come from a per-thread pool of bytes filled by <see cref="System.Security.Cryptography.RandomNumberGenerator"/>
    /// and salted with one <see cref="Guid.NewGuid"/> per refill. The first ID after a pause of more than 1 ms on a thread takes fresh
    /// bits from <see cref="Guid.NewGuid"/> instead, and that GUID becomes the new salt. A cloned virtual machine or a checkpointed and restored process can still repeat
    /// an ID in a narrow case described in the Guids Readme. Set the
    /// AppContext switch <c>CSharpEssentials.Guider.DisableRandomPool</c> to <see langword="true"/> before the first call to take the
    /// random bits from <see cref="Guid.NewGuid"/> on every call.
    /// </remarks>
    /// <returns>A version 7 GUID with the layout of <c>Guid.CreateVersion7()</c>.</returns>
    public static Guid NewGuid() => GuidV7.NewGuid();

    /// <summary>
    /// Creates a new RFC 9562 version 7 GUID that is strictly greater than every GUID this method returned before in the same process (per loaded copy of the assembly).
    /// The 12 bits after the version hold the time inside the millisecond and the first 6 bits of <c>rand_b</c> hold a counter,
    /// so IDs created in the same millisecond also sort in creation order.
    /// </summary>
    /// <remarks>
    /// The counter allows 64 IDs in each 1/4096 of a millisecond, 262,144 IDs per millisecond across all threads. Above that rate,
    /// or while the clock stalls or moves backwards, the value moves one past the last one and the encoded timestamp runs ahead
    /// of the clock until the rate drops. The order holds within one process only. Each ID carries 56 random bits instead of 74.
    /// </remarks>
    /// <returns>A version 7 GUID that is greater than the previous one when compared as big-endian bytes or with <see cref="Guid.CompareTo(Guid)"/>.</returns>
    public static Guid NewMonotonicGuid() => GuidV7.NewMonotonicGuid();
}
