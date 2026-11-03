using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="PlaceOrder"/> through an idempotency behavior. Cse is the CSharpEssentials.Mediator
/// <c>IdempotencyBehavior</c> with <c>StoreFailures = false</c>, its executor and the in-memory record store. Neither
/// MediatR nor Mediator ships an idempotency behavior, so the hand-written columns use a small
/// <see cref="HandWrittenIdempotencyStore"/>: look up the key, compare the fingerprint, replay the stored JSON, or
/// reserve, run, and release on failure.
/// <para>
/// The hand-written store is a lower bound, not a feature match: it has no lease or retention expiry, no scope, no
/// key format check, no modes, no metrics, no options monitor read, no per-request settings object and no reservation
/// token. CSE does all of that on every send, and that cost stays in the row.
/// </para>
/// <para>
/// <see cref="IdempotencyPath.Replay"/> replays a success stored in setup; the handler never runs.
/// <see cref="IdempotencyPath.ExecuteRelease"/> runs a handler that fails, so every send reserves the key, runs the
/// handler and releases the key. The first run that stores a success is not measured: it needs a new key per call,
/// and the store would grow with every invocation. <see cref="FingerprintSource.Default"/> and
/// <see cref="FingerprintSource.DefaultLarge"/> serialize the request to JSON and hash it with SHA-256 on every send,
/// in every column.
/// </para>
/// <para>
/// Every container resolves from the root provider with plain <c>BuildServiceProvider()</c>: the scoped behaviors
/// become root-scoped singletons, and the singleton Mediator builds the pipeline once. The CSE Readme registers
/// <c>AddMediatorBehaviors()</c> as well; this container registers only the idempotency behavior.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class IdempotencyBehaviorBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private PlaceOrder _request = null!;

    [Params(IdempotencyPath.Replay, IdempotencyPath.ExecuteRelease)]
    public IdempotencyPath Path { get; set; }

    [Params(FingerprintSource.Supplied, FingerprintSource.Default, FingerprintSource.DefaultLarge)]
    public FingerprintSource Fingerprint { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cseProvider = BehaviorApp.CseIdempotency();
        _mediatorProvider = BehaviorApp.MediatorHandWrittenIdempotency();
        _mediatRProvider = BehaviorApp.MediatRHandWrittenIdempotency();
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _request = BehaviorSelfCheck.Order(Path, Fingerprint);
        BehaviorSelfCheck.VerifyIdempotency(Path, _request, _cseProvider, _mediatorProvider, _mediatRProvider, MediatorHandWritten, Cse, MediatRHandWritten);
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
    public Result<int> MediatorHandWritten() => Sync.Run(_mediator.Send(_request));

    [Benchmark]
    public Result<int> Cse() => Sync.Run(_cse.Send(_request));

    // https://github.com/jbogard/MediatR/wiki/Behaviors
    [Benchmark]
    public Result<int> MediatRHandWritten() => Sync.Run(_mediatR.Send(_request));
}
