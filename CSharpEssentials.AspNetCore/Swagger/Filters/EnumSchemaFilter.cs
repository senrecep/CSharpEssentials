using CSharpEssentials.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

/// <summary>
/// Describes <see cref="CSharpEssentials.Enums.StringEnumAttribute"/> enums as string schemas whose values are
/// the names used on the wire (<see cref="StringEnumNaming"/>), for bodies as well as query and route parameters.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!StringEnumNaming.IsStringEnum(context.Type))
            return;
        IReadOnlyList<string> values = StringEnumNaming.GetNames(context.Type);

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
