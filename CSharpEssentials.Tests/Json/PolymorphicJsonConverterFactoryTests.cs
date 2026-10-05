using System.Reflection;
using System.Text.Json;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

internal abstract class BaseShape
{
    public string Type { get; set; } = string.Empty;
}

internal sealed class Circle : BaseShape
{
    public double Radius { get; set; }
}

internal sealed class Rectangle : BaseShape
{
    public double Width { get; set; }
    public double Height { get; set; }
}

internal abstract class ScannedShape;

internal sealed class Triangle : ScannedShape
{
    public double Base { get; set; }
}

public class PolymorphicJsonConverterFactoryTests
{
    private static readonly JsonSerializerOptions PolymorphicOptions = new()
    {
        Converters = { new PolymorphicJsonConverterFactory() }
    };

    [Fact]
    public void Serialize_WithPolymorphicType_ShouldIncludeTypeDiscriminator()
    {
        BaseShape shape = new Circle { Type = "Circle", Radius = 5.0 };
        string json = JsonSerializer.Serialize(shape, PolymorphicOptions);

        json.Should().Contain("$type");
        json.Should().Contain("Circle");
        json.Should().Contain("5");
    }

    [Fact]
    public void Deserialize_WithTypeDiscriminator_ShouldDeserializeToCorrectType()
    {
        string json = """{"$type":"CSharpEssentials.Tests.Json.Circle","Type":"Circle","Radius":5.0}""";
        BaseShape? shape = JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions);

        shape.Should().BeOfType<Circle>();
        ((Circle)shape!).Radius.Should().Be(5.0);
    }

    [Fact]
    public void Deserialize_WithDifferentType_ShouldDeserializeToCorrectType()
    {
        string json = """{"$type":"CSharpEssentials.Tests.Json.Rectangle","Type":"Rectangle","Width":10.0,"Height":20.0}""";
        BaseShape? shape = JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions);

        shape.Should().BeOfType<Rectangle>();
        var rect = (Rectangle)shape!;
        rect.Width.Should().Be(10.0);
        rect.Height.Should().Be(20.0);
    }

    [Fact]
    public void Deserialize_WithoutTypeDiscriminator_ShouldThrow()
    {
        string json = """{"Type":"Circle","Radius":5.0}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions));
    }

    [Fact]
    public void Deserialize_WithUnknownType_ShouldThrow()
    {
        string json = """{"$type":"UnknownType","Type":"Circle","Radius":5.0}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions));
    }

    [Fact]
    public void Serialize_WithNull_ShouldSerializeNull()
    {
        BaseShape? shape = null;
        string json = JsonSerializer.Serialize(shape, PolymorphicOptions);

        json.Should().Be("null");
    }

    [Fact]
    public void CanConvert_WithAbstractClass_ShouldReturnTrue()
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(typeof(BaseShape)).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithInterface_ShouldReturnTrue()
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(typeof(IDisposable)).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(IDictionary<string, object?>))]
    [InlineData(typeof(IReadOnlyList<BaseShape>))]
    [InlineData(typeof(System.Collections.IEnumerable))]
    public void CanConvert_WithCollectionInterface_ShouldReturnFalse(Type type)
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(type).Should().BeFalse();
    }

    [Fact]
    public void Serialize_DictionaryInterface_ShouldNotWrapWithTypeDiscriminator()
    {
        JsonSerializerOptions options = new() { Converters = { new PolymorphicJsonConverterFactory() } };
        IDictionary<string, object?> value = new Dictionary<string, object?> { ["a"] = 1 };

        JsonSerializer.Serialize(value, options).Should().Be("{\"a\":1}");
    }

    [Fact]
    public void CanConvert_WithConcreteClass_ShouldReturnFalse()
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(typeof(Circle)).Should().BeFalse();
        factory.CanConvert(typeof(string)).Should().BeFalse();
    }

    [Fact]
    public void Deserialize_WithPartiallyLoadableAssemblyInAppDomain_ShouldUseLoadableTypes()
    {
        Assembly brokenAssembly = PartiallyLoadableAssembly.Create("PolymorphicScan.Broken");
        brokenAssembly.Invoking(a => a.GetTypes()).Should().Throw<ReflectionTypeLoadException>();
        string json = $"{{\"$type\":\"{typeof(Triangle).FullName}\",\"Base\":3}}";

        ScannedShape? shape = JsonSerializer.Deserialize<ScannedShape>(json, PolymorphicOptions);

        shape.Should().BeOfType<Triangle>().Which.Base.Should().Be(3);
        GC.KeepAlive(brokenAssembly);
    }
}
