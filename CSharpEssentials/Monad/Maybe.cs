
using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Interfaces;

namespace CSharpEssentials;

public readonly partial struct Maybe<T> : IMaybe<T>, IEquatable<Maybe<T>>, IEquatable<object>
{
    private readonly T? _value;

    private Maybe(T? value)
    {
        if (Equals(value, default(T)))
        {
            HasValue = false;
            _value = default;
            return;
        }

        HasValue = true;
        _value = value;
    }


    /// <summary>
    /// Indicates whether the Maybe has a value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(true, nameof(_value))]
    public readonly bool HasValue { get; }

    /// <summary>
    /// Indicates whether the Maybe has no value.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(false, nameof(_value))]
    public readonly bool HasNoValue => !HasValue;

    /// <summary>
    /// Gets the value of the Maybe.
    /// </summary>
    public readonly T Value => GetValueOrThrow();

    /// <summary>
    /// Gets the value of the Maybe or throws an exception if there is no value.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public readonly T GetValueOrThrow(string? errorMessage = null) => HasValue ? _value : throw new InvalidOperationException(errorMessage ?? "Maybe has no value.");

    /// <summary>
    /// Gets the value of the Maybe or throws the specified exception if there is no value.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public readonly T GetValueOrThrow(Exception exception) => HasValue ? _value : throw exception;

    /// <summary>
    /// Gets the value of the Maybe or returns the specified default value if there is no value.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public readonly T GetValueOrDefault(T defaultValue) => HasNoValue ? defaultValue : _value;

    /// <summary>
    /// Gets the value of the Maybe or returns the default value of the type if there is no value.
    /// </summary>
    /// <returns></returns>
    public readonly T? GetValueOrDefault() => HasNoValue ? default : _value;

    /// <summary>
    /// Tries to get the value of the Maybe.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(out T? value)
    {
        value = _value;
        return HasValue;
    }

    public void Deconstruct(out bool hasValue, out T? value)
    {
        hasValue = HasValue;
        value = GetValueOrDefault();
    }

    /// <summary>
    /// Tries to get the value of the Maybe.
    /// </summary>
    public static Maybe<T> None => new();

    public static implicit operator Maybe<T>(T? value)
    {
        if (value is Maybe<T> m)
            return m;

        return Maybe.From(value);
    }

    public static implicit operator Maybe<T>(Maybe _) => None;

    /// <summary>
    /// Creates a Maybe from the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Maybe<T> From(T? value) => new(value);

    /// <summary>
    /// Creates a Maybe from the specified function.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Maybe<T> From(Func<T?> func) => new(func());

    /// <summary>
    /// Creates a Maybe from the specified task.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> FromAsync(Task<T?> task, CancellationToken cancellationToken = default)
    {
        T? value = await task.WithCancellation(cancellationToken);

        return new Maybe<T>(value);
    }

    /// <summary>
    /// Creates a Maybe from the specified task function.
    /// </summary>
    /// <param name="taskFunc"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<T>> FromAsync(Func<Task<T?>> taskFunc, CancellationToken cancellationToken = default)
    {
        T? value = await taskFunc().WithCancellation(cancellationToken);

        return new Maybe<T>(value);
    }

    public static bool operator ==(Maybe<T> maybe, T? value)
    {
        if (value is Maybe<T> maybeValue)
            return maybe.Equals(maybeValue);

        if (maybe.HasNoValue)
            return Equals(value, default(T));

        return maybe._value.Equals(value);
    }

    public static bool operator !=(Maybe<T> maybe, T value) => !(maybe == value);

    public static bool operator ==(Maybe<T> maybe, object other) => maybe.Equals(other);

    public static bool operator !=(Maybe<T> maybe, object other) => !(maybe == other);

    public static bool operator ==(Maybe<T> first, Maybe<T> second) => first.Equals(second);

    public static bool operator !=(Maybe<T> first, Maybe<T> second) => !(first == second);

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is Maybe<T> otherMaybe)
            return Equals(otherMaybe);

        if (obj is T otherValue)
            return Equals(otherValue);

        return false;
    }

    public bool Equals(Maybe<T> other)
    {
        if (HasNoValue && other.HasNoValue)
            return true;

        if (HasNoValue || other.HasNoValue)
            return false;

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    public override int GetHashCode()
    {
        if (HasNoValue)
            return 0;

        return _value.GetHashCode();
    }

    public override string ToString()
    {
        if (HasNoValue)
            return "No value";

        return _value.ToString() ?? _value.GetType().Name;
    }
}
public readonly partial record struct Maybe : IMaybe
{
    /// <summary>
    /// Represents a Maybe with no value.
    /// </summary>
    public static Maybe None => new();

    /// <summary>
    /// Indicates whether the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Maybe<T> From<T>(T? value) => Maybe<T>.From(value);

    /// <summary>
    /// Creates a Maybe from the specified function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Maybe<T> From<T>(Func<T?> func) => Maybe<T>.From(func);

    /// <summary>
    /// Creates a Maybe from the specified task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<Maybe<T>> From<T>(Task<T?> valueTask, CancellationToken cancellationToken = default) => Maybe<T>.FromAsync(valueTask, cancellationToken);

    /// <summary>
    /// Creates a Maybe from the specified task function.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="valueTaskFunc"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<Maybe<T>> From<T>(Func<Task<T?>> valueTaskFunc, CancellationToken cancellationToken = default) => Maybe<T>.FromAsync(valueTaskFunc, cancellationToken);
}
