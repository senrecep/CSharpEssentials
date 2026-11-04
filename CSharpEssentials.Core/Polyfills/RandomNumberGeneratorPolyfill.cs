#if NETSTANDARD2_0

namespace System.Security.Cryptography;

internal static class RandomNumberGeneratorPolyfill
{
    public static int GetInt32(int fromInclusive, int toExclusive)
    {
        if (fromInclusive >= toExclusive)
            throw new ArgumentOutOfRangeException(nameof(fromInclusive), "Range is invalid.");

        using var rng = new RNGCryptoServiceProvider();
        uint range = (uint)(toExclusive - fromInclusive);
        const ulong outcomes = 1UL << 32;
        ulong limit = outcomes - outcomes % range;
        byte[] bytes = new byte[4];
        uint random;
        do
        {
            rng.GetBytes(bytes);
            random = BitConverter.ToUInt32(bytes, 0);
        } while (random >= limit);
        return fromInclusive + (int)(random % range);
    }
}

#endif
