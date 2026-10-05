using System.Reflection;
using CSharpEssentials.Core;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Json;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore;

public static class ModelConfigurationExtensions
{
    private static readonly Type EnumToFormattedStringConverterType = typeof(EnumToFormattedStringConverter<>);
    private static readonly Type LegacySnakeCaseEnumConverterType = typeof(LegacySnakeCaseEnumConverter<>);

    /// <summary>
    /// Stores every <see cref="CSharpEssentials.Enums.StringEnumAttribute"/> enum as its JSON name
    /// (<see cref="StringEnumNaming"/>), with a max length that fits the longest name.
    /// </summary>
    public static void ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies) =>
        configurationBuilder.ConfigureEnumConventions(static _ => { }, assemblies);

    /// <summary>
    /// Stores matching enums as strings, configured by <paramref name="configure"/>.
    /// </summary>
    public static void ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        Action<EnumConventionOptions> configure,
        params Assembly[] assemblies)
    {
        var options = new EnumConventionOptions();
        configure(options);
        if (assemblies.Length == 0)
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Type enumType in assemblies
            .SelectMany(GetLoadableTypes)
            .Where(type => type.IsEnum && options.CanConvert(type)))
        {
            Type converterType = options.UseLegacySnakeCase ? LegacySnakeCaseEnumConverterType : EnumToFormattedStringConverterType;
            int enumMaxLength = options.UseLegacySnakeCase
                ? Enum.GetNames(enumType).Max(name => name.ToSnakeCase().Length)
                : StringEnumNaming.GetNames(enumType).Max(name => name.Length);
            configurationBuilder
                .Properties(enumType)
                .HaveConversion(converterType.MakeGenericType(enumType))
                .HaveMaxLength(enumMaxLength);
        }
    }

    // A partially loadable assembly (e.g. a proxy assembly still emitting types) must not break the scan
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }
}
