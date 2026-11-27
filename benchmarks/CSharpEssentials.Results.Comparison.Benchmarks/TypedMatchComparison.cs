using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Folds a result built in setup to a status code: the success value (42), 404 for not found, 409 for conflict, 400 for
/// validation. <c>Case</c> picks the success, the first error slot (not found) or the last one (validation). Match uses
/// each library's match function; Switch uses a C# switch expression over the case types, or the library's type test API.
/// <para>
/// Semantic differences: the single-error-type libraries read the kind from the error (CSE <see cref="Result{TValue}"/> and
/// ErrorOr from the first error's type, CFE from its <see cref="TypedError"/> slot, FluentResults from the error subclass)
/// and so do a second dispatch. FluentResults has no Match; its row uses the README's <c>IsSuccess</c>/<c>Value</c>/
/// <c>Errors[0]</c> idiom. The CSE typed switch and the native union switch are exhaustive with no discard arm. Dunet's base
/// record and OneOf's <c>Value</c> are open to the compiler, so their switches need a discard arm, and OneOf's
/// <c>Value</c> boxes the int. CSE Any's <c>Match</c> returns <c>AnyActionResult&lt;int&gt;</c>, read through <c>.Result</c>.
/// The native union row exists on net11.0 only.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TypedMatchComparison
{
    private const string MatchCategory = "Match";
    private const string SwitchCategory = "Switch";

    private Typed _typed;
    private Result<int> _result;
    private TypedAny _any;
    private TypedOneOf _oneOf;
    private OrderOutcome _dunet = null!;
    private Eo.ErrorOr<int> _errorOr;
    private Cfe.Result<int, TypedError> _cfe;
    private Fr.Result<int> _fluent = null!;
#if NET11_0_OR_GREATER
    private OrderUnion _native;
#endif

    [Params(TypedInputs.Success, TypedInputs.NotFound, TypedInputs.Validation)]
    public string Case { get; set; } = TypedInputs.Success;

    [GlobalSetup]
    public void Setup()
    {
        _typed = TypedInputs.Cse(Case);
        _result = TypedInputs.CseResult(Case);
        _any = TypedInputs.Any(Case);
        _oneOf = TypedInputs.OneOf(Case);
        _dunet = TypedInputs.Dunet(Case);
        _errorOr = TypedInputs.ErrorOr(Case);
        _cfe = TypedInputs.Cfe(Case);
        _fluent = TypedInputs.Fluent(Case);
        string expected = Parity.Of(TypedInputs.Expected(Case));
        Parity.Same(MatchCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_Typed_Match), Parity.Of(CSharpEssentials_Typed_Match())),
            (nameof(CSharpEssentials_Result_Match), Parity.Of(CSharpEssentials_Result_Match())),
            (nameof(CSharpEssentials_Any_Match), Parity.Of(CSharpEssentials_Any_Match())),
            (nameof(OneOf_Match), Parity.Of(OneOf_Match())),
            (nameof(Dunet_Match), Parity.Of(Dunet_Match())),
            (nameof(ErrorOr_Match), Parity.Of(ErrorOr_Match())),
            (nameof(CSharpFunctionalExtensions_Match), Parity.Of(CSharpFunctionalExtensions_Match())),
            (nameof(FluentResults_Match), Parity.Of(FluentResults_Match())));
        Parity.Same(SwitchCategory,
            ("expected", expected),
            (nameof(CSharpEssentials_Typed_Switch), Parity.Of(CSharpEssentials_Typed_Switch())),
            (nameof(CSharpEssentials_Typed_TryGetValue), Parity.Of(CSharpEssentials_Typed_TryGetValue())),
            (nameof(OneOf_Switch), Parity.Of(OneOf_Switch())),
            (nameof(Dunet_Switch), Parity.Of(Dunet_Switch())));
#if NET11_0_OR_GREATER
        _native = TypedInputs.Native(Case);
        Parity.Same(SwitchCategory + ".Native", ("expected", expected), (nameof(NativeUnion_Switch), Parity.Of(NativeUnion_Switch())));
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching-1
    [Benchmark(Baseline = true), BenchmarkCategory(MatchCategory)]
    public int CSharpEssentials_Typed_Match() =>
        _typed.Match(static v => v, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int CSharpEssentials_Result_Match() => _result.Match(static v => v, static errors => TypedInputs.Status(errors[0].Type));

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#pattern-matching
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int CSharpEssentials_Any_Match() =>
        _any.Match(static v => v, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus).Result;

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#matching
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int OneOf_Match() =>
        _oneOf.Match(static v => v, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus);

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#match-method
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int Dunet_Match() =>
        _dunet.Match(static ok => ok.Value, static _ => TypedInputs.NotFoundStatus, static _ => TypedInputs.ConflictStatus, static _ => TypedInputs.ValidationStatus);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#match-1
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int ErrorOr_Match() => _errorOr.Match(static v => v, static errors => TypedInputs.Status(errors[0].Type));

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Extensions/Match.cs
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int CSharpFunctionalExtensions_Match() => _cfe.Match(static v => v, static error => TypedInputs.Status(error));

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#processing-a-result
    [Benchmark, BenchmarkCategory(MatchCategory)]
    public int FluentResults_Match() => _fluent.IsSuccess ? _fluent.Value : TypedInputs.Status(_fluent.Errors[0]);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching-1
    [Benchmark(Baseline = true), BenchmarkCategory(SwitchCategory)]
    public int CSharpEssentials_Typed_Switch() => _typed switch
    {
        int v => v,
        NotFoundError => TypedInputs.NotFoundStatus,
        ConflictError => TypedInputs.ConflictStatus,
        ValidationError => TypedInputs.ValidationStatus,
    };

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#matching-1
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int CSharpEssentials_Typed_TryGetValue()
    {
        if (_typed.TryGetValue(out int value))
            return value;
        if (_typed.TryGetValue(out NotFoundError _))
            return TypedInputs.NotFoundStatus;
        return _typed.TryGetValue(out ConflictError _) ? TypedInputs.ConflictStatus : TypedInputs.ValidationStatus;
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/OneOf/OneOfT3.generated.cs
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int OneOf_Switch() => _oneOf.Value switch
    {
        int v => v,
        NotFoundError => TypedInputs.NotFoundStatus,
        ConflictError => TypedInputs.ConflictStatus,
        ValidationError => TypedInputs.ValidationStatus,
        _ => -1,
    };

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#usage
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int Dunet_Switch() => _dunet switch
    {
        OrderOutcome.Ok ok => ok.Value,
        OrderOutcome.Missing => TypedInputs.NotFoundStatus,
        OrderOutcome.Stale => TypedInputs.ConflictStatus,
        OrderOutcome.Invalid => TypedInputs.ValidationStatus,
        _ => -1,
    };

#if NET11_0_OR_GREATER
    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
    [Benchmark, BenchmarkCategory(SwitchCategory)]
    public int NativeUnion_Switch() => _native switch
    {
        int v => v,
        NotFoundError => TypedInputs.NotFoundStatus,
        ConflictError => TypedInputs.ConflictStatus,
        ValidationError => TypedInputs.ValidationStatus,
    };
#endif
}
