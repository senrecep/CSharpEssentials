using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Json;

namespace CSharpEssentials.Any;

public readonly struct Any<T0, T1, T2, T3, T4>
{
    private static readonly Dictionary<int, Type> _typeMap = new()
    {
        { 0, typeof(T0) },
        { 1, typeof(T1) },
        { 2, typeof(T2) },
        { 3, typeof(T3) },
        { 4, typeof(T4) }
    };

    [JsonConstructor]
    public Any(int index, object? value) => (Index, Value) = Any.Deserialize(_typeMap, index, value);
    private Any(T0 value) => (Index, Value) = (0, value);
    private Any(T1 value) => (Index, Value) = (1, value);
    private Any(T2 value) => (Index, Value) = (2, value);
    private Any(T3 value) => (Index, Value) = (3, value);
    private Any(T4 value) => (Index, Value) = (4, value);

    public readonly int Index { get; }
    public readonly object? Value { get; }

    [JsonIgnore]
    public bool IsFirst => Index == 0;
    [JsonIgnore]
    public bool IsSecond => Index == 1;
    [JsonIgnore]
    public bool IsThird => Index == 2;
    [JsonIgnore]
    public bool IsFourth => Index == 3;
    [JsonIgnore]
    public bool IsFifth => Index == 4;

    public T0 GetFirst() => Index == 0 ? (T0)Value! : throw Any.InvalidOperation;
    public T1 GetSecond() => Index == 1 ? (T1)Value! : throw Any.InvalidOperation;
    public T2 GetThird() => Index == 2 ? (T2)Value! : throw Any.InvalidOperation;
    public T3 GetFourth() => Index == 3 ? (T3)Value! : throw Any.InvalidOperation;
    public T4 GetFifth() => Index == 4 ? (T4)Value! : throw Any.InvalidOperation;

    public static implicit operator Any<T0, T1, T2, T3, T4>(T0 value) => new(value);
    public static implicit operator Any<T0, T1, T2, T3, T4>(T1 value) => new(value);
    public static implicit operator Any<T0, T1, T2, T3, T4>(T2 value) => new(value);
    public static implicit operator Any<T0, T1, T2, T3, T4>(T3 value) => new(value);
    public static implicit operator Any<T0, T1, T2, T3, T4>(T4 value) => new(value);

    public AnyActionStatus Switch(
        Action<T0>? first = null,
        Action<T1>? second = null,
        Action<T2>? third = null,
        Action<T3>? fourth = null,
        Action<T4>? fifth = null)
    {
        ArgumentNullException.ThrowIfNull(Value);
        switch (Index)
        {
            case 0 when first is not null:
                first((T0)Value);
                return AnyActionStatus.Executed;
            case 1 when second is not null:
                second((T1)Value);
                return AnyActionStatus.Executed;
            case 2 when third is not null:
                third((T2)Value);
                return AnyActionStatus.Executed;
            case 3 when fourth is not null:
                fourth((T3)Value);
                return AnyActionStatus.Executed;
            case 4 when fifth is not null:
                fifth((T4)Value);
                return AnyActionStatus.Executed;
            default:
                return AnyActionStatus.NotExecuted;
        }
    }

    public AnyActionResult<TResult> Match<TResult>(
        Func<T0, TResult>? first = null,
        Func<T1, TResult>? second = null,
        Func<T2, TResult>? third = null,
        Func<T3, TResult>? fourth = null,
        Func<T4, TResult>? fifth = null)
    {
        ArgumentNullException.ThrowIfNull(Value);
        return Index switch
        {
            0 when first is not null => first((T0)Value),
            1 when second is not null => second((T1)Value),
            2 when third is not null => third((T2)Value),
            3 when fourth is not null => fourth((T3)Value),
            4 when fifth is not null => fifth((T4)Value),
            _ => AnyActionStatus.NotExecuted
        };
    }

    public static Any<T0, T1, T2, T3, T4> First(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4> Second(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4> Third(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4> Fourth(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4> Fifth(T4 value) => value;

    public override string ToString() => Value.ConvertToJson();
}
