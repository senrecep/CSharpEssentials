using CSharpEssentials.Json;
using FluentAssertions;
using System.Text.Json;

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

        factory.CanConvert(typeof(IEnumerable<>)).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithConcreteClass_ShouldReturnFalse()
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(typeof(Circle)).Should().BeFalse();
        factory.CanConvert(typeof(string)).Should().BeFalse();
    }
}
