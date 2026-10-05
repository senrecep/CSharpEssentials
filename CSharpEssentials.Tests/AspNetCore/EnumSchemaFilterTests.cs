using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.OpenApi.Any;
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

    [StringEnum]
    private enum TestAcronym
    {
        HTTPStatus,
        IOError,
        Value1
    }

    [StringEnum]
    private enum TestCustomNamed
    {
        [JsonStringEnumMemberName("renamed")]
        Original,
        PlainValue
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

    [Fact]
    public void Apply_ForAcronymEnum_ShouldUseSameNamesAsJson()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema();
        JsonSerializerOptions jsonOptions = new() { Converters = { new ConditionalStringEnumConverter() } };

        filter.Apply(schema, CreateContext(typeof(TestAcronym)));

        string[] jsonNames = Enum.GetValues<TestAcronym>()
            .Select(value => JsonSerializer.Serialize(value, jsonOptions).Trim('"'))
            .ToArray();
        EnumValues(schema).Should().Equal(jsonNames);
        EnumValues(schema).Should().Equal("http_status", "io_error", "value1");
    }

    [Fact]
    public void Apply_ShouldHonorJsonStringEnumMemberName()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema();

        filter.Apply(schema, CreateContext(typeof(TestCustomNamed)));

        EnumValues(schema).Should().Equal("renamed", "plain_value");
        schema.Description.Should().Be("Possible values: renamed, plain_value");
    }

    [Fact]
    public void Apply_ForStringEnum_ShouldClearFormatAndReplaceExistingValues()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Enum = [new OpenApiInteger(0), new OpenApiInteger(1), new OpenApiInteger(2)]
        };

        filter.Apply(schema, CreateContext(typeof(TestString)));

        schema.Type.Should().Be("string");
        schema.Format.Should().BeNull();
        EnumValues(schema).Should().Equal("active", "inactive", "pending");
        schema.Enum.Should().AllBeOfType<OpenApiString>();
    }

    [Fact]
    public void Apply_ForNonStringEnum_ShouldLeaveSchemaUntouched()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Description = "original",
            Enum = [new OpenApiInteger(0), new OpenApiInteger(1)]
        };

        filter.Apply(schema, CreateContext(typeof(TestInt)));

        schema.Type.Should().Be("integer");
        schema.Format.Should().Be("int32");
        schema.Description.Should().Be("original");
        schema.Enum.Should().HaveCount(2).And.AllBeOfType<OpenApiInteger>();
    }

    [Fact]
    public void Apply_ForNonEnumType_ShouldLeaveSchemaUntouched()
    {
        var filter = new EnumSchemaFilter();
        var schema = new OpenApiSchema { Type = "string", Format = "uuid" };

        filter.Apply(schema, CreateContext(typeof(Guid)));

        schema.Type.Should().Be("string");
        schema.Format.Should().Be("uuid");
        schema.Enum.Should().BeNullOrEmpty();
    }

    private static IEnumerable<string> EnumValues(OpenApiSchema schema) =>
        schema.Enum.Cast<OpenApiString>().Select(value => value.Value);

    private static SchemaFilterContext CreateContext(Type type)
    {
        // Use reflection to instantiate SchemaFilterContext since its constructor may be complex
        // For this test we use a minimal approach: the filter only checks context.Type
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));

        return new SchemaFilterContext(type, schemaGenerator, schemaRepository, null, null);
    }
}
