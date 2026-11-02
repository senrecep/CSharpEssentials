using CSharpEssentials.Mediator.Benchmarks.Handlers.MediatR50;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Builds every MediatR container of the suite with <c>AddMediatR</c> defaults (<c>IMediator</c> and handlers transient,
/// <c>ForeachAwaitPublisher</c>). <see cref="Dispatch"/>'s open behaviors keep the transient default; <see cref="HandWritten"/>
/// sets CSE's behavior lifetimes through <c>AddBehavior</c>'s lifetime overload.
/// </summary>
public static class MediatRApp
{
    /// <summary>The handlers of the benchmark project; with <paramref name="behaviors"/> 2, two pass-through open behaviors.</summary>
    /// <param name="behaviors">0 or 2.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    public static ServiceProvider Dispatch(int behaviors)
    {
        ServiceCollection services = [];
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
            if (behaviors == 2)
            {
                cfg.AddOpenBehavior(typeof(MediatRPassThroughA<,>));
                cfg.AddOpenBehavior(typeof(MediatRPassThroughB<,>));
            }
        });
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// The handlers of the benchmark project, the CSE validator, and the hand-written validation (scoped) and exception
    /// handling (singleton) behaviors, in that order: the same lifetimes and order CSharpEssentials.Mediator registers its own.
    /// </summary>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    public static ServiceProvider HandWritten()
    {
        ServiceCollection services = [];
        services.AddValidator<CreateOrder, CreateOrderValidator>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
            cfg.AddBehavior<IPipelineBehavior<CreateOrder, Result<int>>, MediatRValidationBehavior>(ServiceLifetime.Scoped);
            cfg.AddBehavior<IPipelineBehavior<CreateOrder, Result<int>>, MediatRExceptionHandlingBehavior>(ServiceLifetime.Singleton);
        });
        return services.BuildServiceProvider();
    }

    /// <summary>Scans the 50 generated handlers, builds the provider, sends <c>Request0(41)</c> once and disposes.</summary>
    /// <returns>The response, 41.</returns>
    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    public static int StartupFirstSend()
    {
        ServiceCollection services = [];
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Request0).Assembly));
        using ServiceProvider provider = services.BuildServiceProvider();
        return Sync.Run(provider.GetRequiredService<IMediator>().Send(new Request0(41)));
    }

    /// <summary>The descriptors <see cref="StartupFirstSend"/> registers for request handlers.</summary>
    /// <returns>The count.</returns>
    public static int StartupHandlerCount()
    {
        ServiceCollection services = [];
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Request0).Assembly));
        return services.Count(static d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }
}
