using CSharpEssentials.DependencyInjection;
using CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionB;

[RegisterScoped(typeof(IFixtureMessage))]
internal sealed class FixtureMessage : IFixtureMessage
{
    public string Text() => "b";
}
