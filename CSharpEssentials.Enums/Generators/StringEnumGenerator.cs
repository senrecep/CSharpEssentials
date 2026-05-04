using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Enums;

[Generator(LanguageNames.CSharp)]
public sealed class StringEnumGenerator : IIncrementalGenerator
{
    private const string AttributeName = "CSharpEssentials.Enums.StringEnumAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> enumSymbols = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (node, _) => node is EnumDeclarationSyntax,
                static (ctx, _) =>
                    (INamedTypeSymbol)ctx.TargetSymbol)
            .Where(static symbol => symbol.DeclaredAccessibility != Accessibility.Private);

        context.RegisterSourceOutput(enumSymbols, static (spc, enumSymbol) =>
        {
            string source = GenerateExtensionsClass(enumSymbol);
            spc.AddSource($"{enumSymbol.Name}Extensions.g.cs", source);
        });
    }

    private static string GenerateExtensionsClass(INamedTypeSymbol enumSymbol)
    {
        string ns = enumSymbol.ContainingNamespace.ToDisplayString();
        string name = enumSymbol.Name;
        string fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        string accessibility = enumSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToDisplayString() ?? "int";

        IFieldSymbol[] members = [.. enumSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(static f => f.ConstantValue is not null)];

        StringBuilder sb = new();
        sb.AppendLine("#nullable enable");
        if (!string.IsNullOrEmpty(ns))
        {
            sb.AppendLine($"namespace {ns};");
        }

        sb.AppendLine($"{accessibility} static class {name}Extensions");
        sb.AppendLine("{");

        GenerateConstants(sb, members);
        GenerateToOptimizedString(sb, members, fullName);
        GenerateToSnakeCase(sb, members, fullName);
        GenerateToKebabCase(sb, members, fullName);
        GenerateToLowerCase(sb, members, fullName);
        GenerateToUpperCase(sb, members, fullName);
        GenerateIsDefined(sb, members, fullName);
        GenerateTryParse(sb, members, fullName, underlyingType);
        GenerateParse(sb, fullName);
        GenerateGetNames(sb, members, fullName);
        GenerateGetValues(sb, members, fullName);
        GenerateAsUnderlyingType(sb, fullName, underlyingType);
        GenerateFallbackHelpers(sb);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void GenerateConstants(StringBuilder sb, IFieldSymbol[] members)
    {
        foreach (IFieldSymbol member in members)
        {
            string snake = ToSnakeCase(member.Name);
            string kebab = ToKebabCase(member.Name);
            sb.AppendLine($"    public const string {member.Name}SnakeCase = \"{snake}\";");
            sb.AppendLine($"    public const string {member.Name}KebabCase = \"{kebab}\";");
        }

        if (members.Length > 0)
        {
            sb.AppendLine();
        }
    }

    private static void GenerateToOptimizedString(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static string ToOptimizedString(this {fullName} value) => value switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"        {fullName}.{member.Name} => nameof({fullName}.{member.Name}),");
        }
        sb.AppendLine($"        _ => value.ToString()");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateToSnakeCase(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static string ToSnakeCase(this {fullName} value) => value switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            string snake = ToSnakeCase(member.Name);
            sb.AppendLine($"        {fullName}.{member.Name} => \"{snake}\",");
        }
        sb.AppendLine($"        _ => ToSnakeCaseFallback(value.ToOptimizedString())");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateToKebabCase(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static string ToKebabCase(this {fullName} value) => value switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            string kebab = ToKebabCase(member.Name);
            sb.AppendLine($"        {fullName}.{member.Name} => \"{kebab}\",");
        }
        sb.AppendLine($"        _ => ToKebabCaseFallback(value.ToOptimizedString())");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateToLowerCase(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static string ToLowerCase(this {fullName} value) => value switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"        {fullName}.{member.Name} => nameof({fullName}.{member.Name}).ToLowerInvariant(),");
        }
        sb.AppendLine($"        _ => value.ToString().ToLowerInvariant()");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateToUpperCase(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static string ToUpperCase(this {fullName} value) => value switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"        {fullName}.{member.Name} => nameof({fullName}.{member.Name}).ToUpperInvariant(),");
        }
        sb.AppendLine($"        _ => value.ToString().ToUpperInvariant()");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateIsDefined(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static bool IsDefined(string name) => name switch");
        sb.AppendLine("    {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"        nameof({fullName}.{member.Name}) => true,");
        }
        sb.AppendLine("        _ => false");
        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void GenerateTryParse(StringBuilder sb, IFieldSymbol[] members, string fullName, string underlyingType)
    {
        sb.AppendLine($"    public static bool TryParse(string? name, out {fullName} value)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"            case string s when s.Equals(nameof({fullName}.{member.Name}), global::System.StringComparison.Ordinal):");
            sb.AppendLine($"                value = {fullName}.{member.Name};");
            sb.AppendLine("                return true;");
        }
        sb.AppendLine($"            case string s when {underlyingType}.TryParse(s, out var numericValue):");
        sb.AppendLine($"                value = ({fullName})numericValue;");
        sb.AppendLine("                return true;");
        sb.AppendLine("            default:");
        sb.AppendLine($"                value = ({fullName})0;");
        sb.AppendLine("                return false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateParse(StringBuilder sb, string fullName)
    {
        sb.AppendLine($"    public static {fullName} Parse(string? name) =>");
        sb.AppendLine("        TryParse(name, out var value) ? value : ThrowValueNotFound(name);");
        sb.AppendLine();
        sb.AppendLine($"    private static {fullName} ThrowValueNotFound(string? name) =>");
        sb.AppendLine("        throw new global::System.ArgumentException($\"Requested value '{name}' was not found.\");");
        sb.AppendLine();
    }

    private static void GenerateGetNames(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine("    public static string[] GetNames() =>");
        sb.Append("        [");
        for (int i = 0; i < members.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append($"nameof({fullName}.{members[i].Name})");
        }
        sb.AppendLine("];");
        sb.AppendLine();
    }

    private static void GenerateGetValues(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static {fullName}[] GetValues() =>");
        sb.Append("        [");
        for (int i = 0; i < members.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append($"{fullName}.{members[i].Name}");
        }
        sb.AppendLine("];");
        sb.AppendLine();
    }

    private static void GenerateAsUnderlyingType(StringBuilder sb, string fullName, string underlyingType)
    {
        sb.AppendLine($"    public static {underlyingType} AsUnderlyingType(this {fullName} value) => ({underlyingType})value;");
        sb.AppendLine();
    }

    private static void GenerateFallbackHelpers(StringBuilder sb)
    {
        sb.AppendLine("    private static string ToSnakeCaseFallback(string input)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.IsNullOrEmpty(input)) return input;");
        sb.AppendLine("        global::System.Text.StringBuilder sb = new(input.Length + 4);");
        sb.AppendLine("        global::System.Globalization.UnicodeCategory previous = global::System.Globalization.UnicodeCategory.OtherSymbol;");
        sb.AppendLine("        bool isFirst = true;");
        sb.AppendLine("        for (int i = 0; i < input.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            char c = input[i];");
        sb.AppendLine("            global::System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);");
        sb.AppendLine("            bool insertSeparator = CheckCategory(previous, current);");
        sb.AppendLine("            bool isSpecial = IsSpecialCharacter(current);");
        sb.AppendLine("            if (!isSpecial)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (insertSeparator && !isFirst) sb.Append('_');");
        sb.AppendLine("                sb.Append(char.ToLowerInvariant(c));");
        sb.AppendLine("                isFirst = false;");
        sb.AppendLine("            }");
        sb.AppendLine("            previous = current;");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static string ToKebabCaseFallback(string input)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.IsNullOrEmpty(input)) return input;");
        sb.AppendLine("        global::System.Text.StringBuilder sb = new(input.Length + 4);");
        sb.AppendLine("        global::System.Globalization.UnicodeCategory previous = global::System.Globalization.UnicodeCategory.OtherSymbol;");
        sb.AppendLine("        bool isFirst = true;");
        sb.AppendLine("        for (int i = 0; i < input.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            char c = input[i];");
        sb.AppendLine("            global::System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);");
        sb.AppendLine("            bool insertSeparator = CheckCategory(previous, current);");
        sb.AppendLine("            bool isSpecial = IsSpecialCharacter(current);");
        sb.AppendLine("            if (!isSpecial)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (insertSeparator && !isFirst) sb.Append('-');");
        sb.AppendLine("                sb.Append(char.ToLowerInvariant(c));");
        sb.AppendLine("                isFirst = false;");
        sb.AppendLine("            }");
        sb.AppendLine("            previous = current;");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static bool CheckCategory(global::System.Globalization.UnicodeCategory previous, global::System.Globalization.UnicodeCategory current) =>");
        sb.AppendLine("        previous != current && (current is global::System.Globalization.UnicodeCategory.UppercaseLetter || current is global::System.Globalization.UnicodeCategory.DecimalDigitNumber || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));");
        sb.AppendLine();
        sb.AppendLine("    private static bool IsSpecialCharacter(global::System.Globalization.UnicodeCategory category) =>");
        sb.AppendLine("        category is not global::System.Globalization.UnicodeCategory.UppercaseLetter");
        sb.AppendLine("             and not global::System.Globalization.UnicodeCategory.LowercaseLetter");
        sb.AppendLine("             and not global::System.Globalization.UnicodeCategory.DecimalDigitNumber;");
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        StringBuilder sb = new(input.Length + 4);
        System.Globalization.UnicodeCategory previous = System.Globalization.UnicodeCategory.OtherSymbol;
        bool isFirst = true;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);
            bool insertSeparator = CheckCategory(previous, current);
            bool isSpecial = IsSpecialCharacter(current);
            if (!isSpecial)
            {
                if (insertSeparator && !isFirst)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
                isFirst = false;
            }
            previous = current;
        }
        return sb.ToString();
    }

    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        StringBuilder sb = new(input.Length + 4);
        System.Globalization.UnicodeCategory previous = System.Globalization.UnicodeCategory.OtherSymbol;
        bool isFirst = true;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            System.Globalization.UnicodeCategory current = char.GetUnicodeCategory(c);
            bool insertSeparator = CheckCategory(previous, current);
            bool isSpecial = IsSpecialCharacter(current);
            if (!isSpecial)
            {
                if (insertSeparator && !isFirst)
                    sb.Append('-');
                sb.Append(char.ToLowerInvariant(c));
                isFirst = false;
            }
            previous = current;
        }
        return sb.ToString();
    }

    private static bool CheckCategory(System.Globalization.UnicodeCategory previous, System.Globalization.UnicodeCategory current) =>
        previous != current && (current is System.Globalization.UnicodeCategory.UppercaseLetter || current is System.Globalization.UnicodeCategory.DecimalDigitNumber || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));

    private static bool IsSpecialCharacter(System.Globalization.UnicodeCategory category) =>
        category is not System.Globalization.UnicodeCategory.UppercaseLetter
             and not System.Globalization.UnicodeCategory.LowercaseLetter
             and not System.Globalization.UnicodeCategory.DecimalDigitNumber;
}
