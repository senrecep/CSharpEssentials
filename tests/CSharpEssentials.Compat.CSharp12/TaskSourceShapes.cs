using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// Untyped async lambda shapes on <see cref="Task{TResult}"/> sources that must bind under C# 12.
/// </summary>
public static class TaskSourceShapes
{
    public static Task<Result<string>> Map(Task<Result<int>> source, CancellationToken cancellationToken) =>
        source.MapAsync(async v =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }, cancellationToken);

    public static Task<Result<int>> MapSync(Task<Result<int>> source) =>
        source.MapAsync(v => v + 1);

    public static Task<Result<int>> Bind(Task<Result<int>> source, CancellationToken cancellationToken) =>
        source.BindAsync(async v =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(v * 2);
        }, cancellationToken);

    public static Task<Result<int>> BindFromResult(Task<Result> source) =>
        source.BindAsync(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(42);
        });

    public static Task<Result<int>> Tap(Task<Result<int>> source, CancellationToken cancellationToken) =>
        source.TapAsync(async _ => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static Task<Result<int>> TapSync(Task<Result<int>> source, ICollection<int> seen) =>
        source.TapAsync(seen.Add);

    public static Task<Result> TapResult(Task<Result> source) =>
        source.TapAsync(async () => await Task.Delay(1).ConfigureAwait(false));

    public static Task<Result<int>> TapWhen(Task<Result<int>> source, Func<bool> condition, CancellationToken cancellationToken) =>
        source.TapAsync(condition, async _ => await Task.Delay(1, cancellationToken).ConfigureAwait(false), cancellationToken);

    public static Task<Result<int>> TapSyncWhen(Task<Result<int>> source, bool condition, ICollection<int> seen) =>
        source.TapAsync(condition, seen.Add);

    public static Task<Result<int>> TapTaskWhen(Task<Result<int>> source, bool condition) =>
        source.TapAsync(condition, _ => Task.CompletedTask);

    public static Task<Result<int>> TapTaskWhenFunc(Task<Result<int>> source, Func<bool> condition) =>
        source.TapAsync(condition, _ => Task.CompletedTask);

    public static Task<Result> TapResultTaskWhen(Task<Result> source, bool condition) =>
        source.TapAsync(condition, () => Task.CompletedTask);

    public static Task<Result> TapResultTaskWhenFunc(Task<Result> source, Func<bool> condition) =>
        source.TapAsync(condition, () => Task.CompletedTask);

    public static Task<Result> TapResultWhen(Task<Result> source, bool condition) =>
        source.TapAsync(condition, async () => await Task.Delay(1).ConfigureAwait(false));

    public static Task<Result<int>> Ensure(Task<Result<int>> source, Error error) =>
        source.EnsureAsync(v => v > 0, error);

    public static Task<Result<int>> MapError(Task<Result<int>> source) =>
        source.MapErrorAsync(async error =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Error.Failure("MAPPED", error.Description);
        });

    public static Task<string> Match(Task<Result<int>> source) =>
        source.MatchAsync(
            async v =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return "ok:" + v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            },
            async errors =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return errors[0].Code;
            });

    public static Task<Result<int>> Then(Task<Result<int>> source) =>
        source.ThenAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(v + 1);
        });
}
