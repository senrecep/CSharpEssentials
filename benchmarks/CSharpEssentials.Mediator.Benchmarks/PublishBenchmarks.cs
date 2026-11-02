using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Publish</c> to 1 or 5 synchronously completing handlers, with each library's default publisher (both
/// <c>ForeachAwaitPublisher</c>: handlers run one after another) and default handler lifetime (MediatR transient, Mediator
/// singleton). The self-check in <see cref="Setup"/> publishes once per column and checks every handler ran exactly once.
/// </summary>
[MemoryDiagnoser]
public class PublishBenchmarks
{
    private static readonly Notification1 One = new();
    private static readonly Notification5 Five = new();
    private ServiceProvider _mediatRProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::Mediator.Mediator _concrete = null!;

    [Params(1, 5)]
    public int Handlers { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _mediatRProvider = MediatRApp.Dispatch(0);
        _mediatorProvider = MediatorApp.Dispatch(0);
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _concrete = _mediatorProvider.GetRequiredService<global::Mediator.Mediator>();
        MediatorSelfCheck.VerifyPublish(Handlers, MediatR, MediatorIMediator, MediatorConcrete);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _mediatRProvider.Dispose();
        _mediatorProvider.Dispose();
    }

    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    // https://github.com/jbogard/MediatR/tree/v12.5.0/src/MediatR/NotificationPublishers
    [Benchmark(Baseline = true)]
    public void MediatR()
    {
        if (Handlers == 1)
        {
            Sync.Run(_mediatR.Publish(One));
        }
        else
        {
            Sync.Run(_mediatR.Publish(Five));
        }
    }

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#49-notification-publishers
    [Benchmark]
    public void MediatorIMediator()
    {
        if (Handlers == 1)
        {
            Sync.Run(_mediator.Publish(One));
        }
        else
        {
            Sync.Run(_mediator.Publish(Five));
        }
    }

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#2-benchmarks
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/src/Mediator.SourceGenerator/Implementation/resources/Mediator.sbn-cs
    [Benchmark]
    public void MediatorConcrete()
    {
        if (Handlers == 1)
        {
            Sync.Run(_concrete.Publish(One));
        }
        else
        {
            Sync.Run(_concrete.Publish(Five));
        }
    }
}
