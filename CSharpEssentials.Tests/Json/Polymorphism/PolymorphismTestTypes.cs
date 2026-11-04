using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;

namespace CSharpEssentials.Tests.Json.Polymorphism;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface ITestEvent;

[JsonTypeDiscriminator("user.created.v1")]
public sealed record UserCreated(Guid Id) : ITestEvent;

[JsonTypeDiscriminator(7)]
public sealed record UserDeleted(Guid Id) : ITestEvent;

public sealed record UserRenamed(Guid Id, string Name) : ITestEvent;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "t")]
[JsonDerivedType(typeof(ClosedCircle), "circle")]
[JsonDerivedType(typeof(ClosedSquare), "square")]
public abstract class ClosedShape;

public sealed class ClosedCircle : ClosedShape
{
    public double Radius { get; set; }
}

[JsonTypeDiscriminator("sq")]
public sealed class ClosedSquare : ClosedShape
{
    public double Side { get; set; }
}

[JsonTypeDiscriminator("tri")]
public sealed class ClosedTriangle : ClosedShape
{
    public double Height { get; set; }
}

[JsonDerivedType(typeof(Cat), "cat")]
public abstract class Pet
{
    public string Name { get; set; } = string.Empty;
}

public sealed class Cat : Pet;

[JsonTypeDiscriminator("dog")]
public sealed class Dog : Pet;

[JsonPolymorphic]
public class Animal
{
    public string Name { get; set; } = string.Empty;
}

[JsonTypeDiscriminator("bird")]
public sealed class Bird : Animal
{
    public bool CanFly { get; set; }
}

[JsonPolymorphic]
public interface IValue;

[JsonTypeDiscriminator("text")]
public sealed class TextValue : IValue
{
    public string Text { get; set; } = string.Empty;
}

[JsonTypeDiscriminator("number")]
public sealed class NumberValue : IValue
{
    public decimal Number { get; set; }
}

public class ValueHolder
{
    [JsonPolymorphicProperty("values")]
    public IValue? Value { get; set; }

    [JsonPolymorphicProperty("values")]
    public List<IValue> Items { get; set; } = [];

    [JsonPolymorphicProperty("kind")]
    public IValue?[] Array { get; set; } = [];

    [JsonPolymorphicProperty("kind")]
    public Dictionary<string, IValue> Map { get; set; } = [];

    public IValue? Other { get; set; }

    public ValueHolder? Child { get; set; }
}

public sealed class SpecialValueHolder : ValueHolder
{
    public string Extra { get; set; } = string.Empty;
}

[JsonPolymorphic]
[JsonAcceptClrTypeNames]
public interface ILegacyEvent;

[JsonTypeDiscriminator("legacy.created")]
public sealed class LegacyCreated : ILegacyEvent
{
    public string Name { get; set; } = string.Empty;
}

[JsonPolymorphic]
public interface IOrphan;

[JsonTypeDiscriminator("duplicate")]
public sealed class TestDuplicateTwo : IFixtureDuplicateEvent;

[JsonSerializable(typeof(IFixtureEvent))]
[JsonSerializable(typeof(Fixtures.JsonEvents.FixtureOrderPlaced))]
[JsonSerializable(typeof(Fixtures.JsonEvents.FixtureOrderCancelled))]
[JsonSerializable(typeof(ITestEvent))]
[JsonSerializable(typeof(UserCreated))]
[JsonSerializable(typeof(ValueHolder))]
[JsonSerializable(typeof(TextValue))]
[JsonSerializable(typeof(NumberValue))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
public sealed partial class PolymorphismTestContext : JsonSerializerContext;
