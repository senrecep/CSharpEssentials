namespace CSharpEssentials.Json;

/// <summary>
/// Lets a polymorphic base read payloads whose discriminator is the full CLR name of a derived type, as written by
/// <see cref="PolymorphicJsonConverterFactory"/> before the hierarchy moved to <see cref="JsonTypeDiscriminatorAttribute"/> values.
/// Writes always use the current discriminator.
/// </summary>
/// <remarks>
/// Needs <see cref="JsonPolymorphismExtensions.AddPolymorphismWithReflection"/> (part of <see cref="EnhancedJsonSerializerOptions.DefaultOptions"/>).
/// A payload with a CLR name is read through a <see cref="System.Text.Json.Nodes.JsonObject"/> copy, so it is slower than a current one.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class JsonAcceptClrTypeNamesAttribute : Attribute;
