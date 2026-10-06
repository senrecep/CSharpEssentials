using CSharpEssentials.DependencyInjection;
using CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionB;

[Decorates(typeof(IFixtureOrdered))]
internal sealed class FixtureOrderedDecorator(IFixtureOrdered inner) : IFixtureOrdered
{
    public string Describe() => $"b0({inner.Describe()})";
}
