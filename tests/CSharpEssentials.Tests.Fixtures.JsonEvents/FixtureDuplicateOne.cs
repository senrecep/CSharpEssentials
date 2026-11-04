using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;

namespace CSharpEssentials.Tests.Fixtures.JsonEvents;

[JsonTypeDiscriminator("duplicate")]
public sealed class FixtureDuplicateOne : IFixtureDuplicateEvent;
