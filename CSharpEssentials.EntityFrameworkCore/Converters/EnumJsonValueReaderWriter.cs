using System.Text.Json;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Storage.Json;
#if NET9_0_OR_GREATER
using System.Linq.Expressions;
using System.Reflection;
#endif

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Reads and writes an enum inside a JSON column mapped by EF Core (<c>ToJson()</c>, design section 11.3). Reads accept wire names,
/// every other spelling and the numbers EF Core wrote before; writes follow the storage of the property.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class EnumJsonValueReaderWriter<TEnum> : JsonValueReaderWriter<TEnum> where TEnum : struct, Enum
{
#if NET9_0_OR_GREATER
    private static readonly ConstructorInfo Constructor = typeof(EnumJsonValueReaderWriter<TEnum>).GetConstructor(
        [typeof(EnumInfo<TEnum>), typeof(EnumConventions), typeof(EnumStorage), typeof(EnumStoredAs?)])!;
#endif

    private readonly EnumColumnCodec<TEnum> _codec;
    private readonly bool _writeNumbers;

    /// <summary>Creates a reader/writer that writes wire names, with the generated metadata of <typeparamref name="TEnum"/> and <see cref="EnumConventions.Default"/>.</summary>
    public EnumJsonValueReaderWriter()
        : this(EnumMetadata.Get<TEnum>(), EnumConventions.Default)
    {
    }

    /// <summary>Creates the reader/writer.</summary>
    /// <param name="info">The enum metadata.</param>
    /// <param name="conventions">The conventions used for reads.</param>
    /// <param name="storage">The storage: <see cref="EnumStorage.Integer"/> writes numbers, otherwise wire names.</param>
    /// <param name="legacyFormat">The format kept by <c>HasLegacyEnumStorage</c>; it wins over <paramref name="storage"/>.</param>
    public EnumJsonValueReaderWriter(EnumInfo<TEnum> info, EnumConventions conventions, EnumStorage storage = EnumStorage.String, EnumStoredAs? legacyFormat = null)
    {
        _codec = new EnumColumnCodec<TEnum>(info, conventions, legacyFormat == EnumStoredAs.Integer ? null : legacyFormat);
        _writeNumbers = legacyFormat == EnumStoredAs.Integer || legacyFormat is null && storage == EnumStorage.Integer;
        Info = info;
        Conventions = conventions;
        Storage = storage;
        LegacyFormat = legacyFormat;
    }

    /// <summary>The enum metadata.</summary>
    public EnumInfo<TEnum> Info { get; }

    /// <summary>The conventions used for reads.</summary>
    public EnumConventions Conventions { get; }

    /// <summary>The storage.</summary>
    public EnumStorage Storage { get; }

    /// <summary>The format kept by <c>HasLegacyEnumStorage</c>, if any.</summary>
    public EnumStoredAs? LegacyFormat { get; }

#if NET9_0_OR_GREATER
    /// <inheritdoc />
    public override Expression ConstructorExpression =>
        Expression.New(
            Constructor,
            Expression.Constant(Info),
            Expression.Constant(Conventions),
            Expression.Constant(Storage),
            Expression.Constant(LegacyFormat, typeof(EnumStoredAs?)));
#endif

    /// <inheritdoc />
    public override TEnum FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        ref Utf8JsonReader reader = ref manager.CurrentReader;
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.TryGetInt64(out long number)
                ? _codec.FromNumber(number)
                : _codec.FromNumber(reader.GetUInt64());
        }

        return _codec.FromText(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TEnum value)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        if (!_writeNumbers)
        {
            writer.WriteStringValue(_codec.ToText(value));
            return;
        }

        ulong raw = _codec.ToRaw(value);
        if (IsSigned(Info.UnderlyingType))
            writer.WriteNumberValue((long)raw);
        else
            writer.WriteNumberValue(raw);
    }

    private static bool IsSigned(Type underlyingType) =>
        underlyingType == typeof(sbyte) || underlyingType == typeof(short) || underlyingType == typeof(int) || underlyingType == typeof(long);
}
