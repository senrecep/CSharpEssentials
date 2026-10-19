using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Opts an MVC action or controller into <c>Idempotency-Key</c> handling by <c>UseIdempotency()</c>. On MVC it is also
/// the action filter that sets <see cref="IIdempotencyExecutionFeature.HandlerStarted"/> right before the action runs
/// (after model validation), and it adds <see cref="IdempotencyFilterInstalledMetadata"/> to the action's endpoint.
/// Minimal API endpoints and groups use <c>.WithIdempotency()</c>, which adds this attribute as metadata together with
/// its own endpoint filter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class IdempotentAttribute : Attribute, IAsyncActionFilter, IOrderedFilter, IControllerModelConvention, IActionModelConvention
{
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

    private static void AddInstalledMetadata(ActionModel action)
    {
        foreach (SelectorModel selector in action.Selectors)
        {
            if (!selector.EndpointMetadata.Contains(IdempotencyFilterInstalledMetadata.Instance))
                selector.EndpointMetadata.Add(IdempotencyFilterInstalledMetadata.Instance);
        }
    }
}
