namespace CSharpEssentials.Json;

/// <summary>
/// Sets the type discriminator property name for the polymorphic values of one property or field. The property type, or its element
/// type for arrays, collections, dictionary values and <see cref="Nullable{T}"/>, must be a base with
/// <see cref="System.Text.Json.Serialization.JsonPolymorphicAttribute"/> or <see cref="System.Text.Json.Serialization.JsonDerivedTypeAttribute"/>.
/// </summary>
/// <remarks>
/// The name applies to every value of that base inside the property, nested values included. The values are serialized in a nested call,
/// so options that preserve references throw <see cref="NotSupportedException"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonPolymorphicPropertyAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="typeDiscriminatorPropertyName">The JSON property that holds the type discriminator, such as <c>"kind"</c>.</param>
    public JsonPolymorphicPropertyAttribute(string typeDiscriminatorPropertyName) =>
        TypeDiscriminatorPropertyName = typeDiscriminatorPropertyName;

    /// <summary>The JSON property that holds the type discriminator.</summary>
    public string TypeDiscriminatorPropertyName { get; }
}
