using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Builds a result through each library's documented conversion and stores it, <see cref="TypedInputs.Batch"/> times per
/// invocation; the report shows the time per build (<c>OperationsPerInvoke</c>). Ok: from the success value 42. CachedError:
/// from the cached not-found error. NewError: from a conflict error created in the loop, so the error's own allocation is
/// part of the row (a <see cref="ConflictError"/> record for the typed rows, OneOf, Any, Dunet, CFE and the native union; an
/// <see cref="Error"/> struct for CSE <see cref="Result{TValue}"/> and ErrorOr; a FluentResults error subclass).
/// <para>
/// Every library holds the same four cases except the single-error-type ones: CSE <see cref="Result{TValue}"/> and ErrorOr
/// keep the kind in the error's type, CFE uses <c>Result&lt;int, TypedError&gt;</c> and FluentResults an error subclass.
/// Dunet builds a record class for every case, including success. The native C# 15 union (net11.0 only) stores the case as
/// <see cref="object"/>, so the success value is boxed.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TypedCreateComparison
{
    private const string OkCategory = "Ok";
    private const string CachedErrorCategory = "CachedError";
    private const string NewErrorCategory = "NewError";

    private readonly int _value = Inputs.Value;
    private readonly Typed[] _typed = new Typed[TypedInputs.Batch];
    private readonly Result<int>[] _result = new Result<int>[TypedInputs.Batch];
    private readonly TypedAny[] _any = new TypedAny[TypedInputs.Batch];
    private readonly TypedOneOf[] _oneOf = new TypedOneOf[TypedInputs.Batch];
    private readonly OrderOutcome[] _dunet = new OrderOutcome[TypedInputs.Batch];
    private readonly Eo.ErrorOr<int>[] _errorOr = new Eo.ErrorOr<int>[TypedInputs.Batch];
    private readonly Cfe.Result<int, TypedError>[] _cfe = new Cfe.Result<int, TypedError>[TypedInputs.Batch];
    private readonly Fr.Result<int>[] _fluent = new Fr.Result<int>[TypedInputs.Batch];
#if NET11_0_OR_GREATER
    private readonly OrderUnion[] _native = new OrderUnion[TypedInputs.Batch];
#endif

    [GlobalSetup]
    public void Setup()
    {
        Parity.Same(OkCategory,
            ("expected", Parity.Of(Inputs.Value)),
            (nameof(CSharpEssentials_Typed_Ok), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_Ok()))),
            (nameof(CSharpEssentials_Result_Ok), Parity.Of(TypedInputs.Read(CSharpEssentials_Result_Ok()))),
            (nameof(CSharpEssentials_Any_Ok), Parity.Of(TypedInputs.Read(CSharpEssentials_Any_Ok()))),
            (nameof(OneOf_Ok), Parity.Of(TypedInputs.Read(OneOf_Ok()))),
            (nameof(Dunet_Ok), Parity.Of(TypedInputs.Read(Dunet_Ok()))),
            (nameof(ErrorOr_Ok), Parity.Of(TypedInputs.Read(ErrorOr_Ok()))),
            (nameof(CSharpFunctionalExtensions_Ok), Parity.Of(TypedInputs.Read(CSharpFunctionalExtensions_Ok()))),
            (nameof(FluentResults_Ok), Parity.Of(TypedInputs.Read(FluentResults_Ok()))));
#if NET11_0_OR_GREATER
        Parity.Same(OkCategory + ".Native", ("expected", Parity.Of(Inputs.Value)), (nameof(NativeUnion_Ok), Parity.Of(TypedInputs.Read(NativeUnion_Ok()))));
#endif
        Parity.Same(CachedErrorCategory,
            ("expected", Parity.Of(TypedInputs.NotFoundStatus)),
            (nameof(CSharpEssentials_Typed_CachedError), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_CachedError()))),
            (nameof(CSharpEssentials_Result_CachedError), Parity.Of(TypedInputs.Read(CSharpEssentials_Result_CachedError()))),
            (nameof(CSharpEssentials_Any_CachedError), Parity.Of(TypedInputs.Read(CSharpEssentials_Any_CachedError()))),
            (nameof(OneOf_CachedError), Parity.Of(TypedInputs.Read(OneOf_CachedError()))),
            (nameof(Dunet_CachedError), Parity.Of(TypedInputs.Read(Dunet_CachedError()))),
            (nameof(ErrorOr_CachedError), Parity.Of(TypedInputs.Read(ErrorOr_CachedError()))),
            (nameof(CSharpFunctionalExtensions_CachedError), Parity.Of(TypedInputs.Read(CSharpFunctionalExtensions_CachedError()))),
            (nameof(FluentResults_CachedError), Parity.Of(TypedInputs.Read(FluentResults_CachedError()))));
#if NET11_0_OR_GREATER
        Parity.Same(CachedErrorCategory + ".Native", ("expected", Parity.Of(TypedInputs.NotFoundStatus)), (nameof(NativeUnion_CachedError), Parity.Of(TypedInputs.Read(NativeUnion_CachedError()))));
#endif
        Parity.Same(NewErrorCategory,
            ("expected", Parity.Of(TypedInputs.ConflictStatus)),
            (nameof(CSharpEssentials_Typed_NewError), Parity.Of(TypedInputs.Read(CSharpEssentials_Typed_NewError()))),
            (nameof(CSharpEssentials_Result_NewError), Parity.Of(TypedInputs.Read(CSharpEssentials_Result_NewError()))),
            (nameof(CSharpEssentials_Any_NewError), Parity.Of(TypedInputs.Read(CSharpEssentials_Any_NewError()))),
            (nameof(OneOf_NewError), Parity.Of(TypedInputs.Read(OneOf_NewError()))),
            (nameof(Dunet_NewError), Parity.Of(TypedInputs.Read(Dunet_NewError()))),
            (nameof(ErrorOr_NewError), Parity.Of(TypedInputs.Read(ErrorOr_NewError()))),
            (nameof(CSharpFunctionalExtensions_NewError), Parity.Of(TypedInputs.Read(CSharpFunctionalExtensions_NewError()))),
            (nameof(FluentResults_NewError), Parity.Of(TypedInputs.Read(FluentResults_NewError()))));
