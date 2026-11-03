using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class SequenceComparison
{
    private const string SequenceCategory = "Sequence";
    private const string CombineAllCategory = "CombineAll";

    private Result<int>[] _cse = [];
    private Cfe.Result<int>[] _cfe = [];
    private Eo.ErrorOr<int>[] _errorOr = [];
    private Fr.Result<int>[] _fluent = [];
    private Result[] _cseUnit = [];
    private Cfe.Result[] _cfeUnit = [];
    private Eo.ErrorOr<Eo.Success>[] _errorOrUnit = [];
    private Fr.Result[] _fluentUnit = [];

    [Params(10, 100)]
    public int N { get; set; }

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        int failAt = Succeed ? -1 : N / 2;
        _cse = new Result<int>[N];
        _cfe = new Cfe.Result<int>[N];
        _errorOr = new Eo.ErrorOr<int>[N];
        _fluent = new Fr.Result<int>[N];
        _cseUnit = new Result[N];
        _cfeUnit = new Cfe.Result[N];
        _errorOrUnit = new Eo.ErrorOr<Eo.Success>[N];
        _fluentUnit = new Fr.Result[N];
        for (int i = 0; i < N; i++)
        {
            bool ok = i != failAt;
            _cse[i] = ok ? i : Inputs.CseError;
            _cfe[i] = ok ? Cfe.Result.Success(i) : Cfe.Result.Failure<int>(Inputs.CfeError);
            _errorOr[i] = ok ? i : Inputs.ErrorOrError;
            _fluent[i] = ok ? Fr.Result.Ok(i) : Fr.Result.Fail<int>(Inputs.FluentError);
            _cseUnit[i] = ok ? Result.Success() : Inputs.CseError;
            _cfeUnit[i] = ok ? Cfe.Result.Success() : Cfe.Result.Failure(Inputs.CfeError);
            _errorOrUnit[i] = ok ? Eo.Result.Success : Inputs.ErrorOrError;
            _fluentUnit[i] = ok ? Fr.Result.Ok() : Fr.Result.Fail(Inputs.FluentError);
        }

        Parity.Same(nameof(SequenceComparison) + "." + SequenceCategory,
            (nameof(CSharpEssentials), ErrorParity.Of(CSharpEssentials_Sequence())),
            (nameof(CSharpFunctionalExtensions), ErrorParity.Of(CSharpFunctionalExtensions_Sequence())),
            (nameof(ErrorOr), ErrorParity.Of(ErrorOr_Sequence())),
            (nameof(FluentResults), ErrorParity.Of(FluentResults_Sequence())));
        Parity.Same(nameof(SequenceComparison) + "." + CombineAllCategory,
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials_CombineAll())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions_CombineAll())),
            (nameof(ErrorOr), Parity.Of(ErrorOr_CombineAll())),
            (nameof(FluentResults), Parity.Of(FluentResults_CombineAll())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(SequenceCategory)]
    public Result<int[]> CSharpEssentials_Sequence() => _cse.Sequence();

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Combine.cs
    [Benchmark, BenchmarkCategory(SequenceCategory)]
    public Cfe.Result<int[]> CSharpFunctionalExtensions_Sequence() =>
        Cfe.ResultExtensions.Combine(_cfe).Map(static values => values.ToArray());

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#support-for-multiple-errors
    [Benchmark, BenchmarkCategory(SequenceCategory)]
    public Eo.ErrorOr<List<int>> ErrorOr_Sequence()
    {
        Eo.ErrorOr<int>[] source = _errorOr;
        List<int> values = [];
        List<Eo.Error> errors = [];
        foreach (Eo.ErrorOr<int> item in source)
        {
            if (item.IsError)
                errors.AddRange(item.Errors);
            else
                values.Add(item.Value);
        }

        return errors.Count > 0 ? errors : values;
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#merging
    [Benchmark, BenchmarkCategory(SequenceCategory)]
    public Fr.Result<IEnumerable<int>> FluentResults_Sequence() => Fr.Result.Merge(_fluent);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#collection-extensions
    [Benchmark(Baseline = true), BenchmarkCategory(CombineAllCategory)]
    public Result CSharpEssentials_CombineAll() => _cseUnit.CombineAll();

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Combine.cs
    [Benchmark, BenchmarkCategory(CombineAllCategory)]
    public Cfe.Result CSharpFunctionalExtensions_CombineAll() => Cfe.Result.Combine(_cfeUnit);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#support-for-multiple-errors
    [Benchmark, BenchmarkCategory(CombineAllCategory)]
    public Eo.ErrorOr<Eo.Success> ErrorOr_CombineAll()
    {
        List<Eo.Error> errors = [];
        foreach (Eo.ErrorOr<Eo.Success> item in _errorOrUnit)
        {
            if (item.IsError)
                errors.AddRange(item.Errors);
        }

        return errors.Count > 0 ? errors : Eo.Result.Success;
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#merging
    [Benchmark, BenchmarkCategory(CombineAllCategory)]
    public Fr.Result FluentResults_CombineAll() => Fr.Result.Merge(_fluentUnit);
}
