using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// One <c>Send</c> of <see cref="LogPing"/> or <see cref="LogPayloadPing"/> through a logging behavior, returning
/// <see cref="Result{TValue}"/>. Cse is the CSharpEssentials.Mediator <c>LoggingBehavior</c>. The hand-written columns
/// log the same three messages with the same <c>[LoggerMessage]</c> templates and levels, time the call with
/// <c>Stopwatch.GetTimestamp</c>, and serialize payloads with the same options CSE uses, so the work is the same.
/// <para>
/// With <see cref="LoggerMode.Disabled"/> every container registers <c>NullLogger&lt;T&gt;</c>; with
/// <see cref="LoggerMode.Enabled"/> a <see cref="CountingLogger{T}"/> that formats every message. CSE reads the request
/// and response type names on every call; the hand-written behaviors keep them in static fields. The MediatR wiki
/// logging sample logs interpolated strings and no elapsed time. It does less work, so it is cited, not measured.
/// </para>
/// <para>
/// The behaviors are singletons resolved from the root provider with plain <c>BuildServiceProvider()</c>. The CSE
/// Readme registers <c>AddMediatorBehaviors()</c> as well; this container registers only the logging behavior.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class LoggingBehaviorBenchmarks
{
    private ServiceProvider _cseProvider = null!;
    private ServiceProvider _mediatorProvider = null!;
    private ServiceProvider _mediatRProvider = null!;
    private global::Mediator.IMediator _cse = null!;
    private global::Mediator.IMediator _mediator = null!;
    private global::MediatR.IMediator _mediatR = null!;
    private LogPing _ping = null!;
    private LogPayloadPing _payloadPing = null!;
    private bool _payload;

    [Params(LoggerMode.Disabled, LoggerMode.Enabled)]
    public LoggerMode Logger { get; set; }

    [Params(LogPayload.None, LogPayload.RequestResponse)]
    public LogPayload Payload { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cseProvider = BehaviorApp.CseLogging(Logger);
        _mediatorProvider = BehaviorApp.MediatorHandWrittenLogging(Logger);
        _mediatRProvider = BehaviorApp.MediatRHandWrittenLogging(Logger);
        _cse = _cseProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediator = _mediatorProvider.GetRequiredService<global::Mediator.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<global::MediatR.IMediator>();
        _ping = new LogPing(41);
        _payloadPing = new LogPayloadPing(41, "a");
        _payload = Payload == LogPayload.RequestResponse;
        BehaviorSelfCheck.VerifyLogging(Logger, Payload, _cseProvider, _mediatorProvider, _mediatRProvider, MediatorHandWritten, Cse, MediatRHandWritten);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cseProvider.Dispose();
        _mediatorProvider.Dispose();
        _mediatRProvider.Dispose();
    }

    // https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
    [Benchmark(Baseline = true)]
    public Result<int> MediatorHandWritten() =>
        _payload ? Sync.Run(_mediator.Send(_payloadPing)) : Sync.Run(_mediator.Send(_ping));

    [Benchmark]
    public Result<int> Cse() =>
        _payload ? Sync.Run(_cse.Send(_payloadPing)) : Sync.Run(_cse.Send(_ping));

    // https://github.com/jbogard/MediatR/wiki/Behaviors
    [Benchmark]
    public Result<int> MediatRHandWritten() =>
        _payload ? Sync.Run(_mediatR.Send(_payloadPing)) : Sync.Run(_mediatR.Send(_ping));
}
