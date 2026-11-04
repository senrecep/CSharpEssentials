using System.Security.Cryptography;

namespace CSharpEssentials.Core;

internal static class GuidV7
{
    private const int _byteCount = 16, _poolSize = 4096;
    private const long _unixEpochTicks = 621_355_968_000_000_000;

#if NETSTANDARD2_0
    private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
#endif

    [ThreadStatic]
    private static byte[]? _pool;

    [ThreadStatic]
    private static int _poolOffset;

    internal static Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        FillRandom(bytes.Slice(6));
        return Compose((DateTime.UtcNow.Ticks - _unixEpochTicks) / TimeSpan.TicksPerMillisecond, bytes);
    }

    internal static Guid Compose(long unixMs, Span<byte> bigEndian)
    {
        Span<byte> b = bigEndian;
        b[0] = (byte)(unixMs >> 40);
        b[1] = (byte)(unixMs >> 32);
        b[2] = (byte)(unixMs >> 24);
        b[3] = (byte)(unixMs >> 16);
        b[4] = (byte)(unixMs >> 8);
        b[5] = (byte)unixMs;
        b[6] = (byte)((b[6] & 0x0F) | 0x70);
        b[8] = (byte)((b[8] & 0x3F) | 0x80);
        return new Guid(
            (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3],
            (short)((b[4] << 8) | b[5]),
            (short)((b[6] << 8) | b[7]),
            b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15]);
    }

    private static void FillRandom(Span<byte> destination)
    {
        byte[]? pool = _pool;
        if (pool is null)
        {
            pool = _pool = new byte[_poolSize];
            _poolOffset = _poolSize;
        }
        if (_poolOffset > _poolSize - destination.Length)
        {
#if NETSTANDARD2_0
            _random.GetBytes(pool);
#else
            RandomNumberGenerator.Fill(pool);
#endif
            _poolOffset = 0;
        }
        pool.AsSpan(_poolOffset, destination.Length).CopyTo(destination);
        _poolOffset += destination.Length;
    }
}
