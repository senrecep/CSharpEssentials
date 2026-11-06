#if NET5_0_OR_GREATER
using System.Security.Cryptography;
using System.Text.Json;

namespace CSharpEssentials.Mediator;

/// <summary>
/// A write-only stream that feeds what <see cref="JsonSerializer"/> writes into SHA-256, so a value is hashed as the
/// bytes of <c>JsonSerializer.SerializeToUtf8Bytes</c> without holding them in one array. One instance is reused per
/// thread; a serialization that throws drops it.
/// </summary>
internal sealed class Sha256JsonStream : Stream
{
    [ThreadStatic]
    private static Sha256JsonStream? t_cached;

    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <summary>Returns the upper-case hex SHA-256 of the JSON of <paramref name="value"/>.</summary>
    public static string HashToHex<T>(T value, JsonSerializerOptions? options)
    {
        Sha256JsonStream stream = t_cached ?? new Sha256JsonStream();
        t_cached = null;
        JsonSerializer.Serialize(stream, value, options);
        Span<byte> hash = stackalloc byte[32];
        stream._hash.GetHashAndReset(hash);
        t_cached = stream;
        return Convert.ToHexString(hash);
    }

    public override void Write(byte[] buffer, int offset, int count) => _hash.AppendData(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => _hash.AppendData(buffer);

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();
}
#endif
