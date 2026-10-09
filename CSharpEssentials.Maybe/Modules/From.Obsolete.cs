namespace CSharpEssentials.Maybe;

public readonly partial record struct Maybe
{
    /// <summary>Obsolete alias of <c>FromAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FromAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> From<T>(Task<T?> valueTask, CancellationToken cancellationToken = default) =>
        FromAsync(valueTask, cancellationToken);

    /// <summary>Obsolete alias of <c>FromAsync</c>. Will be removed in 7.0.</summary>
    [Obsolete("Use FromAsync. Will be removed in 7.0.")]
    public static Task<Maybe<T>> From<T>(Func<Task<T?>> valueTaskFunc, CancellationToken cancellationToken = default) =>
        FromAsync(valueTaskFunc, cancellationToken);
}
