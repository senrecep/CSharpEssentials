using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Generators;

/// <summary>
/// Registers the JSON converter of every closed typed result named in a <c>[JsonSerializable]</c> attribute (directly, as a
/// generic argument or as an array element) with <c>TypedResultJsonConverterFactory.Register</c> from a module initializer, so
/// the factory creates those converters without reflection and System.Text.Json source generation works under Native AOT.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class TypedResultJsonGenerator : IIncrementalGenerator
{
    /// <summary>Tracking name of the registrations found in one attribute target.</summary>
    public const string RegistrationsStep = "TypedResultJsonRegistrations";

    /// <summary>Namespace of the generated registration class.</summary>
    public const string Namespace = "CSharpEssentials.ResultPattern.Generated";

    private const string JsonSerializableAttribute = "System.Text.Json.Serialization.JsonSerializableAttribute";
    private const string FactoryType = "CSharpEssentials.ResultPattern.TypedResultJsonConverterFactory";
    private const string ModuleInitializerAttribute = "System.Runtime.CompilerServices.ModuleInitializerAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<string> registrations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                JsonSerializableAttribute,
                static (_, _) => true,
                static (ctx, _) => Registrations(ctx.SemanticModel.Compilation, ctx.Attributes))
            .SelectMany(static (found, _) => found)
            .WithTrackingName(RegistrationsStep);

        IncrementalValueProvider<Settings> settings = context.CompilationProvider.Select(static (compilation, _) => new Settings(
            compilation.GetTypeByMetadataName(FactoryType) is not null &&
                (compilation is not CSharpCompilation csharp || csharp.LanguageVersion >= LanguageVersion.CSharp9),
            HasModuleInitializerAttribute(compilation),
            ClassName(compilation.AssemblyName)));

        context.RegisterSourceOutput(registrations.Collect().Combine(settings), static (spc, input) =>
        {
            (ImmutableArray<string> found, Settings options) = input;
            if (!options.CanRegister || found.IsEmpty)
                return;

            spc.AddSource("TypedResultJsonRegistration.g.cs", Write(options.RegistrationClass, found.Distinct(StringComparer.Ordinal).OrderBy(static r => r, StringComparer.Ordinal)));
            if (!options.HasModuleInitializer)
                spc.AddSource("TypedResultJsonModuleInitializerAttribute.g.cs", ModuleInitializerPolyfill);
        });
    }

    private static ImmutableArray<string> Registrations(Compilation compilation, ImmutableArray<AttributeData> attributes)
    {
        var symbols = TypedResultSymbols.Create(compilation);
        if (symbols is null)
            return ImmutableArray<string>.Empty;

        ImmutableArray<string>.Builder found = ImmutableArray.CreateBuilder<string>();
        foreach (AttributeData attribute in attributes)
        {
            if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is ITypeSymbol type)
                Collect(type, symbols, compilation, found);
        }

        return found.ToImmutable();
    }

    private static void Collect(ITypeSymbol type, TypedResultSymbols symbols, Compilation compilation, ImmutableArray<string>.Builder found)
    {
        if (type is IArrayTypeSymbol array)
        {
            Collect(array.ElementType, symbols, compilation, found);
            return;
        }

        if (type is not INamedTypeSymbol named)
            return;

        foreach (ITypeSymbol argument in named.TypeArguments)
            Collect(argument, symbols, compilation, found);

        if (symbols.IsTypedResult(named) && IsClosed(named) && compilation.IsSymbolAccessibleWithin(named, compilation.Assembly))
        {
            string arguments = string.Join(", ", named.TypeArguments.Select(static a => a.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            found.Add("global::" + FactoryType + ".Register<" + arguments + ">();");
        }
    }

    private static bool IsClosed(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol or IErrorTypeSymbol => false,
        IArrayTypeSymbol array => IsClosed(array.ElementType),
        INamedTypeSymbol named => named.TypeArguments.All(IsClosed),
        IPointerTypeSymbol or IFunctionPointerTypeSymbol => false,
        _ => true,
    };

    private static bool HasModuleInitializerAttribute(Compilation compilation) =>
        compilation.GetTypesByMetadataName(ModuleInitializerAttribute).Any(type => compilation.IsSymbolAccessibleWithin(type, compilation.Assembly));

    private static string ClassName(string? assemblyName)
    {
        string name = assemblyName ?? "Assembly";
        StringBuilder sanitized = new("TypedResultJsonRegistration_");
        foreach (char c in name)
        {
            if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_')
                sanitized.Append(c);
        }

        uint hash = 2166136261;
        foreach (char c in name)
            hash = (hash ^ c) * 16777619;

        return sanitized.Append('_').Append(hash.ToString("x8", CultureInfo.InvariantCulture)).ToString();
    }

    private static string Write(string className, IEnumerable<string> registrations)
    {
        StringBuilder w = new();
        w.AppendLine("// <auto-generated/>");
        w.AppendLine("#nullable enable");
        w.AppendLine();
        w.AppendLine("namespace " + Namespace);
        w.AppendLine("{");
        w.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCode(\"CSharpEssentials.Results.Generators\", \"1.0.0\")]");
        w.AppendLine("    internal static class " + className);
        w.AppendLine("    {");
        w.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        w.AppendLine("        internal static void Register()");
        w.AppendLine("        {");
        foreach (string registration in registrations)
            w.AppendLine("            " + registration);

        w.AppendLine("        }");
        w.AppendLine("    }");
        w.AppendLine("}");
        return w.ToString();
    }

    private const string ModuleInitializerPolyfill = """
        // <auto-generated/>

        namespace System.Runtime.CompilerServices
        {
            internal sealed partial class ModuleInitializerAttribute : global::System.Attribute
            {
            }
        }

        """;

    private sealed record Settings(bool CanRegister, bool HasModuleInitializer, string RegistrationClass);
}
