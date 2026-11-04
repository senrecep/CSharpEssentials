namespace CSharpEssentials.Json.Generators;

/// <summary>A property or field with <c>[JsonPolymorphicProperty]</c>.</summary>
/// <param name="DeclaringType">The fully qualified declaring type.</param>
/// <param name="MemberName">The member name.</param>
/// <param name="PropertyType">The fully qualified property type.</param>
/// <param name="BaseType">The fully qualified polymorphic base inside the property type.</param>
/// <param name="PropertyName">The discriminator property name as a C# literal.</param>
internal sealed record PropertyModel(string DeclaringType, string MemberName, string PropertyType, string BaseType, string PropertyName);
