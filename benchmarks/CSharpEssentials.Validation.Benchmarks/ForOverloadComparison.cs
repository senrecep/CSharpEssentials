using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// CSE only, no competitor: the same twelve rules on <see cref="WideRequest"/> through <c>For(() =&gt; ...)</c> (the Readme default),
/// <c>For(value, name)</c> (the Readme's hot-path idiom) and inline <c>Validator.ValidateAsync</c> with a static lambda.
/// All three must produce the same error codes.
/// </summary>
[MemoryDiagnoser]
public class ForOverloadComparison
{
    private readonly CseWideRequestValidator _lambda = new();
    private readonly CseWideRequestValueValidator _value = new();
    private WideRequest _model = null!;

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _model = Valid ? new WideRequest { F1 = "Value 1", F2 = "Value 2", F3 = "Value 3", F4 = "Value 4", F5 = "Value 5", F6 = "Value 6", F7 = "Value 7", F8 = "Value 8", F9 = "Value 9", F10 = "Value 10", F11 = "Value 11", F12 = "Value 12" } : new WideRequest { F1 = "", F2 = "", F3 = "", F4 = "", F5 = "", F6 = "", F7 = "", F8 = "", F9 = "", F10 = "", F11 = "", F12 = "" };
        Parity.Same(nameof(ForOverloadComparison),
            ("expected", Valid ? "ok" : "fail:12"),
            (nameof(ForLambda), Codes(ForLambda())),
            (nameof(ForValue), Codes(ForValue())),
            (nameof(InlineValidate), Codes(InlineValidate())));
        Parity.Same(nameof(ForOverloadComparison),
            (nameof(ForLambda), Parity.Of(ForLambda())),
            (nameof(ForValue), Parity.Of(ForValue())),
            (nameof(InlineValidate), Parity.Of(InlineValidate())));
        Parity.Same(nameof(ForOverloadComparison),
            (nameof(ForLambda), AllCodes(ForLambda())),
            (nameof(ForValue), AllCodes(ForValue())),
            (nameof(InlineValidate), AllCodes(InlineValidate())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#quick-start
    [Benchmark(Baseline = true)]
    public Result<WideRequest> ForLambda() => _lambda.ValidateAsync(_model).Run();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#custom-validators-and-manual-failures
    [Benchmark]
    public Result<WideRequest> ForValue() => _value.ValidateAsync(_model).Run();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#inline-static-usage
    [Benchmark]
    public Result<WideRequest> InlineValidate() =>
        Validator.ValidateAsync(_model, static (model, rules) =>
        {
            rules.For(() => model.F1).NotEmpty().MaxLength(100);
            rules.For(() => model.F2).NotEmpty().MaxLength(100);
            rules.For(() => model.F3).NotEmpty().MaxLength(100);
            rules.For(() => model.F4).NotEmpty().MaxLength(100);
            rules.For(() => model.F5).NotEmpty().MaxLength(100);
            rules.For(() => model.F6).NotEmpty().MaxLength(100);
            rules.For(() => model.F7).NotEmpty().MaxLength(100);
            rules.For(() => model.F8).NotEmpty().MaxLength(100);
            rules.For(() => model.F9).NotEmpty().MaxLength(100);
            rules.For(() => model.F10).NotEmpty().MaxLength(100);
            rules.For(() => model.F11).NotEmpty().MaxLength(100);
            rules.For(() => model.F12).NotEmpty().MaxLength(100);
        }).Run();

    private static string Codes(Result<WideRequest> result) => result.IsSuccess ? "ok" : "fail:" + result.Errors.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string AllCodes(Result<WideRequest> result) => result.IsSuccess ? "ok" : string.Join(",", result.Errors.Select(e => e.Code));
}
