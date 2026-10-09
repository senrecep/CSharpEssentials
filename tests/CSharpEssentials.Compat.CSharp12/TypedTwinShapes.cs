#if NET9_0_OR_GREATER
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// The net9.0 form of <c>UntypedTaskOnlyShapes</c>: a Task handler and its ValueTask twin are both applicable to an untyped
/// async lambda, and C# 12 ignores <c>[OverloadResolutionPriority]</c>. Typing the handler picks one overload; C# 13+ needs no cast.
/// </summary>
public static class TypedTwinShapes
{
    public static Task<Result<string>> Map(Result<int> source) =>
        source.MapAsync((Func<int, Task<string>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }));

    public static ValueTask<Result<int>> MapValueTask(Result source) =>
        source.MapAsync(() => ValueTask.FromResult(1));

    public static Task<Result<int>> Tap(Result<int> source) =>
        source.TapAsync((Func<int, Task>)(async _ => await Task.Delay(1).ConfigureAwait(false)));

    public static ValueTask<Result> TapValueTask(Result source) =>
        source.TapAsync(() => ValueTask.CompletedTask);

    public static Task<string> Match(Result<int> source) =>
        source.MatchAsync(
            (Func<int, Task<string>>)(async v =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }),
            async errors =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return errors[0].Code;
            });

    public static Task<Result<int>> Ensure(Result<int> source, Error error) =>
        source.EnsureAsync((Func<int, Task<bool>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v > 0;
        }), error);

    public static Task<Result<int>> Then(Result<int> source) =>
        source.ThenAsync((Func<int, Task<Result<int>>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(v + 1);
        }));

    public static ValueTask<Result> ThenFromValueTask(ValueTask<Result> source) =>
        source.ThenAsync(() => ValueTask.FromResult(Result.Success()));

    public static Task<Result<int>> Else(Result<int> source) =>
        source.ElseAsync((Func<Error[], Task<int>>)(async _ =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return 0;
        }));

    public static ValueTask<Result> ElseFromValueTask(ValueTask<Result> source, Error error) =>
        source.ElseAsync(_ => ValueTask.FromResult(error));

    public static Task<Result<int>> FailIf(Result<int> source, Error error) =>
        source.FailIfAsync((Func<int, Task<bool>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v < 0;
        }), error);

    public static Task SwitchFirst(Result source) =>
        source.SwitchFirstAsync(
            (Func<Task>)(async () => await Task.Delay(1).ConfigureAwait(false)),
            async _ => await Task.Delay(1).ConfigureAwait(false));

    public static ValueTask SwitchLastFromValueTask(ValueTask<Result<int>> source) =>
        source.SwitchLastAsync(_ => ValueTask.CompletedTask, _ => ValueTask.CompletedTask);

    public static Task<Result<int>> TapError(Result<int> source) =>
        source.TapErrorAsync((Func<Error[], Task>)(async _ => await Task.Delay(1).ConfigureAwait(false)));

    public static ValueTask<Result> TapErrorFirstFromValueTask(ValueTask<Result> source) =>
        source.TapErrorFirstAsync(_ => ValueTask.CompletedTask);

    public static Task<Result> ElseDo(Result source) =>
        source.ElseDoAsync((Func<Error[], Task>)(async _ => await Task.Delay(1).ConfigureAwait(false)));

    public static Task<Result<int>> Compensate(Result<int> source) =>
        source.CompensateAsync((Func<Error[], Task<Result<int>>>)(async _ =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(0);
        }));

    public static Task<Result<int>> ThenEnsure(Result<int> source) =>
        source.ThenEnsureAsync((Func<int, Task<Result>>)(async _ =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result.Success();
        }));

    public static ValueTask<Result> FinallyFromValueTask(ValueTask<Result> source) =>
        source.FinallyAsync((Func<Result, Task>)(async _ => await Task.Delay(1).ConfigureAwait(false)));

    public static Task<Result<int>> Try(Error error) =>
        Result.TryAsync((Func<Task<int>>)(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return 1;
        }), _ => error);

    public static ValueTask<Result<int>> TryValueTask(Error error) =>
        Result.TryAsync(() => ValueTask.FromResult(1), _ => error);
}
#endif
