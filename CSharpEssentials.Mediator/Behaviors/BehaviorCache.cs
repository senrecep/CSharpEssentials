using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Shared static cache for pipeline behaviors that need to construct
/// <see cref="Result"/> / <see cref="Result{T}"/> failure responses.
/// <para>
/// <see cref="FailureFactories"/> is keyed by closed generic <see cref="Result{T}"/> types.
/// Growth is bounded by the number of distinct <c>Result&lt;T&gt;</c> response types
/// declared across all handlers in the consuming assembly — typically 5–50 entries.
/// Entries are never stale, so eviction is intentionally absent.
/// </para>
/// </summary>
internal static class BehaviorCache
{
    public static readonly Type ResultType = typeof(Result);
    public static readonly Type GenericResultType = typeof(Result<>);

    /// <summary>
    /// Compiled <c>Result&lt;T&gt;.Failure(errors)</c> factories, keyed by concrete closed generic type.
    /// Bounded by the number of distinct <c>Result&lt;T&gt;</c> handler response types in the assembly.
    /// </summary>
    public static readonly ConcurrentDictionary<Type, Func<Error[], object>> FailureFactories = new();

    /// <summary>
    /// Returns (or compiles and caches) a delegate that invokes <c>Result&lt;T&gt;.Failure(errors)</c>
    /// for the closed generic type derived from <paramref name="responseType"/>.
    /// </summary>
    internal static Func<Error[], object> GetOrCreateFactory(Type responseType)
    {
        Type genericType = GenericResultType.MakeGenericType(responseType.GenericTypeArguments[0]);
        return FailureFactories.GetOrAdd(genericType, static type =>
        {
            MethodInfo method = type.GetMethod(nameof(Result.Failure), [typeof(IEnumerable<Error>)])
                ?? throw new InvalidOperationException($"Method {nameof(Result.Failure)} not found on {type.FullName}.");
            ParameterExpression param = Expression.Parameter(typeof(Error[]), "errors");
            UnaryExpression asEnumerable = Expression.Convert(param, typeof(IEnumerable<Error>));
            MethodCallExpression call = Expression.Call(method, asEnumerable);
            UnaryExpression boxed = Expression.Convert(call, typeof(object));
            return Expression.Lambda<Func<Error[], object>>(boxed, param).Compile();
        });
    }
}
