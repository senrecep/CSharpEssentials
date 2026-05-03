using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    /// Converts a string to a GUID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Guid ToGuidFromString(ReadOnlySpan<char> id)
    {
        Span<char> span = stackalloc char[_inputLength];
        for (int i = default; i < _encodedLength; i++)
            span[i] = id[i] switch
            {
                _hyphen => _slash,
                _underscore => _plus,
                _ => id[i]
            };
        span[_encodedLength] = span[_encodedLength + 1] = _equal;
        Span<byte> bytes = stackalloc byte[_byteCount];
#if NETSTANDARD2_0
        byte[] byteArray = bytes.ToArray();
        Convert.FromBase64CharArray(span.ToArray(), 0, span.Length).CopyTo(byteArray, 0);
        return new Guid(byteArray);
#else
        Convert.TryFromBase64Chars(span, bytes, out _);
        return new Guid(bytes);
#endif
    }

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