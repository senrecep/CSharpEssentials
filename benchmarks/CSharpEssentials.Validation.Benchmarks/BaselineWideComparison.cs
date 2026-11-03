using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
#if NET10_0_OR_GREATER
using Microsoft.Extensions.Validation;
#endif
using Annotations = System.ComponentModel.DataAnnotations;
using FvResult = FluentValidation.Results.ValidationResult;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Twelve required strings of at most 100 characters, all valid or all empty. Like-for-like.
/// Microsoft.Extensions.Validation has no documented standalone (non-ASP.NET) idiom; the row follows the API reference, and the
/// API is experimental (ASP0029). Its rows exist on net10.0 and net11.0 only.
/// </summary>
[MemoryDiagnoser]
public class BaselineWideComparison
{
    private readonly CseWideRequestValidator _cse = new();
    private readonly FvWideRequestValidator _fv = new();
    private WideRequest _model = null!;
#if NET10_0_OR_GREATER
    private ValidationOptions _options = null!;
    private IValidatableInfo _info = null!;
#endif

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _model = Valid ? new WideRequest { F1 = "Value 1", F2 = "Value 2", F3 = "Value 3", F4 = "Value 4", F5 = "Value 5", F6 = "Value 6", F7 = "Value 7", F8 = "Value 8", F9 = "Value 9", F10 = "Value 10", F11 = "Value 11", F12 = "Value 12" } : new WideRequest { F1 = "", F2 = "", F3 = "", F4 = "", F5 = "", F6 = "", F7 = "", F8 = "", F9 = "", F10 = "", F11 = "", F12 = "" };
        string expected = Valid ? "ok" : "fail:F1,F10,F11,F12,F2,F3,F4,F5,F6,F7,F8,F9";
        Parity.Same(nameof(BaselineWideComparison),
            ("expected", expected),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(FluentValidation), Parity.Of(FluentValidation())),
            (nameof(DataAnnotations), Parity.Of(DataAnnotations())));
#if NET10_0_OR_GREATER
        (_options, _info) = MeValidation.Resolve(typeof(WideRequest));
        Parity.Same(nameof(BaselineWideComparison),
            ("expected", expected),
            (nameof(MicrosoftExtensionsValidation), Parity.Of(MicrosoftExtensionsValidation().AsTask().GetAwaiter().GetResult().ValidationErrors)));
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#quick-start
    [Benchmark(Baseline = true)]
    public Result<WideRequest> CSharpEssentials() => _cse.ValidateAsync(_model).Run();

    // https://docs.fluentvalidation.net/en/latest/start.html
    [Benchmark]
    public FvResult FluentValidation() => _fv.Validate(_model);

    // https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.validator.tryvalidateobject
    [Benchmark]
    public List<Annotations.ValidationResult> DataAnnotations()
    {
        var results = new List<Annotations.ValidationResult>();
        Annotations.Validator.TryValidateObject(_model, new Annotations.ValidationContext(_model), results, validateAllProperties: true);
        return results;
    }

#if NET10_0_OR_GREATER
    // https://github.com/dotnet/aspnetcore/blob/v10.0.14/src/Validation/src/ValidationOptions.cs
    // https://github.com/dotnet/aspnetcore/blob/v10.0.14/src/Validation/src/IValidatableInfo.cs
    [Benchmark]
    public async ValueTask<ValidateContext> MicrosoftExtensionsValidation()
    {
        var context = new ValidateContext { ValidationOptions = _options, ValidationContext = new Annotations.ValidationContext(_model) };
        await _info.ValidateAsync(_model, context, default);
        return context;
    }
#endif
}
