using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore.Swagger.Filters;

public sealed partial class ReApplyOptionalRouteParameterOperationFilter : IOperationFilter
{
    private const string _captureName = "routeParameter";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute> httpMethodAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>();

        Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute? httpMethodWithOptional = httpMethodAttributes.FirstOrDefault(m => m.Template?.Contains('?') ?? false);
        if (httpMethodWithOptional?.Template == null || operation.Parameters is null)
            return;

        MatchCollection matches = RouteRegex().Matches(httpMethodWithOptional.Template);

        foreach (Match match in matches.Cast<Match>())
        {
            string name = match.Groups[_captureName].Value;

            if (operation.Parameters.OfType<OpenApiParameter>().FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name) is not { } parameter)
                continue;
            parameter.AllowEmptyValue = true;
            parameter.Required = false;
            parameter.Schema = MakeNullable(parameter.Schema);
        }
    }

    // A $ref cannot carry siblings in OpenAPI 3.0, so a referenced schema gets an allOf wrapper.
    private static OpenApiSchema MakeNullable(IOpenApiSchema? schema)
    {
        OpenApiSchema nullable = schema switch
        {
            OpenApiSchema inline => inline,
            null => new OpenApiSchema(),
            _ => new OpenApiSchema { AllOf = [schema] },
        };
        nullable.Type = (nullable.Type ?? JsonSchemaType.Null) | JsonSchemaType.Null;
        nullable.Default = JsonNullSentinel.JsonNull;
        return nullable;
    }

    [GeneratedRegex(@"{(?<routeParameter>\w+)\?}")]
    private static partial Regex RouteRegex();
}
