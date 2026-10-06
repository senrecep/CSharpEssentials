namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

/// <summary>
/// A service decorated by this assembly and a referencing assembly with different decorator orders.
/// </summary>
public interface IFixtureOrdered
{
    /// <summary>
    /// Returns the decoration chain.
    /// </summary>
    /// <returns>The decoration chain.</returns>
    string Describe();
}
