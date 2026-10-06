using CSharpEssentials.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

[RegisterScoped]
internal sealed class FixtureGreeter : IFixtureGreeter
{
    public string Greet() => "a";
}
