using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Interfaces;

namespace CSharpEssentials.Comparers;

public class ResultBaseComparer : IEqualityComparer<IResultBase>
{
    public bool Equals(IResultBase? x, IResultBase? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;
        if (x.IsSuccess)
            return y.IsSuccess;
        if (y.IsSuccess)
            return false;
        return ResultLogic.CheckIfErrorsAreEqual(x.ErrorsOrEmptyArray, y.ErrorsOrEmptyArray);
    }

    public int GetHashCode([DisallowNull] IResultBase obj)
    {
        if (obj.IsSuccess)
            return obj.IsSuccess.GetHashCode();
        return ResultLogic.CreateErrorCodeHash(obj.ErrorsOrEmptyArray);
    }
}

public sealed class ResultComparer : ResultBaseComparer;

public sealed class ResultComparer<TValue> : IEqualityComparer<IResult<TValue>>
{
    private readonly EqualityComparer<TValue> _comparer;
    public ResultComparer() => _comparer = EqualityComparer<TValue>.Default;
    public ResultComparer(EqualityComparer<TValue> comparer) => _comparer = comparer;
    public bool Equals(IResult<TValue>? x, IResult<TValue>? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;
        if (x.IsSuccess)
            return y.IsSuccess && _comparer.Equals(x.Value, y.Value);
        return y.IsFailure && ResultLogic.CheckIfErrorsAreEqual(x.ErrorsOrEmptyArray, y.ErrorsOrEmptyArray);
    }

    public int GetHashCode([DisallowNull] IResult<TValue> obj)
    {
        if (obj.IsSuccess)
            return obj.Value!.GetHashCode();
        return ResultLogic.CreateErrorCodeHash(obj.ErrorsOrEmptyArray);
    }
}