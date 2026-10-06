using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Enums;

[Generator(LanguageNames.CSharp)]
public sealed class StringEnumGenerator : IIncrementalGenerator
{
    private const string AttributeName = "CSharpEssentials.Enums.StringEnumAttribute";

    private static readonly string[] LineSeparators = ["\r\n", "\n"];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> enumSymbols = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeName,
                static (node, _) => node is EnumDeclarationSyntax,
                static (ctx, _) =>
                    (INamedTypeSymbol)ctx.TargetSymbol)
            .Where(static symbol =>
                symbol.DeclaredAccessibility != Accessibility.Private &&
                symbol.ContainingType is null);

        IncrementalValueProvider<bool> supportsNullable = context.ParseOptionsProvider
            .Select(static (options, _) => options is CSharpParseOptions csharp && csharp.LanguageVersion >= LanguageVersion.CSharp8);

        context.RegisterSourceOutput(enumSymbols.Combine(supportsNullable), static (spc, pair) =>
        {
            INamedTypeSymbol enumSymbol = pair.Left;
            string source = GenerateExtensionsClass(enumSymbol, pair.Right);
            string ns = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToDisplayString() + ".";
            spc.AddSource($"{ns}{enumSymbol.Name}Extensions.g.cs", source);
        });
    }

    private static string GenerateExtensionsClass(INamedTypeSymbol enumSymbol, bool supportsNullable)
    {
        string ns = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToDisplayString();
        string name = enumSymbol.Name;
        string fullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
        string accessibility = enumSymbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        string underlyingType = enumSymbol.EnumUnderlyingType?.ToDisplayString() ?? "int";
        string nullableString = supportsNullable ? "string?" : "string";

        IFieldSymbol[] members = [.. enumSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(static f => f.ConstantValue is not null)];

        StringBuilder body = new();
        body.AppendLine($"{accessibility} static class {name}Extensions");
        body.AppendLine("{");

        GenerateConstants(body, members);
        GenerateNameSwitch(body, "ToOptimizedString", members, fullName, static (_, access) => $"nameof({access})", "value.ToString()");
        GenerateNameSwitch(body, "ToSnakeCase", members, fullName, static (member, _) => $"\"{ToSnakeCase(member.Name)}\"", "ToSnakeCaseFallback(value.ToOptimizedString())");
        GenerateNameSwitch(body, "ToKebabCase", members, fullName, static (member, _) => $"\"{ToKebabCase(member.Name)}\"", "ToKebabCaseFallback(value.ToOptimizedString())");
        GenerateNameSwitch(body, "ToLowerCase", members, fullName, static (_, access) => $"nameof({access}).ToLowerInvariant()", "value.ToString().ToLowerInvariant()");
        GenerateNameSwitch(body, "ToUpperCase", members, fullName, static (_, access) => $"nameof({access}).ToUpperInvariant()", "value.ToString().ToUpperInvariant()");
        GenerateIsDefined(body, members, fullName);
        GenerateTryParse(body, members, fullName, underlyingType, nullableString);
        GenerateParse(body, fullName, nullableString);
        GenerateGetNames(body, members, fullName);
        GenerateGetValues(body, members, fullName);
        GenerateAsUnderlyingType(body, fullName, underlyingType);
        GenerateFallbackHelpers(body);

        body.AppendLine("}");

        StringBuilder sb = new();
        if (supportsNullable)
        {
            sb.AppendLine("#nullable enable");
        }

        if (string.IsNullOrEmpty(ns))
        {
            sb.Append(body);
            return sb.ToString();
        }

        sb.AppendLine($"namespace {ns}");
        sb.AppendLine("{");
        string[] lines = body.ToString().Split(LineSeparators, StringSplitOptions.None);
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].Length > 0)
            {
                sb.Append("    ");
            }

            sb.AppendLine(lines[i]);
        }

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

    private static void GenerateNameSwitch(
        StringBuilder sb,
        string methodName,
        IFieldSymbol[] members,
        string fullName,
        Func<IFieldSymbol, string, string> result,
        string fallback)
    {
        sb.AppendLine($"    public static string {methodName}(this {fullName} value)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (value)");
        sb.AppendLine("        {");
        foreach (IFieldSymbol member in members)
        {
            string memberName = $"{fullName}.{member.Name}";
            sb.AppendLine($"            case {memberName}:");
            sb.AppendLine($"                return {result(member, memberName)};");
        }
        sb.AppendLine("            default:");
        sb.AppendLine($"                return {fallback};");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateIsDefined(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine("    public static bool IsDefined(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");
        foreach (IFieldSymbol member in members)
        {
            sb.AppendLine($"            case nameof({fullName}.{member.Name}):");
        }
        if (members.Length > 0)
        {
            sb.AppendLine("                return true;");
        }
        sb.AppendLine("            default:");
        sb.AppendLine("                return false;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateTryParse(StringBuilder sb, IFieldSymbol[] members, string fullName, string underlyingType, string nullableString)
    {
        sb.AppendLine($"    public static bool TryParse({nullableString} name, out {fullName} value)");
        sb.AppendLine("    {");
        if (members.Length > 0)
        {
            sb.AppendLine("        switch (name)");
            sb.AppendLine("        {");
            foreach (IFieldSymbol member in members)
            {
                sb.AppendLine($"            case nameof({fullName}.{member.Name}):");
                sb.AppendLine($"                value = {fullName}.{member.Name};");
                sb.AppendLine("                return true;");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine($"        {underlyingType} numericValue;");
        sb.AppendLine($"        if (name != null && {underlyingType}.TryParse(name, out numericValue))");
        sb.AppendLine("        {");
        sb.AppendLine($"            value = ({fullName})numericValue;");
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        value = ({fullName})0;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateParse(StringBuilder sb, string fullName, string nullableString)
    {
        sb.AppendLine($"    public static {fullName} Parse({nullableString} name)");
        sb.AppendLine("    {");
        sb.AppendLine($"        {fullName} value;");
        sb.AppendLine("        if (TryParse(name, out value))");
        sb.AppendLine("        {");
        sb.AppendLine("            return value;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        throw new global::System.ArgumentException(\"Requested value '\" + name + \"' was not found.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateGetNames(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine("    public static string[] GetNames()");
        sb.AppendLine("    {");
        sb.Append("        return new string[] { ");
        for (int i = 0; i < members.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append($"nameof({fullName}.{members[i].Name})");
        }
        sb.AppendLine(" };");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateGetValues(StringBuilder sb, IFieldSymbol[] members, string fullName)
    {
        sb.AppendLine($"    public static {fullName}[] GetValues()");
        sb.AppendLine("    {");
        sb.Append($"        return new {fullName}[] {{ ");
        for (int i = 0; i < members.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append($"{fullName}.{members[i].Name}");
        }
        sb.AppendLine(" };");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateAsUnderlyingType(StringBuilder sb, string fullName, string underlyingType)
    {
        sb.AppendLine($"    public static {underlyingType} AsUnderlyingType(this {fullName} value)");
        sb.AppendLine("    {");
        sb.AppendLine($"        return ({underlyingType})value;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateFallbackHelpers(StringBuilder sb)
    {
        GenerateFallbackHelper(sb, "ToSnakeCaseFallback", '_');
        sb.AppendLine();
        GenerateFallbackHelper(sb, "ToKebabCaseFallback", '-');
        sb.AppendLine();
        sb.AppendLine("    private static bool CheckCategory(global::System.Globalization.UnicodeCategory previous, global::System.Globalization.UnicodeCategory current)");
        sb.AppendLine("    {");
        sb.AppendLine("        return previous != current && (current == global::System.Globalization.UnicodeCategory.UppercaseLetter || current == global::System.Globalization.UnicodeCategory.DecimalDigitNumber || IsSpecialCharacter(previous) && !IsSpecialCharacter(current));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static bool IsSpecialCharacter(global::System.Globalization.UnicodeCategory category)");
        sb.AppendLine("    {");
        sb.AppendLine("        return category != global::System.Globalization.UnicodeCategory.UppercaseLetter");
        sb.AppendLine("            && category != global::System.Globalization.UnicodeCategory.LowercaseLetter");
        sb.AppendLine("            && category != global::System.Globalization.UnicodeCategory.DecimalDigitNumber;");
        sb.AppendLine("    }");
    }

    private static void GenerateFallbackHelper(StringBuilder sb, string methodName, char separator)
    {
        sb.AppendLine($"    private static string {methodName}(string input)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (string.IsNullOrEmpty(input)) return input;");
        sb.AppendLine("        global::System.Text.StringBuilder sb = new global::System.Text.StringBuilder(input.Length + 4);");
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
        sb.AppendLine($"                if (insertSeparator && !isFirst) sb.Append('{separator}');");
        sb.AppendLine("                sb.Append(char.ToLowerInvariant(c));");
        sb.AppendLine("                isFirst = false;");
        sb.AppendLine("            }");
        sb.AppendLine("            previous = current;");
        sb.AppendLine("        }");
        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
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
