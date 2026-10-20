using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>Registration, middleware and endpoint extensions for <c>Idempotency-Key</c> handling.</summary>
public static class IdempotencyExtensions
{
    /// <summary>
    /// Registers <see cref="IdempotencyOptions"/> and the <see cref="IIdempotencyRecordStore"/> chosen in
    /// <paramref name="configure"/>. Without a <c>Use…Store()</c> call, <see cref="InMemoryIdempotencyRecordStore"/> is
    /// registered unless a store is already registered. Calling it again replaces the options it configured.
    /// Invalid options throw here, not on the first request.
    /// <para>
    /// The options are read through <c>IOptionsMonitor&lt;IdempotencyOptions&gt;</c>: <paramref name="configure"/> runs
    /// every time they are built, and other <c>Configure</c> or <c>Bind</c> calls on <see cref="IdempotencyOptions"/>
    /// apply after it, so options bound to a reloadable configuration section (for example
    /// <see cref="IdempotencyOptions.Mode"/>) change at runtime. A reload that makes them invalid throws
    /// <c>OptionsValidationException</c> when they are rebuilt and on every request to an idempotent endpoint until it
    /// is fixed. <see cref="IdempotencyOptions"/> itself resolves to the current value. The store is the one chosen
    /// here; a reload does not change it.
    /// </para>
    /// <para>
    /// It also registers an <see cref="IIdempotencyScopeAccessor"/> that reads the user of the current request (unless
    /// one is already registered), so the Mediator idempotency behavior resolves scopes from the same caller as the
    /// middleware, and its <see cref="IdempotencyOptions.RetryAfter"/> is sent with every problem response carrying
    /// <see cref="IdempotencyErrorCodes.InFlight"/>.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIdempotency(this IServiceCollection services, Action<IdempotencyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        IdempotencyOptions options = new();
        configure?.Invoke(options);
        options.Validate();

        services.AddOptions();
        for (int index = services.Count - 1; index >= 0; index--)
        {
            if (services[index] is { IsKeyedService: false, ImplementationInstance: IdempotencyOptionsSetup })
                services.RemoveAt(index);
        }

        if (configure is not null)
            services.AddSingleton<IConfigureOptions<IdempotencyOptions>>(new IdempotencyOptionsSetup(configure));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<IdempotencyOptions>, IdempotencyOptionsValidator>());
        services.Replace(ServiceDescriptor.Transient(static provider => provider.GetRequiredService<IOptionsMonitor<IdempotencyOptions>>().CurrentValue));
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.TryAddSingleton<IIdempotencyScopeAccessor, HttpContextIdempotencyScopeAccessor>();
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
        where TBuilder : IEndpointConventionBuilder =>
        builder.WithIdempotency(new IdempotentAttribute());

    /// <summary>
    /// Same as <see cref="WithIdempotency{TBuilder}(TBuilder)"/>, with settings that override the profile and
    /// <see cref="IdempotencyOptions"/> for this endpoint or group (<see cref="IdempotencyEndpointOptions"/>). An
    /// endpoint's own settings win over its group's.
    /// </summary>
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder, Action<IdempotencyEndpointOptions> configure)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(configure);
        IdempotentAttribute attribute = new();
        configure(attribute.Overrides);
        return builder.WithIdempotency(attribute);
    }

    /// <summary>
    /// Same as <see cref="WithIdempotency{TBuilder}(TBuilder)"/>, with the settings of the profile registered under
    /// <paramref name="profile"/> by <see cref="IdempotencyOptions.AddProfile"/>.
    /// </summary>
    public static TBuilder WithIdempotency<TBuilder>(this TBuilder builder, string profile)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);
        return builder.WithIdempotency(new IdempotentAttribute { Profile = profile });
    }

    private static TBuilder WithIdempotency<TBuilder>(this TBuilder builder, IdempotentAttribute attribute)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(attribute);
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
