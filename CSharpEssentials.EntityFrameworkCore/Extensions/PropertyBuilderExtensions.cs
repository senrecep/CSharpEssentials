using System.Text.Json;
using CSharpEssentials.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Maybe<T>> MaybeConversion<T>(this PropertyBuilder<Maybe<T>> builder)
    {
        return builder.HasConversion(
            value => value.GetValueOrDefault(),
            value => Maybe<T>.From(value));
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