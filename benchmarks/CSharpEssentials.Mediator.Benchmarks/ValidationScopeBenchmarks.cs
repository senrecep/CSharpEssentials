using BenchmarkDotNet.Attributes;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Not a send. One new scope that resolves the validation behavior of <see cref="CreateCustomer"/>, from the
/// <see cref="ValidatorGroups.One"/> containers of <see cref="ValidationFanOutBenchmarks"/>: the scope, three scoped
/// validators and one behavior constructor. That is what a scoped mediator, or MediatR resolved from a request
/// scope, pays on every request. With this suite's singleton Mediator the cost is paid once, not per send.
/// <para>
/// The CSE constructor groups the validators by <c>Order</c> with LINQ (<c>GroupBy</c>, <c>OrderBy</c>, <c>Select</c>,
/// <c>ToArray</c>) and copies the failure observers; the hand-written constructors sort with <c>OrderBy</c> and copy.
/// Each row returns the number of resolved behaviors, which must be 1.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class ValidationScopeBenchmarks
{
    private ServiceProvider _cse = null!;
    private ServiceProvider _mediator = null!;
    private ServiceProvider _mediatR = null!;

    [GlobalSetup]
    public void Setup()
    {
        MediatorSelfCheck.VerifyVersions();
        _cse = BehaviorApp.CseValidation(ValidatorGroups.One);
        _mediator = BehaviorApp.MediatorHandWrittenValidation(ValidatorGroups.One);
        _mediatR = BehaviorApp.MediatRHandWrittenValidation(ValidatorGroups.One);
        BehaviorSelfCheck.VerifyScope(MediatorHandWritten, Cse, MediatRHandWritten, _cse, _mediator, _mediatR);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cse.Dispose();
        _mediator.Dispose();
        _mediatR.Dispose();
    }

    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#331-message-validation-example
    [Benchmark(Baseline = true)]
    public int MediatorHandWritten()
    {
        using IServiceScope scope = _mediator.CreateScope();
        return scope.ServiceProvider.GetServices<global::Mediator.IPipelineBehavior<CreateCustomer, Result<int>>>().Count();
    }

    [Benchmark]
    public int Cse()
    {
        using IServiceScope scope = _cse.CreateScope();
        return scope.ServiceProvider.GetServices<global::Mediator.IPipelineBehavior<CreateCustomer, Result<int>>>().Count();
    }

    // https://github.com/jbogard/MediatR/wiki/Behaviors
    [Benchmark]
    public int MediatRHandWritten()
    {
        using IServiceScope scope = _mediatR.CreateScope();
        return scope.ServiceProvider.GetServices<global::MediatR.IPipelineBehavior<CreateCustomer, Result<int>>>().Count();
    }
}
