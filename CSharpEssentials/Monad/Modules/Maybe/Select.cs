namespace CSharpEssentials;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Maybe<TOut> Select<TOut>(Func<T, TOut> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Maybe<TOut> SelectMany<TOut>(Func<T, Maybe<TOut>> selector)
    {
        return Bind(selector);
    }

    /// <summary>
    /// Maps the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    /// <typeparam name="TP"></typeparam>
    /// <param name="selector"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public Maybe<TP> SelectMany<TS, TP>(
        Func<T, Maybe<TS>> selector,
        Func<T, TS, TP> project)
    {
        return this.GetValueOrDefault(
            x => selector(x).GetValueOrDefault(u => project(x, u), Maybe<TP>.None),
            Maybe<TP>.None);
    }
}
