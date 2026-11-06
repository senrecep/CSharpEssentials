using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Core;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Awaits a task under a live cancellation token that is never cancelled. Pending wraps a task that is still running
/// and then completes it; Completed wraps an already completed task, which both libraries return as is. On cancellation
/// CSE throws <see cref="OperationCanceledException"/> and <c>WaitAsync</c> its subclass
/// <see cref="TaskCanceledException"/>; the setup checks both throw an <see cref="OperationCanceledException"/>.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet owns the lifetime: the source is created in [GlobalSetup] and disposed in [GlobalCleanup].")]
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class WithCancellationComparison
{
    private const string Pending = "Pending";
    private const string Completed = "Completed";
    private const int Value = 42;

    private CancellationTokenSource _cts = null!;
    private readonly Task<int> _completed = Task.FromResult(Value);

    [GlobalSetup]
    public void Setup()
    {
        _cts = new CancellationTokenSource();
        Parity.Same(Pending,
            ("expected", Parity.Of(Value)),
            (nameof(CSharpEssentials_Pending), Parity.Of(CSharpEssentials_Pending().GetAwaiter().GetResult())),
            (nameof(Bcl_Pending), Parity.Of(Bcl_Pending().GetAwaiter().GetResult())));
        Parity.Same(Completed,
            ("expected", Parity.Of(Value)),
            (nameof(CSharpEssentials_Completed), Parity.Of(CSharpEssentials_Completed().GetAwaiter().GetResult())),
            (nameof(Bcl_Completed), Parity.Of(Bcl_Completed().GetAwaiter().GetResult())));
        Parity.Same("Cancelled",
            ("expected", bool.TrueString),
            (nameof(CSharpEssentials_Pending), ThrowsOnCancel(static (t, token) => t.WithCancellation(token))),
            (nameof(Bcl_Pending), ThrowsOnCancel(static (t, token) => t.WaitAsync(token))));
    }

    [GlobalCleanup]
    public void Cleanup() => _cts.Dispose();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#task-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Pending)]
    public async Task<int> CSharpEssentials_Pending()
    {
        var source = new TaskCompletionSource<int>();
        Task<int> task = source.Task.WithCancellation(_cts.Token);
        source.SetResult(Value);
        return await task;
    }

    // https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.waitasync#system-threading-tasks-task-1-waitasync(system-threading-cancellationtoken)
    [Benchmark, BenchmarkCategory(Pending)]
    public async Task<int> Bcl_Pending()
    {
        var source = new TaskCompletionSource<int>();
        Task<int> task = source.Task.WaitAsync(_cts.Token);
        source.SetResult(Value);
        return await task;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#task-helpers
    [Benchmark(Baseline = true), BenchmarkCategory(Completed)]
    public async Task<int> CSharpEssentials_Completed() => await _completed.WithCancellation(_cts.Token);

    // https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1.waitasync#system-threading-tasks-task-1-waitasync(system-threading-cancellationtoken)
    [Benchmark, BenchmarkCategory(Completed)]
    public async Task<int> Bcl_Completed() => await _completed.WaitAsync(_cts.Token);

    private static string ThrowsOnCancel(Func<Task<int>, CancellationToken, Task<int>> wrap)
    {
        using var cts = new CancellationTokenSource();
        Task<int> task = wrap(new TaskCompletionSource<int>().Task, cts.Token);
        cts.Cancel();
        try
        {
            _ = task.GetAwaiter().GetResult();
            return bool.FalseString;
        }
        catch (OperationCanceledException)
        {
            return bool.TrueString;
        }
    }
}
