using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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

internal sealed class DisposableResource : IDisposable
{
    public string Name { get; set; } = string.Empty;

    public void Dispose()
    {
        Name = string.Empty;
    }
}

internal sealed record NodeHolder(JsonNode? Data);

internal sealed record AsyncHolder(IAsyncEnumerable<int> Items);

[JsonDerivedType(typeof(AttrCircle), "circle")]
internal abstract class AttrShape;

internal sealed class AttrCircle : AttrShape
{
    public double Radius { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(AttrSquare), "square")]
internal interface IAttrShape;

internal sealed class AttrSquare : IAttrShape
{
    public double Side { get; set; }
}

internal interface IFill;

internal sealed class Solid : IFill
{
    public string Color { get; set; } = string.Empty;
}

internal sealed class FilledCircle : BaseShape
{
    public IFill? Fill { get; set; }
}

internal abstract class OpenGenericBase;

internal sealed class OpenGenericDerived<TValue> : OpenGenericBase
{
    public TValue? Value { get; set; }
}

internal interface IItemStream<out TItem> : IAsyncEnumerable<TItem>;

internal abstract class ItemStream : IAsyncEnumerable<int>
{
    public abstract IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default);
}

internal abstract class LinkBase
{
    public LinkBase? Next { get; set; }
}

internal sealed class Link : LinkBase;

public class PolymorphicJsonConverterFactoryTests
{
    private static readonly JsonSerializerOptions PolymorphicOptions = new()
    {
        Converters = { new PolymorphicJsonConverterFactory() }
    };

    private static readonly JsonSerializerOptions FactoryOnlyOptions = new()
    {
        Converters = { new PolymorphicJsonConverterFactory() },
    };

    private static readonly JsonSerializerOptions SmallBufferOptions = new()
    {
        DefaultBufferSize = 16,
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
        ((Circle)shape).Radius.Should().Be(5.0);
    }

    [Fact]
    public void Deserialize_WithDifferentType_ShouldDeserializeToCorrectType()
    {
        string json = """{"$type":"CSharpEssentials.Tests.Json.Rectangle","Type":"Rectangle","Width":10.0,"Height":20.0}""";
        BaseShape? shape = JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions);

        shape.Should().BeOfType<Rectangle>();
        var rect = (Rectangle)shape;
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
    [InlineData(typeof(JsonNode))]
    [InlineData(typeof(JsonObject))]
    [InlineData(typeof(JsonArray))]
    [InlineData(typeof(JsonValue))]
    [InlineData(typeof(AttrShape))]
    [InlineData(typeof(IAttrShape))]
    [InlineData(typeof(IAsyncEnumerable<int>))]
    [InlineData(typeof(IItemStream<int>))]
    [InlineData(typeof(ItemStream))]
    public void CanConvert_WithExcludedType_ShouldReturnFalse(Type type)
    {
        PolymorphicJsonConverterFactory factory = new();

        factory.CanConvert(type).Should().BeFalse();
    }

    [Fact]
    public void Serialize_DictionaryInterface_ShouldNotWrapWithTypeDiscriminator()
    {
        IDictionary<string, object?> value = new Dictionary<string, object?> { ["a"] = 1 };

        JsonSerializer.Serialize(value, PolymorphicOptions).Should().Be("{\"a\":1}");
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

    [Fact]
    public void Serialize_JsonNodeProperty_ShouldKeepBuiltInShape()
    {
        string json = JsonSerializer.Serialize(new NodeHolder(new JsonArray(1, 2)), PolymorphicOptions);

        json.Should().Be("{\"Data\":[1,2]}");
    }

    [Fact]
    public void Deserialize_JsonNodeProperty_ShouldReadPlainObject()
    {
        NodeHolder? holder = JsonSerializer.Deserialize<NodeHolder>("{\"Data\":{\"a\":1}}", PolymorphicOptions);

        holder!.Data!["a"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task Serialize_AsyncEnumerableProperty_ShouldWriteArray()
    {
        using MemoryStream stream = new();

        await JsonSerializer.SerializeAsync(stream, new AsyncHolder(Range(2)), PolymorphicOptions);

        System.Text.Encoding.UTF8.GetString(stream.ToArray()).Should().Be("{\"Items\":[0,1]}");
    }

    private static async IAsyncEnumerable<int> Range(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }

    [Fact]
    public void Deserialize_WithJsonDerivedTypeAttribute_ShouldUseAttributeDiscriminator()
    {
        AttrShape? shape = JsonSerializer.Deserialize<AttrShape>("""{"$type":"circle","Radius":1}""", PolymorphicOptions);

        shape.Should().BeOfType<AttrCircle>().Which.Radius.Should().Be(1);
    }

    [Fact]
    public void Serialize_WithJsonPolymorphicAttribute_ShouldUseAttributeDiscriminator()
    {
        IAttrShape shape = new AttrSquare { Side = 2 };

        JsonSerializer.Serialize(shape, PolymorphicOptions).Should().Be("""{"kind":"square","Side":2}""");
    }

    [Fact]
    public void RoundTrip_WithNestedInterfaceProperty_ShouldKeepDerivedType()
    {
        BaseShape shape = new FilledCircle { Fill = new Solid { Color = "red" } };

        string json = JsonSerializer.Serialize(shape, PolymorphicOptions);
        BaseShape? restored = JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions);

        restored.Should().BeOfType<FilledCircle>().Which.Fill.Should().BeOfType<Solid>().Which.Color.Should().Be("red");
    }

    [Fact]
    public void Deserialize_WithDuplicateDerivedTypeNames_ShouldUseFirstAndKeepWorking()
    {
        AssemblyBuilder first = DefineDerived("PolymorphicDup.First");
        AssemblyBuilder second = DefineDerived("PolymorphicDup.Second");
        string json = """{"$type":"Dup.Shape"}""";

        DuplicateNameBase? shape = JsonSerializer.Deserialize<DuplicateNameBase>(json, PolymorphicOptions);
        DuplicateNameBase? again = JsonSerializer.Deserialize<DuplicateNameBase>(json, PolymorphicOptions);

        shape.Should().NotBeNull();
        again!.GetType().FullName.Should().Be("Dup.Shape");
        GC.KeepAlive(first);
        GC.KeepAlive(second);
    }

    [Fact]
    public void Deserialize_WithOpenGenericDiscriminator_ShouldThrowJsonException()
    {
        string json = $"{{\"$type\":\"{typeof(OpenGenericDerived<>).FullName}\"}}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OpenGenericBase>(json, PolymorphicOptions));
    }

    [Fact]
    public void Serialize_WithCycleThroughAbstractMember_ShouldThrowJsonException()
    {
        Link link = new();
        link.Next = link;

        Assert.Throws<JsonException>(() => JsonSerializer.Serialize<LinkBase>(link, PolymorphicOptions));
    }

    [Fact]
    public void RoundTrip_WithNestedAbstractChain_ShouldKeepEveryLevel()
    {
        LinkBase chain = new Link { Next = new Link { Next = new Link() } };

        string json = JsonSerializer.Serialize(chain, PolymorphicOptions);
        LinkBase? restored = JsonSerializer.Deserialize<LinkBase>(json, PolymorphicOptions);

        restored!.Next!.Next.Should().BeOfType<Link>().Which.Next.Should().BeNull();
    }

    [Theory]
    [InlineData("[1]")]
    [InlineData("\"CSharpEssentials.Tests.Json.Circle\"")]
    [InlineData("1")]
    public void Deserialize_WithNonObjectValue_ShouldThrowJsonException(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions));
    }

