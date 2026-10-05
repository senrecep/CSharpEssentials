using System.Runtime.CompilerServices;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Rewrites enum query and route values of the selected endpoint to the C# member name, so the framework binders
/// (Minimal API and MVC) accept every spelling <see cref="StringEnumNaming"/> accepts. Invalid values short-circuit
/// with a 400 problem response.
/// </summary>
internal sealed class EnumBindingMiddleware(RequestDelegate next, IOptions<EnumBindingOptions> options)
{
    private readonly EnumBindingOptions _options = options.Value;
    // Weak keys: endpoints from dynamic data sources (config reloads, hot reload) are replaced, not reused.
    private readonly ConditionalWeakTable<Endpoint, EnumBindingTarget[]> _plans = [];

    public Task InvokeAsync(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint is null)
            return next(context);

        EnumBindingTarget[] targets = _plans.GetValue(endpoint, e => new EnumBindingPlanBuilder(_options.CanBind).Build(e));
        if (targets.Length == 0)
            return next(context);

        List<Error>? errors = null;
        Dictionary<string, StringValues>? query = null;
        JsonNamingPolicy policy = _options.NamingPolicy ?? StringEnumNaming.DefaultPolicy;

        foreach (EnumBindingTarget target in targets)
        {
            if (target.SkipWhenPrefixPresent is { } prefix && HasPrefix(context.Request, prefix))
                continue;

            if (target.Source == EnumBindingSource.Route)
            {
                if (context.Request.RouteValues.TryGetValue(target.Key, out object? routeValue) && routeValue is string text)
                {
                    if (!TryNormalize(target, text, policy, out string? normalized))
                        (errors ??= []).Add(CreateError(target, policy));
                    else if (normalized is null)
                        context.Request.RouteValues.Remove(target.Key);
                    else
                        context.Request.RouteValues[target.Key] = normalized;
                }
                continue;
            }

            if (!context.Request.Query.TryGetValue(target.Key, out StringValues values))
                continue;

            List<string> normalizedValues = [];
            bool valid = true;
            foreach (string? value in values)
            {
                valid = TryNormalize(target, value, policy, out string? normalized);
                if (!valid)
                    break;
                if (normalized is not null)
                    normalizedValues.Add(normalized);
            }

            if (!valid)
            {
                (errors ??= []).Add(CreateError(target, policy));
                continue;
            }

            query ??= context.Request.Query.ToDictionary(static p => p.Key, static p => p.Value, StringComparer.OrdinalIgnoreCase);
            if (normalizedValues.Count == 0)
                query.Remove(target.Key);
            else
                query[target.Key] = new StringValues([.. normalizedValues]);
        }

        if (errors is not null)
            return errors.ToArray().ToProblemResult(statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);

        if (query is not null)
            context.Request.Query = new QueryCollection(query);

        return next(context);
    }

    /// <summary>
    /// Normalizes one value to the C# member name. A blank value of a nullable or collection target yields
    /// <see langword="null"/> (the value is dropped, so the framework binds null or skips the item).
    /// </summary>
    private bool TryNormalize(EnumBindingTarget target, string? value, JsonNamingPolicy policy, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value))
            return target.AllowEmpty;

        if (!StringEnumNaming.TryParse(target.EnumType, value, out object? parsed, policy, _options.AllowIntegerValues))
            return false;

        normalized = parsed!.ToString();
        return true;
    }

    private static bool HasPrefix(HttpRequest request, string prefix)
    {
        foreach (string key in request.Query.Keys)
            if (IsUnderPrefix(key, prefix))
                return true;
        foreach (string key in request.RouteValues.Keys)
            if (IsUnderPrefix(key, prefix))
                return true;
        return false;
    }

    private static bool IsUnderPrefix(string key, string prefix) =>
        key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
        (key.Length == prefix.Length || key[prefix.Length] is '.' or '[');

    private static Error CreateError(EnumBindingTarget target, JsonNamingPolicy policy) =>
        Error.Validation(
            code: target.Key,
            description: $"'{target.Key}' must be one of: {string.Join(", ", StringEnumNaming.GetNames(target.EnumType, policy))}.");
}
