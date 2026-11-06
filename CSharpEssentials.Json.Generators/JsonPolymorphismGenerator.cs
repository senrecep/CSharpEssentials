using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Json.Generators;

/// <summary>
/// Generates, per assembly, a registry that registers the <c>[JsonTypeDiscriminator]</c> types and the <c>[JsonPolymorphicProperty]</c>
/// members in <c>JsonPolymorphism</c> from a module initializer, after calling the registries of referenced assemblies.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class JsonPolymorphismGenerator : IIncrementalGenerator
{
    /// <summary>Tracking name of the valued derived types.</summary>
    public const string DerivedTypesStep = "PolymorphicDerivedTypes";

    /// <summary>Tracking name of the per-property names.</summary>
    public const string PropertiesStep = "PolymorphicProperties";

    /// <summary>Tracking name of the collected registry entries.</summary>
    public const string EntriesStep = "PolymorphicEntries";

    /// <summary>Tracking name of the compilation-wide registry settings.</summary>
    public const string SettingsStep = "PolymorphicRegistrySettings";

    private const string ModuleInitializerAttribute = "System.Runtime.CompilerServices.ModuleInitializerAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<DerivedTypeModel> derivedTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PolymorphismSymbols.TypeDiscriminatorAttribute,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, ct) => PolymorphismModelReader.ReadDerivedType((INamedTypeSymbol)ctx.TargetSymbol, ctx.Attributes[0], ct))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(DerivedTypesStep);

        IncrementalValuesProvider<PropertyModel> properties = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PolymorphismSymbols.PolymorphicPropertyAttribute,
                static (node, _) => node is PropertyDeclarationSyntax or VariableDeclaratorSyntax,
                static (ctx, ct) => PolymorphismModelReader.ReadProperty(ctx.TargetSymbol, ctx.Attributes[0], ct))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(PropertiesStep);

        IncrementalValueProvider<EquatableArray<string>> entries = derivedTypes
            .SelectMany(static (model, _) => PolymorphismSourceWriter.DerivedTypeEntries(model))
            .Collect()
            .Combine(properties.Select(static (model, _) => PolymorphismSourceWriter.PropertyEntry(model)).Collect())
            .Select(static (input, _) => new EquatableArray<string>([.. input.Left.Concat(input.Right).Distinct(StringComparer.Ordinal).OrderBy(static entry => entry, StringComparer.Ordinal)]))
            .WithTrackingName(EntriesStep);

        IncrementalValueProvider<RegistrySettings> settings = context.CompilationProvider
            .Select(static (compilation, ct) => CreateSettings(compilation, ct))
            .WithTrackingName(SettingsStep);

        context.RegisterSourceOutput(entries.Combine(settings), static (spc, input) =>
        {
            (EquatableArray<string> registrations, RegistrySettings registrySettings) = input;
            if (!registrySettings.CanRegister || registrations.Count == 0 && registrySettings.ReferencedRegistries.Count == 0)
                return;

            spc.AddSource(PolymorphismSourceWriter.RegistryFileName, PolymorphismSourceWriter.WriteRegistry(registrySettings, registrations));
            if (!registrySettings.HasModuleInitializerAttribute)
                spc.AddSource("JsonPolymorphismModuleInitializerAttribute.g.cs", PolymorphismSourceWriter.WriteModuleInitializerPolyfill());
        });
    }

    private static RegistrySettings CreateSettings(Compilation compilation, CancellationToken cancellationToken)
    {
        bool canRegister = compilation.GetTypeByMetadataName(PolymorphismSymbols.RegistryType) is not null &&
            (compilation is not CSharpCompilation csharp || csharp.LanguageVersion >= LanguageVersion.CSharp9);
        string className = RegistryNames.ForAssembly(compilation.AssemblyName);
        string[] referenced = canRegister
            ? ReferencedRegistryReader.Read(compilation, "global::" + RegistryNames.Namespace + "." + className, cancellationToken)
            : [];
        return new RegistrySettings(canRegister, HasModuleInitializerAttribute(compilation), className, new EquatableArray<string>(referenced));
    }

    private static bool HasModuleInitializerAttribute(Compilation compilation)
    {
        foreach (INamedTypeSymbol type in compilation.GetTypesByMetadataName(ModuleInitializerAttribute))
        {
            if (compilation.IsSymbolAccessibleWithin(type, compilation.Assembly))
                return true;
        }

        return false;
    }
}
