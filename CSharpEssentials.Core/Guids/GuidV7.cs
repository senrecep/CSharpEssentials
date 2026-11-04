using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CSharpEssentials.Core;

internal static class GuidV7
{
    private const int _byteCount = 16, _fractionBits = 12, _counterBits = 6, _timestampShift = _fractionBits + _counterBits;
    private const long _fractionSlots = 1 << _fractionBits, _counterMask = (1 << _counterBits) - 1;
    private const int _poolSize = 1024, _randomStart = 6, _randomLength = _byteCount - _randomStart;
    private const long _unixEpochTicks = 621_355_968_000_000_000;
    private const int _lastPackedSlot = 8;
    internal const string DisableRandomPoolSwitch = "CSharpEssentials.Guider.DisableRandomPool";

    private static readonly bool _usePool = !(AppContext.TryGetSwitch(DisableRandomPoolSwitch, out bool disabled) && disabled);
#if NETSTANDARD2_0
    private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
#endif

    [ThreadStatic]
    private static byte[]? _pool;

    [ThreadStatic]
    private static int _poolOffset;

    [ThreadStatic]
    private static long _lastCallTicks;

    private static readonly long[] _lastPacked = new long[_lastPackedSlot * 2];

    internal static byte[]? CurrentPool() => _pool;

    internal static int CurrentPoolOffset() => _poolOffset;

    internal static Guid NewGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        long utcTicks = DateTime.UtcNow.Ticks;
        FillRandom(bytes, utcTicks, _usePool);
        return Compose((utcTicks - _unixEpochTicks) / TimeSpan.TicksPerMillisecond, bytes);
    }

    internal static Guid NewMonotonicGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        long utcTicks = DateTime.UtcNow.Ticks;
        FillRandom(bytes, utcTicks, _usePool);
        return ComposePacked(NextPacked(utcTicks), bytes);
    }

    internal static Guid NewSqlServerGuid()
    {
        Span<byte> bytes = stackalloc byte[_byteCount];
        long utcTicks = DateTime.UtcNow.Ticks;
        FillRandom(bytes, utcTicks, _usePool);
        return ComposeSqlServer(NextPacked(utcTicks), bytes);
    }

    private static long NextPacked(long utcTicks)
    {
        long last, packed;
        do
        {
            last = Volatile.Read(ref _lastPacked[_lastPackedSlot]);
            packed = Pack(last, utcTicks);
        }
        while (Interlocked.CompareExchange(ref _lastPacked[_lastPackedSlot], packed, last) != last);
        return packed;
    }

    internal static long Pack(long last, long utcTicks)
    {
        long sinceEpoch = utcTicks - _unixEpochTicks;
        long fraction = sinceEpoch % TimeSpan.TicksPerMillisecond * _fractionSlots / TimeSpan.TicksPerMillisecond;
        long packed = ((sinceEpoch / TimeSpan.TicksPerMillisecond << _fractionBits) | fraction) << _counterBits;
        return packed > last ? packed : last + 1;
    }

    internal static Guid ComposePacked(long packed, Span<byte> bigEndian)
    {
        bigEndian[6] = (byte)((packed >> (_counterBits + 8)) & 0x0F);
        bigEndian[7] = (byte)(packed >> _counterBits);
        bigEndian[8] = (byte)(packed & _counterMask);
        return Compose(packed >> _timestampShift, bigEndian);
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
        return FromBigEndian(b);
    }

    internal static Guid ComposeSqlServer(long packed, Span<byte> bigEndian)
    {
        Span<byte> b = bigEndian;
        long unixMs = packed >> _timestampShift;
        int sequence = (int)(packed & ((1 << _timestampShift) - 1));
        b.Slice(10, 6).CopyTo(b);
        b[6] = (byte)((b[6] & 0x0F) | 0x80);
        b[7] = (byte)((sequence << 4) | (b[7] & 0x0F));
        b[8] = (byte)(0x80 | (sequence >> 12));
        b[9] = (byte)(sequence >> 4);
        b[10] = (byte)(unixMs >> 40);
        b[11] = (byte)(unixMs >> 32);
        b[12] = (byte)(unixMs >> 24);
        b[13] = (byte)(unixMs >> 16);
        b[14] = (byte)(unixMs >> 8);
        b[15] = (byte)unixMs;
        return FromBigEndian(b);
    }

    private static Guid FromBigEndian(ReadOnlySpan<byte> b) =>
        new(
            (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3],
            (short)((b[4] << 8) | b[5]),
            (short)((b[6] << 8) | b[7]),
            b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15]);

    internal static void FillRandom(Span<byte> bytes, long utcTicks, bool usePool)
    {
        long sinceLastCall = utcTicks - _lastCallTicks;
        _lastCallTicks = utcTicks;
        if (usePool && (ulong)sinceLastCall <= TimeSpan.TicksPerMillisecond)
            TakeFromPool(bytes.Slice(_randomStart));
        else
        {
            FillFromNewGuid(bytes);
            if (usePool && _pool is { } pool)
                bytes.CopyTo(pool.AsSpan(_poolSize));
        }
        long fraction = utcTicks % TimeSpan.TicksPerMillisecond;
        bytes[14] ^= (byte)(fraction >> 8);
        bytes[15] ^= (byte)fraction;
    }

    private static void TakeFromPool(Span<byte> destination)
    {
        byte[] pool = _pool ?? AllocatePool();
        int offset = _poolOffset;
        if (offset > _poolSize - _randomLength)
        {
            Refill(pool);
            offset = 0;
        }
        ReadOnlySpan<byte> source = pool.AsSpan(offset, _randomLength);
        ReadOnlySpan<byte> salt = pool.AsSpan(_poolSize, _randomLength);
        for (int i = 0; i < _randomLength; i++)
            destination[i] = (byte)(source[i] ^ salt[i]);
        _poolOffset = offset + _randomLength;
    }

    private static void Refill(byte[] pool)
    {
#if NETSTANDARD2_0
        _random.GetBytes(pool);
#else
        RandomNumberGenerator.Fill(pool);
#endif
        WriteNewGuid(pool.AsSpan(_poolSize));
    }

    private static byte[] AllocatePool()
    {
        _pool = new byte[_poolSize + _byteCount];
        _poolOffset = _poolSize;
        return _pool;
    }

    private static void FillFromNewGuid(Span<byte> bytes)
    {
        WriteNewGuid(bytes);
        bytes[6] = bytes[0];
        bytes[7] = bytes[1];
    }

    private static void WriteNewGuid(Span<byte> destination)
    {
        var random = Guid.NewGuid();
#if NETSTANDARD
        MemoryMarshal.Write(destination, ref random);
#else
        MemoryMarshal.Write(destination, in random);
#endif
    }
}
