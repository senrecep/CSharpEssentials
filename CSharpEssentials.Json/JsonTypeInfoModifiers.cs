using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// <see cref="JsonTypeInfo"/> modifiers for <see cref="DefaultJsonTypeInfoResolver.Modifiers"/> or
/// <see cref="JsonTypeInfoResolver.WithAddedModifier"/>. They read only the metadata the resolver built, so they are trim and
/// native AOT safe.
/// </summary>
public static class JsonTypeInfoModifiers
{
    /// <summary>
    /// Marks a property as required when it binds to a constructor parameter that is a non-nullable value type and has no default
    /// value, so a missing <c>int</c>, enum, <c>DateTime</c> or <c>Guid</c> fails deserialization instead of binding as
    /// <see langword="default"/>. Reference types and <see cref="Nullable{T}"/> parameters stay optional, and so does a property
    /// without a setter (a get-only property assigned in the constructor), because System.Text.Json rejects a required property
    /// that cannot be set.
    /// </summary>
    /// <remarks>
    /// A missing required property throws <see cref="System.Text.Json.JsonException"/>. Serialization does not change.
    /// <c>RespectRequiredConstructorParameters</c> on .NET 9 and later goes further and requires every parameter without a default.
    /// </remarks>
    /// <param name="typeInfo">The contract to change.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeInfo"/> is <see langword="null"/>.</exception>
    public static void RequireValueTypeConstructorParameters(JsonTypeInfo typeInfo)
    {
        _ = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.AssociatedParameter is { HasDefaultValue: false, ParameterType: { IsValueType: true } type }
                && property.Set is not null
                && Nullable.GetUnderlyingType(type) is null)
                property.IsRequired = true;
        }
    }
}
