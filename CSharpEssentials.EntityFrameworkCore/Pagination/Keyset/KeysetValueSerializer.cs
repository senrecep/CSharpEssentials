using System.Globalization;
using System.Text.Json;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>Writes and reads the typed key values of a cursor payload.</summary>
internal static class KeysetValueSerializer
{
    public static bool IsSupported(Type type) =>
        type.IsEnum
            ? Type.GetTypeCode(Enum.GetUnderlyingType(type)) != TypeCode.UInt64
            : type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)
              || type == typeof(decimal) || type == typeof(double) || type == typeof(float)
              || type == typeof(string) || type == typeof(Guid)
              || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan)
              || type == typeof(DateOnly) || type == typeof(TimeOnly);

    public static void Write(Utf8JsonWriter writer, Type type, object value)
    {
        switch (value)
        {
            case Enum:
                writer.WriteNumberValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                break;
            case int v:
                writer.WriteNumberValue(v);
                break;
            case long v:
                writer.WriteNumberValue(v);
                break;
            case short v:
                writer.WriteNumberValue(v);
                break;
            case byte v:
                writer.WriteNumberValue(v);
                break;
            case decimal v:
                writer.WriteNumberValue(v);
                break;
            case double v when !double.IsFinite(v):
                writer.WriteStringValue(v.ToString(CultureInfo.InvariantCulture));
                break;
            case double v:
                writer.WriteNumberValue(v);
                break;
            case float v when !float.IsFinite(v):
                writer.WriteStringValue(v.ToString(CultureInfo.InvariantCulture));
                break;
            case float v:
                writer.WriteNumberValue(v);
                break;
            case string v:
                writer.WriteStringValue(v);
                break;
            case Guid v:
                writer.WriteStringValue(v);
                break;
            case DateTime v:
                writer.WriteStringValue(v);
                break;
            case DateTimeOffset v:
                writer.WriteStringValue(v);
                break;
            case TimeSpan v:
                writer.WriteNumberValue(v.Ticks);
                break;
            case DateOnly v:
                writer.WriteNumberValue(v.DayNumber);
                break;
            case TimeOnly v:
                writer.WriteNumberValue(v.Ticks);
                break;
            default:
                throw new NotSupportedException($"Keyset key type '{type}' is not supported.");
        }
    }

    public static bool TryRead(JsonElement element, Type type, out object? value)
    {
        value = null;
        if (type.IsEnum)
        {
            if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt64(out long raw))
                return false;
            value = Enum.ToObject(type, raw);
            return true;
        }

        if (type == typeof(string))
        {
            if (element.ValueKind != JsonValueKind.String)
                return false;
            value = element.GetString();
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number)
            return TryReadNumber(element, type, out value);

        return element.ValueKind == JsonValueKind.String && TryReadString(element, type, out value);
    }

    private static bool TryReadNumber(JsonElement element, Type type, out object? value)
    {
        value = null;
        if (type == typeof(int) && element.TryGetInt32(out int i))
            value = i;
        else if (type == typeof(long) && element.TryGetInt64(out long l))
            value = l;
        else if (type == typeof(short) && element.TryGetInt16(out short s))
            value = s;
        else if (type == typeof(byte) && element.TryGetByte(out byte b))
            value = b;
        else if (type == typeof(decimal) && element.TryGetDecimal(out decimal m))
            value = m;
        else if (type == typeof(double) && element.TryGetDouble(out double d))
            value = d;
        else if (type == typeof(float) && element.TryGetSingle(out float f))
            value = f;
        else if (type == typeof(TimeSpan) && element.TryGetInt64(out long spanTicks))
            value = TimeSpan.FromTicks(spanTicks);
        else if (type == typeof(DateOnly) && element.TryGetInt32(out int dayNumber)
                 && dayNumber >= DateOnly.MinValue.DayNumber && dayNumber <= DateOnly.MaxValue.DayNumber)
            value = DateOnly.FromDayNumber(dayNumber);
        else if (type == typeof(TimeOnly) && element.TryGetInt64(out long timeTicks)
                 && timeTicks >= TimeOnly.MinValue.Ticks && timeTicks <= TimeOnly.MaxValue.Ticks)
            value = new TimeOnly(timeTicks);

        return value is not null;
    }

    private static bool TryReadString(JsonElement element, Type type, out object? value)
    {
        value = null;
        if (type == typeof(Guid) && element.TryGetGuid(out Guid guid))
            value = guid;
        else if (type == typeof(DateTime) && element.TryGetDateTime(out DateTime dateTime))
            value = dateTime;
        else if (type == typeof(DateTimeOffset) && element.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffset))
            value = dateTimeOffset;
        else if (type == typeof(double) && TryReadNonFinite(element, out double nonFinite))
            value = nonFinite;
        else if (type == typeof(float) && TryReadNonFinite(element, out double nonFiniteSingle))
            value = (float)nonFiniteSingle;

        return value is not null;
    }

    // Non-finite doubles and floats are written as the invariant strings "NaN", "Infinity" and "-Infinity".
    private static bool TryReadNonFinite(JsonElement element, out double value)
    {
        if (element.ValueEquals("NaN"))
            value = double.NaN;
        else if (element.ValueEquals("Infinity"))
            value = double.PositiveInfinity;
        else if (element.ValueEquals("-Infinity"))
            value = double.NegativeInfinity;
        else
        {
            value = 0;
            return false;
        }

        return true;
    }
}
