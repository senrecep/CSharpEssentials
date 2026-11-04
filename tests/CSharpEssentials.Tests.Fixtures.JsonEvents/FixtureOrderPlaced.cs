using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;

namespace CSharpEssentials.Tests.Fixtures.JsonEvents;

[JsonTypeDiscriminator("order.placed.v1")]
public sealed record FixtureOrderPlaced(Guid Id, decimal Total) : IFixtureEvent;
