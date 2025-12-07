using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CSharpEssentials.Core;

public static class RandomItemsExtensions
{
    public static T GetRandomItem<T>(this Span<T> source)
    {
        int index = RandomNumberGenerator.GetInt32(0, source.Length);
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

        Span<bool> selectedIndices = stackalloc bool[sourceLength];
        var resultArray = new T[count];
        int index = 0;

        while (index < count)
        {
            int randomIndex = RandomNumberGenerator.GetInt32(0, sourceLength);
            if (!selectedIndices[randomIndex].IsFalse())
                continue;
            selectedIndices[randomIndex] = true;
            resultArray[index++] = source[randomIndex];
        }

        return resultArray;
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


    private static void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            (array[j], array[i]) = (array[i], array[j]);
        }
    }

}
