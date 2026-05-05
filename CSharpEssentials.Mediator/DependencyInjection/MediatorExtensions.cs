using Mediator;

namespace Microsoft.Extensions.DependencyInjection;

public static class MediatorExtensions
{
    public static readonly Type[] DefaultPipelineBehaviors =
    [
        typeof(CSharpEssentials.Mediator.ValidationBehavior<,>),
        typeof(CSharpEssentials.Mediator.LoggingBehavior<,>),
        typeof(CSharpEssentials.Mediator.CachingBehavior<,>),
        typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>)
    ];

    public static IServiceCollection AddMediatorBehaviors(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ValidationBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.LoggingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.CachingBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.TransactionScopeBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorValidationBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.ValidationBehavior<,>));
        return services;
    }

    public static IServiceCollection AddMediatorLoggingBehavior(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CSharpEssentials.Mediator.LoggingBehavior<,>));
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
