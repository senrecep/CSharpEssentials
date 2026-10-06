using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class ConstantFormatter
{
    public static string Format(TypedConstant constant)
    {
        if (constant.IsNull)
        {
            return "null";
        }

        if (constant.Kind == TypedConstantKind.Type)
        {
            return "typeof(" + TypeNames.ForTypeOf((ITypeSymbol)constant.Value!) + ")";
        }

        if (constant.Kind == TypedConstantKind.Enum)
        {
            return FormatEnum((INamedTypeSymbol)constant.Type!, constant.Value!);
        }

        return constant.Kind == TypedConstantKind.Array ? FormatArray(constant) : FormatPrimitive(constant.Value!);
    }

    public static string FormatDefault(ITypeSymbol type, object? value)
    {
        ITypeSymbol valueType = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable
            ? nullable.TypeArguments[0]
            : type;
        if (value is null)
        {
            return "default(" + TypeNames.FullyQualified(type) + ")";
        }

        return valueType is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType
            ? FormatEnum(enumType, value)
            : FormatPrimitive(value);
    }

    private static string FormatArray(TypedConstant constant)
    {
        string elementType = constant.Type is IArrayTypeSymbol array ? TypeNames.FullyQualified(array.ElementType) : "object";
        return "new " + elementType + "[] { " + string.Join(", ", constant.Values.Select(Format)) + " }";
    }

    private static string FormatEnum(INamedTypeSymbol enumType, object value)
    {
        string typeName = TypeNames.FullyQualified(enumType);
        IFieldSymbol? member = enumType.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(field => field.HasConstantValue && Equals(field.ConstantValue, value));
        return member is null
            ? "((" + typeName + ")" + FormatPrimitive(value) + ")"
            : typeName + "." + member.Name;
    }

    private static string FormatPrimitive(object value) => value switch
    {
        string text => SymbolDisplay.FormatLiteral(text, quote: true),
        char character => SymbolDisplay.FormatLiteral(character, quote: true),
        bool flag => flag ? "true" : "false",
        int number => number.ToString(CultureInfo.InvariantCulture),
        float number => FormatSingle(number),
        double number => FormatDouble(number),
        decimal number => number.ToString(CultureInfo.InvariantCulture) + "m",
        byte number => Cast("byte", number.ToString(CultureInfo.InvariantCulture)),
        sbyte number => Cast("sbyte", number.ToString(CultureInfo.InvariantCulture)),
        short number => Cast("short", number.ToString(CultureInfo.InvariantCulture)),
        ushort number => Cast("ushort", number.ToString(CultureInfo.InvariantCulture)),
        uint number => number.ToString(CultureInfo.InvariantCulture) + "u",
        long number => number.ToString(CultureInfo.InvariantCulture) + "L",
        ulong number => number.ToString(CultureInfo.InvariantCulture) + "UL",
        _ => SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false),
    };

    private static string FormatSingle(float number)
    {
        if (float.IsNaN(number))
        {
            return "float.NaN";
        }

        if (float.IsInfinity(number))
        {
            return number > 0 ? "float.PositiveInfinity" : "float.NegativeInfinity";
        }

        return number.ToString("R", CultureInfo.InvariantCulture) + "f";
    }

    private static string FormatDouble(double number)
    {
        if (double.IsNaN(number))
        {
            return "double.NaN";
        }

        if (double.IsInfinity(number))
        {
            return number > 0 ? "double.PositiveInfinity" : "double.NegativeInfinity";
        }

        return number.ToString("R", CultureInfo.InvariantCulture) + "d";
    }

    private static string Cast(string keyword, string literal) => "((" + keyword + ")" + literal + ")";
}
