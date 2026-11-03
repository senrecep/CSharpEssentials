using System.Text.Json.Serialization;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>Base of the hierarchy System.Text.Json handles through its declared derived types.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(StjDog), "CSharpEssentials.Json.Comparison.Benchmarks.StjDog")]
[JsonDerivedType(typeof(StjCat), "CSharpEssentials.Json.Comparison.Benchmarks.StjCat")]
public abstract class StjAnimal
{
    /// <summary>The name.</summary>
    public string Name { get; set; } = string.Empty;
}
