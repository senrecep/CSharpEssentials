using System.Reflection;
using System.Runtime.CompilerServices;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Guards the shape of the async matrix: every flavour-matched cell exists, and every Task overload that has a ValueTask twin
/// on the same receiver carries <c>[OverloadResolutionPriority(1)]</c> so untyped async lambdas keep binding to it.
/// Reflection is used only here, in the tests; the library stays reflection-free.
/// </summary>
public sealed class AsyncMatrixCompletenessTests
{
    private static readonly string[] UngatedCells =
    [
        "Result: Task<Result<TOut>> MapAsync<TOut>(Func<Task<TOut>>, CancellationToken)",
        "Result<TValue>: Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TOut>> MapAsync<TOut>(ValueTask<Result>, Func<ValueTask<TOut>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TOut>> MapAsync<TValue, TOut>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<TOut>>, CancellationToken)",
        "ResultExtensions: Task<Result<TOut>> BindAsync<TOut>(Task<Result>, Func<Task<Result<TOut>>>, CancellationToken)",
        "Result: Task<Result> TapAsync(Func<Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapAsync(Func<TValue, Task>, CancellationToken)",
        "ResultExtensions: Task<Result> TapAsync(Task<Result>, Func<Task>, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> TapAsync<TValue>(Task<Result<TValue>>, Func<TValue, Task>, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> TapAsync<TValue>(Task<Result<TValue>>, Func<Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapAsync(ValueTask<Result>, Func<ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapAsync<TValue>(ValueTask<Result<TValue>>, Func<ValueTask>, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> EnsureAsync<TValue>(Task<Result<TValue>>, Func<TValue, bool>, Error, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> EnsureAsync<TValue>(Task<Result<TValue>>, Func<TValue, bool>, Func<TValue, Error>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> EnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, bool>, Error, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> EnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, bool>, Func<TValue, Error>, CancellationToken)",
    ];

