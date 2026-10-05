namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Consumes a <see cref="ValueTask{T}"/> synchronously without paying the
/// <see cref="ValueTask{T}.AsTask"/> allocation on the completed fast path,
/// so benchmark numbers reflect real <c>await</c> usage of sync-completing validators.
/// </summary>
internal static class ValueTaskRunner
{
    public static T Run<T>(this ValueTask<T> valueTask)
    {
        if (valueTask.IsCompletedSuccessfully)
            return valueTask.GetAwaiter().GetResult();
        return valueTask.AsTask().GetAwaiter().GetResult();
    }
}
