using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="CreateOrder"/> through a validation behavior and an exception handling behavior, returning
/// <see cref="Result{TValue}"/> in every column. Cse is Mediator plus the CSharpEssentials.Mediator <c>ValidationBehavior</c>
/// and <c>ExceptionHandlingBehavior</c> as its Readme registers them; the hand-written columns implement the same two
/// behaviors the way each library documents behaviors. All three run the same <see cref="CreateOrderValidator"/>, so the
/// difference is the behaviors and the dispatcher, not the rules.
/// <para>
/// Extra work CSE does and the hand-written behaviors do not: on every send it type-tests the request for
/// <c>IValidationModeOverride</c> to pick the validation mode, it copies the validator's errors, and on failure it calls the
/// registered <c>IValidationFailureObserver</c>s (the default one records a metrics counter and checks the logger) and builds
/// the failure through a cached compiled factory over <c>Result&lt;T&gt;.Failure(IEnumerable&lt;Error&gt;)</c> with reflection
/// type checks and a box/unbox. That is real cost and stays in the row.
/// </para>
/// <para>
/// Both validation behaviors share CSE's other rules: the cancellation check first, and a validator exception other than
/// <see cref="OperationCanceledException"/> becomes a <c>Validator.Exception</c> failure. All three exception behaviors turn
/// a handler exception into one <c>Error.Exception(ex)</c> failure; the self-check sends an order whose handler throws.
/// </para>
/// <para>
/// Lifetimes match CSE everywhere: validation behavior and validator scoped, exception behavior singleton. All three
/// containers resolve from the root provider, so the scoped services become root-scoped singletons. This works with plain
/// <c>BuildServiceProvider()</c> and throws for every column with <c>validateScopes: true</c>. Mediator (singleton default)
/// builds the pipeline once. MediatR looks up the behaviors with <c>GetServices</c> on every send, but it gets the same
/// cached instances back.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class BehaviorBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private CreateOrder _order = null!;

    [Params(OrderInput.Valid, OrderInput.Invalid)]
    public OrderInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cseProvider = MediatorApp.Cse();
        _mediatorProvider = MediatorApp.HandWritten();
        _mediatRProvider = MediatRApp.HandWritten();
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _order = MediatorSelfCheck.Order(Input);
        MediatorSelfCheck.VerifyBehaviors(_cseProvider, _mediatorProvider, _mediatRProvider);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cseProvider.Dispose();
        _mediatorProvider.Dispose();
        _mediatRProvider.Dispose();
    }

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    [Benchmark(Baseline = true)]
    public Result<int> MediatorHandWritten() => Sync.Run(_mediator.Send(_order));

    [Benchmark]
    public Result<int> Cse() => Sync.Run(_cse.Send(_order));

    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    [Benchmark]
    public Result<int> MediatRHandWritten() => Sync.Run(_mediatR.Send(_order));
}
