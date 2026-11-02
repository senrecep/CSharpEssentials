
using System.Globalization;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

public class SwashbuckleSchemaIdFactory
{
    private readonly Dictionary<string, List<string>> _schemaNameRepetition = [];
#if NET9_0_OR_GREATER
    private readonly Lock _gate = new();
#else
    private readonly object _gate = new();
#endif

    /// <summary>
    /// Builds every id from the namespace-qualified type name (<c>NsB.Order</c>, generic arguments in front), so the
    /// ids do not depend on the order in which types are first seen. On by default. Set it to <see langword="false"/>
    /// to get the 7.x short names (<c>Order</c>, <c>DtoBox</c>), where two types that share a name get a numeric
    /// suffix in the order they are first seen.
    /// </summary>
    public bool QualifyWithNamespace { get; init; } = true;

    private string DefaultSchemaIdSelector(Type modelType)
    {
        if (QualifyWithNamespace && modelType.IsArray)
            return DefaultSchemaIdSelector(modelType.GetElementType()!) + "Array";

        if (!modelType.IsConstructedGenericType)
            return TypeName(modelType).Replace("[]", "Array");

        string prefix = modelType.GetGenericArguments()
            .Select(DefaultSchemaIdSelector)
            .Aggregate((previous, current) => previous + current);

        return prefix + TypeName(modelType.GetGenericTypeDefinition()).Split('`')[0];
    }

    private string TypeName(Type type) =>
        QualifyWithNamespace && type.FullName is { } fullName ? fullName.Replace('+', '.') : type.Name;

    public string GetSchemaId(Type modelType)
    {
        string id = DefaultSchemaIdSelector(modelType);

        string fullName = modelType.FullName ?? string.Empty;
        int index;
        lock (_gate)
        {
            if (!_schemaNameRepetition.TryGetValue(id, out List<string>? modelNameList))
            {
                modelNameList = [];
                _schemaNameRepetition.Add(id, modelNameList);
            }

            if (!string.IsNullOrEmpty(fullName) && !modelNameList.Contains(fullName))
                modelNameList.Add(fullName);

            index = modelNameList.IndexOf(fullName);
        }

        return $"{id}{(index >= 1 ? index.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
    }
}
