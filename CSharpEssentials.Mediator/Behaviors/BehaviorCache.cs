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
    internal static readonly Type ResultType = typeof(Result);
    internal static readonly Type GenericResultType = typeof(Result<>);

    /// <summary>
    /// Compiled <c>Result&lt;T&gt;.Failure(errors)</c> factories, keyed by concrete closed generic type.
    /// Bounded by the number of distinct <c>Result&lt;T&gt;</c> handler response types in the assembly.
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, Func<Error[], object>> FailureFactories = new();

    /// <summary>
    /// Returns (or compiles and caches) a delegate that invokes <c>Result&lt;T&gt;.Failure(errors)</c>
    /// for the closed generic <paramref name="responseType"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="responseType"/> is not a generic type.
    /// Callers must verify <see cref="Type.IsGenericType"/> before calling.
    /// </exception>
    internal static Func<Error[], object> GetOrCreateFactory(Type responseType)
    {
        if (!responseType.IsGenericType)
            throw new ArgumentException($"Expected a generic type, but got {responseType.FullName}.", nameof(responseType));

        return FailureFactories.GetOrAdd(responseType, static type =>
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
