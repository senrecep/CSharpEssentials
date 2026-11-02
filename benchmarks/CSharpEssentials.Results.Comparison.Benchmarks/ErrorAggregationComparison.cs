using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// Validate three independent checks on an <see cref="Order"/> (Id &gt; 0, Total &gt; 0, Id even) and collect every
/// error. All-pass uses Order(42, 10); all-fail uses Order(-1, -5), which fails all three. The three messages are
/// distinct because CFE's <c>Combine</c> de-duplicates repeated messages. CSE uses the README's <c>CombineAll</c>
/// idiom, CFE <c>Result.Combine</c>, ErrorOr the README's manual <c>List&lt;Error&gt;</c>, FluentResults
/// <c>Result.Merge</c>. For reference, CSE's typed <c>Result&lt;T&gt;.Combine(r1, r2, r3)</c> skips the array and list.
/// </summary>
[MemoryDiagnoser]
public class ErrorAggregationComparison
{
    private const string IdMessage = "Id must be positive";
    private const string TotalMessage = "Total must be positive";
    private const string EvenMessage = "Id must be even";

    private static readonly Error CseId = Error.Validation("Order.Id", IdMessage);
    private static readonly Error CseTotal = Error.Validation("Order.Total", TotalMessage);
    private static readonly Error CseEven = Error.Validation("Order.IdEven", EvenMessage);
    private static readonly Eo.Error ErrorOrId = Eo.Error.Validation("Order.Id", IdMessage);
    private static readonly Eo.Error ErrorOrTotal = Eo.Error.Validation("Order.Total", TotalMessage);
    private static readonly Eo.Error ErrorOrEven = Eo.Error.Validation("Order.IdEven", EvenMessage);
    private static readonly Fr.Error FluentId = new(IdMessage);
    private static readonly Fr.Error FluentTotal = new(TotalMessage);
    private static readonly Fr.Error FluentEven = new(EvenMessage);

    private Order _order = null!;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _order = Succeed ? new Order(Inputs.Value, 10m) : new Order(-1, -5m);
        Parity.Same(nameof(ErrorAggregationComparison),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(CSharpFunctionalExtensions), Parity.Of(CSharpFunctionalExtensions())),
            (nameof(ErrorOr), Parity.Of(ErrorOr())),
            (nameof(FluentResults), Parity.Of(FluentResults())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results/Readme.MD#collection-extensions
    [Benchmark(Baseline = true)]
    public Result CSharpEssentials()
    {
        Order order = _order;
        return new[]
        {
            Result.FailureIf(order.Id <= 0, CseId),
            Result.FailureIf(order.Total <= 0, CseTotal),
            Result.FailureIf(order.Id % 2 != 0, CseEven),
        }.CombineAll();
    }

    // https://github.com/vkhorikov/CSharpFunctionalExtensions/blob/v3.7.0/CSharpFunctionalExtensions/Result/Methods/Combine.cs
    [Benchmark]
    public Cfe.Result CSharpFunctionalExtensions()
    {
        Order order = _order;
        return Cfe.Result.Combine(
            Cfe.Result.FailureIf(order.Id <= 0, IdMessage),
            Cfe.Result.FailureIf(order.Total <= 0, TotalMessage),
            Cfe.Result.FailureIf(order.Id % 2 != 0, EvenMessage));
    }

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#support-for-multiple-errors
    [Benchmark]
    public Eo.ErrorOr<Eo.Success> ErrorOr()
    {
        Order order = _order;
        List<Eo.Error> errors = [];
        if (order.Id <= 0)
            errors.Add(ErrorOrId);
        if (order.Total <= 0)
            errors.Add(ErrorOrTotal);
        if (order.Id % 2 != 0)
            errors.Add(ErrorOrEven);
        return errors.Count > 0 ? errors : Eo.Result.Success;
    }

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#merging
    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#create-a-result-depending-on-successfailure-condition
    [Benchmark]
    public Fr.Result FluentResults()
    {
        Order order = _order;
        return Fr.Result.Merge(
            Fr.Result.FailIf(order.Id <= 0, FluentId),
            Fr.Result.FailIf(order.Total <= 0, FluentTotal),
            Fr.Result.FailIf(order.Id % 2 != 0, FluentEven));
    }
}
