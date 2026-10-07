using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public class EnumSchemaFilterTests
{
    private static readonly IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

    [StringEnum]
    internal enum TestString
    {
        Active,
        Inactive,
        Pending
    }

    [StringEnum]
    internal enum TestAcronym
    {
        HTTPStatus,
        IOError,
        Value1
    }

    [StringEnum]
    internal enum TestCustomNamed
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
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema();
        SchemaFilterContext context = CreateContext(typeof(TestString));

        filter.Apply(schema, context);

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Enum.Should().HaveCount(3);
        schema.Description.Should().Contain("| `active` | 0 |").And.Contain("| `pending` | 2 |");
        Strings(schema, "x-enum-varnames").Should().Equal("Active", "Inactive", "Pending");
        Nodes(schema, "x-enum-numeric-values").Select(value => value!.GetValue<long>()).Should().Equal(0L, 1L, 2L);
    }

    [Fact]
    public void Apply_Twice_ShouldKeepOneTableAndTheUserDescription()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema { Description = "Account state." };

        filter.Apply(schema, CreateContext(typeof(TestString)));
        filter.Apply(schema, CreateContext(typeof(TestString)));

        schema.Description.Should().StartWith("Account state.\n\n| value | number | description |");
        schema.Description.Split("| value | number | description |").Should().HaveCount(2);
    }

    [Fact]
    public void Apply_ForIntEnum_ShouldNotModifySchema()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema { Type = JsonSchemaType.Integer };
        SchemaFilterContext context = CreateContext(typeof(TestInt));

        filter.Apply(schema, context);

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Enum.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Apply_ForAcronymEnum_ShouldUseSameNamesAsJson()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema();
        JsonSerializerOptions jsonOptions = new() { Converters = { new EnumConverterFactory(EnumConventions.Default) } };

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
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema();

        filter.Apply(schema, CreateContext(typeof(TestCustomNamed)));

        EnumValues(schema).Should().Equal("renamed", "plain_value");
        schema.Description.Should().Be(
            "| value | number | description |\n|---|---|---|\n| `renamed` | 0 |  |\n| `plain_value` | 1 |  |");
    }

    [Fact]
    public void Apply_ForStringEnum_ShouldClearFormatAndReplaceExistingValues()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Enum = [JsonValue.Create(0), JsonValue.Create(1), JsonValue.Create(2)]
        };

        filter.Apply(schema, CreateContext(typeof(TestString)));

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().BeNull();
        EnumValues(schema).Should().Equal("active", "inactive", "pending");
        schema.Enum.Should().AllSatisfy(value => value.GetValueKind().Should().Be(JsonValueKind.String));
    }

    [Fact]
    public void Apply_ForNonStringEnum_ShouldLeaveSchemaUntouched()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer,
            Format = "int32",
            Description = "original",
            Enum = [JsonValue.Create(0), JsonValue.Create(1)]
        };

        filter.Apply(schema, CreateContext(typeof(TestInt)));

        schema.Type.Should().Be(JsonSchemaType.Integer);
        schema.Format.Should().Be("int32");
        schema.Description.Should().Be("original");
        schema.Enum.Select(value => value.GetValue<int>()).Should().Equal(0, 1);
    }

    [Fact]
    public void Apply_ForNonEnumType_ShouldLeaveSchemaUntouched()
    {
        var filter = new EnumSchemaFilter(_services);
        var schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" };

        filter.Apply(schema, CreateContext(typeof(Guid)));

        schema.Type.Should().Be(JsonSchemaType.String);
        schema.Format.Should().Be("uuid");
        schema.Enum.Should().BeNullOrEmpty();
    }

    private static JsonArray Nodes(OpenApiSchema schema, string extension) =>
        ((JsonNodeExtension)schema.Extensions![extension]).Node.AsArray();

    private static IEnumerable<string> Strings(OpenApiSchema schema, string extension) =>
        Nodes(schema, extension).Select(value => value!.GetValue<string>());

    private static IEnumerable<string> EnumValues(OpenApiSchema schema) =>
        schema.Enum!.Select(value => value.GetValue<string>());

    private static SchemaFilterContext CreateContext(Type type)
    {
        // Use reflection to instantiate SchemaFilterContext since its constructor may be complex
        // For this test we use a minimal approach: the filter only checks context.Type
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions()));

        return new SchemaFilterContext(type, schemaGenerator, schemaRepository, null, null);
    }
}
