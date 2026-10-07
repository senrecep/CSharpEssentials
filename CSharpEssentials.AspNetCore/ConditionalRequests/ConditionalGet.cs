using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The conditional GET logic shared by <see cref="ConditionalGetEndpointFilter"/> and <see cref="ConditionalGetResultFilter"/>.
/// </summary>
internal static class ConditionalGet
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> ValueAccessors = new();

    public static bool IsGetOrHead(string method) => HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

    /// <summary>
    /// The resource of a returned value: the value of a successful <c>Result&lt;T&gt;</c>, otherwise the value itself;
    /// <see langword="null"/> (pass through) for a failed or valueless result, strings and problem details.
    /// </summary>
    public static object? GetResource(object value)
    {
        object? resource = value;
        if (value is IResultBase result)
        {
            if (!result.IsSuccess)
                return null;
            resource = ValueAccessors.GetOrAdd(value.GetType(), GetValueProperty) is { } property ? property.GetValue(value) : null;
        }
        return resource is string or ProblemDetails ? null : resource;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Result<>))]
    private static PropertyInfo? GetValueProperty(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>)
            ? type.GetProperty(nameof(Result<>.Value))
            : null;

    /// <summary>
    /// Sets <c>ETag</c> and <c>Last-Modified</c> from the validators of <paramref name="resource"/> and evaluates
    /// <c>If-None-Match</c>, then <c>If-Modified-Since</c> (RFC 9110 section 13.2.2).
    /// </summary>
    /// <returns><see langword="true"/> when the response is <c>304 Not Modified</c>.</returns>
    public static bool Apply(HttpContext httpContext, object resource, string caller)
    {
        IHeaderDictionary headers = httpContext.Response.Headers;
        if (headers.ETag.Count > 0)
            return false;
        IServiceProvider services = httpContext.RequestServices;
        if (ResourceValidatorsResolver.GetRequired(services, caller).Resolve(services, resource) is not { } validators)
            return false;

        DateTimeOffset? lastModified = validators.LastModified is { } value ? ToHttpDate(value, services) : null;
        if (validators.ETag is { } etag)
            headers.ETag = etag.ToString();
        if (lastModified is { } date)
            headers.LastModified = HeaderUtilities.FormatDate(date);

        return IsNotModified(httpContext.Request, validators.ETag, lastModified);
    }

    // Second precision (HTTP dates), and never in the future (RFC 9110 section 8.8.2.1).
    private static DateTimeOffset ToHttpDate(DateTimeOffset value, IServiceProvider services)
    {
        DateTimeOffset now = (services.GetService<TimeProvider>() ?? TimeProvider.System).GetUtcNow();
        long ticks = Math.Min(value.UtcTicks, now.UtcTicks);
        return new DateTimeOffset(ticks - ticks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
    }

    private static bool IsNotModified(HttpRequest request, EntityTagHeaderValue? etag, DateTimeOffset? lastModified)
    {
        bool hasIfNoneMatch = request.Headers.IfNoneMatch.Count > 0;
        if (!hasIfNoneMatch && (lastModified is null || request.Headers.IfModifiedSince.Count == 0))
            return false;

        RequestHeaders typed = request.GetTypedHeaders();
        if (hasIfNoneMatch)
        {
            foreach (EntityTagHeaderValue tag in typed.IfNoneMatch)
            {
                if (tag.Equals(EntityTagHeaderValue.Any) || etag is not null && tag.Compare(etag, useStrongComparison: false))
                    return true;
            }
            return false;
        }

        return typed.IfModifiedSince is { } since && lastModified <= since;
    }
}
