using CSharpEssentials.DependencyInjection;
using CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionB;

[Decorates(typeof(IFixtureGreeter))]
internal sealed class FixtureGreeterDecorator(IFixtureGreeter inner) : IFixtureGreeter
{
    public string Greet() => $"b({inner.Greet()})";
}
