using CSharpEssentials.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

[Decorates(typeof(IFixtureMessage))]
internal sealed class FixtureMessageDecorator(IFixtureMessage inner) : IFixtureMessage
{
    public string Text() => $"a({inner.Text()})";
}
