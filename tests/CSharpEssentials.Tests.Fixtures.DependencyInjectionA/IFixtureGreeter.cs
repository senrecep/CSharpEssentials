namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

/// <summary>
/// A greeter registered by this assembly and decorated by a referencing assembly.
/// </summary>
public interface IFixtureGreeter
{
    /// <summary>
    /// Returns the greeting.
    /// </summary>
    /// <returns>The greeting.</returns>
    string Greet();
}
