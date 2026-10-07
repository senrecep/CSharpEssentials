using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.AspNetCore;

public static class IdempotencyExtensions
{
    /// <summary>
    /// Registers <see cref="IdempotencyOptions"/> and the <see cref="IIdempotencyStore"/> chosen in
    /// <paramref name="configure"/>. Without a <c>Use…Store()</c> call, <see cref="InMemoryIdempotencyStore"/> is
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

    /// <summary>Opts an endpoint or a route group into <c>Idempotency-Key</c> handling.</summary>
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new IdempotentAttribute());
    }
}
