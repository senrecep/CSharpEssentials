using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Json;

namespace CSharpEssentials.Any;

public static class Any
{
    public static Any<T0, T1> Create<T0, T1>(T0 value) => value;
    public static Any<T0, T1> Create<T0, T1>(T1 value) => value;

    public static Any<T0, T1, T2> Create<T0, T1, T2>(T0 value) => value;
    public static Any<T0, T1, T2> Create<T0, T1, T2>(T1 value) => value;
    public static Any<T0, T1, T2> Create<T0, T1, T2>(T2 value) => value;

    public static Any<T0, T1, T2, T3> Create<T0, T1, T2, T3>(T0 value) => value;
    public static Any<T0, T1, T2, T3> Create<T0, T1, T2, T3>(T1 value) => value;
    public static Any<T0, T1, T2, T3> Create<T0, T1, T2, T3>(T2 value) => value;
    public static Any<T0, T1, T2, T3> Create<T0, T1, T2, T3>(T3 value) => value;

    public static Any<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4> Create<T0, T1, T2, T3, T4>(T4 value) => value;

    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T4 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5> Create<T0, T1, T2, T3, T4, T5>(T5 value) => value;


    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T4 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T5 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Create<T0, T1, T2, T3, T4, T5, T6>(T6 value) => value;

    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T4 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T5 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T6 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6, T7> Create<T0, T1, T2, T3, T4, T5, T6, T7>(T7 value) => value;


    internal static (int index, object? value) Deserialize(Dictionary<int, Type> typeMap, int index, object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!typeMap.TryGetValue(index, out Type? type))
            throw new InvalidOperationException($"{index} is not valid index for Any<>");


        if (type.IsInstanceOfType(value))
        {
            return (index, value);
        }

        if (value is JsonElement je)
        {
            return (index, je.GetRawText().ConvertFromJson(type));
        }

        throw new InvalidOperationException($"Value is not valid type for Any<>");
    }

    internal static Exception InvalidOperation => new InvalidOperationException("No value");

}
