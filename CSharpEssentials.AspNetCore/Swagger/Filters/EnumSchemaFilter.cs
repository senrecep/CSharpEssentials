using CSharpEssentials.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Describes <see cref="CSharpEssentials.Enums.StringEnumAttribute"/> enums as string schemas whose values are
/// the names used on the wire (<see cref="EnumMetadata"/>), for bodies as well as query and route parameters.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!EnumMetadata.TryGet(context.Type, out IEnumInfo? info))
            return;
        IReadOnlyList<string> values = info.WireNames;

        var enumValues = values
            .Select(name => new OpenApiString(name))
            .Cast<IOpenApiAny>()
            .ToList();

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = enumValues;
        schema.Description = $"Possible values: {string.Join(", ", values)}";
    }
}
