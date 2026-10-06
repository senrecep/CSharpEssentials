using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Generates, per <c>[StringEnum]</c> enum, the extension class with the wire-name helpers and the <c>EnumInfo</c> metadata, and per
/// assembly a module initializer that registers the metadata in <c>EnumMetadata</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class StringEnumGenerator : IIncrementalGenerator
{
    /// <summary>Tracking name of the per-enum model step.</summary>
    public const string EnumModelsStep = "EnumModels";

    /// <summary>Tracking name of the compilation-wide settings.</summary>
    public const string SettingsStep = "EnumGeneratorSettings";

    /// <summary>Tracking name of the collected registry entries.</summary>
    public const string RegistryStep = "EnumRegistry";

    /// <summary>Tracking name of the <c>ModuleInitializerAttribute</c> availability check.</summary>
    public const string ModuleInitializerStep = "EnumModuleInitializer";

    private const string AttributeName = "CSharpEssentials.Enums.StringEnumAttribute";

    private const string NamingProperty = "build_property.CSharpEssentialsEnumNaming";

    private const string ModuleInitializerAttribute = "System.Runtime.CompilerServices.ModuleInitializerAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumModel> enums = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (node, _) => node is EnumDeclarationSyntax,
                static (ctx, ct) => EnumModelReader.Read((INamedTypeSymbol)ctx.TargetSymbol, ctx.Attributes[0], ct))
            .Where(static model => model is not null)
            .Select(static (model, _) => model!)
            .WithTrackingName(EnumModelsStep);

        IncrementalValueProvider<GeneratorSettings> settings = context.ParseOptionsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (input, _) => CreateSettings(input.Left, input.Right))
            .WithTrackingName(SettingsStep);

        context.RegisterSourceOutput(enums.Combine(settings), static (spc, input) =>
        {
            (EnumModel model, GeneratorSettings generatorSettings) = input;
            string prefix = model.Namespace.Length == 0 ? string.Empty : model.Namespace + ".";
            spc.AddSource(prefix + model.ExtensionsClassName + ".g.cs", StringEnumSourceWriter.WriteExtensions(model, generatorSettings));
        });

        IncrementalValueProvider<EquatableArray<string>> registry = enums
            .Select(static (model, _) => QualifiedExtensionsClass(model))
            .Collect()
            .Select(static (classes, _) => new EquatableArray<string>([.. classes.OrderBy(static name => name, StringComparer.Ordinal)]))
            .WithTrackingName(RegistryStep);

        IncrementalValueProvider<bool> hasModuleInitializer = context.CompilationProvider
            .Select(static (compilation, _) => HasModuleInitializerAttribute(compilation))
            .WithTrackingName(ModuleInitializerStep);

        context.RegisterSourceOutput(registry.Combine(settings).Combine(hasModuleInitializer), static (spc, input) =>
        {
            ((EquatableArray<string> classes, GeneratorSettings generatorSettings), bool hasAttribute) = input;
            if (classes.Count == 0 || !generatorSettings.SupportsRegistration)
                return;

            spc.AddSource(StringEnumSourceWriter.RegistryClassName + ".g.cs", StringEnumSourceWriter.WriteRegistry(classes));
            if (!hasAttribute)
                spc.AddSource("ModuleInitializerAttribute.g.cs", StringEnumSourceWriter.WriteModuleInitializerPolyfill());
        });
    }

    private static GeneratorSettings CreateSettings(ParseOptions parseOptions, AnalyzerConfigOptionsProvider options)
    {
        LanguageVersion version = parseOptions is CSharpParseOptions csharp ? csharp.LanguageVersion : LanguageVersion.Default;
        _ = options.GlobalOptions.TryGetValue(NamingProperty, out string? naming);
        return new GeneratorSettings(
            version >= LanguageVersion.CSharp8,
            version >= LanguageVersion.CSharp9,
            EnumWireNaming.Parse(naming));
    }

    private static string QualifiedExtensionsClass(EnumModel model) =>
        model.Namespace.Length == 0
            ? "global::" + model.ExtensionsClassName
            : "global::" + model.Namespace + "." + model.ExtensionsClassName;

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
