using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;

namespace CSharpEssentials.Tests.Json.Polymorphism;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
internal interface ITestEvent;

[JsonTypeDiscriminator("user.created.v1")]
internal sealed record UserCreated(Guid Id) : ITestEvent;

[JsonTypeDiscriminator(7)]
internal sealed record UserDeleted(Guid Id) : ITestEvent;

internal sealed record UserRenamed(Guid Id, string Name) : ITestEvent;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "t")]
[JsonDerivedType(typeof(ClosedCircle), "circle")]
[JsonDerivedType(typeof(ClosedSquare), "square")]
internal abstract class ClosedShape;

internal sealed class ClosedCircle : ClosedShape
{
    public double Radius { get; set; }
}

[JsonTypeDiscriminator("sq")]
internal sealed class ClosedSquare : ClosedShape
{
    public double Side { get; set; }
}

[JsonTypeDiscriminator("tri")]
internal sealed class ClosedTriangle : ClosedShape
{
    public double Height { get; set; }
}

[JsonDerivedType(typeof(Cat), "cat")]
internal abstract class Pet
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class Cat : Pet;

[JsonTypeDiscriminator("dog")]
internal sealed class Dog : Pet;

[JsonPolymorphic]
internal class Animal
{
    public string Name { get; set; } = string.Empty;
}

[JsonTypeDiscriminator("bird")]
internal sealed class Bird : Animal
{
    public bool CanFly { get; set; }
}

[JsonPolymorphic]
internal interface IValue;

[JsonTypeDiscriminator("text")]
internal sealed class TextValue : IValue
{
    public string Text { get; set; } = string.Empty;
}

[JsonTypeDiscriminator("number")]
internal sealed class NumberValue : IValue
{
    public decimal Number { get; set; }
}

internal class ValueHolder
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

internal sealed class SpecialValueHolder : ValueHolder
{
    public string Extra { get; set; } = string.Empty;
}

[JsonPolymorphic]
[JsonAcceptClrTypeNames]
internal interface ILegacyEvent;

[JsonTypeDiscriminator("legacy.created")]
internal sealed class LegacyCreated : ILegacyEvent
{
    public string Name { get; set; } = string.Empty;
}

[JsonPolymorphic]
internal interface IOrphan;

[JsonTypeDiscriminator("duplicate")]
internal sealed class TestDuplicateTwo : IFixtureDuplicateEvent;

[JsonSerializable(typeof(IFixtureEvent))]
[JsonSerializable(typeof(Fixtures.JsonEvents.FixtureOrderPlaced))]
[JsonSerializable(typeof(Fixtures.JsonEvents.FixtureOrderCancelled))]
[JsonSerializable(typeof(ITestEvent))]
[JsonSerializable(typeof(UserCreated))]
[JsonSerializable(typeof(ValueHolder))]
[JsonSerializable(typeof(TextValue))]
[JsonSerializable(typeof(NumberValue))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
internal sealed partial class PolymorphismTestContext : JsonSerializerContext;
