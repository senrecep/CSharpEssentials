using CSharpEssentials.Maybe;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// Untyped async lambda shapes on <see cref="Task{TResult}"/> and <see cref="ValueTask{TResult}"/> Maybe sources that must bind under C# 12.
/// </summary>
public static class MaybeSourceShapes
{
    public static Task TaskExecute(Task<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.ExecuteAsync(async v => await Task.Delay(v, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static Task TaskExecuteNoValue(Task<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.ExecuteNoValueAsync(async () => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static Task<Maybe<int>> TaskOr(Task<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.OrAsync(async () =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return 1;
        }, cancellationToken);

    public static Task ValueTaskExecute(ValueTask<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.ExecuteAsync(async v => await Task.Delay(v, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static Task ValueTaskExecuteNoValue(ValueTask<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.ExecuteNoValueAsync(async () => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static ValueTask<Maybe<int>> ValueTaskOr(ValueTask<Maybe<int>> source, CancellationToken cancellationToken) =>
        source.OrAsync(async () =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return 1;
        }, cancellationToken);
}
