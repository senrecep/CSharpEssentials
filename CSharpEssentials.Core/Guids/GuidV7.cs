using System.Runtime.InteropServices;

namespace CSharpEssentials.Core;

internal static class GuidV7
{
    private const int _byteCount = 16, _counterBits = 12;
    private const long _counterSlots = 1 << _counterBits;
    private const long _unixEpochTicks = 621_355_968_000_000_000;

    private static long _lastPacked;

    internal static Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        FillRandom(bytes);
        return Compose((DateTime.UtcNow.Ticks - _unixEpochTicks) / TimeSpan.TicksPerMillisecond, bytes);
    }

    internal static Guid NewMonotonicGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        FillRandom(bytes);
        long utcTicks = DateTime.UtcNow.Ticks;
        long last, packed;
        do
        {
            last = Volatile.Read(ref _lastPacked);
            packed = Pack(last, utcTicks);
        }
        while (Interlocked.CompareExchange(ref _lastPacked, packed, last) != last);
        return ComposePacked(packed, bytes);
    }

    internal static long Pack(long last, long utcTicks)
    {
        long sinceEpoch = utcTicks - _unixEpochTicks;
        long counter = sinceEpoch % TimeSpan.TicksPerMillisecond * _counterSlots / TimeSpan.TicksPerMillisecond;
        long packed = (sinceEpoch / TimeSpan.TicksPerMillisecond << _counterBits) | counter;
        return packed > last ? packed : last + 1;
    }

    internal static Guid ComposePacked(long packed, Span<byte> bigEndian)
    {
        bigEndian[6] = (byte)((packed >> 8) & 0x0F);
        bigEndian[7] = (byte)packed;
        return Compose(packed >> _counterBits, bigEndian);
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

    private static void FillRandom(Span<byte> bytes)
    {
        var random = Guid.NewGuid();
#if NETSTANDARD
        MemoryMarshal.Write(bytes, ref random);
#else
        MemoryMarshal.Write(bytes, in random);
#endif
        bytes[6] = bytes[0];
        bytes[7] = bytes[1];
    }
}
