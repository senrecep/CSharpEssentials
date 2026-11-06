using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One request/response <c>Send</c> of <see cref="Ping"/> through a mediator resolved once from the root provider, with 0 or
/// 2 pass-through behaviors. Each library runs its documented defaults: MediatR resolves the transient handler and transient
/// behaviors from DI and composes the pipeline on every send; Mediator (singleton by default) composes it once per container
/// and then calls a cached delegate. That difference is how the libraries work, not a setup choice.
/// The self-check in <see cref="Setup"/> sends once per column and checks the response and how many behaviors ran.
/// </summary>
[MemoryDiagnoser]
public class SendBenchmarks
{
    private static readonly Ping Request = new(41);
    private ServiceProvider _mediatRProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::Mediator.Mediator _concrete = null!;

    [Params(0, 2)]
    public int Behaviors { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _mediatRProvider = MediatRApp.Dispatch(Behaviors);
        _mediatorProvider = MediatorApp.Dispatch(Behaviors);
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _concrete = _mediatorProvider.GetRequiredService<global::Mediator.Mediator>();
        MediatorSelfCheck.VerifySend(Behaviors, MediatR, MediatorIMediator, MediatorConcrete);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _mediatRProvider.Dispose();
        _mediatorProvider.Dispose();
    }

    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/Mediator.cs
    [Benchmark(Baseline = true)]
    public int MediatR() => Sync.Run(_mediatR.Send(Request));

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#43-create-irequest-type
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#34-configuration
    [Benchmark]
    public int MediatorIMediator() => Sync.Run(_mediator.Send(Request));

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#2-benchmarks
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/src/Mediator.SourceGenerator/Implementation/resources/Mediator.sbn-cs
    [Benchmark]
    public int MediatorConcrete() => Sync.Run(_concrete.Send(Request));
}
