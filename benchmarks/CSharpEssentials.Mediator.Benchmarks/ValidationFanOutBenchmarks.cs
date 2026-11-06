using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="CreateCustomer"/> through a validation behavior that runs three validators (Name, Email,
/// Age), returning <see cref="Result{TValue}"/> in every column. Cse is the CSharpEssentials.Mediator
/// <c>ValidationBehavior</c>; the hand-written columns are Mediator/MediatR pipeline behaviors (Mediator README 4.4,
/// MediatR wiki Behaviors) that run the validators one by one in <c>Order</c> and collect the errors; the Mediator
/// README 3.3.1 example only checks a single <c>IValidate.IsValid</c> and throws. All three run the same validators and return the same
/// codes in the same order.
/// <para>
/// Extra work CSE does and the hand-written behaviors do not: it groups the validators by <c>Order</c> and runs the
/// validators of one group concurrently (a no-op here, every validator completes synchronously, but the group array
/// is still built), removes duplicate errors with LINQ <c>Distinct</c> on failure (no duplicates here, the cost stays),
/// type-tests the request for <c>IValidationModeOverride</c>, calls the failure observers, and builds the failure
/// through <c>BehaviorCache</c>. That is real cost and stays in the row.
/// </para>
/// <para>
/// Every container resolves from the root provider with plain <c>BuildServiceProvider()</c>: the scoped behavior and
/// validators become root-scoped singletons, and the singleton Mediator builds the pipeline once. The CSE Readme
/// registers <c>AddMediatorBehaviors()</c> as well; this container registers only the validation behavior.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class ValidationFanOutBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private CreateCustomer _request = null!;

    [Params(ValidatorGroups.One, ValidatorGroups.Two)]
    public ValidatorGroups Groups { get; set; }

    [Params(CustomerInput.Valid, CustomerInput.Invalid)]
    public CustomerInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cseProvider = BehaviorApp.CseValidation(Groups);
        _mediatorProvider = BehaviorApp.MediatorHandWrittenValidation(Groups);
        _mediatRProvider = BehaviorApp.MediatRHandWrittenValidation(Groups);
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _request = BehaviorSelfCheck.Customer(Input);
        BehaviorSelfCheck.VerifyValidation(Groups, _cseProvider, _mediatorProvider, _mediatRProvider);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cseProvider.Dispose();
        _mediatorProvider.Dispose();
        _mediatRProvider.Dispose();
    }

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#331-message-validation-example
    [Benchmark(Baseline = true)]
    public Result<int> MediatorHandWritten() => Sync.Run(_mediator.Send(_request));

    [Benchmark]
    public Result<int> Cse() => Sync.Run(_cse.Send(_request));

    // https://github.com/jbogard/MediatR/wiki/Behaviors
    [Benchmark]
    public Result<int> MediatRHandWritten() => Sync.Run(_mediatR.Send(_request));
}
