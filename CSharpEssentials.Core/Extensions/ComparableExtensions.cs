namespace CSharpEssentials.Core;

public static class ComparableExtensions
{
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
    {
        Comparer<T> comparer = Comparer<T>.Default;
        return comparer.Compare(value, min) >= 0 && comparer.Compare(value, max) <= 0;
    }

    public static bool IsBetweenExclusive<T>(this T value, T min, T max) where T : IComparable<T>
    {
        Comparer<T> comparer = Comparer<T>.Default;
        return comparer.Compare(value, min) > 0 && comparer.Compare(value, max) < 0;
    }
}
