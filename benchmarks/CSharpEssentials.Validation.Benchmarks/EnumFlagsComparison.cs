using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Annotations = System.ComponentModel.DataAnnotations;
using FvResult = FluentValidation.Results.ValidationResult;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Only defined bits set on a plain <c>[Flags]</c> enum: CSE <c>HasOnlyDefinedFlags</c>, FluentValidation <c>IsInEnum</c> (which
/// accepts combinations of defined flags) and DataAnnotations <c>[EnumDataType]</c>. Valid is <c>Read | Write</c>, invalid is the
/// undefined bit <c>8</c>. Like-for-like on outcome. The CSE row runs the plain-enum path, which converts through
/// <c>Convert.ToInt64(object)</c>.
/// </summary>
[MemoryDiagnoser]
public class EnumFlagsComparison
{
    private readonly CsePermissionsValidator _cse = new();
    private readonly FvPermissionsValidator _fv = new();
    private PermissionsRequest _model = null!;

    [Params(true, false)]
    public bool Valid { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _model = new PermissionsRequest { Permissions = Valid ? ShipmentPermissions.Read | ShipmentPermissions.Write : (ShipmentPermissions)8 };
        Parity.Same(nameof(EnumFlagsComparison),
            ("expected", Valid ? "ok" : "fail:Permissions"),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(FluentValidation), Parity.Of(FluentValidation())),
            (nameof(DataAnnotations), Parity.Of(DataAnnotations())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#enums-tenum-tenum
    [Benchmark(Baseline = true)]
    public Result<PermissionsRequest> CSharpEssentials() => _cse.ValidateAsync(_model).Run();

    // https://docs.fluentvalidation.net/en/latest/built-in-validators.html#enum-validator
    [Benchmark]
    public FvResult FluentValidation() => _fv.Validate(_model);

    // https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.enumdatatypeattribute
    [Benchmark]
    public List<Annotations.ValidationResult> DataAnnotations()
    {
        var results = new List<Annotations.ValidationResult>();
        Annotations.Validator.TryValidateObject(_model, new Annotations.ValidationContext(_model), results, validateAllProperties: true);
        return results;
    }
}
