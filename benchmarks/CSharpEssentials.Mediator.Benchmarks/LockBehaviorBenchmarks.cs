using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="ReserveStock"/> through a keyed lock behavior, single thread, no contention. Cse is the
/// CSharpEssentials.Mediator <c>LockBehavior</c> over the default <c>InProcessResourceLock</c>. Neither MediatR nor
/// Mediator ships a lock behavior, so the hand-written columns lock with AsyncKeyedLock as its README shows (one static
/// <c>AsyncKeyedLocker&lt;string&gt;</c>, default options). Same wait semantics everywhere: no timeout, cancellable.
/// <para>
/// CSE goes through the <c>IResourceLock</c> abstraction and releases with an <c>IAsyncDisposable</c> handle;
/// AsyncKeyedLock releases with a pooled <c>IDisposable</c>. Every behavior is scoped and resolved from the root provider
/// with plain <c>BuildServiceProvider()</c>, so it becomes a root-scoped singleton, and the singleton Mediator builds
/// the pipeline once. The CSE Readme registers <c>AddMediatorBehaviors()</c> as well; this container registers only
/// the lock behavior.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class LockBehaviorBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private readonly ReserveStock _request = new("sku-1");

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cseProvider = BehaviorApp.CseLock();
        _mediatorProvider = BehaviorApp.MediatorHandWrittenLock();
        _mediatRProvider = BehaviorApp.MediatRHandWrittenLock();
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        BehaviorSelfCheck.VerifyLock(_cseProvider, _mediatorProvider, _mediatRProvider, _request);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cseProvider.Dispose();
        _mediatorProvider.Dispose();
        _mediatRProvider.Dispose();
    }

    // https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
    [Benchmark(Baseline = true)]
    public Result<int> MediatorHandWritten() => Sync.Run(_mediator.Send(_request));

    [Benchmark]
    public Result<int> Cse() => Sync.Run(_cse.Send(_request));

    // https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
    [Benchmark]
    public Result<int> MediatRHandWritten() => Sync.Run(_mediatR.Send(_request));
}
