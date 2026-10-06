using CSharpEssentials.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

[Decorates(typeof(IFixtureOrdered), Order = 1)]
internal sealed class FixtureOrderedDecorator(IFixtureOrdered inner) : IFixtureOrdered
{
    public string Describe() => $"a1({inner.Describe()})";
}
