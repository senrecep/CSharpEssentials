using Mediator;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
// https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
// https://github.com/martinothamar/Mediator/tree/v3.0.2/samples/basic/ConsoleAOT
services.AddMediator(options =>
{
    options.GenerateTypesAsInternal = true;
    options.PipelineBehaviors = [typeof(PassThroughA<,>), typeof(PassThroughB<,>)];
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
    public ValueTask<int> Handle(Ping request, CancellationToken cancellationToken) => new(request.Value + 1);
}

internal sealed class PassThroughA<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        Probe.BehaviorCalls++;
        return next(message, cancellationToken);
    }
}

internal sealed class PassThroughB<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        Probe.BehaviorCalls++;
        return next(message, cancellationToken);
    }
}
