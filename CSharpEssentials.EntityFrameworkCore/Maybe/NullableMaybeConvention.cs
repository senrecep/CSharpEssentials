using System.Reflection;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// Maps every public read-write <c>Maybe&lt;T&gt;?</c> property of an entity type or a complex type to a nullable
/// column, as <c>HasNullableMaybeConversion</c> does per property. Ignored properties and properties with a converter
/// configured by the user are left alone. <c>Maybe&lt;T&gt;?</c> where <c>T</c> is itself a <see cref="Nullable{T}"/>
/// (<c>Maybe&lt;int?&gt;?</c>) is not supported: the convention skips it and EF Core rejects it when the model is built,
/// so ignore such a property or give it your own converter.
/// </summary>
/// <remarks>
/// EF Core does not discover <c>Maybe&lt;T&gt;?</c> properties because it has no mapping for them, and pre-convention
/// configuration cannot target the open generic type. The convention therefore reads the CLR properties of each entity
/// type and complex type once, while the model is built; nothing here runs per query or per save.
/// </remarks>
internal sealed class NullableMaybeConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IComplexPropertyAddedConvention
{
    private const BindingFlags DeclaredPublicProperties = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

    public void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context) =>
        Discover(entityTypeBuilder, entityTypeBuilder.Metadata.ClrType);

    public void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (entityTypeBuilder.Metadata.BaseType == newBaseType)
            Discover(entityTypeBuilder, entityTypeBuilder.Metadata.ClrType);
    }

    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        IConventionComplexType complexType = propertyBuilder.Metadata.ComplexType;
        Discover(complexType.Builder, complexType.ClrType);
    }

    private static void Discover(IConventionTypeBaseBuilder typeBuilder, Type clrType)
    {
        foreach (PropertyInfo property in clrType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (IsCandidate(property, out Type valueType))
                typeBuilder.Property(property)?.HasConversion(NullableMaybeConverterFactory.Create(valueType));
        }
    }

    private static bool IsCandidate(PropertyInfo property, out Type valueType)
    {
        valueType = typeof(void);
        if (Nullable.GetUnderlyingType(property.PropertyType) is not { IsGenericType: true } maybeType
            || maybeType.GetGenericTypeDefinition() != typeof(Maybe<>)
            || property.GetIndexParameters().Length != 0)
        {
            return false;
        }

        valueType = maybeType.GetGenericArguments()[0];
        return Nullable.GetUnderlyingType(valueType) is null
            && (property.SetMethod is not null
                || property.DeclaringType?.GetProperty(property.Name, DeclaredPublicProperties)?.SetMethod is not null);
    }
}
