using System.Reflection;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore;
using CSharpEssentials.Core;

namespace CSharpEssentials.EntityFrameworkCore;

public static class ModelConfigurationExtensions
{
    private static readonly Type EnumToFormattedStringConverterType = typeof(EnumToFormattedStringConverter<>);
    public static void ConfigureEnumConventions(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Type enumType in assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsEnum && type.GetCustomAttribute<StringEnumAttribute>() != null))
        {
            int enumMaxLength = Enum.GetNames(enumType).Max(name => name.ToSnakeCase().Length);
            configurationBuilder
                .Properties(enumType)
                .HaveConversion(EnumToFormattedStringConverterType.MakeGenericType(enumType))
                .HaveMaxLength(enumMaxLength);
        }
    }
}