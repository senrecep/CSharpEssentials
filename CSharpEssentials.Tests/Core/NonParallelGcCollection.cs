namespace CSharpEssentials.Tests.Core;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class NonParallelGcCollection
{
    public const string Name = "NonParallelGc";
}
