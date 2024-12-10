
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSharpEssentials.AspNetCore;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;
        Error[] errors = [.. context.ModelState
            .Where(arg => arg.Value != null)
            .SelectMany(state => state.Value!.Errors.Select(x => Error.Validation($"validation.{state.Key}", x.ErrorMessage)))];
        context.Result = errors.ToActionResult(context.HttpContext);
    }
}