using CSharpEssentials.Mediator;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

public class BehaviorCacheTests
{
    [Fact]
    public void GetOrCreateFactory_Should_Throw_ArgumentException_When_ResponseType_Is_Not_Generic()
    {
        Action act = () => BehaviorCache.GetOrCreateFactory(typeof(string));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("responseType");
    }
}
