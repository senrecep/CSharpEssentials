using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Finds the validators of a value: the <see cref="IETagSource{T}"/> registered for its runtime type or the nearest base type
/// (the last registration of a type wins), otherwise <see cref="IETagGenerator"/>. Both are resolved from the request services.
/// </summary>
internal sealed class ResourceValidatorsResolver
{
    private readonly Dictionary<Type, ETagSourceRegistration> _sources = [];
    private readonly ConcurrentDictionary<Type, ETagSourceRegistration?> _byRuntimeType = new();
    private readonly Func<Type, ETagSourceRegistration?> _find;

    public ResourceValidatorsResolver(IEnumerable<ETagSourceRegistration> registrations)
    {
        foreach (ETagSourceRegistration registration in registrations)
            _sources[registration.Type] = registration;
        _find = Find;
    }

    public ResourceValidators? Resolve(IServiceProvider services, object value)
    {
        ResourceValidators? validators = _byRuntimeType.GetOrAdd(value.GetType(), _find) is { } source
            ? source.GetValidators(services, value)
            : services.GetRequiredService<IETagGenerator>().GetValidators(value);
        return validators is { ETag: null, LastModified: null } ? null : validators;
    }

    public static ResourceValidatorsResolver GetRequired(IServiceProvider services, string caller) =>
        services.GetService<ResourceValidatorsResolver>()
        ?? throw new InvalidOperationException(
            $"{caller} requires the conditional request services. Call services.AddConditionalRequests() when registering services.");

    private ETagSourceRegistration? Find(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            if (_sources.TryGetValue(current, out ETagSourceRegistration? registration))
                return registration;
        }
        return null;
    }
}
