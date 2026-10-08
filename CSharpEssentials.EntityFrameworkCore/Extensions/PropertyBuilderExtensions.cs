using System.Text.Json;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Json;
using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore;

public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Stores a non-nullable <see cref="Maybe{T}"/> property in a <c>NOT NULL</c> column of <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This mapping cannot represent <see cref="Maybe{T}.None"/>. For a value type <typeparamref name="T"/> it is lossy:
    /// <c>None</c> is stored as <c>default(T)</c> and read back as <c>Some(default(T))</c>. For a reference type
    /// <typeparamref name="T"/> it writes <c>NULL</c> into the <c>NOT NULL</c> column, so inserting <c>None</c> fails. EF Core
    /// cannot make a non-nullable struct property optional. To store absence, declare the property as
    /// <c>Maybe&lt;T&gt;?</c> and use <c>HasNullableMaybeConversion</c> or <c>ConfigureNullableMaybeConventions</c>.
    /// </remarks>
    /// <typeparam name="T">The value type of the <see cref="Maybe{T}"/>.</typeparam>
    /// <param name="builder">The property builder.</param>
    /// <returns>The same builder.</returns>
    public static PropertyBuilder<Maybe<T>> MaybeConversion<T>(this PropertyBuilder<Maybe<T>> builder)
    {
        return builder.HasConversion(
            value => value.GetValueOrDefault(),
            value => Maybe<T>.From(value));
    }

    /// <summary>
    /// Stores a <c>Maybe&lt;T&gt;?</c> property of a value type in a nullable column of <typeparamref name="T"/> with
    /// <see cref="NullableMaybeConverter{T}"/>. <see langword="null"/> and <see cref="Maybe{T}.None"/> are both written as
    /// <c>NULL</c>, and <c>NULL</c> is read back as <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="builder">The property builder.</param>
    /// <returns>The same builder.</returns>
    public static PropertyBuilder<Maybe<T>?> HasNullableMaybeConversion<T>(this PropertyBuilder<Maybe<T>?> builder)
        where T : struct
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasConversion(new NullableMaybeConverter<T>());
    }

    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder, string columnType = "jsonb", JsonSerializerOptions? options = null)
    {
        JsonSerializerOptions jsonOptions = options ?? EnhancedJsonSerializerOptions.DefaultOptions;
        return builder.HasConversion(
                  v => v.ConvertToJson(jsonOptions),
                  v => v.ConvertFromJson<TProperty>(jsonOptions) ?? default!)
              .HasColumnType(columnType);
    }
}
