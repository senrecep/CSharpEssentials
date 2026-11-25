using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
#if NET
using System.Buffers;
#endif

namespace CSharpEssentials.Core;

[SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
    Justification = "RFC 9562 defines UUIDv5 as SHA-1; the hash names an ID and protects nothing.")]
[SuppressMessage("Security", "S4790:Use a stronger hashing algorithm",
    Justification = "RFC 9562 defines UUIDv5 as SHA-1; the hash names an ID and protects nothing.")]
internal static class GuidV5
{
    private const int _namespaceLength = 16;
#if NET
    private const int _stackLimit = 256;
#endif
    private static readonly UTF8Encoding _strictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static Guid Create(Guid namespaceId, string name)
    {
#if NET
        int count = _strictUtf8.GetByteCount(name);
        byte[]? rented = count > _stackLimit ? ArrayPool<byte>.Shared.Rent(count) : null;
        Span<byte> bytes = rented is null ? stackalloc byte[_stackLimit] : rented;
        try
        {
            int written = _strictUtf8.GetBytes(name, bytes);
            return Create(namespaceId, bytes[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
#else
        return Create(namespaceId, _strictUtf8.GetBytes(name));
#endif
    }

    internal static Guid Create(Guid namespaceId, ReadOnlySpan<byte> name)
    {
#if NET
        int length = _namespaceLength + name.Length;
        byte[]? rented = length > _stackLimit ? ArrayPool<byte>.Shared.Rent(length) : null;
        Span<byte> input = rented is null ? stackalloc byte[_stackLimit] : rented;
        try
        {
            _ = namespaceId.TryWriteBytes(input, bigEndian: true, out _);
            name.CopyTo(input[_namespaceLength..]);
            Span<byte> hash = stackalloc byte[20];
            _ = SHA1.HashData(input[..length], hash);
            hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
            return new Guid(hash[..16], bigEndian: true);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
#else
        byte[] input = new byte[_namespaceLength + name.Length];
        byte[] namespaceBytes = namespaceId.ToByteArray();
        SwapGuidByteOrder(namespaceBytes);
        namespaceBytes.CopyTo(input, 0);
        name.CopyTo(input.AsSpan(_namespaceLength));

        byte[] hash;
        using (var sha1 = SHA1.Create())
            hash = sha1.ComputeHash(input);

        Array.Resize(ref hash, 16);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        SwapGuidByteOrder(hash);
        return new Guid(hash);
#endif
    }

#if !NET
    private static void SwapGuidByteOrder(byte[] bytes)
    {
        (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
        (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }
#endif
}
