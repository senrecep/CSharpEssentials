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
        "ResultExtensions: Task<Result> TapAsync(Task<Result>, bool, Func<Task>, CancellationToken)",
        "ResultExtensions: Task<Result> TapAsync(Task<Result>, Func<bool>, Func<Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapAsync(ValueTask<Result>, bool, Func<ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapAsync(ValueTask<Result>, Func<bool>, Func<ValueTask>, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> TapAsync<TValue>(Task<Result<TValue>>, bool, Func<TValue, Task>, CancellationToken)",
        "ResultExtensions: Task<Result<TValue>> TapAsync<TValue>(Task<Result<TValue>>, Func<bool>, Func<TValue, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapAsync<TValue>(ValueTask<Result<TValue>>, bool, Func<TValue, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapAsync<TValue>(ValueTask<Result<TValue>>, Func<bool>, Func<TValue, ValueTask>, CancellationToken)",
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

        "Result: ValueTask<Result> ElseAsync(Func<Error[], ValueTask<Error>>, CancellationToken)",
        "Result: ValueTask<Result> ElseAsync(Func<Error[], ValueTask<IEnumerable<Error>>>, CancellationToken)",
        "Result: ValueTask<Result> ElseAsync(ValueTask<Error>, CancellationToken)",
        "Result: ValueTask SwitchFirstAsync(Func<ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "Result: ValueTask SwitchLastAsync(Func<ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> TapErrorAsync(Func<Error[], ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> TapErrorFirstAsync(Func<Error, ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> ElseDoAsync(Func<Error[], ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> ElseDoFirstAsync(Func<Error, ValueTask>, CancellationToken)",
        "Result: ValueTask<Result> CompensateAsync(Func<Error[], ValueTask<Result>>, CancellationToken)",
        "Result: ValueTask<Result> CompensateFirstAsync(Func<Error, ValueTask<Result>>, CancellationToken)",
        "Result: ValueTask<Result> TryAsync(Func<ValueTask>, Func<Exception, Error>, CancellationToken)",
        "Result: ValueTask<Result<TValue>> TryAsync<TValue>(Func<ValueTask<TValue>>, Func<Exception, Error>, CancellationToken)",
        "Result: ValueTask<Result<TValue>> TryAsync<TValue>(Func<ValueTask<Result<TValue>>>, Func<Exception, Error>, CancellationToken)",
        "Result: ValueTask<Result> TryAsync(Func<ValueTask<Result>>, Func<Exception, Error>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<TValue>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<Error>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseAsync(Func<Error[], ValueTask<Error[]>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseAsync(ValueTask<Error>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseAsync(ValueTask<TValue>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> FailIfAsync(Func<TValue, ValueTask<bool>>, Error, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> FailIfAsync(Func<TValue, ValueTask<bool>>, Func<TValue, ValueTask<Error>>, CancellationToken)",
        "Result<TValue>: ValueTask SwitchFirstAsync(Func<TValue, ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask SwitchLastAsync(Func<TValue, ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> TapErrorAsync(Func<Error[], ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> TapErrorFirstAsync(Func<Error, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseDoAsync(Func<Error[], ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ElseDoFirstAsync(Func<Error, ValueTask>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> CompensateAsync(Func<Error[], ValueTask<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> CompensateFirstAsync(Func<Error, ValueTask<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ThenEnsureAsync(Func<TValue, ValueTask<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: ValueTask<Result<TValue>> ThenEnsureAsync(Func<TValue, ValueTask<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, Func<Error[], ValueTask<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, Func<Error[], ValueTask<IEnumerable<Error>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, ValueTask<Error>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchFirstAsync(ValueTask<Result>, Func<ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchLastAsync(ValueTask<Result>, Func<ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapErrorAsync(ValueTask<Result>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapErrorFirstAsync(ValueTask<Result>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseDoAsync(ValueTask<Result>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseDoFirstAsync(ValueTask<Result>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> CompensateAsync(ValueTask<Result>, Func<Error[], ValueTask<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> CompensateFirstAsync(ValueTask<Result>, Func<Error, ValueTask<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<TOut> FinallyAsync<TOut>(ValueTask<Result>, Func<Result, ValueTask<TOut>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> FinallyAsync(ValueTask<Result>, Func<Result, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask<TValue>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask<Error[]>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, ValueTask<Error>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, ValueTask<TValue>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FailIfAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<bool>>, Error, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FailIfAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<bool>>, Func<TValue, ValueTask<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchLastAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapErrorAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapErrorFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseDoAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseDoFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, ValueTask>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> CompensateAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], ValueTask<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> CompensateFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, ValueTask<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, ValueTask<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<TOut> FinallyAsync<TValue, TOut>(ValueTask<Result<TValue>>, Func<Result<TValue>, ValueTask<TOut>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FinallyAsync<TValue>(ValueTask<Result<TValue>>, Func<Result<TValue>, ValueTask>, CancellationToken)",
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

        "Result: Task<Result> ElseAsync(Func<Error[], Task<Error>>, CancellationToken)",
        "Result: Task<Result> ElseAsync(Func<Error[], Task<IEnumerable<Error>>>, CancellationToken)",
        "Result: Task<Result> ElseAsync(Task<Error>, CancellationToken)",
        "Result: Task SwitchFirstAsync(Func<Task>, Func<Error, Task>, CancellationToken)",
        "Result: Task SwitchLastAsync(Func<Task>, Func<Error, Task>, CancellationToken)",
        "Result: Task<Result> TapErrorAsync(Func<Error[], Task>, CancellationToken)",
        "Result: Task<Result> TapErrorFirstAsync(Func<Error, Task>, CancellationToken)",
        "Result: Task<Result> ElseDoAsync(Func<Error[], Task>, CancellationToken)",
        "Result: Task<Result> ElseDoFirstAsync(Func<Error, Task>, CancellationToken)",
        "Result: Task<Result> CompensateAsync(Func<Error[], Task<Result>>, CancellationToken)",
        "Result: Task<Result> CompensateFirstAsync(Func<Error, Task<Result>>, CancellationToken)",
        "Result: Task<Result> TryAsync(Func<Task>, Func<Exception, Error>, CancellationToken)",
        "Result: Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>>, Func<Exception, Error>, CancellationToken)",
        "Result: Task<Result<TValue>> TryAsync<TValue>(Func<Task<Result<TValue>>>, Func<Exception, Error>, CancellationToken)",
        "Result: Task<Result> TryAsync(Func<Task<Result>>, Func<Exception, Error>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseAsync(Func<Error[], Task<TValue>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error[]>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseAsync(Task<Error>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseAsync(Task<TValue>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>>, Error, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>>, Func<TValue, Task<Error>>, CancellationToken)",
        "Result<TValue>: Task SwitchFirstAsync(Func<TValue, Task>, Func<Error, Task>, CancellationToken)",
        "Result<TValue>: Task SwitchLastAsync(Func<TValue, Task>, Func<Error, Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapErrorAsync(Func<Error[], Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> TapErrorFirstAsync(Func<Error, Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseDoAsync(Func<Error[], Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ElseDoFirstAsync(Func<Error, Task>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> CompensateAsync(Func<Error[], Task<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> CompensateFirstAsync(Func<Error, Task<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result<TValue>>>, CancellationToken)",
        "Result<TValue>: Task<Result<TValue>> ThenEnsureAsync(Func<TValue, Task<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, Func<Error[], Task<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, Func<Error[], Task<IEnumerable<Error>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseAsync(ValueTask<Result>, Task<Error>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchFirstAsync(ValueTask<Result>, Func<Task>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchLastAsync(ValueTask<Result>, Func<Task>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapErrorAsync(ValueTask<Result>, Func<Error[], Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> TapErrorFirstAsync(ValueTask<Result>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseDoAsync(ValueTask<Result>, Func<Error[], Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> ElseDoFirstAsync(ValueTask<Result>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> CompensateAsync(ValueTask<Result>, Func<Error[], Task<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> CompensateFirstAsync(ValueTask<Result>, Func<Error, Task<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result> FinallyAsync(ValueTask<Result>, Func<Result, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task<TValue>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task<Error[]>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Task<Error>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseAsync<TValue>(ValueTask<Result<TValue>>, Task<TValue>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FailIfAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task<bool>>, Error, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FailIfAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task<bool>>, Func<TValue, Task<Error>>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask SwitchLastAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapErrorAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> TapErrorFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseDoAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ElseDoFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, Task>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> CompensateAsync<TValue>(ValueTask<Result<TValue>>, Func<Error[], Task<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> CompensateFirstAsync<TValue>(ValueTask<Result<TValue>>, Func<Error, Task<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task<Result<TValue>>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> ThenEnsureAsync<TValue>(ValueTask<Result<TValue>>, Func<TValue, Task<Result>>, CancellationToken)",
        "ResultExtensions: ValueTask<Result<TValue>> FinallyAsync<TValue>(ValueTask<Result<TValue>>, Func<Result<TValue>, Task>, CancellationToken)",
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
