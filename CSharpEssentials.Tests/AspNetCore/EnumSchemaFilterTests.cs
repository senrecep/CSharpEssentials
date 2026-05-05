using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public class EnumSchemaFilterTests
{
    [StringEnum]
    private enum TestString
    {
        Active,
        Inactive,
        Pending
    }

    private enum TestInt
    {
        One,
        Two
    }

    [Fact]
    public void Apply_ForStringEnum_ShouldSetSchemaTypeToString()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(TestString));

        filter.Apply(schema, context);

        schema.Type.Should().Be("string");
        schema.Enum.Should().HaveCount(3);
        schema.Description.Should().Contain("active, inactive, pending");
    }

    [Fact]
    public void Apply_ForIntEnum_ShouldNotModifySchema()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema { Type = "integer" };
        SchemaFilterContext context = CreateContext(typeof(TestInt));

        filter.Apply(schema, context);

        schema.Type.Should().Be("integer");
        schema.Enum.Should().BeNullOrEmpty();
    }

    private static SchemaFilterContext CreateContext(Type type)
    {
        // Use reflection to instantiate SchemaFilterContext since its constructor may be complex
        // For this test we use a minimal approach: the filter only checks context.Type
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));

        return new SchemaFilterContext(type, schemaGenerator, schemaRepository, null, null);
    }
}
