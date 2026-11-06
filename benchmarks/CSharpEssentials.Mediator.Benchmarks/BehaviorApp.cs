using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Builds the containers of the behavior suites: one CSharpEssentials.Mediator container, one hand-written Mediator
/// container and one hand-written MediatR container per scenario. Mediator containers go through
/// <see cref="MediatorApp.Register"/>, the one <c>AddMediator</c> call of the project. MediatR containers follow
/// <see cref="MediatRApp.HandWritten"/>. Every container registers <c>ILogger&lt;T&gt;</c> (a <c>NullLogger&lt;T&gt;</c>
/// unless the scenario says otherwise) and is built with plain <c>BuildServiceProvider()</c>.
/// <para>
/// The CSE Readme registers <c>AddMediatorBehaviors()</c> next to each behavior. These CSE containers register only the
/// behavior under test, so each row isolates one behavior.
/// </para>
/// </summary>
public static class BehaviorApp
{
    /// <summary>CSE <c>ValidationBehavior</c> and the validators of <paramref name="groups"/>.</summary>
    /// <param name="groups">The validator layout.</param>
    /// <returns>The root provider.</returns>
    public static ServiceProvider CseValidation(ValidatorGroups groups)
    {
        IServiceCollection services = NewMediator();
        services.AddMediatorValidationBehavior();
        AddValidators(services, groups);
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written fan-out validation behavior (scoped) and the validators of <paramref name="groups"/>.</summary>
    /// <param name="groups">The validator layout.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#331-message-validation-example
    public static ServiceProvider MediatorHandWrittenValidation(ValidatorGroups groups)
    {
        IServiceCollection services = NewMediator();
        services.AddScoped<global::Mediator.IPipelineBehavior<CreateCustomer, Result<int>>, MediatorFanOutValidationBehavior>();
        AddValidators(services, groups);
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written fan-out validation behavior (scoped) and the validators of <paramref name="groups"/>.</summary>
    /// <param name="groups">The validator layout.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/wiki/Behaviors
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    public static ServiceProvider MediatRHandWrittenValidation(ValidatorGroups groups)
    {
        IServiceCollection services = NewMediatR(static cfg => cfg.AddBehavior<IPipelineBehavior<CreateCustomer, Result<int>>, MediatRFanOutValidationBehavior>(ServiceLifetime.Scoped));
        AddValidators(services, groups);
        return services.BuildServiceProvider();
    }

    /// <summary>CSE <c>IdempotencyBehavior</c> with <c>StoreFailures = false</c>, its executor and the in-memory store.</summary>
    /// <returns>The root provider.</returns>
    public static ServiceProvider CseIdempotency()
    {
        IServiceCollection services = NewMediator();
        services.AddMediatorIdempotencyBehavior(static o => o.StoreFailures = false);
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written idempotency behavior (scoped) and its store (singleton).</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    public static ServiceProvider MediatorHandWrittenIdempotency()
    {
        IServiceCollection services = NewMediator();
        services.AddSingleton<HandWrittenIdempotencyStore>();
        services.AddScoped<global::Mediator.IPipelineBehavior<PlaceOrder, Result<int>>, MediatorIdempotencyBehavior>();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written idempotency behavior (scoped) and its store (singleton).</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/wiki/Behaviors
    // https://github.com/jbogard/MediatR/blob/v12.5.0/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs
    public static ServiceProvider MediatRHandWrittenIdempotency()
    {
        IServiceCollection services = NewMediatR(static cfg => cfg.AddBehavior<IPipelineBehavior<PlaceOrder, Result<int>>, MediatRIdempotencyBehavior>(ServiceLifetime.Scoped));
        services.AddSingleton<HandWrittenIdempotencyStore>();
        return services.BuildServiceProvider();
    }

    /// <summary>CSE <c>LockBehavior</c> (scoped) over <c>InProcessResourceLock</c> (singleton).</summary>
    /// <returns>The root provider.</returns>
    public static ServiceProvider CseLock()
    {
        IServiceCollection services = NewMediator();
        services.AddMediatorLockBehavior();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written AsyncKeyedLock behavior (scoped).</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    // https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
    public static ServiceProvider MediatorHandWrittenLock()
    {
        IServiceCollection services = NewMediator();
        services.AddScoped<global::Mediator.IPipelineBehavior<ReserveStock, Result<int>>, MediatorLockBehavior>();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written AsyncKeyedLock behavior (scoped).</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/wiki/Behaviors
    // https://github.com/MarkCiliaVincenti/AsyncKeyedLock/blob/8.1.2/README.md#installation-and-usage
    public static ServiceProvider MediatRHandWrittenLock() =>
        NewMediatR(static cfg => cfg.AddBehavior<IPipelineBehavior<ReserveStock, Result<int>>, MediatRLockBehavior>(ServiceLifetime.Scoped)).BuildServiceProvider();

    /// <summary>CSE <c>CachingBehavior</c> (singleton) over the distributed memory cache.</summary>
    /// <returns>The root provider.</returns>
    // https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
    public static ServiceProvider CseCaching()
    {
        IServiceCollection services = NewMediator();
        services.AddDistributedMemoryCache();
        services.AddMediatorCachingBehavior();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written caching behavior (singleton) over the distributed memory cache.</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    // https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
    public static ServiceProvider MediatorHandWrittenCaching()
    {
        IServiceCollection services = NewMediator();
        services.AddDistributedMemoryCache();
        services.AddSingleton<global::Mediator.IPipelineBehavior<GetProduct, ProductView>, MediatorCachingBehavior>();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written caching behavior (singleton) over the distributed memory cache.</summary>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/wiki/Behaviors
    // https://learn.microsoft.com/aspnet/core/performance/caching/distributed#distributed-memory-cache
    public static ServiceProvider MediatRHandWrittenCaching()
    {
        IServiceCollection services = NewMediatR(static cfg => cfg.AddBehavior<IPipelineBehavior<GetProduct, ProductView>, MediatRCachingBehavior>(ServiceLifetime.Singleton));
        services.AddDistributedMemoryCache();
        return services.BuildServiceProvider();
    }

    /// <summary>CSE <c>LoggingBehavior</c> (singleton) with the logger of <paramref name="logger"/>.</summary>
    /// <param name="logger">The logger every container registers.</param>
    /// <returns>The root provider.</returns>
    public static ServiceProvider CseLogging(LoggerMode logger)
    {
        IServiceCollection services = NewMediator(logger);
        services.AddMediatorLoggingBehavior();
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written logging behavior (open generic, singleton) with the logger of <paramref name="logger"/>.</summary>
    /// <param name="logger">The logger every container registers.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/martinothamar/Mediator/blob/v3.0.2/README.md#44-use-pipeline-behaviors
    // https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
    public static ServiceProvider MediatorHandWrittenLogging(LoggerMode logger)
    {
        IServiceCollection services = NewMediator(logger);
        services.AddSingleton(typeof(global::Mediator.IPipelineBehavior<,>), typeof(MediatorLoggingBehavior<,>));
        return services.BuildServiceProvider();
    }

    /// <summary>The hand-written logging behavior (open generic, singleton) with the logger of <paramref name="logger"/>.</summary>
    /// <param name="logger">The logger every container registers.</param>
    /// <returns>The root provider.</returns>
    // https://github.com/jbogard/MediatR/wiki/Behaviors
    // https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator
    public static ServiceProvider MediatRHandWrittenLogging(LoggerMode logger) =>
        NewMediatR(static cfg => cfg.AddOpenBehavior(typeof(MediatRLoggingBehavior<,>), ServiceLifetime.Singleton), logger).BuildServiceProvider();

    private static ServiceCollection NewMediator(LoggerMode logger = LoggerMode.Disabled)
    {
        ServiceCollection services = [];
        MediatorApp.Register(services);
        AddLogger(services, logger);
        return services;
    }

    // https://github.com/jbogard/MediatR/blob/v12.5.0/README.md#registering-with-iservicecollection
    private static ServiceCollection NewMediatR(Action<MediatRServiceConfiguration> behaviors, LoggerMode logger = LoggerMode.Disabled)
    {
        ServiceCollection services = [];
        AddLogger(services, logger);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly);
            behaviors(cfg);
        });
        return services;
    }

    private static void AddLogger(IServiceCollection services, LoggerMode logger) =>
        services.AddSingleton(typeof(ILogger<>), logger == LoggerMode.Enabled ? typeof(CountingLogger<>) : typeof(NullLogger<>));

    private static void AddValidators(IServiceCollection services, ValidatorGroups groups)
    {
        services.AddValidator<CreateCustomer, CustomerNameValidator>();
        services.AddValidator<CreateCustomer, CustomerEmailValidator>();
        if (groups == ValidatorGroups.One)
        {
            services.AddValidator<CreateCustomer, CustomerAgeValidator>();
        }
        else
        {
            services.AddValidator<CreateCustomer, CustomerAgeLateValidator>();
        }
    }
}
