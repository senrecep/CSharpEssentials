using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpEssentials.Maybe;

namespace CSharpEssentials.These;

public readonly record struct These<TError, TValue>
{
    [JsonConstructor]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by System.Text.Json")]
    private These(bool hasLeft, bool hasRight, TError? leftOrDefault, TValue? rightOrDefault)
    {
        HasLeft = hasLeft;
        HasRight = hasRight;
        LeftOrDefault = leftOrDefault;
        RightOrDefault = rightOrDefault;
    }

    // Raw backing state — serialized for JSON round-trip support
    // HasLeft is true for both Left and Both states
    [JsonPropertyName("isLeft")]
    public bool HasLeft { get; }

    // HasRight is true for both Right and Both states
    [JsonPropertyName("isRight")]
    public bool HasRight { get; }

    [JsonPropertyName("left")]
    public TError? LeftOrDefault { get; }

    [JsonPropertyName("right")]
    public TValue? RightOrDefault { get; }

    // Computed discriminators — excluded from JSON, derived from HasLeft/HasRight
    [JsonIgnore]
    public bool IsLeft => HasLeft && !HasRight;

    [JsonIgnore]
    public bool IsRight => !HasLeft && HasRight;

    [JsonIgnore]
    public bool IsBoth => HasLeft && HasRight;

    public static These<TError, TValue> Left(TError error) => new(true, false, error, default);
    public static These<TError, TValue> Right(TValue value) => new(false, true, default, value);
    public static These<TError, TValue> Both(TError error, TValue value) => new(true, true, error, value);

    public These<TError, TResult> Map<TResult>(Func<TValue, TResult> mapper)
    {
        if (IsRight)
            return These<TError, TResult>.Right(mapper(RightOrDefault!));
        if (IsBoth)
            return These<TError, TResult>.Both(LeftOrDefault!, mapper(RightOrDefault!));
        return These<TError, TResult>.Left(LeftOrDefault!);
    }

    public These<TNewError, TValue> MapLeft<TNewError>(Func<TError, TNewError> mapper)
    {
        if (IsLeft)
            return These<TNewError, TValue>.Left(mapper(LeftOrDefault!));
        if (IsBoth)
            return These<TNewError, TValue>.Both(mapper(LeftOrDefault!), RightOrDefault!);
        return These<TNewError, TValue>.Right(RightOrDefault!);
    }

    public These<TError, TResult> FlatMap<TResult>(Func<TValue, These<TError, TResult>> mapper)
    {
        if (IsRight || IsBoth)
            return mapper(RightOrDefault!);
        return These<TError, TResult>.Left(LeftOrDefault!);
    }

    public These<TError, TValue> Tap(Action<TValue> action)
    {
        if (IsRight || IsBoth)
            action(RightOrDefault!);
        return this;
    }

    public These<TError, TValue> TapLeft(Action<TError> action)
    {
        if (IsLeft || IsBoth)
            action(LeftOrDefault!);
        return this;
    }

    public TResult Match<TResult>(
        Func<TError, TResult> onLeft,
        Func<TValue, TResult> onRight,
        Func<TError, TValue, TResult> onBoth)
    {
        if (IsLeft)
            return onLeft(LeftOrDefault!);
        if (IsRight)
            return onRight(RightOrDefault!);
        return onBoth(LeftOrDefault!, RightOrDefault!);
    }

    public Maybe<TValue> GetRight()
        => (IsRight || IsBoth) ? Maybe<TValue>.From(RightOrDefault) : Maybe<TValue>.None;

    public Maybe<TError> GetLeft()
        => (IsLeft || IsBoth) ? Maybe<TError>.From(LeftOrDefault) : Maybe<TError>.None;
}
