namespace CSharpEssentials.Json.Generators;

/// <summary>Compilation-wide inputs of the registry.</summary>
/// <param name="CanRegister">The compilation references CSharpEssentials.Json and uses C# 9 or newer (module initializers).</param>
/// <param name="HasModuleInitializerAttribute">Whether <c>ModuleInitializerAttribute</c> is available, so no polyfill is needed.</param>
/// <param name="ClassName">The registry class name, from the sanitized assembly name.</param>
/// <param name="ReferencedRegistries">The fully qualified registries of referenced assemblies, called first.</param>
internal sealed record RegistrySettings(
    bool CanRegister,
    bool HasModuleInitializerAttribute,
    string ClassName,
    EquatableArray<string> ReferencedRegistries);
