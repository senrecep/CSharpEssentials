namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Converts the Maybe to a List.
    /// </summary>
    /// <returns></returns>
    public List<T> ToList() =>
#if NET8_0_OR_GREATER
        this.GetValueOrDefault<T, List<T>>(value => [value], []);
#else
        this.GetValueOrDefault<T, List<T>>(value => [value], []);
#endif
}
