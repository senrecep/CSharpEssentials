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
        else if (placement == LockPlacement.InsideTransaction)
        {
            int position = transaction + 1;
            if (position < services.Count && IsBehavior(services[position], typeof(CSharpEssentials.Mediator.IdempotencyBehavior<,>)))
                position++;
            services.Insert(position, behavior);
        }
        else
        {
            services.Insert(transaction, behavior);
        }

        return services;
    }

    /// <summary>
    /// Registers <see cref="CSharpEssentials.Mediator.IdempotencyBehavior{TRequest, TResponse}"/> (scoped) for
    /// <see cref="IIdempotentRequest"/> requests, its <see cref="IdempotencyBehaviorOptions"/>, and the
    /// <see cref="CSharpEssentials.Idempotency.IIdempotencyExecutor"/> with <c>AddIdempotencyExecutor</c>. Configure the
    /// executor defaults, profiles and record store with <c>AddIdempotencyExecutor</c> and an
    /// <see cref="CSharpEssentials.Idempotency.IIdempotencyRecordStore"/> registration.
    /// <para>
    /// With <see cref="IdempotencyPlacement.OutsideTransaction"/> the behavior runs before the lock and transaction
    /// behaviors; with <see cref="IdempotencyPlacement.InsideTransaction"/> right after the transaction behavior and
    /// before a lock placed inside it. Call it after <c>AddMediatorBehaviors</c>, so validation, logging and exception
    /// handling run before it: with no lock or transaction behavior registered it is added at the end. Calling it again
    /// configures the same options and moves the behavior instead of adding a second one.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The placement is <see cref="IdempotencyPlacement.InsideTransaction"/> and no transaction behavior is registered,
    /// a setting is invalid, or <see cref="IdempotencyBehaviorOptions"/> is already registered with a factory or type.
    /// </exception>
    public static IServiceCollection AddMediatorIdempotencyBehavior(
        this IServiceCollection services,
        Action<IdempotencyBehaviorOptions>? configure = null)
    {
        services.AddIdempotencyExecutor();

        ServiceDescriptor? existing = services.LastOrDefault(descriptor =>
            descriptor.ServiceType == typeof(IdempotencyBehaviorOptions) && !descriptor.IsKeyedService);
        IdempotencyBehaviorOptions options;
        if (existing is null)
        {
            options = new IdempotencyBehaviorOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);
        }
        else if (existing.ImplementationInstance is IdempotencyBehaviorOptions registered)
        {
            options = registered;
            configure?.Invoke(options);
        }
        else
        {
            throw new InvalidOperationException(
                $"{nameof(IdempotencyBehaviorOptions)} is already registered with a factory or type. Register it as an instance so its placement is known.");
        }

        if ((uint)options.Placement > (uint)IdempotencyPlacement.InsideTransaction)
            throw new InvalidOperationException($"IdempotencyBehaviorOptions.Placement ({options.Placement}) is not a defined value.");
        if ((uint)options.UnscopedKeyBehavior > (uint)CSharpEssentials.Idempotency.UnscopedKeyBehavior.Global)
            throw new InvalidOperationException($"IdempotencyBehaviorOptions.UnscopedKeyBehavior ({options.UnscopedKeyBehavior}) is not a defined value.");
        if (options.ScopeResolver is null)
            throw new InvalidOperationException("IdempotencyBehaviorOptions.ScopeResolver must not be null.");

        for (int index = services.Count - 1; index >= 0; index--)
        {
            if (IsBehavior(services[index], typeof(CSharpEssentials.Mediator.IdempotencyBehavior<,>)))
                services.RemoveAt(index);
        }

        int transaction = IndexOf(services, IsTransactionBehavior);
        var behavior = ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.IdempotencyBehavior<,>));
        if (options.Placement == IdempotencyPlacement.InsideTransaction)
        {
            if (transaction < 0)
                throw new InvalidOperationException(
                    "IdempotencyPlacement.InsideTransaction needs a registered transaction behavior. Call AddMediatorBehaviors, AddMediatorTransactionBehavior or AddMediatorTransactionRunnerBehavior first.");
            services.Insert(transaction + 1, behavior);
            return services;
        }

        int lockBehavior = IndexOf(services, static descriptor => IsBehavior(descriptor, typeof(CSharpEssentials.Mediator.LockBehavior<,>)));
        int position = transaction < 0 || lockBehavior >= 0 && lockBehavior < transaction ? lockBehavior : transaction;
        if (position < 0)
            services.Add(behavior);
        else
            services.Insert(position, behavior);
        return services;
    }

    private static int IndexOf(IServiceCollection services, Func<ServiceDescriptor, bool> predicate)
    {
        for (int index = 0; index < services.Count; index++)
        {
            if (predicate(services[index]))
                return index;
        }

        return -1;
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
