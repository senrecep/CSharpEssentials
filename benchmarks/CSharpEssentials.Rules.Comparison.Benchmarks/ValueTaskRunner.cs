namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// Consumes a <see cref="ValueTask{T}"/> synchronously without paying the <see cref="ValueTask{T}.AsTask"/> allocation on the
/// completed fast path, so the numbers reflect an <c>await</c> of a sync-completing evaluation.
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
