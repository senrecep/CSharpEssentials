using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
#if NET10_0_OR_GREATER
using Microsoft.Extensions.Validation;
#endif
using Annotations = System.ComponentModel.DataAnnotations;
using FvResult = FluentValidation.Results.ValidationResult;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Four-property model, valid or with every property failing exactly one rule. Like-for-like on outcome; the email checks do
/// different work: CSE runs a regex, while FluentValidation (default mode), DataAnnotations and Microsoft.Extensions.Validation
/// check for an <c>@</c>. The invalid email has no <c>@</c>, so all four reject it.
/// Microsoft.Extensions.Validation has no documented standalone (non-ASP.NET) idiom; the row follows the API reference, and the
/// API is experimental (ASP0029). Its rows exist on net10.0 and net11.0 only.
/// </summary>
[MemoryDiagnoser]
public class BaselineSimpleComparison
{
    private readonly CseSignupValidator _cse = new();
    private readonly FvSignupValidator _fv = new();
    private SignupRequest _model = null!;
#if NET10_0_OR_GREATER
    private ValidationOptions _options = null!;
    private IValidatableInfo _info = null!;
#endif

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _model = Valid ? new SignupRequest { Name = "John Doe", Email = "john.doe@example.com", Age = 30, Tags = ["customer"] } : new SignupRequest { Name = "", Email = "not-an-email", Age = -1, Tags = [] };
        string expected = Valid ? "ok" : "fail:Age,Email,Name,Tags";
        Parity.Same(nameof(BaselineSimpleComparison),
            ("expected", expected),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(FluentValidation), Parity.Of(FluentValidation())),
            (nameof(DataAnnotations), Parity.Of(DataAnnotations())));
#if NET10_0_OR_GREATER
        (_options, _info) = MeValidation.Resolve(typeof(SignupRequest));
        Parity.Same(nameof(BaselineSimpleComparison),
            ("expected", expected),
            (nameof(MicrosoftExtensionsValidation), Parity.Of(MicrosoftExtensionsValidation().AsTask().GetAwaiter().GetResult())));
#endif
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#quick-start
    [Benchmark(Baseline = true)]
    public Result<SignupRequest> CSharpEssentials() => _cse.ValidateAsync(_model).Run();

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
    public async ValueTask<Dictionary<string, string[]>?> MicrosoftExtensionsValidation()
    {
        var context = new ValidateContext { ValidationOptions = _options, ValidationContext = new Annotations.ValidationContext(_model) };
        await _info.ValidateAsync(_model, context, default);
        return context.ValidationErrors;
    }
#endif
}
