using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// Untyped async lambda shapes on <see cref="ValueTask{TResult}"/> sources that must bind under C# 12.
/// </summary>
public static class ValueTaskSourceShapes
{
    public static ValueTask<Result<int>> Map(ValueTask<Result<int>> source, CancellationToken cancellationToken) =>
        source.MapAsync(async v =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return v + 1;
        }, cancellationToken);

    public static ValueTask<Result<int>> Bind(ValueTask<Result<int>> source) =>
        source.BindAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(v * 2);
        });

    public static ValueTask<Result<int>> Tap(ValueTask<Result<int>> source, CancellationToken cancellationToken) =>
        source.TapAsync(async _ => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static ValueTask<Result> TapResult(ValueTask<Result> source) =>
        source.TapAsync(async () => await Task.Delay(1).ConfigureAwait(false));

    public static ValueTask<Result<int>> TapWhen(ValueTask<Result<int>> source, bool condition, CancellationToken cancellationToken) =>
        source.TapAsync(condition, async _ => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static ValueTask<Result> TapValueTaskWhen(ValueTask<Result> source, bool condition) =>
        source.TapAsync(condition, () => ValueTask.CompletedTask);

    public static ValueTask<Result> TapValueTaskWhenFunc(ValueTask<Result> source, Func<bool> condition) =>
        source.TapAsync(condition, () => ValueTask.CompletedTask);

    public static ValueTask<Result<int>> TapValueTaskWhenOfT(ValueTask<Result<int>> source, bool condition) =>
        source.TapAsync(condition, _ => ValueTask.CompletedTask);

    public static ValueTask<Result<int>> TapValueTaskWhenFuncOfT(ValueTask<Result<int>> source, Func<bool> condition) =>
        source.TapAsync(condition, _ => ValueTask.CompletedTask);

    public static ValueTask<Result> TapResultWhen(ValueTask<Result> source, Func<bool> condition) =>
        source.TapAsync(condition, async () => await Task.Delay(1).ConfigureAwait(false));

    public static ValueTask<Result<int>> Ensure(ValueTask<Result<int>> source, Error error) =>
        source.EnsureAsync(v => v > 0, _ => error);

    public static ValueTask<Result> ThenSync(ValueTask<Result> source) =>
        source.ThenAsync(Result.Success);
}
