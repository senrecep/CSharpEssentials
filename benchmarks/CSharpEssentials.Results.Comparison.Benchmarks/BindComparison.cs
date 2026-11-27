using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Bind <c>x => Ok(x + 1)</c> over a success or failure built in setup. CSE <c>Then</c> has the same body as
/// <c>Bind</c>; ErrorOr binds with the <c>Then(Func&lt;T, ErrorOr&lt;TNext&gt;&gt;)</c> overload. Each library builds
/// the inner success through its documented implicit conversion. The typed row binds a <c>Result&lt;int, FailureError&gt;</c>
/// with the same error code and message.
/// </summary>
[MemoryDiagnoser]
public class BindComparison
{
    private Result<int> _cse;
    private Result<int, FailureError> _typed;
    private Cfe.Result<int> _cfe;
    private Eo.ErrorOr<int> _errorOr;
    private Fr.Result<int> _fluent = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _cse = Inputs.Cse(Succeed);
        _typed = Inputs.Typed(Succeed);
        _cfe = Inputs.CfeOf(Succeed);
        _errorOr = Inputs.ErrorOr(Succeed);
        _fluent = Inputs.Fluent(Succeed);
        Parity.Same(nameof(BindComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpEssentials_Typed), Parity.Of(CSharpEssentials_Typed())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(FluentResults), Parity.Of(FluentResults())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#chaining
    [Benchmark(Baseline = true)]
    public Result<int> CSharpEssentials() => _cse.Bind(static x => (Result<int>)(x + 1));

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#combinators
    [Benchmark]
    public Result<int, FailureError> CSharpEssentials_Typed() => _typed.Bind(static x => (Result<int, FailureError>)(x + 1));

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Bind.cs
    [Benchmark]
    public Cfe.Result<int> CSharpFunctionalExtensions() => _cfe.Bind(static x => (Cfe.Result<int>)(x + 1));

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#then-1
    [Benchmark]
    public Eo.ErrorOr<int> ErrorOr() => _errorOr.Then(static x => (Eo.ErrorOr<int>)(x + 1));

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#bind-the-result-to-another-result
    [Benchmark]
    public Fr.Result<int> FluentResults() => _fluent.Bind(static x => (Fr.Result<int>)(x + 1));
}
