using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="GetProduct"/> through a caching behavior over the distributed memory cache, returning
/// <see cref="ProductView"/>. Cse is the CSharpEssentials.Mediator <c>CachingBehavior</c>. Neither MediatR nor Mediator
/// ships a caching behavior, so the hand-written columns do the same steps: get, deserialize on a hit; on a miss run the
/// handler, serialize, set with the request's expiration. Both log hit, miss and set with <c>[LoggerMessage]</c> to a
/// disabled logger, and both use the default <c>System.Text.Json</c> options.
/// <para>
/// <see cref="CachePath.Hit"/> reads an entry stored by one send in setup. <see cref="CachePath.Miss"/> removes the entry
/// after every send, in every column, so each send misses and stores again.
/// </para>
/// <para>
/// The request builds its key the way the CSE Readme shows, with an interpolated <c>CacheKey</c> property. CSE reads
/// that property up to four times per miss and twice per hit, so it builds the key string each time; the hand-written
/// behaviors read it once. CSE also type-tests the response for <c>IResultBase</c> on every miss. That cost stays in
/// the CSE row. Every container resolves from the root provider with plain <c>BuildServiceProvider()</c>. The CSE
/// Readme registers <c>AddMediatorBehaviors()</c> as well; this container registers only the caching behavior.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class CachingBehaviorBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private IDistributedCache _cseCache = null!;
    private IDistributedCache _mediatorCache = null!;
    private IDistributedCache _mediatRCache = null!;
    private readonly GetProduct _request = new(BehaviorSelfCheck.ProductId);
    private bool _miss;

    [Params(CachePath.Hit, CachePath.Miss)]
    public CachePath Path { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _miss = Path == CachePath.Miss;
        _cseProvider = BehaviorApp.CseCaching();
        _mediatorProvider = BehaviorApp.MediatorHandWrittenCaching();
        _mediatRProvider = BehaviorApp.MediatRHandWrittenCaching();
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _cseCache = _cseProvider.GetRequiredService<IDistributedCache>();
        _mediatorCache = _mediatorProvider.GetRequiredService<IDistributedCache>();
        _mediatRCache = _mediatRProvider.GetRequiredService<IDistributedCache>();
        BehaviorSelfCheck.VerifyCaching(Path, _cseProvider, _mediatorProvider, _mediatRProvider, MediatorHandWritten, Cse, MediatRHandWritten);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cseProvider.Dispose();
        _mediatorProvider.Dispose();
        _mediatRProvider.Dispose();
    }

    // https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
    [Benchmark(Baseline = true)]
    public ProductView MediatorHandWritten() => Evict(Sync.Run(_mediator.Send(_request)), _mediatorCache);

    [Benchmark]
    public ProductView Cse() => Evict(Sync.Run(_cse.Send(_request)), _cseCache);

    // https://github.com/jbogard/MediatR/wiki/Behaviors
    [Benchmark]
    public ProductView MediatRHandWritten() => Evict(Sync.Run(_mediatR.Send(_request)), _mediatRCache);

    private ProductView Evict(ProductView view, IDistributedCache cache)
    {
        if (_miss)
        {
            cache.Remove(BehaviorSelfCheck.ProductKey);
        }

        return view;
    }
}
