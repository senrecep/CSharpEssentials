namespace CSharpEssentials.Tests.Fixtures.DependencyInjectionA;

/// <summary>
/// A message decorated by this assembly and registered by a referencing assembly.
/// </summary>
public interface IFixtureMessage
{
    /// <summary>
    /// Returns the message text.
    /// </summary>
    /// <returns>The message text.</returns>
    string Text();
}
