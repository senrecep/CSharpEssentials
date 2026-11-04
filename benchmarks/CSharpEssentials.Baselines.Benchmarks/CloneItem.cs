using CSharpEssentials.Clone;

namespace CSharpEssentials.Baselines.Benchmarks;

/// <summary>A small reference type whose <see cref="Clone"/> copies its fields, as in the package Readme.</summary>
public sealed class CloneItem(int id, string name) : ICloneable<CloneItem>
{
    public int Id { get; } = id;

    public string Name { get; } = name;

    public CloneItem Clone() => new(Id, Name);
}
