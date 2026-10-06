using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Reads a <c>[StringEnum]</c> enum symbol into a value-equatable <see cref="EnumModel"/>.
/// </summary>
internal static class EnumModelReader
{
    private const string JsonMemberNameAttribute = "System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute";
    private const string EnumMemberAttribute = "System.Runtime.Serialization.EnumMemberAttribute";
    private const string EnumAliasAttribute = "CSharpEssentials.Enums.EnumAliasAttribute";
    private const string EnumFallbackAttribute = "CSharpEssentials.Enums.EnumFallbackAttribute";
    private const string DescriptionAttribute = "System.ComponentModel.DescriptionAttribute";
    private const string ObsoleteAttribute = "System.ObsoleteAttribute";
    private const string FlagsAttribute = "System.FlagsAttribute";

    private static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    /// <summary>
    /// Returns <see langword="null"/> when no namespace-level class can reach the enum (private, protected or file-local in the chain,
    /// or a generic containing type).
    /// </summary>
    public static EnumModel? Read(INamedTypeSymbol symbol, AttributeData stringEnum, CancellationToken cancellationToken)
    {
        if (!TryGetAccessibility(symbol, out bool isPublic))
            return null;

        List<string> names = [symbol.Name];
        for (INamedTypeSymbol? containing = symbol.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            names.Insert(0, containing.Name);
        }

        SpecialType underlying = symbol.EnumUnderlyingType?.SpecialType ?? SpecialType.System_Int32;
        bool isSigned = underlying is SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_Int32 or SpecialType.System_Int64;

        List<EnumMemberModel> members = [];
        foreach (ISymbol member in symbol.GetMembers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (member is IFieldSymbol { HasConstantValue: true, ConstantValue: not null } field)
                members.Add(ReadMember(field, isSigned, cancellationToken));
        }

        return new EnumModel(
            symbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : symbol.ContainingNamespace.ToDisplayString(),
            string.Join("_", names) + "Extensions",
            symbol.ToDisplayString(FullyQualifiedFormat),
            isPublic,
            Keyword(underlying),
            isSigned,
            HasAttribute(symbol, FlagsAttribute),
            GetNamedInt(stringEnum, "Naming"),
            GetNamedInt(stringEnum, "Storage"),
            new EquatableArray<EnumMemberModel>([.. members]));
    }

    private static EnumMemberModel ReadMember(IFieldSymbol field, bool isSigned, CancellationToken cancellationToken)
    {
        object value = field.ConstantValue!;
        ulong raw = isSigned
            ? unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture))
            : Convert.ToUInt64(value, CultureInfo.InvariantCulture);

        string? jsonName = null;
        string? enumMemberName = null;
        string? description = null;
        string[] aliases = [];
        bool isObsolete = false;
        bool isFallback = false;
        foreach (AttributeData attribute in field.GetAttributes())
        {
            string? name = attribute.AttributeClass?.ToDisplayString();
            if (name == JsonMemberNameAttribute)
                jsonName ??= GetConstructorString(attribute);
            else if (name == EnumMemberAttribute)
                enumMemberName = GetNamedString(attribute, "Value");
            else if (name == EnumAliasAttribute)
                aliases = GetConstructorStrings(attribute);
            else if (name == DescriptionAttribute)
                description = GetConstructorString(attribute);
            else if (name == ObsoleteAttribute)
                isObsolete = true;
            else if (name == EnumFallbackAttribute)
                isFallback = true;
        }

        return new EnumMemberModel(
            field.Name,
            raw,
            jsonName ?? enumMemberName,
            new EquatableArray<string>(aliases),
            description ?? ReadSummary(field, cancellationToken),
            isObsolete,
            isFallback);
    }

    /// <summary>
    /// Whether a namespace-level generated class can reach the enum: no private, protected or file-local type in the chain and no
    /// generic containing type.
    /// </summary>
    public static bool IsReachable(INamedTypeSymbol symbol) => TryGetAccessibility(symbol, out _);

    private static bool TryGetAccessibility(INamedTypeSymbol symbol, out bool isPublic)
    {
        isPublic = true;
        bool isContainingType = false;
        for (INamedTypeSymbol? current = symbol; current is not null; current = current.ContainingType)
        {
            if (current.IsFileLocal || isContainingType && current.IsGenericType)
                return false;

            isContainingType = true;

            Accessibility accessibility = current.DeclaredAccessibility;
            if (accessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)
                isPublic = false;
            else if (accessibility != Accessibility.Public)
                return false;
        }

        return true;
    }

    private static string Keyword(SpecialType type)
    {
        if (type == SpecialType.System_Byte)
            return "byte";
        if (type == SpecialType.System_SByte)
            return "sbyte";
        if (type == SpecialType.System_Int16)
            return "short";
        if (type == SpecialType.System_UInt16)
            return "ushort";
        if (type == SpecialType.System_UInt32)
            return "uint";
        if (type == SpecialType.System_Int64)
            return "long";
        return type == SpecialType.System_UInt64 ? "ulong" : "int";
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName)
    {
        foreach (AttributeData attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == metadataName)
                return true;
        }

        return false;
    }

    private static int GetNamedInt(AttributeData attribute, string name)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is int value)
                return value;
        }

        return 0;
    }

    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is string value)
                return value;
        }

        return null;
    }

    private static string? GetConstructorString(AttributeData attribute) =>
        attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value as string : null;

    private static string[] GetConstructorStrings(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Kind != TypedConstantKind.Array)
            return [];

        List<string> values = [];
        foreach (TypedConstant item in attribute.ConstructorArguments[0].Values)
        {
            if (item.Value is string value)
                values.Add(value);
        }

        return [.. values];
    }

    private static string? ReadSummary(IFieldSymbol field, CancellationToken cancellationToken)
    {
        string? xml = field.GetDocumentationCommentXml(cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            string? summary = XElement.Parse(xml).Element("summary")?.Value;
            if (summary is null)
                return null;

            string normalized = string.Join(" ", summary.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return normalized.Length == 0 ? null : normalized;
        }
        catch (XmlException)
        {
            return null;
        }
    }
}
