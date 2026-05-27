using CSharpEssentials.AspNetCore.Swagger.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CSharpEssentials.Tests.AspNetCore;

public class ReApplyOptionalRouteParameterOperationFilterTests
{
    private static OperationFilterContext CreateContext(MethodInfo methodInfo)
    {
        var apiDescription = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription();
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));
        return new OperationFilterContext(apiDescription, schemaGenerator, schemaRepository, methodInfo);
    }

    [HttpGet("{id?}")]
    private static void MethodWithOptionalRoute(int id) { /* Test fixture — route attribute detection target. */ }

    [HttpGet("{id}")]
    private static void MethodWithRequiredRoute(int id) { /* Test fixture — route attribute detection target. */ }

    private static void MethodWithNoRoute() { /* Test fixture — route attribute detection target. */ }

    [Fact]
    public void Apply_Should_MarkParameterAsOptional_When_RouteHasOptionalSegment()
    {
        var filter = new ReApplyOptionalRouteParameterOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Schema = new OpenApiSchema()
                }
            ]
        };
        MethodInfo methodInfo = typeof(ReApplyOptionalRouteParameterOperationFilterTests)
            .GetMethod(nameof(MethodWithOptionalRoute), BindingFlags.NonPublic | BindingFlags.Static)!;
        OperationFilterContext context = CreateContext(methodInfo);

        filter.Apply(operation, context);

        OpenApiParameter param = operation.Parameters[0];
        param.Required.Should().BeFalse();
        param.AllowEmptyValue.Should().BeTrue();
        param.Schema.Nullable.Should().BeTrue();
    }

    [Fact]
    public void Apply_Should_NotModifyParameters_When_RouteHasNoOptionalSegment()
    {
        var filter = new ReApplyOptionalRouteParameterOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Schema = new OpenApiSchema()
                }
            ]
        };
        MethodInfo methodInfo = typeof(ReApplyOptionalRouteParameterOperationFilterTests)
            .GetMethod(nameof(MethodWithRequiredRoute), BindingFlags.NonPublic | BindingFlags.Static)!;
        OperationFilterContext context = CreateContext(methodInfo);

        filter.Apply(operation, context);

        operation.Parameters[0].Required.Should().BeTrue();
    }

    [Fact]
    public void Apply_Should_DoNothing_When_MethodHasNoHttpMethodAttribute()
    {
        var filter = new ReApplyOptionalRouteParameterOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters =
            [
                new OpenApiParameter
                {
                    Name = "id",
                    In = ParameterLocation.Path,
                    Required = true,
                    Schema = new OpenApiSchema()
                }
            ]
        };
        MethodInfo methodInfo = typeof(ReApplyOptionalRouteParameterOperationFilterTests)
            .GetMethod(nameof(MethodWithNoRoute), BindingFlags.NonPublic | BindingFlags.Static)!;
        OperationFilterContext context = CreateContext(methodInfo);

        filter.Apply(operation, context);

        operation.Parameters[0].Required.Should().BeTrue();
    }

    [Fact]
    public void Apply_Should_DoNothing_When_OptionalParamNotInOperationParameters()
    {
        var filter = new ReApplyOptionalRouteParameterOperationFilter();
        var operation = new OpenApiOperation
        {
            Parameters = []
        };
        MethodInfo methodInfo = typeof(ReApplyOptionalRouteParameterOperationFilterTests)
            .GetMethod(nameof(MethodWithOptionalRoute), BindingFlags.NonPublic | BindingFlags.Static)!;
        OperationFilterContext context = CreateContext(methodInfo);

        Action act = () => filter.Apply(operation, context);

        act.Should().NotThrow();
        operation.Parameters.Should().BeEmpty();
    }
}