    private static readonly string[] GatedCells =
    [
        "Result: ValueTask<Result<TOut>> MapAsync<TOut>(Func<ValueTask<TOut>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TOut>> MapAsync<TOut>(Func<TValue, ValueTask<TOut>>, CancellationToken)",
        "Result: ValueTask<Result> TapAsync(Func<ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> TapAsync(Func<TValue, ValueTask>, CancellationToken)",

        "Result: ValueTask<T> MatchAsync<T>(Func<ValueTask<T>>, Func<Error[], ValueTask<T>>, CancellationToken)",
        "Result: ValueTask<T> MatchFirstAsync<T>(Func<ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "Result: ValueTask<T> MatchLastAsync<T>(Func<ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "Result: ValueTask SwitchAsync(Func<ValueTask>, Func<Error[], ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> ThenAsync(Func<ValueTask<Result>>, CancellationToken)",
        "Result: ValueTask<Result> ThenDoAsync(Func<ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<T> MatchAsync<T>(Func<TValue, ValueTask<T>>, Func<Error[], ValueTask<T>>, CancellationToken)",
        "Result<TValue>: ValueTask<T> MatchFirstAsync<T>(Func<TValue, ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "Result<TValue>: ValueTask<T> MatchLastAsync<T>(Func<TValue, ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "Result<TValue>: ValueTask SwitchAsync(Func<TValue, ValueTask>, Func<Error[], ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<T>> ThenAsync<T>(Func<TValue, ValueTask<Result<T>>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<T>> ThenAsync<T>(Func<TValue, ValueTask<T>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ThenDoAsync(Func<TValue, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> EnsureAsync(Func<TValue, ValueTask<bool>>, Error, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> EnsureAsync(Func<TValue, ValueTask<bool>>, Func<TValue, Error>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> TapIfAsync(bool, Func<TValue, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> TapIfAsync(Func<TValue, bool>, Func<TValue, ValueTask>, CancellationToken)",

        "ResultExtensions: ValueTask<T> MatchAsync<T>(ValueTask<Result>, Func<ValueTask<T>>, Func<Error[], ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask<T> MatchFirstAsync<T>(ValueTask<Result>, Func<ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask<T> MatchLastAsync<T>(ValueTask<Result>, Func<ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchAsync(ValueTask<Result>, Func<ValueTask>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ThenAsync(ValueTask<Result>, Func<ValueTask<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ThenDoAsync(ValueTask<Result>, Func<ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<T> MatchAsync<TValue, T>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<T>>, Func<Error[], ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask<T> MatchFirstAsync<TValue, T>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask<T> MatchLastAsync<TValue, T>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<T>>, Func<Error, ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<T>> ThenAsync<TValue, T>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<Result<T>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<T>> ThenAsync<TValue, T>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<T>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ThenDoAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> EnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<bool>>, Error, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> EnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<bool>>, Func<TValue, Error>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapIfAsync<TValue>(ValueTask<Result<TValue>>, bool, Func<TValue, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapIfAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, bool>, Func<TValue, ValueTask>, CancellationToken)",
    ];

    // Task-handler overloads that share a receiver with a ValueTask twin, so they need priority 1 for untyped async lambdas.
    private static readonly string[] PrioritizedTaskOverloads =
    [
        "Result: Task<Result<TOut>> MapAsync<TOut>(Func<Task<TOut>>, CancellationToken)",
        "Result<TValue>: Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>>, CancellationToken)",
        "Result: Task<Result> TapAsync(Func<Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapAsync(Func<TValue, Task>, CancellationToken)",
        "Result: Task<T> MatchAsync<T>(Func<Task<T>>, Func<Error[], Task<T>>, CancellationToken)",
        "Result: Task<T> MatchFirstAsync<T>(Func<Task<T>>, Func<Error, Task<T>>, CancellationToken)",
        "Result: Task<T> MatchLastAsync<T>(Func<Task<T>>, Func<Error, Task<T>>, CancellationToken)",
        "Result: Task SwitchAsync(Func<Task>, Func<Error[], Task>, CancellationToken)",
        "Result: Task<Result> ThenAsync(Func<Task<Result>>, CancellationToken)",
        "Result: Task<Result> ThenDoAsync(Func<Task>, CancellationToken)",
        "Result<TValue>: Task<Result<T>> ThenAsync<T>(Func<TValue, Task<Result<T>>>, CancellationToken)",
        "Result<TValue>: Task<Result<T>> ThenAsync<T>(Func<TValue, Task<T>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ThenDoAsync(Func<TValue, Task>, CancellationToken)",
        "Result<TValue>: Task SwitchAsync(Func<TValue, Task>, Func<Error[], Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapIfAsync(bool, Func<TValue, Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapIfAsync(Func<TValue, bool>, Func<TValue, Task>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchAsync(ValueTask<Result>, Func<Task>, Func<Error[], Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ThenAsync(ValueTask<Result>, Func<Task<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ThenDoAsync(ValueTask<Result>, Func<Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapIfAsync<TValue>(ValueTask<Result<TValue>>, bool, Func<TValue, Task>, CancellationToken)",
    ];

    private static readonly Lazy<Dictionary<string, MethodInfo>> Surface = new(BuildSurface);

    private static Dictionary<string, MethodInfo> BuildSurface()
    {
        var surface = new Dictionary<string, MethodInfo>(StringComparer.Ordinal);
        foreach (Type type in new[] { typeof(Result), typeof(Result<>), typeof(ResultExtensions) })
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                surface.TryAdd(Signature(type, method), method);
        }
        return surface;
    }

    private static string Signature(Type declaringType, MethodInfo method)
    {
        string generics = method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(Format)) + ">"
            : string.Empty;
        string parameters = string.Join(", ", method.GetParameters().Select(p => Format(p.ParameterType)));
        return $"{Format(declaringType)}: {Format(method.ReturnType)} {method.Name}{generics}({parameters})";
    }

    private static string Format(Type type)
    {
        if (type == typeof(bool))
            return "bool";
        if (type.IsArray)
            return Format(type.GetElementType()!) + "[]";
        if (!type.IsGenericType)
            return type.Name;
        string name = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)];
        return name + "<" + string.Join(", ", type.GetGenericArguments().Select(Format)) + ">";
    }

    [Fact]
    public void Matrix_Should_ContainEveryUngatedCell_When_InspectingPublicSurface()
    {
        IEnumerable<string> missing = UngatedCells.Where(cell => !Surface.Value.ContainsKey(cell));

        missing.Should().BeEmpty();
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void Matrix_Should_ContainEveryNet9Cell_When_InspectingPublicSurface()
    {
        IEnumerable<string> missing = GatedCells.Where(cell => !Surface.Value.ContainsKey(cell));

        missing.Should().BeEmpty();
    }
#endif

    [Fact]
    public void TaskOverloads_Should_CarryPriorityOne_When_AValueTaskTwinSharesTheReceiver()
    {
        IEnumerable<string> withoutPriority = PrioritizedTaskOverloads
            .Where(signature => !Surface.Value.TryGetValue(signature, out MethodInfo? method)
                || method.GetCustomAttribute<OverloadResolutionPriorityAttribute>()?.Priority != 1);

        withoutPriority.Should().BeEmpty();
    }

    [Fact]
    public void ValueTaskOverloads_Should_KeepDefaultPriority_When_TheyTwinATaskOverload()
    {
        IEnumerable<string> prioritized = GatedCells
            .Where(signature => Surface.Value.TryGetValue(signature, out MethodInfo? method)
                && method.GetCustomAttribute<OverloadResolutionPriorityAttribute>() is not null);

        prioritized.Should().BeEmpty();
    }
}
