#if NETSTANDARD2_0

namespace System.Security.Cryptography;

internal static class RandomNumberGeneratorPolyfill
{
    public static int GetInt32(int fromInclusive, int toExclusive)
    {
        if (fromInclusive >= toExclusive)
            throw new ArgumentOutOfRangeException(nameof(fromInclusive), "Range is invalid.");

        using var rng = new RNGCryptoServiceProvider();
        int range = toExclusive - fromInclusive;
        byte[] bytes = new byte[4];
        rng.GetBytes(bytes);
        uint random = BitConverter.ToUInt32(bytes, 0);
        return fromInclusive + (int)(random % (uint)range);
    }
}

#endif
