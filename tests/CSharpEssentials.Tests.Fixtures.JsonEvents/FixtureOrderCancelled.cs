using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;

namespace CSharpEssentials.Tests.Fixtures.JsonEvents;

[JsonTypeDiscriminator("order.cancelled.v1")]
public sealed record FixtureOrderCancelled(Guid Id, string Reason) : IFixtureEvent;
