using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CSharpEssentials;
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
            if (selectedIndices[randomIndex].IsFalse())
            {
                selectedIndices[randomIndex] = true;
                resultArray[index++] = source[randomIndex];
            }
        }

        return resultArray;
    }

    public static T[] GetRandomItems<T>(this List<T> source, int count) =>
        CollectionsMarshal.AsSpan(source).GetRandomItems(count);

    public static T GetRandomItem<T>(this List<T> source) =>
        CollectionsMarshal.AsSpan(source).GetRandomItem();

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
