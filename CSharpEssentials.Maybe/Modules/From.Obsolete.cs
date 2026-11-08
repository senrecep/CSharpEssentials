namespace CSharpEssentials.Maybe;

public readonly partial record struct Maybe
{
    /// <summary>Obsolete alias of <c>FromAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use FromAsync. Will be removed in a future major version.")]
    public static Task<Maybe<T>> From<T>(Task<T?> valueTask, CancellationToken cancellationToken = default) =>
        FromAsync(valueTask, cancellationToken);

    /// <summary>Obsolete alias of <c>FromAsync</c>. Will be removed in a future major version.</summary>
    [Obsolete("Use FromAsync. Will be removed in a future major version.")]
    public static Task<Maybe<T>> From<T>(Func<Task<T?>> valueTaskFunc, CancellationToken cancellationToken = default) =>
        FromAsync(valueTaskFunc, cancellationToken);
}
