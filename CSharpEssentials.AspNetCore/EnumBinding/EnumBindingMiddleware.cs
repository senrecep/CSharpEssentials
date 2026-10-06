using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Rewrites the enum route, query, header and form values of the selected endpoint to the C# member name, so the
/// framework binders (Minimal API and MVC) accept every value the enum conventions accept. A rejected value
/// short-circuits with a 400 problem response.
/// </summary>
internal sealed class EnumBindingMiddleware(RequestDelegate next, EnumConventionsRegistration registration)
{
    // Weak keys: endpoints from dynamic data sources (config reloads, hot reload) are replaced, not reused.
    private readonly ConditionalWeakTable<Endpoint, EnumBindingTarget[]> _plans = [];
    private readonly ConcurrentDictionary<Type, EnumBindingNormalizer?> _normalizers = new();

    public Task InvokeAsync(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint is null)
            return next(context);

        EnumBindingTarget[] targets = _plans.GetValue(endpoint, e => new EnumBindingPlanBuilder(GetNormalizer).Build(e));
        return targets.Length == 0 ? next(context) : BindAsync(context, targets);
    }

    private EnumBindingNormalizer? GetNormalizer(Type enumType) =>
        _normalizers.GetOrAdd(enumType, static (type, r) =>
            r.Resolve(type) is { } info ? EnumBindingNormalizer.Create(info, r.Conventions) : null, registration);

    private async Task BindAsync(HttpContext context, EnumBindingTarget[] targets)
    {
        HttpRequest request = context.Request;
        List<Error>? errors = null;
        Dictionary<string, StringValues>? query = null;
        Dictionary<string, StringValues>? form = null;
        HashSet<string>? claimed = null;
        IFormCollection? originalForm = request.HasFormContentType && Array.Exists(targets, static t => t.Source == EnumBindingSource.Form)
            ? await request.ReadFormAsync(context.RequestAborted)
            : null;

        foreach (EnumBindingTarget target in targets)
        {
            if (target.SkipWhenPrefixPresent is { } prefix && HasPrefix(request, originalForm, prefix))
                continue;

            switch (target.Source)
            {
                case EnumBindingSource.Route:
                    if (request.RouteValues.TryGetValue(target.Key, out object? routeValue) && routeValue is string text &&
                        Claim(target, ref claimed))
                    {
                        if (!TryNormalize(target, text, out StringValues normalized))
                            (errors ??= []).Add(CreateError(target, text));
                        else if (normalized.Count == 0)
                            request.RouteValues.Remove(target.Key);
                        else
                            request.RouteValues[target.Key] = normalized.ToString();
                    }
                    break;

                case EnumBindingSource.Query:
                    if (request.Query.TryGetValue(target.Key, out StringValues queryValues) &&
                        Claim(target, ref claimed) &&
                        Normalize(target, queryValues, ref errors) is { } queryResult)
                    {
                        query ??= request.Query.ToDictionary(static p => p.Key, static p => p.Value, StringComparer.OrdinalIgnoreCase);
                        Set(query, target.Key, queryResult);
                    }
                    break;

                case EnumBindingSource.Header:
                    if (request.Headers.TryGetValue(target.Key, out StringValues headerValues) &&
                        Normalize(target, headerValues, ref errors) is { } headerResult)
                    {
                        if (headerResult.Count == 0)
                            request.Headers.Remove(target.Key);
                        else
                            request.Headers[target.Key] = headerResult;
                    }
                    break;

                case EnumBindingSource.Form:
                    if (originalForm is not null && originalForm.TryGetValue(target.Key, out StringValues formValues) &&
                        Claim(target, ref claimed) &&
                        Normalize(target, formValues, ref errors) is { } formResult)
                    {
                        form ??= originalForm.ToDictionary(static p => p.Key, static p => p.Value, StringComparer.OrdinalIgnoreCase);
                        Set(form, target.Key, formResult);
                    }
                    break;

                default:
                    break;
            }
        }

        if (errors is not null)
        {
            await errors.ToArray().ToProblemResult(statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
            return;
        }

        if (query is not null)
            request.Query = new QueryCollection(query);
        if (form is not null)
            request.Form = new FormCollection(form, originalForm!.Files);

        await next(context);
    }

    /// <summary>
    /// The normalized values of one key, or <see langword="null"/> when the values are unchanged or after adding the error
    /// of a rejected value, so the request collections are only copied when a value actually changes.
    /// </summary>
    private StringValues? Normalize(EnumBindingTarget target, StringValues values, ref List<Error>? errors)
    {
        List<string> normalized = [];
        foreach (string? value in values)
        {
            if (!TryNormalize(target, value, out StringValues parts))
            {
                (errors ??= []).Add(CreateError(target, value));
                return null;
            }
            foreach (string? part in parts)
                normalized.Add(part!);
        }
        StringValues result = new([.. normalized]);
        return result.Equals(values) ? null : (StringValues?)result;
    }

    /// <summary>
    /// Normalizes one value. An empty value of a nullable or collection target yields no value (the framework binds null
    /// or skips the item).
    /// </summary>
    private static bool TryNormalize(EnumBindingTarget target, string? value, out StringValues normalized)
    {
        normalized = StringValues.Empty;
        if (string.IsNullOrEmpty(value))
            return target.AllowEmpty;

        List<string> output = [];
        if (!target.Normalizer.TryNormalize(value, target.IsCollection, output))
            return false;
        normalized = new StringValues([.. output]);
        return true;
    }

    private static bool Claim(EnumBindingTarget target, ref HashSet<string>? claimed) =>
        !target.FirstSourceWins || (claimed ??= [with(StringComparer.OrdinalIgnoreCase)]).Add(target.Key);

    private static void Set(Dictionary<string, StringValues> values, string key, StringValues normalized)
    {
        if (normalized.Count == 0)
            values.Remove(key);
        else
            values[key] = normalized;
    }

    private static bool HasPrefix(HttpRequest request, IFormCollection? form, string prefix)
    {
        foreach (string key in request.Query.Keys)
            if (IsUnderPrefix(key, prefix))
                return true;
        foreach (string key in request.RouteValues.Keys)
            if (IsUnderPrefix(key, prefix))
                return true;
        if (form is not null)
            foreach (string key in form.Keys)
                if (IsUnderPrefix(key, prefix))
                    return true;
        return false;
    }

    private static bool IsUnderPrefix(string key, string prefix) =>
        key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
        (key.Length == prefix.Length || key[prefix.Length] is '.' or '[');

    private Error CreateError(EnumBindingTarget target, string? value)
    {
        EnumValueError error = target.Normalizer.CreateError(value, target.Key);
        return registration.CreateError(error, target.Key);
    }
}
