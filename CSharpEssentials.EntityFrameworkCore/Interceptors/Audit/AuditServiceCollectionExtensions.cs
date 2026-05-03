using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Extension methods for registering audit interceptor services.
/// </summary>
public static class AuditServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IAuditUserIdProvider"/> with a factory delegate.
    /// <para>Use when you need access to <see cref="IServiceProvider"/> to resolve the user ID
    /// (e.g. from HttpContext, ClaimsPrincipal).</para>
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddAuditUserIdProvider(sp =>
    ///     sp.GetRequiredService&lt;IHttpContextAccessor&gt;()
    ///       .HttpContext?.User?.FindFirst("sub")?.Value ?? "anonymous");
    /// </code>
    /// </example>
    public static IServiceCollection AddAuditUserIdProvider(
        this IServiceCollection services,
        Func<IServiceProvider, string> factory)
    {
        return services.AddScoped<IAuditUserIdProvider>(sp =>
            new DelegateAuditUserIdProvider(factory(sp)));
    }

    /// <summary>
    /// Registers <see cref="IAuditUserIdProvider"/> with a simple value factory.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddAuditUserIdProvider(() => "system");
    /// </code>
    /// </example>
    public static IServiceCollection AddAuditUserIdProvider(
        this IServiceCollection services,
        Func<string> factory)
    {
        return services.AddScoped<IAuditUserIdProvider>(_ =>
            new DelegateAuditUserIdProvider(factory()));
    }

    /// <summary>
    /// Registers a typed <see cref="IAuditUserIdProvider{TUserId}"/> with a factory delegate.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddAuditUserIdProvider&lt;Guid&gt;(sp =>
    ///     sp.GetRequiredService&lt;ICurrentUserService&gt;().UserId);
    /// </code>
    /// </example>
    public static IServiceCollection AddAuditUserIdProvider<TUserId>(
        this IServiceCollection services,
        Func<IServiceProvider, TUserId> factory)
        where TUserId : notnull
    {
        return services.AddScoped<IAuditUserIdProvider>(sp =>
            new DelegateAuditUserIdProvider<TUserId>(factory(sp)));
    }

    /// <summary>
    /// Registers a typed <see cref="IAuditUserIdProvider{TUserId}"/> with a simple value factory.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddAuditUserIdProvider&lt;Guid&gt;(() => Guid.Parse("..."));
    /// </code>
    /// </example>
    public static IServiceCollection AddAuditUserIdProvider<TUserId>(
        this IServiceCollection services,
        Func<TUserId> factory)
        where TUserId : notnull
    {
        return services.AddScoped<IAuditUserIdProvider>(_ =>
            new DelegateAuditUserIdProvider<TUserId>(factory()));
    }

    /// <summary>
    /// Registers <see cref="AuditInterceptor"/> along with <see cref="TimeProvider"/>
    /// (if not already registered) as a ready-to-use audit pipeline.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddAuditInterceptor(() => "demo-user");
    ///
    /// // or with IServiceProvider:
    /// services.AddAuditInterceptor(sp =>
    ///     sp.GetRequiredService&lt;IHttpContextAccessor&gt;()
    ///       .HttpContext?.User?.Identity?.Name ?? "anonymous");
    /// </code>
    /// </example>
    public static IServiceCollection AddAuditInterceptor(
        this IServiceCollection services,
        Func<IServiceProvider, string> userIdFactory)
    {
        services.AddAuditUserIdProvider(userIdFactory);
        services.TryAddAuditInterceptorCore();
        return services;
    }

    /// <inheritdoc cref="AddAuditInterceptor(IServiceCollection, Func{IServiceProvider, string})"/>
    public static IServiceCollection AddAuditInterceptor(
        this IServiceCollection services,
        Func<string> userIdFactory)
    {
        services.AddAuditUserIdProvider(userIdFactory);
        services.TryAddAuditInterceptorCore();
        return services;
    }

    /// <inheritdoc cref="AddAuditInterceptor(IServiceCollection, Func{IServiceProvider, string})"/>
    public static IServiceCollection AddAuditInterceptor<TUserId>(
        this IServiceCollection services,
        Func<IServiceProvider, TUserId> userIdFactory)
        where TUserId : notnull
    {
        services.AddAuditUserIdProvider(userIdFactory);
        services.TryAddAuditInterceptorCore();
        return services;
    }

    /// <inheritdoc cref="AddAuditInterceptor(IServiceCollection, Func{IServiceProvider, string})"/>
    public static IServiceCollection AddAuditInterceptor<TUserId>(
        this IServiceCollection services,
        Func<TUserId> userIdFactory)
        where TUserId : notnull
    {
        services.AddAuditUserIdProvider(userIdFactory);
        services.TryAddAuditInterceptorCore();
        return services;
    }

    private static void TryAddAuditInterceptorCore(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditInterceptor>();
    }

    // ── Internal implementations ────────────────────────────────────────

    private sealed class DelegateAuditUserIdProvider(string userId) : IAuditUserIdProvider
    {
        public string GetCurrentUserId() => userId;
    }

    private sealed class DelegateAuditUserIdProvider<TUserId>(TUserId userId)
        : IAuditUserIdProvider<TUserId> where TUserId : notnull
    {
        public TUserId GetCurrentUserId() => userId;
    }
}
