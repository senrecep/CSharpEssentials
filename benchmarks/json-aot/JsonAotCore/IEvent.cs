using System.Text.Json.Serialization;

namespace JsonAotCore;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface IEvent
{
    Guid Id { get; }
}
