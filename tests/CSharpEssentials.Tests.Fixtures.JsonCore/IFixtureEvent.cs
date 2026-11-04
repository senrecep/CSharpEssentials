using System.Text.Json.Serialization;

namespace CSharpEssentials.Tests.Fixtures.JsonCore;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface IFixtureEvent
{
    Guid Id { get; }
}
