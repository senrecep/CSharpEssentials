namespace CSharpEssentials.Json.Generators;

/// <summary>A valued derived type and the polymorphic bases it registers under.</summary>
/// <param name="DerivedType">The fully qualified derived type.</param>
/// <param name="Value">The discriminator as a C# literal.</param>
/// <param name="BaseTypes">The fully qualified bases that do not list the type in <c>[JsonDerivedType]</c>.</param>
internal sealed record DerivedTypeModel(string DerivedType, string Value, EquatableArray<string> BaseTypes);
