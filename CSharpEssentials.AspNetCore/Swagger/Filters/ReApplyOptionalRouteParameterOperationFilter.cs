using System.Text.RegularExpressions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
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

        Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute? httpMethodWithOptional = httpMethodAttributes?.FirstOrDefault(m => m.Template?.Contains('?') ?? false);
        if (httpMethodWithOptional?.Template == null)
            return;

        MatchCollection matches = RouteRegex().Matches(httpMethodWithOptional.Template);

        foreach (Match match in matches.Cast<Match>())
        {
            string name = match.Groups[_captureName].Value;

            OpenApiParameter? parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);
            if (parameter == null)
                continue;
            parameter.AllowEmptyValue = true;
            parameter.Required = false;
            parameter.Schema.Default = new OpenApiString(null);
            parameter.Schema.Nullable = true;
        }
    }

    [GeneratedRegex(@"{(?<routeParameter>\w+)\?}")]
    private static partial Regex RouteRegex();
}