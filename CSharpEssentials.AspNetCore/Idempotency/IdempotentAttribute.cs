using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Opts an MVC action or controller into <c>Idempotency-Key</c> handling by <c>UseIdempotency()</c>. On MVC it is also
/// the action filter that sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/> right before the action runs
/// (after model validation), and it adds <see cref="IdempotencyFilterInstalledMetadata"/> to the action's endpoint.
/// Minimal API endpoints and groups use <c>.WithIdempotency()</c>, which adds this attribute as metadata together with
/// its own endpoint filter.
/// <para>
/// Its named arguments override <see cref="IdempotencyOptions"/> for the action or controller, with the same
/// precedence as <see cref="IdempotencyEndpointOptions"/>: the action, then the controller, then <see cref="Profile"/>,
/// then the global options. An argument that is not given falls back; reading it returns its type's default.
/// <see cref="IdempotencyOptions.ShouldStore"/> cannot be an attribute argument; select a profile that sets it.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class IdempotentAttribute : Attribute, IAsyncActionFilter, IOrderedFilter, IControllerModelConvention, IActionModelConvention
{
    internal IdempotencyEndpointOptions Overrides { get; } = new();

    /// <summary>The profile whose settings apply where this attribute gives none (<see cref="IdempotencyEndpointOptions.Profile"/>).</summary>
    public string? Profile
    {
        get => Overrides.Profile;
        set => Overrides.Profile = value;
    }

    /// <summary>Overrides <see cref="IdempotencyOptions.RetentionPeriod"/>, in seconds.</summary>
    public int RetentionSeconds
    {
        get => ToSeconds(Overrides.RetentionPeriod);
        set => Overrides.RetentionPeriod = TimeSpan.FromSeconds(value);
    }

    /// <summary>Overrides <see cref="IdempotencyOptions.InFlightTimeout"/> (the lease), in seconds.</summary>
    public int LeaseSeconds
    {
        get => ToSeconds(Overrides.InFlightTimeout);
        set => Overrides.InFlightTimeout = TimeSpan.FromSeconds(value);
    }

    /// <summary>Overrides <see cref="IdempotencyOptions.ServerErrorRetention"/>, in seconds.</summary>
    public int ServerErrorRetentionSeconds
    {
        get => ToSeconds(Overrides.ServerErrorRetention);
        set => Overrides.ServerErrorRetention = TimeSpan.FromSeconds(value);
    }

    /// <summary>Overrides <see cref="IdempotencyOptions.RequireKey"/>.</summary>
    public bool RequireKey
    {
        get => Overrides.RequireKey.GetValueOrDefault();
        set => Overrides.RequireKey = value;
    }

    /// <summary>Overrides <see cref="IdempotencyOptions.UnscopedKeyBehavior"/>.</summary>
    public UnscopedKeyBehavior UnscopedKeyBehavior
    {
        get => Overrides.UnscopedKeyBehavior.GetValueOrDefault();
        set => Overrides.UnscopedKeyBehavior = value;
    }

    /// <summary>Service key of a keyed store used for this action (<see cref="IdempotencyEndpointOptions.StoreKey"/>).</summary>
    public string? StoreKey
    {
        get => Overrides.StoreKey;
        set => Overrides.StoreKey = value;
    }

    int IOrderedFilter.Order => int.MaxValue;

    Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IdempotencyExecutionFeature.MarkStarted(context.HttpContext);
        return next();
    }

    void IControllerModelConvention.Apply(ControllerModel controller)
    {
        foreach (ActionModel action in controller.Actions)
            AddInstalledMetadata(action);
    }

    void IActionModelConvention.Apply(ActionModel action) => AddInstalledMetadata(action);

    private static int ToSeconds(TimeSpan? value) => value is { } seconds ? (int)seconds.TotalSeconds : 0;

    private static void AddInstalledMetadata(ActionModel action)
    {
        foreach (SelectorModel selector in action.Selectors)
        {
            if (!selector.EndpointMetadata.Contains(IdempotencyFilterInstalledMetadata.Instance))
                selector.EndpointMetadata.Add(IdempotencyFilterInstalledMetadata.Instance);
        }
    }
}
