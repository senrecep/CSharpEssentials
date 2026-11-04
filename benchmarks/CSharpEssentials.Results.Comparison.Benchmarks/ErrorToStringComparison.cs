using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ErrorToStringComparison
{
    private const string ErrorCategory = "Error";
    private const string MetadataCategory = "ErrorWithMetadata";
    private const string ResultCategory = "Result";

    private Error _cse;
    private Eo.Error _errorOr;
    private Fr.Error _fluent = null!;
    private Error _cseMetadata;
    private Eo.Error _errorOrMetadata;
    private Fr.Error _fluentMetadata = null!;
    private Result<int> _cseResult;
    private Cfe.Result<int> _cfeResult;
    private Eo.ErrorOr<int> _errorOrResult;
    private Fr.Result<int> _fluentResult = null!;

    [GlobalSetup]
    public void Setup()
    {
        string code = ErrorInputs.Code;
        string message = Inputs.Message;
        _cse = Error.Validation(code, message);
        _errorOr = Eo.Error.Validation(code, message);
        _fluent = new Fr.Error(message);
        _cseMetadata = Error.Validation(code, message, ErrorInputs.CseMetadata());
        _errorOrMetadata = Eo.Error.Validation(code, message, ErrorInputs.ErrorOrMetadata());
        _fluentMetadata = new Fr.Error(message)
            .WithMetadata(ErrorInputs.MetadataKeys[0], ErrorInputs.MetadataValues[0])
            .WithMetadata(ErrorInputs.MetadataKeys[1], ErrorInputs.MetadataValues[1])
            .WithMetadata(ErrorInputs.MetadataKeys[2], ErrorInputs.MetadataValues[2]);
        _cseResult = Inputs.Value;
        _cfeResult = Cfe.Result.Success(Inputs.Value);
        _errorOrResult = Inputs.Value;
        _fluentResult = Fr.Result.Ok(Inputs.Value);

        Parity.Same(nameof(ErrorToStringComparison) + "." + ErrorCategory,
            (nameof(CSharpEssentials), CSharpEssentials_Error()),
            (nameof(ErrorOr), ErrorOr_Error()));
        Parity.Same(nameof(ErrorToStringComparison) + "." + ErrorCategory,
            ("Expected", "has:" + message),
            (nameof(FluentResults), ErrorParity.Has(FluentResults_Error(), message)));
        Parity.Same(nameof(ErrorToStringComparison) + "." + MetadataCategory,
            ("Expected", "has:" + message),
            (nameof(CSharpEssentials), ErrorParity.Has(CSharpEssentials_ErrorWithMetadata(), message)),
            (nameof(ErrorOr), ErrorParity.Has(ErrorOr_ErrorWithMetadata(), message)),
            (nameof(FluentResults), ErrorParity.Has(FluentResults_ErrorWithMetadata(), message)));
        Parity.Same(nameof(ErrorToStringComparison) + "." + ResultCategory,
            ("Expected", "has:42"),
            (nameof(CSharpEssentials), ErrorParity.Has(CSharpEssentials_Result(), "42")),
            (nameof(CSharpFunctionalExtensions), ErrorParity.Has(CSharpFunctionalExtensions_Result(), "42")),
            (nameof(ErrorOr), ErrorParity.Has(ErrorOr_Result(), "42")),
            (nameof(FluentResults), ErrorParity.Has(FluentResults_Result(), "42")));
    }

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display
    [Benchmark(Baseline = true), BenchmarkCategory(ErrorCategory)]
    public string CSharpEssentials_Error() => _cse.ToString();

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display
    [Benchmark, BenchmarkCategory(ErrorCategory)]
    public string ErrorOr_Error() => _errorOr.ToString();

    // https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs
    [Benchmark, BenchmarkCategory(ErrorCategory)]
    public string FluentResults_Error() => _fluent.ToString();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#error-metadata
    [Benchmark(Baseline = true), BenchmarkCategory(MetadataCategory)]
    public string CSharpEssentials_ErrorWithMetadata() => _cseMetadata.ToString();

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display
    [Benchmark, BenchmarkCategory(MetadataCategory)]
    public string ErrorOr_ErrorWithMetadata() => _errorOrMetadata.ToString();

    // https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/Error.cs
    [Benchmark, BenchmarkCategory(MetadataCategory)]
    public string FluentResults_ErrorWithMetadata() => _fluentMetadata.ToString();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/ResultT.cs
    [Benchmark(Baseline = true), BenchmarkCategory(ResultCategory)]
    public string CSharpEssentials_Result() => _cseResult.ToString();

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/ToString.cs
    [Benchmark, BenchmarkCategory(ResultCategory)]
    public string CSharpFunctionalExtensions_Result() => _cfeResult.ToString();

    // https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display
    [Benchmark, BenchmarkCategory(ResultCategory)]
    public string ErrorOr_Result() => _errorOrResult.ToString();

    // https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Results/Result.cs
    [Benchmark, BenchmarkCategory(ResultCategory)]
    public string FluentResults_Result() => _fluentResult.ToString();
}
