using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// The failure test the CSE behaviors run on every response: <c>response is not IResultBase { IsFailure: true }</c>
/// (<c>TransactionOutcome.ShouldCommit</c>, CSharpEssentials.Mediator/Behaviors/TransactionOutcome.cs:8; CachingBehavior and
/// IdempotencyBehavior run the same test). The behavior is generic over <c>TResponse</c>, so the test runs on an unconstrained
/// type parameter; for a struct response the JIT may box the value to test it against the interface. The helper copies the
/// pattern into a non-inlined generic method, because the internal original is not visible here, and one method call stands
/// for the behavior's own method boundary.
/// <para>
/// Rows: a class response (no box possible, baseline), <see cref="Result{TValue}"/> and a typed
/// <c>Result&lt;int, NotFoundError&gt;</c> (both structs), and the typed struct through a helper constrained to
/// <see cref="IResultBase"/>, which reads <c>IsFailure</c> without a box. The <c>*_Mediator</c> rows call the shipped
/// <c>TransactionOutcome.ShouldCommit</c> itself, so they follow changes to the library; the copied rows do not.
/// <c>Succeed</c> picks a success or a failure.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class ResultBaseCheckBenchmarks
{
    private static readonly NotFoundError Missing = new("Order.NotFound", "Order 42 was not found");

    private string _class = null!;
    private Result<int> _result;
    private Result<int, NotFoundError> _typed;

    [Params(true, false)]
    public bool Succeed { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _class = "ok";
        _result = Succeed ? 42 : Missing.ToError();
        _typed = Succeed ? 42 : Missing;
        bool expected = Succeed;
        if (!ClassResponse() || Result() != expected || Typed() != expected || Typed_Constrained() != expected
            || Result_Mediator() != expected || Typed_Mediator() != expected)
            throw new InvalidOperationException($"{nameof(ResultBaseCheckBenchmarks)}: the rows disagree for Succeed={Succeed}.");
    }

    [Benchmark(Baseline = true)]
    public bool ClassResponse() => ShouldCommit(_class);

    [Benchmark]
    public bool Result() => ShouldCommit(_result);

    [Benchmark]
    public bool Typed() => ShouldCommit(_typed);

    [Benchmark]
    public bool Typed_Constrained() => ShouldCommitConstrained(_typed);

    [Benchmark]
    public bool Result_Mediator() => ShouldCommitMediator(_result);

    [Benchmark]
    public bool Typed_Mediator() => ShouldCommitMediator(_typed);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ShouldCommitMediator<TResponse>(TResponse response) =>
        TransactionOutcome.ShouldCommit(response);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ShouldCommit<TResponse>(TResponse response) =>
        response is not IResultBase { IsFailure: true };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ShouldCommitConstrained<TResponse>(TResponse response)
        where TResponse : IResultBase =>
        !response.IsFailure;
}
