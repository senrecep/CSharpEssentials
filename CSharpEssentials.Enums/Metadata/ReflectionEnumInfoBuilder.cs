using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSharpEssentials.Enums;

/// <summary>
/// Builds <see cref="EnumInfo{TEnum}"/> with reflection for enums without generated metadata.
/// </summary>
internal static class ReflectionEnumInfoBuilder
{
    private const string JsonMemberNameAttribute = "System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute";
    private const string EnumMemberAttribute = "System.Runtime.Serialization.EnumMemberAttribute";

    [RequiresUnreferencedCode(EnumMetadata.ReflectionMessage)]
    [RequiresDynamicCode(EnumMetadata.ReflectionMessage)]
    public static IEnumInfo Build(Type enumType, EnumNaming naming) =>
        (IEnumInfo)new Func<EnumNaming, EnumInfo<DayOfWeek>>(BuildTyped<DayOfWeek>).Method.GetGenericMethodDefinition()
            .MakeGenericMethod(enumType).Invoke(null, [naming]);

    [RequiresUnreferencedCode(EnumMetadata.ReflectionMessage)]
    private static EnumInfo<TEnum> BuildTyped<TEnum>(EnumNaming naming) where TEnum : struct, Enum
    {
        Type type = typeof(TEnum);
        StringEnumAttribute? stringEnum = type.GetCustomAttribute<StringEnumAttribute>();
        EnumNaming effective = stringEnum is { Naming: not EnumNaming.Default } ? stringEnum.Naming : naming;
        if (effective == EnumNaming.Default)
            effective = EnumNaming.SnakeCaseLower;

        bool signed = EnumTypeTraits<TEnum>.IsSigned;
        List<EnumMemberInfo<TEnum>> members = [];
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not TEnum value)
                continue;
            ulong raw = ToRaw(value, signed);
            string wireName = GetDeclaredName(field) ?? EnumNameConverter.Convert(field.Name, effective);

            List<string> aliases = [.. field.GetCustomAttribute<EnumAliasAttribute>()?.Aliases ?? []];
            string legacy = EnumNameConverter.ToLegacySnakeCase(field.Name);
            if (!string.Equals(legacy, wireName, StringComparison.Ordinal) && !aliases.Contains(legacy, StringComparer.Ordinal))
                aliases.Add(legacy);

            members.Add(new EnumMemberInfo<TEnum>(value, raw, field.Name, wireName)
            {
                Aliases = aliases,
                Description = field.GetCustomAttribute<DescriptionAttribute>()?.Description,
                IsObsolete = field.IsDefined(typeof(ObsoleteAttribute), inherit: false),
                IsFallback = field.IsDefined(typeof(EnumFallbackAttribute), inherit: false),
            });
        }

        return new EnumInfo<TEnum>(
            members,
            value => ToRaw(value, signed),
            raw => (TEnum)(signed ? Enum.ToObject(type, unchecked((long)raw)) : Enum.ToObject(type, raw)),
            type.IsDefined(typeof(FlagsAttribute), inherit: false),
            stringEnum?.Storage ?? EnumStorage.Default);
    }

    private static ulong ToRaw<TEnum>(TEnum value, bool signed) where TEnum : struct, Enum =>
        signed
            ? unchecked((ulong)System.Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture))
            : System.Convert.ToUInt64(value, System.Globalization.CultureInfo.InvariantCulture);

    private static string? GetDeclaredName(FieldInfo field)
    {
        string? enumMemberValue = null;
        foreach (CustomAttributeData attribute in field.GetCustomAttributesData())
        {
            string? name = attribute.AttributeType.FullName;
            if (name == JsonMemberNameAttribute && attribute.ConstructorArguments.Count == 1 &&
                attribute.ConstructorArguments[0].Value is string jsonName)
            {
                return jsonName;
            }

            if (name == EnumMemberAttribute)
            {
                foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
                {
                    if (argument.MemberName == "Value" && argument.TypedValue.Value is string value)
                        enumMemberValue = value;
                }
            }
        }

        return enumMemberValue;
    }
}