#if NET11_0_OR_GREATER
        Parity.Same(NewErrorCategory + ".Native", ("expected", Parity.Of(TypedInputs.ConflictStatus)), (nameof(NativeUnion_NewError), Parity.Of(TypedInputs.Read(NativeUnion_NewError()))));
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#typed-errors-resultt-e1-resultt-e1-e2-resultt-e1-e2-e3
    [Benchmark(Baseline = true, OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public Typed CSharpEssentials_Typed_Ok()
    {
        Typed[] sink = _typed;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#returning-failures
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public Result<int> CSharpEssentials_Result_Ok()
    {
        Result<int>[] sink = _result;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#creating-unions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public TypedAny CSharpEssentials_Any_Ok()
    {
        TypedAny[] sink = _any;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#as-a-method-return-value
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public TypedOneOf OneOf_Ok()
    {
        TypedOneOf[] sink = _oneOf;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#implicit-conversions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public OrderOutcome Dunet_Ok()
    {
        OrderOutcome[] sink = _dunet;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#using-implicit-conversion
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public Eo.ErrorOr<int> ErrorOr_Ok()
    {
        Eo.ErrorOr<int>[] sink = _errorOr;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public Cfe.Result<int, TypedError> CSharpFunctionalExtensions_Ok()
    {
        Cfe.Result<int, TypedError>[] sink = _cfe;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#implicit-conversion-from-t-to-success-result-resultt
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public Fr.Result<int> FluentResults_Ok()
    {
        Fr.Result<int>[] sink = _fluent;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }

#if NET11_0_OR_GREATER
    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(OkCategory)]
    public OrderUnion NativeUnion_Ok()
    {
        OrderUnion[] sink = _native;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = _value;
        return sink[^1];
    }
#endif

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#typed-errors-resultt-e1-resultt-e1-e2-resultt-e1-e2-e3
    [Benchmark(Baseline = true, OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public Typed CSharpEssentials_Typed_CachedError()
    {
        Typed[] sink = _typed;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#returning-failures
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public Result<int> CSharpEssentials_Result_CachedError()
    {
        Result<int>[] sink = _result;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.CseMissing;
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#creating-unions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public TypedAny CSharpEssentials_Any_CachedError()
    {
        TypedAny[] sink = _any;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#as-a-method-return-value
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public TypedOneOf OneOf_CachedError()
    {
        TypedOneOf[] sink = _oneOf;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#implicit-conversions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public OrderOutcome Dunet_CachedError()
    {
        OrderOutcome[] sink = _dunet;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#using-implicit-conversion
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public Eo.ErrorOr<int> ErrorOr_CachedError()
    {
        Eo.ErrorOr<int>[] sink = _errorOr;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.ErrorOrMissing;
        return sink[^1];
    }

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public Cfe.Result<int, TypedError> CSharpFunctionalExtensions_CachedError()
    {
        Cfe.Result<int, TypedError>[] sink = _cfe;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#implicit-conversion-from-t-to-success-result-resultt
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public Fr.Result<int> FluentResults_CachedError()
    {
        Fr.Result<int>[] sink = _fluent;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Fr.Result.Fail<int>(TypedInputs.FluentMissing);
        return sink[^1];
    }

#if NET11_0_OR_GREATER
    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(CachedErrorCategory)]
    public OrderUnion NativeUnion_CachedError()
    {
        OrderUnion[] sink = _native;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = TypedInputs.Missing;
        return sink[^1];
    }
#endif

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#typed-errors-resultt-e1-resultt-e1-e2-resultt-e1-e2-e3
    [Benchmark(Baseline = true, OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public Typed CSharpEssentials_Typed_NewError()
    {
        Typed[] sink = _typed;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#returning-failures
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public Result<int> CSharpEssentials_Result_NewError()
    {
        Result<int>[] sink = _result;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Error.Conflict(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#creating-unions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public TypedAny CSharpEssentials_Any_NewError()
    {
        TypedAny[] sink = _any;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#as-a-method-return-value
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public TypedOneOf OneOf_NewError()
    {
        TypedOneOf[] sink = _oneOf;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#implicit-conversions
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public OrderOutcome Dunet_NewError()
    {
        OrderOutcome[] sink = _dunet;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#using-implicit-conversion
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public Eo.ErrorOr<int> ErrorOr_NewError()
    {
        Eo.ErrorOr<int>[] sink = _errorOr;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Eo.Error.Conflict(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/ResultTE.cs
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public Cfe.Result<int, TypedError> CSharpFunctionalExtensions_NewError()
    {
        Cfe.Result<int, TypedError>[] sink = _cfe;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#implicit-conversion-from-t-to-success-result-resultt
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public Fr.Result<int> FluentResults_NewError()
    {
        Fr.Result<int>[] sink = _fluent;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Fr.Result.Fail<int>(new FluentConflict(TypedInputs.ConflictText));
        return sink[^1];
    }

#if NET11_0_OR_GREATER
    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
    [Benchmark(OperationsPerInvoke = TypedInputs.Batch), BenchmarkCategory(NewErrorCategory)]
    public OrderUnion NativeUnion_NewError()
    {
        OrderUnion[] sink = _native;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new ConflictError(TypedInputs.ConflictCode, TypedInputs.ConflictText);
        return sink[^1];
    }
#endif
}
