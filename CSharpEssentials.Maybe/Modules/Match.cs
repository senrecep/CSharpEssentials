
namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <returns></returns>
    public TE Match<TE>(Func<T, TE> some, Func<TE> none) =>
        HasValue ? some(Value) : none();

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public TE Match<TE, TContext>(
        Func<T, TContext, TE> some,
        Func<TContext, TE> none,
        TContext context
    ) => HasValue ? some(Value, context) : none(context);

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <param name="some"></param>
    /// <param name="none"></param>
    public void Match(Action<T> some, Action none)
    {
        if (HasValue)
            some(Value);
        else
            none();
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    public void Match<TContext>(
        Action<T, TContext> some,
        Action<TContext> none,
        TContext context
    )
    {
        if (HasValue)
            some(Value, context);
        else
            none(context);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TE> Match<TE>(
           Func<T, CancellationToken, Task<TE>> some,
           Func<CancellationToken, Task<TE>> none,
           CancellationToken cancellationToken = default
       ) => HasValue
            ? await some(Value, cancellationToken)
            : await none(cancellationToken);

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TE> Match<TE, TContext>(
        Func<T, TContext, CancellationToken, Task<TE>> some,
        Func<TContext, CancellationToken, Task<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        return HasValue
            ? await some(Value, context, cancellationToken)
            : await none(context, cancellationToken);
    }


    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Match(
        Func<T, CancellationToken, Task> some,
        Func<CancellationToken, Task> none,
        CancellationToken cancellationToken = default
    )
    {
        if (HasValue)
            await some(Value, cancellationToken);
        else
            await none(cancellationToken);
    }


    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Match<TContext>(
        Func<T, TContext, CancellationToken, Task> some,
        Func<TContext, CancellationToken, Task> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (HasValue)
            await some(Value, context, cancellationToken);
        else
            await none(context, cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<TE> Match<TE>(
          Func<T, CancellationToken, ValueTask<TE>> some,
          Func<CancellationToken, ValueTask<TE>> none,
          CancellationToken cancellationToken = default
      )
    {
        return HasValue
            ? await some(Value, cancellationToken)
            : await none(cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<TE> Match<TE, TContext>(
        Func<T, TContext, CancellationToken, ValueTask<TE>> some,
        Func<TContext, CancellationToken, ValueTask<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        return HasValue
            ? await some(Value, context, cancellationToken)
            : await none(context, cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask Match(
        Func<T, CancellationToken, ValueTask> some,
        Func<CancellationToken, ValueTask> none,
        CancellationToken cancellationToken = default
    )
    {
        if (HasValue)
            await some(Value, cancellationToken);
        else
            await none(cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask Match<TContext>(
        Func<T, TContext, CancellationToken, ValueTask> some,
        Func<TContext, CancellationToken, ValueTask> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (HasValue)
            await some(Value, context, cancellationToken);
        else
            await none(context, cancellationToken);
    }

}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <returns></returns>
    public static TE Match<TE, TKey, TValue>(
        in this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TE> some,
        Func<TE> none
    )
    {
        return maybe.HasValue
            ? some.Invoke(maybe.Value.Key, maybe.Value.Value)
            : none.Invoke();
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static TE Match<TE, TKey, TValue, TContext>(
        in this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, TE> some,
        Func<TContext, TE> none,
        TContext context
    )
    {
        return maybe.HasValue
            ? some.Invoke(maybe.Value.Key, maybe.Value.Value, context)
            : none.Invoke(context);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    public static void Match<TKey, TValue>(
        in this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Action<TKey, TValue> some,
        Action none
    )
    {
        if (maybe.HasValue)
            some.Invoke(maybe.Value.Key, maybe.Value.Value);
        else
            none.Invoke();
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    public static void Match<TKey, TValue, TContext>(
        in this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Action<TKey, TValue, TContext> some,
        Action<TContext> none,
        TContext context
    )
    {
        if (maybe.HasValue)
            some.Invoke(maybe.Value.Key, maybe.Value.Value, context);
        else
            none.Invoke(context);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TE> Match<TE, TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, ValueTask<TE>> some,
        Func<CancellationToken, ValueTask<TE>> none,
        CancellationToken cancellationToken = default
    )
    {
        return maybe.HasValue
            ? await some.Invoke(
                maybe.Value.Key,
                maybe.Value.Value,
                cancellationToken
            )
            : await none.Invoke(cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TE> Match<TE, TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, ValueTask<TE>> some,
        Func<TContext, CancellationToken, ValueTask<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        return maybe.HasValue
            ? await some.Invoke(
                maybe.Value.Key,
                maybe.Value.Value,
                context,
                cancellationToken
            )
            : await none.Invoke(context, cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask Match<TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, ValueTask> some,
        Func<CancellationToken, ValueTask> none,
        CancellationToken cancellationToken = default
    )
    {
        if (maybe.HasNoValue)
        {
            await none.Invoke(cancellationToken);
            return;
        }
        await some.Invoke(
                maybe.Value.Key,
                maybe.Value.Value,
                cancellationToken
            );
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask Match<TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, ValueTask> some,
        Func<TContext, CancellationToken, ValueTask> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (maybe.HasNoValue)
        {
            await none.Invoke(context, cancellationToken);
            return;
        }

        await some.Invoke(
            maybe.Value.Key,
            maybe.Value.Value,
            context,
            cancellationToken
        );
    }


    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TE> Match<TE, TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, Task<TE>> some,
        Func<CancellationToken, Task<TE>> none,
        CancellationToken cancellationToken = default
    )
    {
        return maybe.HasValue
            ? await some.Invoke(
                maybe.Value.Key,
                maybe.Value.Value,
                cancellationToken
            )
            : await none.Invoke(cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TE"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TE> Match<TE, TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, Task<TE>> some,
        Func<TContext, CancellationToken, Task<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        return maybe.HasValue
            ? await some.Invoke(
                maybe.Value.Key,
                maybe.Value.Value,
                context,
                cancellationToken
            )
            : await none.Invoke(context, cancellationToken);
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Match<TKey, TValue>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, CancellationToken, Task> some,
        Func<CancellationToken, Task> none,
        CancellationToken cancellationToken = default
    )
    {
        if (maybe.HasNoValue)
        {
            await none.Invoke(cancellationToken);
            return;
        }
        await some.Invoke(
            maybe.Value.Key,
            maybe.Value.Value,
            cancellationToken
        );
    }

    /// <summary>
    /// Matches the value of the Maybe to a new value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="some"></param>
    /// <param name="none"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Match<TKey, TValue, TContext>(
        this Maybe<KeyValuePair<TKey, TValue>> maybe,
        Func<TKey, TValue, TContext, CancellationToken, Task> some,
        Func<TContext, CancellationToken, Task> none,
        TContext context,
        CancellationToken cancellationToken = default
    )
    {
        if (maybe.HasNoValue)
        {
            await none.Invoke(context, cancellationToken);
            return;
        }
        await some.Invoke(
            maybe.Value.Key,
            maybe.Value.Value,
            context,
            cancellationToken
        );
    }
}