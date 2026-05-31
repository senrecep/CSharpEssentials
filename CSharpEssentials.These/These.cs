using CSharpEssentials.Maybe;

namespace CSharpEssentials.These;

public readonly record struct These<TError, TValue>
{
    private readonly bool _isLeft;
    private readonly bool _isRight;
    private readonly TError? _leftValue;
    private readonly TValue? _rightValue;

    private These(bool isLeft, bool isRight, TError? left, TValue? right)
    {
        _isLeft = isLeft;
        _isRight = isRight;
        _leftValue = left;
        _rightValue = right;
    }

    public bool IsLeft => _isLeft && !_isRight;
    public bool IsRight => !_isLeft && _isRight;
    public bool IsBoth => _isLeft && _isRight;

    public static These<TError, TValue> Left(TError error) => new(true, false, error, default);
    public static These<TError, TValue> Right(TValue value) => new(false, true, default, value);
    public static These<TError, TValue> Both(TError error, TValue value) => new(true, true, error, value);

    public These<TError, TResult> Map<TResult>(Func<TValue, TResult> mapper)
    {
        if (IsRight)
            return These<TError, TResult>.Right(mapper(_rightValue!));
        if (IsBoth)
            return These<TError, TResult>.Both(_leftValue!, mapper(_rightValue!));
        return These<TError, TResult>.Left(_leftValue!);
    }

    public These<TNewError, TValue> MapLeft<TNewError>(Func<TError, TNewError> mapper)
    {
        if (IsLeft)
            return These<TNewError, TValue>.Left(mapper(_leftValue!));
        if (IsBoth)
            return These<TNewError, TValue>.Both(mapper(_leftValue!), _rightValue!);
        return These<TNewError, TValue>.Right(_rightValue!);
    }

    public These<TError, TResult> FlatMap<TResult>(Func<TValue, These<TError, TResult>> mapper)
    {
        if (IsRight || IsBoth)
            return mapper(_rightValue!);
        return These<TError, TResult>.Left(_leftValue!);
    }

    public These<TError, TValue> Tap(Action<TValue> action)
    {
        if (IsRight || IsBoth)
            action(_rightValue!);
        return this;
    }

    public These<TError, TValue> TapLeft(Action<TError> action)
    {
        if (IsLeft || IsBoth)
            action(_leftValue!);
        return this;
    }

    public TResult Match<TResult>(Func<TError, TResult> onLeft, Func<TValue, TResult> onRight, Func<TError, TValue, TResult> onBoth)
    {
        if (IsLeft)
            return onLeft(_leftValue!);
        if (IsRight)
            return onRight(_rightValue!);
        return onBoth(_leftValue!, _rightValue!);
    }

    public Maybe<TValue> GetRight()
        => (IsRight || IsBoth) ? Maybe<TValue>.From(_rightValue) : Maybe<TValue>.None;

    public Maybe<TError> GetLeft()
        => (IsLeft || IsBoth) ? Maybe<TError>.From(_leftValue) : Maybe<TError>.None;
}
