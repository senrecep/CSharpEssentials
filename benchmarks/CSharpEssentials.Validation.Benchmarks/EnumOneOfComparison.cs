using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using FvResult = FluentValidation.Results.ValidationResult;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Value is one of <c>Pending</c> or <c>Shipped</c>: CSE <c>IsOneOf</c> against FluentValidation <c>Must</c>, its documented route
/// for a custom predicate (it has no subset validator for enums). Valid is <c>Shipped</c>, invalid is <c>Delivered</c>. Different
/// work on the invalid path: FluentValidation's message is the generic predicate text, CSE formats the allowed values.
/// </summary>
[MemoryDiagnoser]
public class EnumOneOfComparison
{
    private readonly CseStatusOneOfValidator _cse = new();
    private readonly FvStatusOneOfValidator _fv = new();
    private StatusRequest _model = null!;

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _model = new StatusRequest { Status = Valid ? ShipmentStatus.Shipped : ShipmentStatus.Delivered };
        Parity.Same(nameof(EnumOneOfComparison),
            ("expected", Valid ? "ok" : "fail:Status"),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(FluentValidation), Parity.Of(FluentValidation())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#enums-tenum-tenum
    [Benchmark(Baseline = true)]
    public Result<StatusRequest> CSharpEssentials() => _cse.ValidateAsync(_model).Run();

    // https://docs.fluentvalidation.net/en/latest/built-in-validators.html#predicate-validator
    [Benchmark]
    public FvResult FluentValidation() => _fv.Validate(_model);
}
