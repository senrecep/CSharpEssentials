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
        services.AddMediatorTransactionBehavior();
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

    /// <summary>
    /// Registers <see cref="CSharpEssentials.Mediator.TransactionScopeBehavior{TRequest, TResponse}"/>. It takes the
    /// pipeline position of a registered <see cref="CSharpEssentials.Mediator.TransactionBehavior{TRequest, TResponse}"/>,
    /// so only one transaction behavior ever wraps an <see cref="ITransactionalRequest"/>.
    /// </summary>
    public static IServiceCollection AddMediatorTransactionBehavior(this IServiceCollection services) =>
        services.SetTransactionBehavior(
            ServiceDescriptor.Singleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>)));

    /// <summary>
    /// Registers <see cref="CSharpEssentials.Mediator.TransactionBehavior{TRequest, TResponse}"/>, which runs
    /// <see cref="ITransactionalRequest"/> handlers through the registered
    /// <see cref="CSharpEssentials.Transactions.ITransactionRunner"/>. It takes the pipeline position of a registered
    /// <see cref="CSharpEssentials.Mediator.TransactionScopeBehavior{TRequest, TResponse}"/>, so only one transaction
    /// behavior ever wraps a request. Register an <see cref="CSharpEssentials.Transactions.ITransactionRunner"/> as well,
    /// for example with <c>AddEfCoreTransactionRunner&lt;TDbContext&gt;()</c>.
    /// </summary>
    public static IServiceCollection AddMediatorTransactionRunnerBehavior(this IServiceCollection services) =>
        services.SetTransactionBehavior(
            ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.TransactionBehavior<,>)));

    /// <summary>
    /// Registers <see cref="CSharpEssentials.Mediator.LockBehavior{TRequest, TResponse}"/> (scoped) for
    /// <see cref="ILockedRequest"/> requests, and <see cref="CSharpEssentials.Locking.InProcessResourceLock"/> as the
    /// <see cref="CSharpEssentials.Locking.IResourceLock"/> unless one is already registered.
    /// <para>
    /// The behavior is placed before or after the registered transaction behavior according to <paramref name="placement"/>;
    /// with no transaction behavior it is added at the end. Calling it again moves the behavior instead of adding a second one.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="placement"/> is <see cref="LockPlacement.InsideTransaction"/> and no transaction behavior is registered.
    /// </exception>
    public static IServiceCollection AddMediatorLockBehavior(
        this IServiceCollection services,
        LockPlacement placement = LockPlacement.OutsideTransaction)
    {
        services.TryAddSingleton<CSharpEssentials.Locking.IResourceLock, CSharpEssentials.Locking.InProcessResourceLock>();

        for (int index = services.Count - 1; index >= 0; index--)
        {
            if (IsBehavior(services[index], typeof(CSharpEssentials.Mediator.LockBehavior<,>)))
                services.RemoveAt(index);
        }

        int transaction = -1;
        for (int index = 0; index < services.Count && transaction < 0; index++)
        {
            if (IsTransactionBehavior(services[index]))
                transaction = index;
        }

        var behavior = ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.LockBehavior<,>));
        if (transaction < 0)
        {
            if (placement == LockPlacement.InsideTransaction)
                throw new InvalidOperationException(
                    "LockPlacement.InsideTransaction needs a registered transaction behavior. Call AddMediatorBehaviors, AddMediatorTransactionBehavior or AddMediatorTransactionRunnerBehavior first.");
            services.Add(behavior);
        }
        else
        {
            services.Insert(placement == LockPlacement.InsideTransaction ? transaction + 1 : transaction, behavior);
        }

        return services;
    }

    private static IServiceCollection SetTransactionBehavior(this IServiceCollection services, ServiceDescriptor behavior)
    {
        int position = -1;
        for (int index = services.Count - 1; index >= 0; index--)
        {
            if (!IsTransactionBehavior(services[index]))
                continue;
            services.RemoveAt(index);
            position = index;
        }

        if (position < 0)
            services.Add(behavior);
        else
            services.Insert(position, behavior);
        return services;
    }

    private static bool IsTransactionBehavior(ServiceDescriptor descriptor) =>
        IsBehavior(descriptor, typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>))
        || IsBehavior(descriptor, typeof(CSharpEssentials.Mediator.TransactionBehavior<,>));

    private static bool IsBehavior(ServiceDescriptor descriptor, Type implementationType) =>
        descriptor.ServiceType == typeof(IPipelineBehavior<,>)
        && !descriptor.IsKeyedService
        && descriptor.ImplementationType == implementationType;
}
