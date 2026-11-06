namespace CSharpEssentials.Json;

/// <summary>
/// Gives a derived type its type discriminator under every base it derives from that carries
/// <see cref="System.Text.Json.Serialization.JsonPolymorphicAttribute"/> or <see cref="System.Text.Json.Serialization.JsonDerivedTypeAttribute"/>.
/// The base does not have to list the type, so the type can live in any assembly.
/// </summary>
/// <remarks>
/// The bundled source generator registers the type at module initialization, which <see cref="JsonPolymorphismExtensions.AddPolymorphism"/>
/// reads without reflection. <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/> also reads the attribute by reflection.
/// A <see cref="System.Text.Json.Serialization.JsonDerivedTypeAttribute"/> entry on the base for the same type wins over this value.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class JsonTypeDiscriminatorAttribute : Attribute
{
    /// <summary>Creates the attribute with a string discriminator such as <c>"user.created.v1"</c>.</summary>
    /// <param name="typeDiscriminator">The discriminator written to and read from JSON.</param>
    public JsonTypeDiscriminatorAttribute(string typeDiscriminator) => TypeDiscriminator = typeDiscriminator;

    /// <summary>Creates the attribute with an integer discriminator.</summary>
    /// <param name="typeDiscriminator">The discriminator written to and read from JSON.</param>
    public JsonTypeDiscriminatorAttribute(int typeDiscriminator) => TypeDiscriminator = typeDiscriminator;

    /// <summary>The discriminator: a <see cref="string"/> or an <see cref="int"/>.</summary>
    public object TypeDiscriminator { get; }
}
