
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

    private string DefaultSchemaIdSelector(Type modelType)
    {
        if (!modelType.IsConstructedGenericType)
            return modelType.Name.Replace("[]", "Array");

        string prefix = modelType.GetGenericArguments()
            .Select(DefaultSchemaIdSelector)
            .Aggregate((previous, current) => previous + current);

        return prefix + modelType.Name.Split('`')[0];
    }

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
