using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// One async Bind (<c>x + 1</c>) over an awaitable source that holds 42 or the not-found error. With <c>Yield</c> false the
/// source and the step are already completed; with <c>Yield</c> true both complete after <see cref="Task.Yield"/>. The
/// completed sources are cached in setup; the step delegates are cached too and build a new completed task per call, the
/// same way for every library. The Task rows use a <c>Task</c> source and a <c>Task</c> step; the ValueTask rows use a
/// <c>ValueTask</c> source and step.
/// <para>
/// ErrorOr (<c>ThenAsync</c>), CFE (<c>Bind</c> on <c>Task&lt;Result&lt;T, E&gt;&gt;</c>) and FluentResults (<c>Bind</c> on
/// <c>Task&lt;Result&lt;T&gt;&gt;</c>) have Task overloads only. CFE carries one <see cref="TypedError"/> slot; FluentResults
/// keeps the kind in an error subclass and returns a class result.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class TypedAsyncBindComparison
{
    private Typed _typed;
    private Result<int> _result;
    private Eo.ErrorOr<int> _errorOr;
    private Cfe.Result<int, TypedError> _cfe;
    private Task<Typed> _typedCompleted = null!;
    private Task<Result<int>> _resultCompleted = null!;
    private Task<Eo.ErrorOr<int>> _errorOrCompleted = null!;
    private Task<Cfe.Result<int, TypedError>> _cfeCompleted = null!;
    private Fr.Result<int> _fluent = null!;
    private Task<Fr.Result<int>> _fluentCompleted = null!;
    private Func<int, Task<Fr.Result<int>>> _fluentStep = null!;
    private Func<int, Task<Typed>> _typedStep = null!;
    private Func<int, ValueTask<Typed>> _typedValueStep = null!;
    private Func<int, Task<Result<int>>> _resultStep = null!;
    private Func<int, ValueTask<Result<int>>> _resultValueStep = null!;
    private Func<int, Task<Eo.ErrorOr<int>>> _errorOrStep = null!;
    private Func<int, Task<Cfe.Result<int, TypedError>>> _cfeStep = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [Params(false, true)]
    public bool Yield { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string kind = Succeed ? TypedInputs.Success : TypedInputs.NotFound;
        _typed = TypedInputs.Cse(kind);
        _result = TypedInputs.CseResult(kind);
        _errorOr = TypedInputs.ErrorOr(kind);
        _cfe = TypedInputs.Cfe(kind);
        _typedCompleted = Task.FromResult(_typed);
        _resultCompleted = Task.FromResult(_result);
        _errorOrCompleted = Task.FromResult(_errorOr);
        _cfeCompleted = Task.FromResult(_cfe);
        _fluent = TypedInputs.Fluent(kind);
        _fluentCompleted = Task.FromResult(_fluent);
        if (Yield)
        {
            _typedStep = static x => Later<Typed>(x + 1);
            _typedValueStep = static x => LaterValue<Typed>(x + 1);
            _resultStep = static x => Later<Result<int>>(x + 1);
            _resultValueStep = static x => LaterValue<Result<int>>(x + 1);
            _errorOrStep = static x => Later<Eo.ErrorOr<int>>(x + 1);
            _cfeStep = static x => Later<Cfe.Result<int, TypedError>>(x + 1);
            _fluentStep = static x => Later(Fr.Result.Ok(x + 1));
        }
        else
        {
            _typedStep = static x => Task.FromResult<Typed>(x + 1);
            _typedValueStep = static x => new ValueTask<Typed>(x + 1);
            _resultStep = static x => Task.FromResult<Result<int>>(x + 1);
            _resultValueStep = static x => new ValueTask<Result<int>>(x + 1);
            _errorOrStep = static x => Task.FromResult<Eo.ErrorOr<int>>(x + 1);
            _cfeStep = static x => Task.FromResult<Cfe.Result<int, TypedError>>(x + 1);
            _fluentStep = static x => Task.FromResult(Fr.Result.Ok(x + 1));
        }

        Parity.Same(nameof(TypedAsyncBindComparison),
            ("expected", Parity.Of(Succeed ? Inputs.Value + 1 : TypedInputs.NotFoundStatus)),
            (nameof(CSharpEssentials_Typed_Task), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_Task().GetAwaiter().GetResult()))),
            (nameof(CSharpEssentials_Typed_ValueTask), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_ValueTask().AsTask().GetAwaiter().GetResult()))),
            (nameof(CSharpEssentials_Result_Task), Parity.Of(TypedInputs.Read(CSharpEssentials_Result_Task().GetAwaiter().GetResult()))),
            (nameof(CSharpEssentials_Result_ValueTask), Parity.Of(TypedInputs.Read(CSharpEssentials_Result_ValueTask().AsTask().GetAwaiter().GetResult()))),
            (nameof(ErrorOr_Task), Parity.Of(TypedInputs.Read(ErrorOr_Task().GetAwaiter().GetResult()))),
            (nameof(CSharpFunctionalExtensions_Task), Parity.Of(TypedInputs.Read(CSharpFunctionalExtensions_Task().GetAwaiter().GetResult()))),
            (nameof(FluentResults_Task), Parity.Of(TypedInputs.Read(FluentResults_Task().GetAwaiter().GetResult()))));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#async-combinators
    [Benchmark(Baseline = true)]
    public Task<Typed> CSharpEssentials_Typed_Task() =>
        (Yield ? Later(_typed) : _typedCompleted).BindAsync(_typedStep);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#async-combinators
    [Benchmark]
    public ValueTask<Typed> CSharpEssentials_Typed_ValueTask() =>
        (Yield ? LaterValue(_typed) : new ValueTask<Typed>(_typed)).BindAsync(_typedValueStep);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#async-overload-matrix
    [Benchmark]
    public Task<Result<int>> CSharpEssentials_Result_Task() =>
        (Yield ? Later(_result) : _resultCompleted).BindAsync(_resultStep);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#async-overload-matrix
    [Benchmark]
    public ValueTask<Result<int>> CSharpEssentials_Result_ValueTask() =>
        (Yield ? LaterValue(_result) : new ValueTask<Result<int>>(_result)).BindAsync(_resultValueStep);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#mixing-then-thendo-thenasync-thendoasync
    [Benchmark]
    public Task<Eo.ErrorOr<int>> ErrorOr_Task() =>
        (Yield ? Later(_errorOr) : _errorOrCompleted).ThenAsync(_errorOrStep);

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Bind.Task.cs
    [Benchmark]
    public Task<Cfe.Result<int, TypedError>> CSharpFunctionalExtensions_Task() =>
        (Yield ? Later(_cfe) : _cfeCompleted).Bind(_cfeStep);

    // https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Extensions/ResultExtensions.cs
    [Benchmark]
    public Task<Fr.Result<int>> FluentResults_Task() =>
        Fr.Extensions.ResultExtensions.Bind(Yield ? Later(_fluent) : _fluentCompleted, _fluentStep);

    private static async Task<T> Later<T>(T value)
    {
        await Task.Yield();
        return value;
    }

    private static async ValueTask<T> LaterValue<T>(T value)
    {
        await Task.Yield();
        return value;
    }
}
