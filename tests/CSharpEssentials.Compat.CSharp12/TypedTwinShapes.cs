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
}
#endif
