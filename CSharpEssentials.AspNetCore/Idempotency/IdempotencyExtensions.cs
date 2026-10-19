using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.AspNetCore;

/// <summary>Registration, middleware and endpoint extensions for <c>Idempotency-Key</c> handling.</summary>
public static class IdempotencyExtensions
{
    /// <summary>
    /// Registers <see cref="IdempotencyOptions"/> and the <see cref="IIdempotencyRecordStore"/> chosen in
    /// <paramref name="configure"/>. Without a <c>Use…Store()</c> call, <see cref="InMemoryIdempotencyRecordStore"/> is
    /// registered unless a store is already registered. Calling it again replaces the options.
    /// Invalid options throw here, not on the first request.
    /// </summary>
    public static IServiceCollection AddIdempotency(this IServiceCollection services, Action<IdempotencyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        IdempotencyOptions options = new();
        configure?.Invoke(options);
        options.Validate();
        services.Replace(ServiceDescriptor.Singleton(options));
        services.TryAddSingleton(TimeProvider.System);
        options.StoreRegistration(services);
        return services;
    }

    /// <summary>
    /// Adds the middleware that handles <c>Idempotency-Key</c> for endpoints marked with <c>.WithIdempotency()</c> or
    /// <see cref="IdempotentAttribute"/>. Call it after <c>UseRouting</c>, <c>UseAuthentication</c> and
    /// <c>UseAuthorization</c>, so the endpoint and the user are known.
    /// </summary>
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<IdempotencyMiddleware>();
    }

    /// <summary>
    /// Opts an endpoint or a route group into <c>Idempotency-Key</c> handling. On Minimal API endpoints it also adds
    /// the endpoint filter that sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/>; endpoint filters added
    /// after this call run inside it. MVC actions ignore endpoint filters: mark them with
    /// <see cref="IdempotentAttribute"/>, otherwise only their 2xx responses are stored.
    /// </summary>
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(new IdempotentAttribute());
        builder.Add(static endpoint =>
        {
            if (endpoint.Metadata.Any(static metadata => metadata is ActionDescriptor))
                return;
            if (!endpoint.FilterFactories.Contains(IdempotencyEndpointFilter.Factory))
                endpoint.FilterFactories.Add(IdempotencyEndpointFilter.Factory);
            if (!endpoint.Metadata.Contains(IdempotencyFilterInstalledMetadata.Instance))
                endpoint.Metadata.Add(IdempotencyFilterInstalledMetadata.Instance);
        });
        return builder;
    }
}
