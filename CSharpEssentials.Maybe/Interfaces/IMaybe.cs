namespace CSharpEssentials.Maybe.Interfaces;

public interface IMaybe;
public partial interface IMaybe<T> : IMaybe
{
    T Value { get; }
    bool HasValue { get; }
    bool HasNoValue { get; }

    T? AsNullable();
    Task<Maybe<TOut>> BindAsync<TOut>(
            Func<T, Task<Maybe<TOut>>> selector,
            CancellationToken cancellationToken = default);

    Maybe<TOut> Bind<TOut>(
        Func<T, Maybe<TOut>> selector);

    Maybe<TOut> Bind<TOut, TContext>(
        Func<T, TContext, Maybe<TOut>> selector,
        TContext context
    );

    Task Execute(Func<T, Task> action, CancellationToken cancellationToken = default);
    Task Execute(Func<T, ValueTask> valueTask, CancellationToken cancellationToken = default);
    void Execute(Action<T> action);
    Task ExecuteNoValue(Func<Task> action, CancellationToken cancellationToken = default);
    Task ExecuteNoValue(Func<ValueTask> valueTask, CancellationToken cancellationToken = default);
    void ExecuteNoValue(Action action);

    Task<T> GetValueOrDefaultAsync(Func<Task<T>> defaultValue, CancellationToken cancellationToken = default);
    Task<TOut> GetValueOrDefaultAsync<TOut>(Func<T, TOut> selector,
        Func<Task<TOut>> defaultValue, CancellationToken cancellationToken = default);
    Task<TOut> GetValueOrDefaultAsync<TOut>(Func<T, Task<TOut>> selector,
        TOut defaultValue, CancellationToken cancellationToken = default);

    Task<Maybe<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> selector, CancellationToken cancellationToken = default);
    ValueTask<Maybe<TOut>> MapAsync<TOut>(Func<T, ValueTask<TOut>> valueTask, CancellationToken cancellationToken = default);
    Maybe<TOut> Map<TOut>(Func<T, TOut> selector);

    TE Match<TE>(Func<T, TE> some, Func<TE> none);
    TE Match<TE, TContext>(
        Func<T, TContext, TE> some,
        Func<TContext, TE> none,
        TContext context
    );
    void Match(Action<T> some, Action none);
    void Match<TContext>(
        Action<T, TContext> some,
        Action<TContext> none,
        TContext context
    );
    Task<TE> Match<TE>(
           Func<T, CancellationToken, Task<TE>> some,
           Func<CancellationToken, Task<TE>> none,
           CancellationToken cancellationToken = default
       );

    Task<TE> Match<TE, TContext>(
        Func<T, TContext, CancellationToken, Task<TE>> some,
        Func<TContext, CancellationToken, Task<TE>> none,
        TContext context,
        CancellationToken cancellationToken = default
    );

    Task Match(
        Func<T, CancellationToken, Task> some,
        Func<CancellationToken, Task> none,
        CancellationToken cancellationToken = default
    );
    Task Match<TContext>(
        Func<T, TContext, CancellationToken, Task> some,
        Func<TContext, CancellationToken, Task> none,
        TContext context,
        CancellationToken cancellationToken = default
    );
    ValueTask<TE> Match<TE>(
          Func<T, CancellationToken, ValueTask<TE>> some,
          Func<CancellationToken, ValueTask<TE>> none,
          CancellationToken cancellationToken = default
      );
    ValueTask<TE> Match<TE, TContext>(
      Func<T, TContext, CancellationToken, ValueTask<TE>> some,
      Func<TContext, CancellationToken, ValueTask<TE>> none,
      TContext context,
      CancellationToken cancellationToken = default
  );
    ValueTask Match(
         Func<T, CancellationToken, ValueTask> some,
         Func<CancellationToken, ValueTask> none,
         CancellationToken cancellationToken = default
     );
    ValueTask Match<TContext>(
       Func<T, TContext, CancellationToken, ValueTask> some,
       Func<TContext, CancellationToken, ValueTask> none,
       TContext context,
       CancellationToken cancellationToken = default
   );

    Maybe<T> Or(Func<T> fallbackOperation);
    Maybe<T> Or(Maybe<T> fallback);
    Maybe<T> Or(Func<Maybe<T>> fallbackOperation);
    Task<Maybe<T>> Or(Func<Task<T>> fallbackOperation, CancellationToken cancellationToken = default);
    Task<Maybe<T>> Or(Task<Maybe<T>> fallback, CancellationToken cancellationToken = default);
    Task<Maybe<T>> Or(Func<Task<Maybe<T>>> fallbackOperation, CancellationToken cancellationToken = default);
    ValueTask<Maybe<T>> Or(Func<ValueTask<T>> valueTaskFallbackOperation, CancellationToken cancellationToken = default);
    ValueTask<Maybe<T>> Or(ValueTask<Maybe<T>> valueTaskFallback, CancellationToken cancellationToken = default);
    ValueTask<Maybe<T>> Or(Func<ValueTask<Maybe<T>>> valueTaskFallbackOperation, CancellationToken cancellationToken = default);

    Maybe<TOut> Select<TOut>(Func<T, TOut> selector);
    Maybe<TOut> SelectMany<TOut>(Func<T, Maybe<TOut>> selector);
    Maybe<TP> SelectMany<TS, TP>(
        Func<T, Maybe<TS>> selector,
        Func<T, TS, TP> project);

    List<T> ToList();

    Maybe<T> Where(Func<T, bool> predicate);
    Task<Maybe<T>> Where(Func<T, Task<bool>> predicate, CancellationToken cancellationToken = default);
    ValueTask<Maybe<T>> Where(Func<T, ValueTask<bool>> predicate, CancellationToken cancellationToken = default);
}