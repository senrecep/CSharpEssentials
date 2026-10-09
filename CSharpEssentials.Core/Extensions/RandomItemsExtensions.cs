#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Buffers;
using System.Security.Cryptography;

namespace CSharpEssentials.Core;

public static class RandomItemsExtensions
{
    private const int StackAllocThreshold = 256;

    public static T GetRandomItem<T>(this Span<T> source)
    {
        int index = GetRandomInt32(0, source.Length);
        return source[index];
    }
    public static T[] GetRandomItems<T>(this Span<T> source, int count)
    {
        int sourceLength = source.Length;
        if (count >= sourceLength)
        {
            var result = new T[sourceLength];
            source.CopyTo(result);
            Shuffle(result);
            return result;
        }

        bool[]? rented = sourceLength <= StackAllocThreshold ? null : ArrayPool<bool>.Shared.Rent(sourceLength);
        Span<bool> selectedIndices = rented is null ? stackalloc bool[StackAllocThreshold] : rented.AsSpan(0, sourceLength);
        selectedIndices.Clear();

        try
        {
            var resultArray = new T[count];
            int index = 0;

            while (index < count)
            {
                int randomIndex = GetRandomInt32(0, sourceLength);
                if (!selectedIndices[randomIndex].IsFalse())
                    continue;
                selectedIndices[randomIndex] = true;
                resultArray[index++] = source[randomIndex];
            }

            return resultArray;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<bool>.Shared.Return(rented);
        }
    }

    public static T[] GetRandomItems<T>(this List<T> source, int count)
#if NET5_0_OR_GREATER
        => CollectionsMarshal.AsSpan(source).GetRandomItems(count);
#else
        => source.ToArray().AsSpan().GetRandomItems(count);
#endif

    public static T GetRandomItem<T>(this List<T> source)
#if NET5_0_OR_GREATER
        => CollectionsMarshal.AsSpan(source).GetRandomItem();
#else
        => source.ToArray().AsSpan().GetRandomItem();
#endif

    public static T[] GetRandomItems<T>(this T[] source, int count) =>
        source.AsSpan().GetRandomItems(count);

    public static T GetRandomItem<T>(this T[] source) =>
        source.AsSpan().GetRandomItem();


    private static int GetRandomInt32(int fromInclusive, int toExclusive)
    {
#if NETSTANDARD2_0
        return RandomNumberGeneratorPolyfill.GetInt32(fromInclusive, toExclusive);
#else
        return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
#endif
    }

    private static void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = GetRandomInt32(0, i + 1);
            (array[j], array[i]) = (array[i], array[j]);
        }
    }

}
