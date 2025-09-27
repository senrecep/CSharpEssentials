namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Converts the Maybe to a List.
    /// </summary>
    /// <returns></returns>
    public List<T> ToList() =>
        this.GetValueOrDefault<T, List<T>>(value => [value], []);
}