    [Fact]
    public void Deserialize_WithTypeOnlyOnNestedObject_ShouldThrowJsonException()
    {
        string json = """{"Fill":{"$type":"CSharpEssentials.Tests.Json.Solid","Color":"red"}}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions));
    }

    [Fact]
    public void Deserialize_WithEscapedTypePropertyName_ShouldReadDiscriminator()
    {
        string json = """{"\u0024type":"CSharpEssentials.Tests.Json.Circle","Radius":2}""";

        JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions).Should().BeOfType<Circle>().Which.Radius.Should().Be(2);
    }

    [Fact]
    public void Deserialize_WithDuplicateTypeProperty_ShouldUseTheLastOne()
    {
        string json = """{"$type":"CSharpEssentials.Tests.Json.Circle","$type":"CSharpEssentials.Tests.Json.Rectangle","Width":3}""";

        JsonSerializer.Deserialize<BaseShape>(json, PolymorphicOptions).Should().BeOfType<Rectangle>().Which.Width.Should().Be(3);
    }

    [Fact]
    public void Deserialize_ArrayOfShapes_ShouldReadEveryElement()
    {
        string json = """[{"$type":"CSharpEssentials.Tests.Json.Circle","Radius":1},{"Width":2,"$type":"CSharpEssentials.Tests.Json.Rectangle"},null]""";

        List<BaseShape?>? shapes = JsonSerializer.Deserialize<List<BaseShape?>>(json, PolymorphicOptions);

        shapes.Should().HaveCount(3);
        shapes![0].Should().BeOfType<Circle>().Which.Radius.Should().Be(1);
        shapes[1].Should().BeOfType<Rectangle>().Which.Width.Should().Be(2);
        shapes[2].Should().BeNull();
    }

    [Fact]
    public async Task DeserializeAsync_WithTypeLastAfterNestedObject_ShouldReadAcrossBuffers()
    {
        string json = """{"Fill":{"$type":"CSharpEssentials.Tests.Json.Solid","Color":"red"},"Type":"filled","$type":"CSharpEssentials.Tests.Json.FilledCircle"}""";
        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json));

        BaseShape? shape = await JsonSerializer.DeserializeAsync<BaseShape>(stream, SmallBufferOptions);

        FilledCircle circle = shape.Should().BeOfType<FilledCircle>().Subject;
        circle.Type.Should().Be("filled");
        circle.Fill.Should().BeOfType<Solid>().Which.Color.Should().Be("red");
    }

    [Fact]
    public void Deserialize_WithFrameworkTypeName_ShouldThrow()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new PolymorphicJsonConverterFactory());
        string json = """{"$type":"System.IO.MemoryStream"}""";

        Action act = () => JsonSerializer.Deserialize<IDisposable>(json, options);

        act.Should().Throw<JsonException>().WithMessage("*Unknown type discriminator 'System.IO.MemoryStream'*");
    }

    [Fact]
    public void Deserialize_WithUserTypeBehindFrameworkInterface_ShouldWork()
    {
        string json = """{"$type":"CSharpEssentials.Tests.Json.DisposableResource","Name":"a"}""";

        IDisposable? value = JsonSerializer.Deserialize<IDisposable>(json, FactoryOnlyOptions);

        value.Should().BeOfType<DisposableResource>().Which.Name.Should().Be("a");
    }

    private static AssemblyBuilder DefineDerived(string assemblyName)
    {
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
        TypeBuilder type = assembly.DefineDynamicModule(assemblyName)
            .DefineType("Dup.Shape", TypeAttributes.Public | TypeAttributes.Sealed, typeof(DuplicateNameBase));
        type.DefineDefaultConstructor(MethodAttributes.Public);
        type.CreateType();
        return assembly;
    }
}
