using CSharpEssentials.Mediator.Benchmarks.Handlers.Mediator50;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation.Extensions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Builds every Mediator container of the suite with the <c>AddMediator</c> defaults: singleton lifetime and
/// <c>ForeachAwaitPublisher</c>. This is the one <c>AddMediator</c> call of the benchmark project, because the source
/// generator applies its options to the whole project; <c>Assemblies</c> keeps the generated handler library (which has its
/// own generated mediator, see <see cref="StartupFirstSend"/>) out of this one. Behaviors are DI registrations per container,
/// so the 0-behavior and 2-behavior containers differ only in those descriptors.
/// </summary>
public static class MediatorApp
{
    /// <summary>The handlers of the benchmark project; with <paramref name="behaviors"/> 2, two pass-through open behaviors.</summary>
    /// <param name="behaviors">0 or 2.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#34-configuration
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#332-error-logging-example
    public static ServiceProvider Dispatch(int behaviors)
    {
        ServiceCollection services = [];
        Add(services);
        if (behaviors == 2)
        {
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MediatorPassThroughA<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(MediatorPassThroughB<,>));
        }

        return services.BuildServiceProvider();
    }

    /// <summary>The CSharpEssentials.Mediator validation and exception handling behaviors and the validator, as its Readme registers them.</summary>
    /// <returns>The root provider (plain <c>BuildServiceProvider()</c>; see <see cref="BehaviorBenchmarks"/> on scope validation).</returns>
    public static ServiceProvider Cse()
    {
        ServiceCollection services = [];
        Add(services);
        services.AddMediatorValidationBehavior();
        services.AddMediatorExceptionHandlingBehavior();
        services.AddValidator<CreateOrder, CreateOrderValidator>();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written validation (scoped) and exception handling (singleton) behaviors and the CSE validator, in that order.</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#331-message-validation-example
    public static ServiceProvider HandWritten()
    {
        ServiceCollection services = [];
        Add(services);
        services.AddValidator<CreateOrder, CreateOrderValidator>();
        services.AddScoped<IPipelineBehavior<CreateOrder, Result<int>>, MediatorValidationBehavior>();
        services.AddSingleton<IPipelineBehavior<CreateOrder, Result<int>>, MediatorExceptionHandlingBehavior>();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Registers the 50 generated handlers through the <c>AddMediator</c> generated for that library, builds the provider,
    /// sends <c>Request0(41)</c> once and disposes.
    /// </summary>
    /// <returns>The response, 41.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#42-add-mediator-to-di-container
    public static int StartupFirstSend()
    {
        ServiceCollection services = [];
        MediatorS50.Add(services);
        using ServiceProvider provider = services.BuildServiceProvider();
        return Sync.Run(provider.GetRequiredService<IMediator>().Send(new Request0(41)));
    }

    /// <summary>The descriptors <see cref="StartupFirstSend"/> registers for request handlers.</summary>
    /// <returns>The count.</returns>
    public static int StartupHandlerCount() =>
        MediatorS50.Add(new ServiceCollection())
            .Count(static d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

    private static void Add(IServiceCollection services) => services.AddMediator(static options => options.Assemblies = [typeof(Ping)]);
}
