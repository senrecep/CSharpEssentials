
using System.Text.Json.Serialization;
using CSharpEssentials.Json;

namespace CSharpEssentials.Any;

public readonly struct Any<T0, T1>
{
    private static readonly Dictionary<int, Type> TypeMap = new()
    {
        { 0, typeof(T0) },
        { 1, typeof(T1) }
    };

    [JsonConstructor]
    public Any(int index, object? value) => (Index, Value) = Any.Deserialize(TypeMap, index, value);
    private Any(T0 value) => (Index, Value) = (0, value);
    private Any(T1 value) => (Index, Value) = (1, value);

    public int Index { get; }
    public object? Value { get; }

    [JsonIgnore]
    public bool IsFirst => Index == 0;
    [JsonIgnore]
    public bool IsSecond => Index == 1;

    public T0 GetFirst() => Index == 0 ? (T0)Value! : throw Any.InvalidOperation;
    public T1 GetSecond() => Index == 1 ? (T1)Value! : throw Any.InvalidOperation;

    public static implicit operator Any<T0, T1>(T0 value) => new(value);
    public static implicit operator Any<T0, T1>(T1 value) => new(value);

    public AnyActionStatus Switch(
        Action<T0>? first = null,
        Action<T1>? second = null)
    {
        if (Value is null)
            throw new InvalidOperationException("Value cannot be null");
        switch (Index)
        {
            case 0 when first is not null:
                first((T0)Value);
                return AnyActionStatus.Executed;
            case 1 when second is not null:
                second((T1)Value);
                return AnyActionStatus.Executed;
            default:
                return AnyActionStatus.NotExecuted;
        }
    }

    public AnyActionResult<TResult> Match<TResult>(
        Func<T0, TResult>? first = null,
        Func<T1, TResult>? second = null)
    {
        if (Value is null)
            throw new InvalidOperationException("Value cannot be null");
        return Index switch
        {
            0 when first is not null => first((T0)Value),
            1 when second is not null => second((T1)Value),
            _ => AnyActionStatus.NotExecuted
        };
    }

    public static Any<T0, T1> First(T0 value) => value;
    public static Any<T0, T1> Second(T1 value) => value;

    public override string ToString() => Value.ConvertToJson();
}

