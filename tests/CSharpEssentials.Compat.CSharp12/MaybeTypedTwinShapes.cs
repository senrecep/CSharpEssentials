using CSharpEssentials.Maybe;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// Instance and key/value Maybe members have a Task handler and a ValueTask twin on every target. C# 12 ignores
/// <c>[OverloadResolutionPriority]</c>, so an untyped async lambda reports CS0121 there; typing the handler picks one overload.
/// C# 13+ needs no cast.
/// </summary>
public static class MaybeTypedTwinShapes
{
    public static Task Execute(Maybe<int> source, CancellationToken cancellationToken) =>
        source.ExecuteAsync((Func<int, Task>)(async v => await Task.Delay(v, cancellationToken).ConfigureAwait(false)), cancellationToken);

    public static Task ExecuteValueTask(Maybe<int> source, CancellationToken cancellationToken) =>
        source.ExecuteAsync(_ => ValueTask.CompletedTask, cancellationToken);

    public static Task ExecuteNoValue(Maybe<int> source, CancellationToken cancellationToken) =>
        source.ExecuteNoValueAsync((Func<Task>)(async () => await Task.Delay(1, cancellationToken).ConfigureAwait(false)), cancellationToken);

    public static Task<Maybe<int>> Or(Maybe<int> source, CancellationToken cancellationToken) =>
        source.OrAsync((Func<Task<int>>)(async () =>
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return 1;
        }), cancellationToken);

    public static ValueTask<Maybe<int>> OrValueTask(Maybe<int> source, CancellationToken cancellationToken) =>
        source.OrAsync(() => ValueTask.FromResult(1), cancellationToken);

    public static Task<string> Match(Maybe<int> source, CancellationToken cancellationToken) =>
        source.MatchAsync(
            (Func<int, CancellationToken, Task<string>>)(async (v, ct) =>
            {
                await Task.Delay(v, ct).ConfigureAwait(false);
                return "some";
            }),
            async ct =>
            {
                await Task.Delay(1, ct).ConfigureAwait(false);
                return "none";
            },
            cancellationToken);

    public static Task<string> KeyValueMatch(Maybe<KeyValuePair<string, int>> source, CancellationToken cancellationToken) =>
        source.MatchAsync(
            (Func<string, int, CancellationToken, Task<string>>)(async (k, v, ct) =>
            {
                await Task.Delay(v, ct).ConfigureAwait(false);
                return k;
            }),
            async ct =>
            {
                await Task.Delay(1, ct).ConfigureAwait(false);
                return "none";
            },
            cancellationToken);
}
