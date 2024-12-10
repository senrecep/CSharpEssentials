using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace CSharpEssentials;
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
        MemoryMarshal.TryWrite(bytes, in id);
        Base64.EncodeToUtf8(bytes, span, out _, out _);
        Span<char> chars = stackalloc char[_encodedLength];
        for (int i = default; i < _encodedLength; i++)
            chars[i] = span[i] switch
            {
                _slashByte => _hyphen,
                _plusByte => _underscore,
                _ => (char)span[i]
            };
        return new string(chars);
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
        Convert.TryFromBase64Chars(span, bytes, out _);
        return new Guid(bytes);
    }

    /// <summary>
    /// Creates a new GUID.
    /// </summary>
    /// <returns></returns>
    public static Guid NewGuid() => Guid.CreateVersion7();
}