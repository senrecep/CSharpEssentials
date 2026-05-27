using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Comparers;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultComparerTests
{
    private static readonly Error ErrorA = Error.Failure("ERR_A", "Error A");
    private static readonly Error ErrorB = Error.Validation("ERR_B", "Error B");

    #region ResultBaseComparer

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnTrue_When_BothNull()
    {
        var comparer = new ResultBaseComparer();

        bool result = comparer.Equals(null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnFalse_When_FirstIsNull()
    {
        var comparer = new ResultBaseComparer();
        IResultBase other = Result.Success();

        bool result = comparer.Equals(null, other);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnFalse_When_SecondIsNull()
    {
        var comparer = new ResultBaseComparer();
        IResultBase other = Result.Success();

        bool result = comparer.Equals(other, null);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnTrue_When_BothSucceed()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Success();

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnFalse_When_OneSucceedsAndOneFails()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnFalse_When_OneFailsAndOneSucceeds()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Failure(ErrorA);
        IResultBase y = Result.Success();

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnTrue_When_BothFailWithSameErrors()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Failure(ErrorA);
        IResultBase y = Result.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultBaseComparer_Equals_Should_ReturnFalse_When_BothFailWithDifferentErrors()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Failure(ErrorA);
        IResultBase y = Result.Failure(ErrorB);

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultBaseComparer_GetHashCode_Should_BeSame_When_BothSucceed()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Success();

        int hashX = comparer.GetHashCode(x);
        int hashY = comparer.GetHashCode(y);

        hashX.Should().Be(hashY);
    }

    [Fact]
    public void ResultBaseComparer_GetHashCode_Should_BeSame_When_BothFailWithSameErrors()
    {
        var comparer = new ResultBaseComparer();
        IResultBase x = Result.Failure(ErrorA);
        IResultBase y = Result.Failure(ErrorA);

        int hashX = comparer.GetHashCode(x);
        int hashY = comparer.GetHashCode(y);

        hashX.Should().Be(hashY);
    }

    [Fact]
    public void ResultBaseComparer_GetHashCode_Should_BeDifferent_When_SuccessVsFailure()
    {
        var comparer = new ResultBaseComparer();
        IResultBase success = Result.Success();
        IResultBase failure = Result.Failure(ErrorA);

        int hashSuccess = comparer.GetHashCode(success);
        int hashFailure = comparer.GetHashCode(failure);

        hashSuccess.Should().NotBe(hashFailure);
    }

    #endregion

    #region ResultComparer

    [Fact]
    public void ResultComparer_Equals_Should_ReturnTrue_When_BothSucceed()
    {
        var comparer = new ResultComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Success();

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparer_Equals_Should_ReturnFalse_When_OneSucceedsAndOneFails()
    {
        var comparer = new ResultComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparer_Equals_Should_ReturnTrue_When_BothFailWithSameErrors()
    {
        var comparer = new ResultComparer();
        IResultBase x = Result.Failure(ErrorA);
        IResultBase y = Result.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparer_GetHashCode_Should_BeSame_When_BothSucceed()
    {
        var comparer = new ResultComparer();
        IResultBase x = Result.Success();
        IResultBase y = Result.Success();

        int hashX = comparer.GetHashCode(x);
        int hashY = comparer.GetHashCode(y);

        hashX.Should().Be(hashY);
    }

    #endregion

    #region ResultComparer<TValue>

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnTrue_When_BothNull()
    {
        var comparer = new ResultComparer<int>();

        bool result = comparer.Equals(null, null);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnFalse_When_FirstIsNull()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> other = 42.ToResult();

        bool result = comparer.Equals(null, other);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnFalse_When_SecondIsNull()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> other = 42.ToResult();

        bool result = comparer.Equals(other, null);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnTrue_When_BothSucceedWithSameValue()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = 42.ToResult();
        IResult<int> y = 42.ToResult();

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnFalse_When_BothSucceedWithDifferentValues()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = 42.ToResult();
        IResult<int> y = 99.ToResult();

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnFalse_When_OneSucceedsAndOneFails()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = 42.ToResult();
        IResult<int> y = Result<int>.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnTrue_When_BothFailWithSameErrors()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = Result<int>.Failure(ErrorA);
        IResult<int> y = Result<int>.Failure(ErrorA);

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_ReturnFalse_When_BothFailWithDifferentErrors()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = Result<int>.Failure(ErrorA);
        IResult<int> y = Result<int>.Failure(ErrorB);

        bool result = comparer.Equals(x, y);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResultComparerT_Equals_Should_UseCustomComparer_When_Provided()
    {
        var customComparer = EqualityComparer<string>.Create(
            (a, b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase),
            s => s?.ToUpperInvariant().GetHashCode() ?? 0);
        var comparer = new ResultComparer<string>(customComparer);
        IResult<string> x = "hello".ToResult();
        IResult<string> y = "HELLO".ToResult();

        bool result = comparer.Equals(x, y);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResultComparerT_GetHashCode_Should_BeSame_When_BothSucceedWithSameValue()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = 42.ToResult();
        IResult<int> y = 42.ToResult();

        int hashX = comparer.GetHashCode(x);
        int hashY = comparer.GetHashCode(y);

        hashX.Should().Be(hashY);
    }

    [Fact]
    public void ResultComparerT_GetHashCode_Should_BeSame_When_BothFailWithSameErrors()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> x = Result<int>.Failure(ErrorA);
        IResult<int> y = Result<int>.Failure(ErrorA);

        int hashX = comparer.GetHashCode(x);
        int hashY = comparer.GetHashCode(y);

        hashX.Should().Be(hashY);
    }

    [Fact]
    public void ResultComparerT_GetHashCode_Should_BeDifferent_When_SuccessVsFailure()
    {
        var comparer = new ResultComparer<int>();
        IResult<int> success = 42.ToResult();
        IResult<int> failure = Result<int>.Failure(ErrorA);

        int hashSuccess = comparer.GetHashCode(success);
        int hashFailure = comparer.GetHashCode(failure);

        hashSuccess.Should().NotBe(hashFailure);
    }

    #endregion
}
