using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Extension methods for registering slow query interceptor services.
/// </summary>
public static class SlowQueryServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SlowQueryInterceptor"/> with configurable options.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSlowQueryInterceptor(options =>
    ///     options.Threshold = TimeSpan.FromMilliseconds(500));
    /// </code>
    /// </example>
    public static IServiceCollection AddSlowQueryInterceptor(
        this IServiceCollection services,
        Action<SlowQueryOptions>? configure = null)
    {
        SlowQueryOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<SlowQueryInterceptor>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="SlowQueryInterceptor"/> with a specific threshold.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSlowQueryInterceptor(TimeSpan.FromSeconds(2));
    /// </code>
    /// </example>
    public static IServiceCollection AddSlowQueryInterceptor(
        this IServiceCollection services,
        TimeSpan threshold)
    {
        return services.AddSlowQueryInterceptor(options => options.Threshold = threshold);
    }
}
