using CSharpEssentials.Mediator;

using Mediator;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatorExtensions
{
    public static readonly Type[] DefaultPipelineBehaviors =
    [
        typeof(CSharpEssentials.Mediator.ValidationBehavior<,>),
        typeof(CSharpEssentials.Mediator.LoggingBehavior<,>),
        typeof(CSharpEssentials.Mediator.ExceptionHandlingBehavior<,>),
        typeof(CSharpEssentials.Mediator.CachingBehavior<,>),
        typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>)
    ];

    public static IServiceCollection AddMediatorBehaviors(this IServiceCollection services)
    {
        services.AddMediatorValidationOptions();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ValidationBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ExceptionHandlingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.CachingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorValidationBehavior(this IServiceCollection services)
    {
        services.AddMediatorValidationOptions();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ValidationBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorValidationBehavior(
        this IServiceCollection services,
        Action<ValidationBehaviorOptions> configure)
    {
        services.AddMediatorValidationOptions(configure);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ValidationBehavior<,>));
        return services;
    }

    /// <summary>
    /// Registers <see cref="ValidationBehaviorOptions"/> and the default
    /// <see cref="LoggingValidationFailureObserver"/> without registering the behavior itself.
    /// Use it with <see cref="DefaultPipelineBehaviors"/> on Native AOT. Calling it again
    /// configures the same options instance. When the options are already registered with a
    /// factory or implementation type, that registration is kept and passing <paramref name="configure"/> throws.
    /// </summary>
    public static IServiceCollection AddMediatorValidationOptions(
        this IServiceCollection services,
        Action<ValidationBehaviorOptions>? configure = null)
    {
        ServiceDescriptor? existing = services.LastOrDefault(descriptor =>
            descriptor.ServiceType == typeof(ValidationBehaviorOptions) && !descriptor.IsKeyedService);

        if (existing is null)
        {
            ValidationBehaviorOptions options = new();
            configure?.Invoke(options);
            services.AddSingleton(options);
        }
        else if (existing.ImplementationInstance is ValidationBehaviorOptions options)
        {
            configure?.Invoke(options);
        }
        else if (configure is not null)
        {
            throw new InvalidOperationException(
                $"{nameof(ValidationBehaviorOptions)} is already registered with a factory or type. Configure it there instead of passing a configure delegate.");
        }

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidationFailureObserver, LoggingValidationFailureObserver>());
        return services;
    }

    public static IServiceCollection AddMediatorLoggingBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.LoggingBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorExceptionHandlingBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ExceptionHandlingBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorCachingBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.CachingBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorTransactionBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>));
        return services;
    }
}
