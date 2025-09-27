using System.Reflection;
using CSharpEssentials.Core;
using CSharpEssentials.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        bool isStringEnum = context.Type.IsEnum && context.Type.GetCustomAttribute<StringEnumAttribute>() != null;
        if (isStringEnum.IsFalse())
            return;
        string[] values = [.. Enum.GetNames(context.Type).Select(x => x.ToSnakeCase())];

        var enumValues = values
            .Select(name => new OpenApiString(name))
            .Cast<IOpenApiAny>()
            .ToList();

        schema.Type = "string";
        schema.Enum = enumValues;
        schema.Description = $"Possible values: {string.Join(", ", values)}";
    }
}