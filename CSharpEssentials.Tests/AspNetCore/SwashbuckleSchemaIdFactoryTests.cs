using CSharpEssentials.AspNetCore.Swagger.Filters;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore;

public class SwashbuckleSchemaIdFactoryTests
{
    [Fact]
    public void GetSchemaId_ForSimpleType_ShouldReturnTypeName()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        string schemaId = factory.GetSchemaId(typeof(string));

        schemaId.Should().Be("String");
    }

    [Fact]
    public void GetSchemaId_ForGenericType_ShouldReturnFormattedName()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        string schemaId = factory.GetSchemaId(typeof(List<string>));

        schemaId.Should().Be("StringList");
    }

    [Fact]
    public void GetSchemaId_ForArrayType_ShouldReturnArraySuffix()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        string schemaId = factory.GetSchemaId(typeof(int[]));

        schemaId.Should().Be("Int32Array");
    }

    [Fact]
    public void GetSchemaId_ForSameTypeTwice_ShouldReturnSameId()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        string schemaId1 = factory.GetSchemaId(typeof(DateTime));
        string schemaId2 = factory.GetSchemaId(typeof(DateTime));

        schemaId1.Should().Be(schemaId2);
    }

    [Fact]
    public void GetSchemaId_ForNestedGeneric_ShouldReturnFormattedName()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        string schemaId = factory.GetSchemaId(typeof(Dictionary<string, int>));

        schemaId.Should().Be("StringInt32Dictionary");
    }
}
