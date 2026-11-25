using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// A four-step chain over a result built in setup: Map <c>x * 2</c>, Bind <c>x + 1</c>, Bind that fails with the not-found
/// error when <c>x &lt;= 0</c>, then Match to the status code. <c>Succeed=true</c> starts from 42 and ends at 85;
/// <c>Succeed=false</c> starts from the not-found error and ends at 404. Each library builds the inner success through its
/// implicit conversion.
/// <para>
/// Semantic differences: CFE carries one <see cref="TypedError"/> slot, ErrorOr and CSE <see cref="Result{TValue}"/> read the
/// kind from the error's type. ErrorOr maps and binds with <c>Then</c>. OneOf has no Bind; its row maps with <c>MapT0</c> and
/// binds with <c>Match</c>, rewrapping each error case, as its README shows for chaining. FluentResults has no Match; its last
/// step uses the README's <c>IsSuccess</c>/<c>Value</c>/<c>Errors[0]</c> idiom.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class TypedBindChainComparison
{
    private Typed _typed;
    private Result<int> _result;
    private TypedOneOf _oneOf;
    private Eo.ErrorOr<int> _errorOr;
    private Cfe.Result<int, TypedError> _cfe;
    private Fr.Result<int> _fluent = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string kind = Succeed ? TypedInputs.Success : TypedInputs.NotFound;
        _typed = TypedInputs.Cse(kind);
        _result = TypedInputs.CseResult(kind);
        _oneOf = TypedInputs.OneOf(kind);
        _errorOr = TypedInputs.ErrorOr(kind);
        _cfe = TypedInputs.Cfe(kind);
        _fluent = TypedInputs.Fluent(kind);
        Parity.Same(nameof(TypedBindChainComparison),
            ("expected", Parity.Of(Succeed ? 85 : TypedInputs.NotFoundStatus)),
            (nameof(CSharpEssentials_Typed), Parity.Of(CSharpEssentials_Typed())),
            (nameof(CSharpEssentials_Result), Parity.Of(CSharpEssentials_Result())),
            (nameof(OneOf), Parity.Of(OneOf())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(FluentResults), Parity.Of(FluentResults())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#combinators
    [Benchmark(Baseline = true)]
    public int CSharpEssentials_Typed() => _typed
        .Map(static x => x * 2)
        .Bind(static x => (Typed)(x + 1))
        .Bind(static x => x > 0 ? (Typed)x : TypedInputs.Missing)
        .Match(static v => v, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#chaining
    [Benchmark]
    public int CSharpEssentials_Result() => _result
        .Map(static x => x * 2)
        .Bind(static x => (Result<int>)(x + 1))
        .Bind(static x => x > 0 ? (Result<int>)x : TypedInputs.CseMissing)
        .Match(static v => v, static errors => TypedInputs.Status(errors[0].Type));

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/OneOf/OneOfT3.generated.cs
    [Benchmark]
    public int OneOf() => _oneOf
        .MapT0(static x => x * 2)
        .Match(static x => (TypedOneOf)(x + 1), static e => e, static e => e, static e => e)
        .Match(static x => x > 0 ? (TypedOneOf)x : TypedInputs.Missing, static e => e, static e => e, static e => e)
        .Match(static v => v, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#mixing-features-then-failif-else-switch-match
    [Benchmark]
    public int ErrorOr() => _errorOr
        .Then(static x => x * 2)
        .Then(static x => (Eo.ErrorOr<int>)(x + 1))
        .Then(static x => x > 0 ? (Eo.ErrorOr<int>)x : TypedInputs.ErrorOrMissing)
        .Match(static v => v, static errors => TypedInputs.Status(errors[0].Type));

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/README.md#compose-multiple-operations-in-a-single-chain
    [Benchmark]
    public int CSharpFunctionalExtensions() => _cfe
        .Map(static x => x * 2)
        .Bind(static x => (Cfe.Result<int, TypedError>)(x + 1))
        .Bind(static x => x > 0 ? (Cfe.Result<int, TypedError>)x : TypedInputs.Missing)
        .Match(static v => v, static error => TypedInputs.Status(error));

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#bind-the-result-to-another-result
    [Benchmark]
    public int FluentResults()
    {
        Fr.Result<int> r = _fluent
            .Map(static x => x * 2)
            .Bind(static x => (Fr.Result<int>)(x + 1))
            .Bind(static x => x > 0 ? Fr.Result.Ok(x) : Fr.Result.Fail<int>(TypedInputs.FluentMissing));
        return r.IsSuccess ? r.Value : TypedInputs.Status(r.Errors[0]);
    }
}
