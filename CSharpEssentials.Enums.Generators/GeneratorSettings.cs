namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Compilation-wide inputs of the enum generator.
/// </summary>
/// <param name="SupportsNullable">C# 8 or newer.</param>
/// <param name="SupportsRegistration">C# 9 or newer: metadata, registration and the wire-name helpers are generated.</param>
/// <param name="ProjectNaming">The parsed <c>CSharpEssentialsEnumNaming</c> MSBuild property (0 when unset or invalid).</param>
internal sealed record GeneratorSettings(bool SupportsNullable, bool SupportsRegistration, int ProjectNaming);
