using CSharpEssentials.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

[RegisterScoped]
internal sealed class FixtureOrdered : IFixtureOrdered
{
    public string Describe() => "a";
}
