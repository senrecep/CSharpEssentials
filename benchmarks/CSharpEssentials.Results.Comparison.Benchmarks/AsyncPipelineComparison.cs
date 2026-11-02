using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Five-step async pipeline over a <c>Task</c> source holding 42: Bind (async step, x + 1) -&gt; Map (x * 2) -&gt;
/// Ensure (x &gt; 0) -&gt; Tap (write a field) -&gt; Map (x + 1). With <c>Succeed</c> false the async Bind step returns a
/// failure, so the failure is created at step 1 and flows through the rest. With <c>Yield</c> false the source and the
/// step complete synchronously; with <c>Yield</c> true both complete after <see cref="Task.Yield"/>. Delegates that
/// capture state are cached in setup, the same way for every library. FluentResults has no column: of the five steps
/// only Bind and a sync-mapper Map exist on <c>Task&lt;Result&lt;T&gt;&gt;</c> (no Ensure, no Tap).
/// </summary>
[MemoryDiagnoser]
public class AsyncPipelineComparison
{
    private static readonly Task<Result<int>> CseCompleted = Task.FromResult<Result<int>>(Inputs.Value);
    private static readonly Task<Cfe.Result<int>> CfeCompleted = Task.FromResult<Cfe.Result<int>>(Inputs.Value);
    private static readonly Task<Eo.ErrorOr<int>> ErrorOrCompleted = Task.FromResult<Eo.ErrorOr<int>>(Inputs.Value);

    private Func<int, Task<Result<int>>> _cseStep = null!;
    private Func<int, Task<Cfe.Result<int>>> _cfeStep = null!;
    private Func<int, Task<Eo.ErrorOr<int>>> _errorOrStep = null!;
    private Action<int> _tap = null!;
    private int _sink;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool succeed = Succeed;
        _tap = x => _sink = x;
        if (Yield)
        {
            _cseStep = x => CseStepYield(x, succeed);
            _cfeStep = x => CfeStepYield(x, succeed);
            _errorOrStep = x => ErrorOrStepYield(x, succeed);
        }
        else
        {
            _cseStep = x => Task.FromResult(CseStep(x, succeed));
            _cfeStep = x => Task.FromResult(CfeStep(x, succeed));
            _errorOrStep = x => Task.FromResult(ErrorOrStep(x, succeed));
        }

        Parity.Same(nameof(AsyncPipelineComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials().GetAwaiter().GetResult()) + Sink()),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions().GetAwaiter().GetResult()) + Sink()),
            (nameof(ErrorOr), Parity.Of(ErrorOr().GetAwaiter().GetResult()) + Sink()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#async-overload-matrix
    [Benchmark(Baseline = true)]
    public Task<Result<int>> CSharpEssentials() =>
        (Yield ? CseSourceYield() : CseCompleted)
            .BindAsync(_cseStep)
            .MapAsync(static x => x * 2)
            .EnsureAsync(static x => x > 0, Inputs.CseError)
            .TapAsync(_tap)
            .MapAsync(static x => x + 1);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#compose-multiple-operations-in-a-single-chain
    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Bind.Task.cs
    [Benchmark]
    public Task<Cfe.Result<int>> CSharpFunctionalExtensions() =>
        (Yield ? CfeSourceYield() : CfeCompleted)
            .Bind(_cfeStep)
            .Map(static x => x * 2)
            .Ensure(static x => x > 0, Inputs.CfeError)
            .Tap(_tap)
            .Map(static x => x + 1);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#mixing-then-thendo-thenasync-thendoasync
    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#mixing-features-then-failif-else-switch-match
    [Benchmark]
    public Task<Eo.ErrorOr<int>> ErrorOr() =>
        (Yield ? ErrorOrSourceYield() : ErrorOrCompleted)
            .ThenAsync(_errorOrStep)
            .Then(static x => x * 2)
            .FailIf(static x => x <= 0, Inputs.ErrorOrError)
            .ThenDo(_tap)
            .Then(static x => x + 1);

    private static Result<int> CseStep(int x, bool succeed) => succeed ? x + 1 : Inputs.CseError;

    private static Cfe.Result<int> CfeStep(int x, bool succeed) =>
        succeed ? x + 1 : Cfe.Result.Failure<int>(Inputs.CfeError);

    private static Eo.ErrorOr<int> ErrorOrStep(int x, bool succeed) => succeed ? x + 1 : Inputs.ErrorOrError;

    private static async Task<Result<int>> CseStepYield(int x, bool succeed)
    {
        await Task.Yield();
        return CseStep(x, succeed);
    }

    private static async Task<Cfe.Result<int>> CfeStepYield(int x, bool succeed)
    {
        await Task.Yield();
        return CfeStep(x, succeed);
    }

    private static async Task<Eo.ErrorOr<int>> ErrorOrStepYield(int x, bool succeed)
    {
        await Task.Yield();
        return ErrorOrStep(x, succeed);
    }

    private static async Task<Result<int>> CseSourceYield()
    {
        await Task.Yield();
        return Inputs.Value;
    }

    private static async Task<Cfe.Result<int>> CfeSourceYield()
    {
        await Task.Yield();
        return Inputs.Value;
    }

    private static async Task<Eo.ErrorOr<int>> ErrorOrSourceYield()
    {
        await Task.Yield();
        return Inputs.Value;
    }

    /// <summary>Reads and resets the sink so the parity check also proves the Tap step ran exactly on success.</summary>
    private string Sink()
    {
        int sink = _sink;
        _sink = 0;
        return "|sink:" + sink.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
