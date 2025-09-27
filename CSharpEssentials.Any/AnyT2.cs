using CSharpEssentials.Json;

namespace CSharpEssentials.Any;

public readonly struct Any<T0, T1>
{
    private readonly object? _value;

    private Any(int index, object? value)
    {
        Index = index;
        _value = value;
    }

    public readonly int Index { get; }
    public readonly object? Value { get; }

    public bool IsFirst => Index == 0;
    public bool IsSecond => Index == 1;

    public T0 GetFirst() => Index == 0 ? (T0)_value! : throw InvalidOperation;
    public T1 GetSecond() => Index == 1 ? (T1)_value! : throw InvalidOperation;

    public static implicit operator Any<T0, T1>(T0 value) => new(0, value);
    public static implicit operator Any<T0, T1>(T1 value) => new(1, value);

    public void Switch(Action<T0> f0, Action<T1> f1)
    {
        switch (Index)
        {
            case 0:
                f0((T0)_value!);
                break;
            case 1:
                f1((T1)_value!);
                break;
            default:
                throw InvalidOperation;
        }
    }

    public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1)
    {
        return Index switch
        {
            0 => f0((T0)_value!),
            1 => f1((T1)_value!),
            _ => throw InvalidOperation
        };
    }

    public static Any<T0, T1> First(T0 value) => value;
    public static Any<T0, T1> Second(T1 value) => value;

    public override string ToString() => _value.ConvertToJson();
    private static Exception InvalidOperation => new InvalidOperationException("No value");
}

