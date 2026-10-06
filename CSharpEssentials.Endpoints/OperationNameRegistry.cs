using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints;

internal sealed class OperationNameRegistry
{
    private const char Separator = '_';

    private const string AnyMethod = "Any";

    private static readonly ConditionalWeakTable<IServiceProvider, OperationNameRegistry> Registries = [];

#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif

    private readonly List<Type> _types = [];

    private readonly Dictionary<string, Type> _typesByFullName = [with(StringComparer.Ordinal)];

    private readonly Dictionary<Type, string> _baseNames = [];

    private readonly Dictionary<Type, List<OperationNameSlot>> _slotsByType = [];

    private readonly Dictionary<string, OperationNameSlot> _slotsByKey = [with(StringComparer.Ordinal)];

    private readonly HashSet<string> _explicitNames = [with(StringComparer.Ordinal)];

    private int _mappings;

    private bool _resolved;

    public static OperationNameRegistry For(IServiceProvider services) =>
        Registries.GetValue(services, static _ => new OperationNameRegistry());

    public int Register(Type endpointType)
    {
        string fullName = endpointType.FullName ?? endpointType.Name;
        lock (_gate)
        {
            if (_typesByFullName.TryGetValue(fullName, out Type? existing))
            {
                if (existing != endpointType)
                {
                    throw new InvalidOperationException(
                        $"Endpoint types '{existing.AssemblyQualifiedName}' and '{endpointType.AssemblyQualifiedName}' have the same full name, " +
                        "so OperationNaming.TypeName cannot give them distinct operation names. Rename one of them or use OperationNaming.Custom(...).");
                }
            }
            else
            {
                _typesByFullName.Add(fullName, endpointType);
                _types.Add(endpointType);
                _baseNames.Add(endpointType, GetBaseName(endpointType));
                _slotsByType.Add(endpointType, []);
                _resolved = false;
            }

            return _mappings++;
        }
    }

    public void Apply(EndpointBuilder endpoint, Type endpointType, int mapping)
    {
        IEndpointNameMetadata? existing = endpoint.Metadata.OfType<IEndpointNameMetadata>().LastOrDefault();
        if (existing is not null)
        {
            if (existing is not OperationNameMetadata)
            {
                lock (_gate)
                {
                    if (_explicitNames.Add(existing.EndpointName))
                    {
                        _resolved = false;
                    }
                }
            }

            return;
        }

        string methodSuffix = GetMethodSuffix(endpoint.Metadata.OfType<IHttpMethodMetadata>().LastOrDefault()?.HttpMethods);
        string pattern = (endpoint as RouteEndpointBuilder)?.RoutePattern.RawText ?? string.Empty;
        string key = string.Create(CultureInfo.InvariantCulture, $"{mapping}|{methodSuffix}|{pattern}");
        OperationNameSlot? slot;
        lock (_gate)
        {
            if (!_slotsByKey.TryGetValue(key, out slot))
            {
                slot = new OperationNameSlot(endpointType, methodSuffix);
                _slotsByKey.Add(key, slot);
                _slotsByType[endpointType].Add(slot);
                _resolved = false;
            }
        }

        endpoint.Metadata.Add(new OperationNameMetadata(this, slot));
    }

    public void Reserve(IReadOnlyList<string> names)
    {
        lock (_gate)
        {
            foreach (string name in names)
            {
                if (_explicitNames.Add(name))
                {
                    _resolved = false;
                }
            }
        }
    }

    public string Resolve(OperationNameSlot slot)
    {
        lock (_gate)
        {
            if (!_resolved)
            {
                ResolveAll();
                _resolved = true;
            }

            return slot.Name;
        }
    }

    private void ResolveAll()
    {
        Dictionary<string, int> baseNameCounts = [with(StringComparer.Ordinal)];
        foreach (string baseName in _baseNames.Values)
        {
            baseNameCounts[baseName] = baseNameCounts.GetValueOrDefault(baseName) + 1;
        }

        HashSet<string> used = [with(StringComparer.Ordinal), .. _explicitNames];
        foreach (Type type in _types)
        {
            string baseName = _baseNames[type];
            string typeName = baseNameCounts[baseName] > 1 ? GetQualifiedName(type, baseName) : baseName;
            List<OperationNameSlot> slots = _slotsByType[type];
            Dictionary<string, int> suffixCounts = [with(StringComparer.Ordinal)];
            foreach (OperationNameSlot slot in slots)
            {
                suffixCounts[slot.MethodSuffix] = suffixCounts.GetValueOrDefault(slot.MethodSuffix) + 1;
            }

            Dictionary<string, int> suffixIndexes = [with(StringComparer.Ordinal)];
            foreach (OperationNameSlot slot in slots)
            {
                string candidate = typeName;
                if (slots.Count > 1)
                {
                    candidate = typeName + Separator + slot.MethodSuffix;
                    if (suffixCounts[slot.MethodSuffix] > 1)
                    {
                        int index = suffixIndexes.GetValueOrDefault(slot.MethodSuffix) + 1;
                        suffixIndexes[slot.MethodSuffix] = index;
                        candidate = string.Create(CultureInfo.InvariantCulture, $"{candidate}{Separator}{index}");
                    }
                }

                string name = candidate;
                int attempt = 1;
                while (!used.Add(name))
                {
                    attempt++;
                    name = string.Create(CultureInfo.InvariantCulture, $"{candidate}{Separator}{attempt}");
                }

                slot.Name = name;
            }
        }
    }

    private static string GetBaseName(Type endpointType)
    {
        List<string> parts = [];
        for (Type? current = endpointType; current is not null; current = current.DeclaringType)
        {
            parts.Add(Sanitize(StripArity(current.Name)));
        }

        parts.Reverse();
        return string.Join(Separator, parts);
    }

    private static string GetQualifiedName(Type endpointType, string baseName) =>
        string.IsNullOrEmpty(endpointType.Namespace) ? baseName : Sanitize(endpointType.Namespace) + Separator + baseName;

    private static string GetMethodSuffix(IReadOnlyList<string>? methods)
    {
        if (methods is null || methods.Count == 0)
        {
            return AnyMethod;
        }

        StringBuilder builder = new();
        foreach (string method in methods)
        {
            if (builder.Length > 0)
            {
                builder.Append(Separator);
            }

            for (int index = 0; index < method.Length; index++)
            {
                char character = index == 0 ? char.ToUpperInvariant(method[index]) : char.ToLowerInvariant(method[index]);
                builder.Append(char.IsAsciiLetterOrDigit(character) ? character : Separator);
            }
        }

        return builder.ToString();
    }

    private static string StripArity(string name)
    {
        int tick = name.IndexOf('`', StringComparison.Ordinal);
        return tick < 0 ? name : name[..tick];
    }

    private static string Sanitize(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            builder.Append(char.IsAsciiLetterOrDigit(character) ? character : Separator);
        }

        return builder.ToString();
    }
}
