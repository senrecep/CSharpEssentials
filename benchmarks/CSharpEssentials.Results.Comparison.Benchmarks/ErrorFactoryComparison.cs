using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ErrorFactoryComparison
{
    private const int Batch = 16;
    private const string Plain = "Plain";
    private const string WithMetadata = "WithMetadata";

    private readonly Error[] _cseSink = new Error[Batch];
    private readonly Eo.Error[] _errorOrSink = new Eo.Error[Batch];
    private readonly Fr.Error[] _fluentSink = new Fr.Error[Batch];
    private string[] _codes = [];
    private string[] _descriptions = [];
    private string _code = string.Empty;
    private string _description = string.Empty;
    private string _key1 = string.Empty;
    private string _key2 = string.Empty;
    private string _key3 = string.Empty;
    private object _value1 = string.Empty;
    private object _value2 = string.Empty;
    private object _value3 = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _codes = ErrorInputs.Codes(Batch);
        _descriptions = ErrorInputs.Descriptions(Batch);
        _code = ErrorInputs.Code;
        _description = Inputs.Message;
        (_key1, _key2, _key3) = (ErrorInputs.MetadataKeys[0], ErrorInputs.MetadataKeys[1], ErrorInputs.MetadataKeys[2]);
        (_value1, _value2, _value3) = (ErrorInputs.MetadataValues[0], ErrorInputs.MetadataValues[1], ErrorInputs.MetadataValues[2]);

        Error[] cse = CSharpEssentials_Plain();
        Eo.Error[] errorOr = ErrorOr_Plain();
        Fr.Error[] fluent = FluentResults_Plain();
        for (int i = 0; i < Batch; i++)
        {
            Parity.Same(nameof(ErrorFactoryComparison) + "." + Plain,
                (nameof(CSharpEssentials), ErrorParity.Full(cse[i])),
                (nameof(ErrorOr), ErrorParity.Full(errorOr[i])));
            Parity.Same(nameof(ErrorFactoryComparison) + "." + Plain,
                (nameof(CSharpEssentials), ErrorParity.Core(cse[i])),
                (nameof(FluentResults), ErrorParity.Core(fluent[i])));
        }

        Parity.Same(nameof(ErrorFactoryComparison) + "." + WithMetadata,
            (nameof(CSharpEssentials), ErrorParity.Full(CSharpEssentials_WithMetadata())),
            (nameof(ErrorOr), ErrorParity.Full(ErrorOr_WithMetadata())));
        Parity.Same(nameof(ErrorFactoryComparison) + "." + WithMetadata,
            (nameof(CSharpEssentials), ErrorParity.Core(CSharpEssentials_WithMetadata())),
            (nameof(FluentResults), ErrorParity.Core(FluentResults_WithMetadata())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#creating-errors
    [Benchmark(Baseline = true, OperationsPerInvoke = Batch), BenchmarkCategory(Plain)]
    public Error[] CSharpEssentials_Plain()
    {
        string[] codes = _codes;
        string[] descriptions = _descriptions;
        Error[] sink = _cseSink;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Error.Validation(codes[i], descriptions[i]);
        return sink;
    }

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#built-in-error-types
    [Benchmark(OperationsPerInvoke = Batch), BenchmarkCategory(Plain)]
    public Eo.Error[] ErrorOr_Plain()
    {
        string[] codes = _codes;
        string[] descriptions = _descriptions;
        Eo.Error[] sink = _errorOrSink;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = Eo.Error.Validation(codes[i], descriptions[i]);
        return sink;
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#creating-a-result
    [Benchmark(OperationsPerInvoke = Batch), BenchmarkCategory(Plain)]
    public Fr.Error[] FluentResults_Plain()
    {
        string[] descriptions = _descriptions;
        Fr.Error[] sink = _fluentSink;
        for (int i = 0; i < sink.Length; i++)
            sink[i] = new Fr.Error(descriptions[i]);
        return sink;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#error-metadata
    [Benchmark(Baseline = true), BenchmarkCategory(WithMetadata)]
    public Error CSharpEssentials_WithMetadata() =>
        Error.Validation(_code, _description,
            new ErrorMetadata().AddMetadata(_key1, _value1).AddMetadata(_key2, _value2).AddMetadata(_key3, _value3));

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#built-in-error-types
    [Benchmark, BenchmarkCategory(WithMetadata)]
    public Eo.Error ErrorOr_WithMetadata() =>
        Eo.Error.Validation(_code, _description,
            new Dictionary<string, object> { { _key1, _value1 }, { _key2, _value2 }, { _key3, _value3 } });

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#metadata
    [Benchmark, BenchmarkCategory(WithMetadata)]
    public Fr.Error FluentResults_WithMetadata() =>
        new Fr.Error(_description).WithMetadata(_key1, _value1).WithMetadata(_key2, _value2).WithMetadata(_key3, _value3);
}
