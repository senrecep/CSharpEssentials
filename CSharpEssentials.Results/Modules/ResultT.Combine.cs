using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Combines two results into a single result containing a tuple of their values.
    /// If either result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, TOther)> Combine<TOther>(Result<TValue> first, Result<TOther> second)
    {
        if (first.IsSuccess && second.IsSuccess)
        {
            return (first.Value, second.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines three results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3)> Combine<T2, T3>(Result<TValue> first, Result<T2> second, Result<T3> third)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess)
        {
            return (first.Value, second.Value, third.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines four results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3, T4)> Combine<T2, T3, T4>(Result<TValue> first, Result<T2> second, Result<T3> third, Result<T4> fourth)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess && fourth.IsSuccess)
        {
            return (first.Value, second.Value, third.Value, fourth.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);
        if (fourth.IsFailure)
            errors.AddRange(fourth.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines five results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3, T4, T5)> Combine<T2, T3, T4, T5>(Result<TValue> first, Result<T2> second, Result<T3> third, Result<T4> fourth, Result<T5> fifth)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess && fourth.IsSuccess && fifth.IsSuccess)
        {
            return (first.Value, second.Value, third.Value, fourth.Value, fifth.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);
        if (fourth.IsFailure)
            errors.AddRange(fourth.ErrorsOrEmptyArray);
        if (fifth.IsFailure)
            errors.AddRange(fifth.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines six results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3, T4, T5, T6)> Combine<T2, T3, T4, T5, T6>(Result<TValue> first, Result<T2> second, Result<T3> third, Result<T4> fourth, Result<T5> fifth, Result<T6> sixth)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess && fourth.IsSuccess && fifth.IsSuccess && sixth.IsSuccess)
        {
            return (first.Value, second.Value, third.Value, fourth.Value, fifth.Value, sixth.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);
        if (fourth.IsFailure)
            errors.AddRange(fourth.ErrorsOrEmptyArray);
        if (fifth.IsFailure)
            errors.AddRange(fifth.ErrorsOrEmptyArray);
        if (sixth.IsFailure)
            errors.AddRange(sixth.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines seven results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3, T4, T5, T6, T7)> Combine<T2, T3, T4, T5, T6, T7>(Result<TValue> first, Result<T2> second, Result<T3> third, Result<T4> fourth, Result<T5> fifth, Result<T6> sixth, Result<T7> seventh)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess && fourth.IsSuccess && fifth.IsSuccess && sixth.IsSuccess && seventh.IsSuccess)
        {
            return (first.Value, second.Value, third.Value, fourth.Value, fifth.Value, sixth.Value, seventh.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);
        if (fourth.IsFailure)
            errors.AddRange(fourth.ErrorsOrEmptyArray);
        if (fifth.IsFailure)
            errors.AddRange(fifth.ErrorsOrEmptyArray);
        if (sixth.IsFailure)
            errors.AddRange(sixth.ErrorsOrEmptyArray);
        if (seventh.IsFailure)
            errors.AddRange(seventh.ErrorsOrEmptyArray);

        return errors;
    }

    /// <summary>
    /// Combines eight results into a single result containing a tuple of their values.
    /// If any result is a failure, returns a failure with all errors.
    /// </summary>
    public static Result<(TValue, T2, T3, T4, T5, T6, T7, T8)> Combine<T2, T3, T4, T5, T6, T7, T8>(Result<TValue> first, Result<T2> second, Result<T3> third, Result<T4> fourth, Result<T5> fifth, Result<T6> sixth, Result<T7> seventh, Result<T8> eighth)
    {
        if (first.IsSuccess && second.IsSuccess && third.IsSuccess && fourth.IsSuccess && fifth.IsSuccess && sixth.IsSuccess && seventh.IsSuccess && eighth.IsSuccess)
        {
            return (first.Value, second.Value, third.Value, fourth.Value, fifth.Value, sixth.Value, seventh.Value, eighth.Value);
        }

        List<Error> errors = [];
        if (first.IsFailure)
            errors.AddRange(first.ErrorsOrEmptyArray);
        if (second.IsFailure)
            errors.AddRange(second.ErrorsOrEmptyArray);
        if (third.IsFailure)
            errors.AddRange(third.ErrorsOrEmptyArray);
        if (fourth.IsFailure)
            errors.AddRange(fourth.ErrorsOrEmptyArray);
        if (fifth.IsFailure)
            errors.AddRange(fifth.ErrorsOrEmptyArray);
        if (sixth.IsFailure)
            errors.AddRange(sixth.ErrorsOrEmptyArray);
        if (seventh.IsFailure)
            errors.AddRange(seventh.ErrorsOrEmptyArray);
        if (eighth.IsFailure)
            errors.AddRange(eighth.ErrorsOrEmptyArray);

        return errors;
    }
}
