using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern;

public static partial class ResultExtensions
{
    private static IEnumerable<T> EnsureSource<T>(IEnumerable<T> source)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source is null)
            throw new ArgumentNullException(nameof(source));
#endif

        return source;
    }

    public static Result CombineAll(this IEnumerable<Result> source)
    {
        source = EnsureSource(source);
        List<Error> errors = [];

        foreach (Result result in source)
        {
            if (result.IsFailure)
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors.Count == 0 ? Result.Success() : errors;
    }

    public static Result CombineAll(this IEnumerable<IResultBase> source)
    {
        source = EnsureSource(source);
        List<Error> errors = [];

        foreach (IResultBase result in source)
        {
            if (result.IsFailure)
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors.Count == 0 ? Result.Success() : errors;
    }

    public static Result<TValue[]> CombineAll<TValue>(this IEnumerable<Result<TValue>> source) => source.Sequence();

    public static Result<TValue[]> CombineAll<TValue>(this IEnumerable<IResult<TValue>> source) => EnsureSource(source).Sequence();

    public static Result<TValue[]> Sequence<TValue>(this IEnumerable<Result<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];
        List<Error> errors = [];

        foreach (Result<TValue> result in source)
        {
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors.Count == 0 ? successes.ToArray() : errors;
    }

    public static Result<TValue[]> Sequence<TValue>(this IEnumerable<IResult<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];
        List<Error> errors = [];

        foreach (IResult<TValue> result in source)
        {
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors.Count == 0 ? successes.ToArray() : errors;
    }

    public static Result<TOut[]> Traverse<TSource, TOut>(this IEnumerable<TSource> source, Func<TSource, Result<TOut>> selector) =>
        EnsureSource(source).Select(selector).Sequence();

    public static (TValue[] Successes, Error[] Errors) Partition<TValue>(this IEnumerable<Result<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];
        List<Error> errors = [];

        foreach (Result<TValue> result in source)
        {
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return (successes.ToArray(), errors.ToArray());
    }

    public static (TValue[] Successes, Error[] Errors) Partition<TValue>(this IEnumerable<IResult<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];
        List<Error> errors = [];

        foreach (IResult<TValue> result in source)
        {
            if (result.IsSuccess)
                successes.Add(result.Value);
            else
                errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return (successes.ToArray(), errors.ToArray());
    }

    public static Result FirstFailureOrSuccesses(this IEnumerable<Result> source)
    {
        source = EnsureSource(source);
        foreach (Result result in source)
        {
            if (result.IsFailure)
                return result.ErrorsOrEmptyArray;
        }

        return Result.Success();
    }

    public static Result FirstFailureOrSuccesses(this IEnumerable<IResultBase> source)
    {
        source = EnsureSource(source);
        foreach (IResultBase result in source)
        {
            if (result.IsFailure)
                return result.ErrorsOrEmptyArray;
        }

        return Result.Success();
    }

    public static Result<TValue[]> FirstFailureOrSuccesses<TValue>(this IEnumerable<Result<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];

        foreach (Result<TValue> result in source)
        {
            if (result.IsFailure)
                return result.ErrorsOrEmptyArray;

            successes.Add(result.Value);
        }

        return successes.ToArray();
    }

    public static Result<TValue[]> FirstFailureOrSuccesses<TValue>(this IEnumerable<IResult<TValue>> source)
    {
        source = EnsureSource(source);
        List<TValue> successes = [];

        foreach (IResult<TValue> result in source)
        {
            if (result.IsFailure)
                return result.ErrorsOrEmptyArray;

            successes.Add(result.Value);
        }

        return successes.ToArray();
    }
}
