namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Converts the Maybe monad to a nullable value.
    /// </summary>
    /// <returns></returns>
    public T? AsNullable()
    {
        if (TryGetValue(out T? result))
            return result;
        return default;
    }
}
