using CSharpEssentials.Errors;

namespace CSharpEssentials.Results;

internal static class ResultLogic
{
    internal static bool CheckIfErrorsAreEqual(Error[] left, Error[] right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
            if (!left[i].Equals(right[i]))
                return false;
        return true;
    }

    internal static int CreateErrorCodeHash(Error[] errors)
    {
        var hashCode = new HashCode();
        for (int i = 0; i < errors.Length; i++)
            hashCode.Add(errors[i]);
        return hashCode.ToHashCode();
    }

    internal static InvalidOperationException CreateCannotAccessErrorsException() =>
        new("The Errors property cannot be accessed when no errors have been recorded. Check IsError before accessing Errors.");

    internal static ArgumentException CreateEmptyErrorArrayException() =>
         new("Cannot create an Result from an empty collection of errors. Provide at least one error.");

    internal static InvalidOperationException CreateCannotAccessFirstErrorException() =>
        new("The FirstError property cannot be accessed when no errors have been recorded. Check IsError before accessing FirstError.");

    internal static InvalidOperationException CreateCannotAccessLastErrorException() =>
        new("The LastError property cannot be accessed when no errors have been recorded. Check IsError before accessing LastError.");

    internal static InvalidOperationException CreateCannotAccessValueException() =>
        new("The Value property cannot be accessed when an error has been recorded. Check IsSuccess before accessing Value.");
}
