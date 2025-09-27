using CSharpEssentials.Json;

namespace CSharpEssentials.Any;

public readonly struct Any<T0, T1, T2, T3, T4, T5, T6>
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
    public bool IsThird => Index == 2;
    public bool IsFourth => Index == 3;
    public bool IsFifth => Index == 4;
    public bool IsSixth => Index == 5;
    public bool IsSeventh => Index == 6;


    public T0 GetFirst() => Index == 0 ? (T0)_value! : throw InvalidOperation;
    public T1 GetSecond() => Index == 1 ? (T1)_value! : throw InvalidOperation;
    public T2 GetThird() => Index == 2 ? (T2)_value! : throw InvalidOperation;
    public T3 GetFourth() => Index == 3 ? (T3)_value! : throw InvalidOperation;
    public T4 GetFifth() => Index == 4 ? (T4)_value! : throw InvalidOperation;
    public T5 GetSixth() => Index == 5 ? (T5)_value! : throw InvalidOperation;
    public T6 GetSeventh() => Index == 6 ? (T6)_value! : throw InvalidOperation;

    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T0 value) => new(0, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T1 value) => new(1, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T2 value) => new(2, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T3 value) => new(3, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T4 value) => new(4, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T5 value) => new(5, value);
    public static implicit operator Any<T0, T1, T2, T3, T4, T5, T6>(T6 value) => new(6, value);

    public void Switch(Action<T0> f0, Action<T1> f1, Action<T2> f2, Action<T3> f3, Action<T4> f4, Action<T5> f5, Action<T6> f6)
    {
        switch (Index)
        {
            case 0:
                f0((T0)_value!);
                break;
            case 1:
                f1((T1)_value!);
                break;
            case 2:
                f2((T2)_value!);
                break;
            case 3:
                f3((T3)_value!);
                break;
            case 4:
                f4((T4)_value!);
                break;
            case 5:
                f5((T5)_value!);
                break;
            case 6:
                f6((T6)_value!);
                break;
            default:
                throw InvalidOperation;
        }
    }

    public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1, Func<T2, TResult> f2, Func<T3, TResult> f3, Func<T4, TResult> f4, Func<T5, TResult> f5, Func<T6, TResult> f6)
    {
        return Index switch
        {
            0 => f0((T0)_value!),
            1 => f1((T1)_value!),
            2 => f2((T2)_value!),
            3 => f3((T3)_value!),
            4 => f4((T4)_value!),
            5 => f5((T5)_value!),
            6 => f6((T6)_value!),
            _ => throw InvalidOperation
        };
    }

    public static Any<T0, T1, T2, T3, T4, T5, T6> First(T0 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Second(T1 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Third(T2 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Fourth(T3 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Fifth(T4 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Sixth(T5 value) => value;
    public static Any<T0, T1, T2, T3, T4, T5, T6> Seventh(T6 value) => value;

    public override string ToString() => _value.ConvertToJson();
    private static Exception InvalidOperation => new InvalidOperationException("No value");
}
