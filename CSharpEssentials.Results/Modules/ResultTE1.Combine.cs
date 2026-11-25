// Produced by scripts/generate-typed-results.py; do not edit.
namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Combines 2 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2), E1> Combine<T2>(Result<T, E1> first, Result<T2, E1> second)
        where T2 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue), null);
    }

    /// <summary>Combines 3 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3), E1> Combine<T2, T3>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third)
        where T2 : notnull
        where T3 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue), null);
    }

    /// <summary>Combines 4 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <typeparam name="T4">The success value type of the fourth result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="fourth">The fourth result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3, T4), E1> Combine<T2, T3, T4>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third, Result<T4, E1> fourth)
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        if (fourth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fourth.Tag), default!, fourth.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue, fourth.RawValue), null);
    }

    /// <summary>Combines 5 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <typeparam name="T4">The success value type of the fourth result.</typeparam>
    /// <typeparam name="T5">The success value type of the fifth result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="fourth">The fourth result.</param>
    /// <param name="fifth">The fifth result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3, T4, T5), E1> Combine<T2, T3, T4, T5>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third, Result<T4, E1> fourth, Result<T5, E1> fifth)
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        if (fourth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fourth.Tag), default!, fourth.RawError);

        if (fifth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fifth.Tag), default!, fifth.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue, fourth.RawValue, fifth.RawValue), null);
    }

    /// <summary>Combines 6 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <typeparam name="T4">The success value type of the fourth result.</typeparam>
    /// <typeparam name="T5">The success value type of the fifth result.</typeparam>
    /// <typeparam name="T6">The success value type of the sixth result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="fourth">The fourth result.</param>
    /// <param name="fifth">The fifth result.</param>
    /// <param name="sixth">The sixth result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3, T4, T5, T6), E1> Combine<T2, T3, T4, T5, T6>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third, Result<T4, E1> fourth, Result<T5, E1> fifth, Result<T6, E1> sixth)
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        if (fourth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fourth.Tag), default!, fourth.RawError);

        if (fifth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fifth.Tag), default!, fifth.RawError);

        if (sixth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(sixth.Tag), default!, sixth.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue, fourth.RawValue, fifth.RawValue, sixth.RawValue), null);
    }

    /// <summary>Combines 7 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <typeparam name="T4">The success value type of the fourth result.</typeparam>
    /// <typeparam name="T5">The success value type of the fifth result.</typeparam>
    /// <typeparam name="T6">The success value type of the sixth result.</typeparam>
    /// <typeparam name="T7">The success value type of the seventh result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="fourth">The fourth result.</param>
    /// <param name="fifth">The fifth result.</param>
    /// <param name="sixth">The sixth result.</param>
    /// <param name="seventh">The seventh result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3, T4, T5, T6, T7), E1> Combine<T2, T3, T4, T5, T6, T7>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third, Result<T4, E1> fourth, Result<T5, E1> fifth, Result<T6, E1> sixth, Result<T7, E1> seventh)
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
        where T7 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        if (fourth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fourth.Tag), default!, fourth.RawError);

        if (fifth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fifth.Tag), default!, fifth.RawError);

        if (sixth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(sixth.Tag), default!, sixth.RawError);

        if (seventh.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(seventh.Tag), default!, seventh.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue, fourth.RawValue, fifth.RawValue, sixth.RawValue, seventh.RawValue), null);
    }

    /// <summary>Combines 8 results with the same error types into one result that holds a tuple of their values. The results
    /// are checked in order and the first error is returned; the later results are not looked at.</summary>
    /// <typeparam name="T2">The success value type of the second result.</typeparam>
    /// <typeparam name="T3">The success value type of the third result.</typeparam>
    /// <typeparam name="T4">The success value type of the fourth result.</typeparam>
    /// <typeparam name="T5">The success value type of the fifth result.</typeparam>
    /// <typeparam name="T6">The success value type of the sixth result.</typeparam>
    /// <typeparam name="T7">The success value type of the seventh result.</typeparam>
    /// <typeparam name="T8">The success value type of the eighth result.</typeparam>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <param name="third">The third result.</param>
    /// <param name="fourth">The fourth result.</param>
    /// <param name="fifth">The fifth result.</param>
    /// <param name="sixth">The sixth result.</param>
    /// <param name="seventh">The seventh result.</param>
    /// <param name="eighth">The eighth result.</param>
    /// <returns>A result that holds every value, or the first error.</returns>
    /// <exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>
    public static Result<(T, T2, T3, T4, T5, T6, T7, T8), E1> Combine<T2, T3, T4, T5, T6, T7, T8>(Result<T, E1> first, Result<T2, E1> second, Result<T3, E1> third, Result<T4, E1> fourth, Result<T5, E1> fifth, Result<T6, E1> sixth, Result<T7, E1> seventh, Result<T8, E1> eighth)
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
        where T7 : notnull
        where T8 : notnull
    {
        if (first.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(first.Tag), default!, first.RawError);

        if (second.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(second.Tag), default!, second.RawError);

        if (third.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(third.Tag), default!, third.RawError);

        if (fourth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fourth.Tag), default!, fourth.RawError);

        if (fifth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(fifth.Tag), default!, fifth.RawError);

        if (sixth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(sixth.Tag), default!, sixth.RawError);

        if (seventh.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(seventh.Tag), default!, seventh.RawError);

        if (eighth.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(eighth.Tag), default!, eighth.RawError);

        return new(TypedResultLogic.SuccessTag, (first.RawValue, second.RawValue, third.RawValue, fourth.RawValue, fifth.RawValue, sixth.RawValue, seventh.RawValue, eighth.RawValue), null);
    }
}
