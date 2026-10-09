using System.Buffers.Text;
using System.Runtime.CompilerServices;
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
    /// Converts a GUID to a string.
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
                _slashByte => _hyphen,
                _plusByte => _underscore,
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
                _hyphen => _slash,
                _underscore => _plus,
                _ when IsBase64Alphanumeric(c) => c,
                _ => throw new FormatException("The value contains a character that is not part of the URL-safe alphabet.")
            };
        }
        span[_encodedLength] = span[_encodedLength + 1] = _equal;
        Span<byte> bytes = stackalloc byte[_byteCount];
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
        if (!Convert.TryFromBase64Chars(span, bytes, out int written) || written != _byteCount)
            throw new FormatException("The value is not a valid URL-safe encoded GUID.");
        return new Guid(bytes);
#endif
    }

    private static bool IsBase64Alphanumeric(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';

    /// <summary>
    /// Creates a new GUID.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NewGuid()
#if NET9_0_OR_GREATER
        => Guid.CreateVersion7();
#else
        => Guid.NewGuid();
#endif
}
