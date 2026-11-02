using MediatR;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
// https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
// https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
    cfg.AddOpenBehavior(typeof(PassThroughA<,>));
    cfg.AddOpenBehavior(typeof(PassThroughB<,>));
});

using ServiceProvider provider = services.BuildServiceProvider();
int response = await provider.GetRequiredService<IMediator>().Send(new Ping(41));
bool ok = response == 42 && Probe.BehaviorCalls == 2;
Console.WriteLine($"response {response} behaviors {Probe.BehaviorCalls} {(ok ? "ok" : "MISMATCH")}");
return ok ? 0 : 1;

internal static class Probe
{
    public static int BehaviorCalls;
}

internal sealed record Ping(int Value) : IRequest<int>;

internal sealed class PingHandler : IRequestHandler<Ping, int>
{
    public Task<int> Handle(Ping request, CancellationToken cancellationToken) => Task.FromResult(request.Value + 1);
}

internal sealed class PassThroughA<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Probe.BehaviorCalls++;
        return next(cancellationToken);
    }
}

internal sealed class PassThroughB<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Probe.BehaviorCalls++;
        return next(cancellationToken);
    }
}
