namespace CSharpEssentials.Json;

/// <summary>
/// Lets a polymorphic base read payloads whose discriminator is the full CLR name of a derived type, as written by
/// <see cref="PolymorphicJsonConverterFactory"/> before the hierarchy moved to <see cref="JsonTypeDiscriminatorAttribute"/> values.
/// Writes use the current discriminator.
/// </summary>
/// <remarks>
/// Needs <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/> (part of <see cref="EnhancedJsonSerializerOptions.DefaultOptions"/>).
/// When the base uses another discriminator name, a CLR name in <c>$type</c> is read too. Every payload of the base is read through a
/// <see cref="System.Text.Json.Nodes.JsonObject"/> copy. A value typed as <see cref="object"/> is written without a discriminator. Throws
/// <see cref="NotSupportedException"/> when the options preserve references.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class JsonAcceptClrTypeNamesAttribute : Attribute;
