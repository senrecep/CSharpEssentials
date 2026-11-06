using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTCombineArityTests
{
    private static Result<int> Ok(int value) => value;

    private static Result<int> Fail(int position) => Error.Failure($"E{position}", $"Error {position}");

    [Fact]
    public void Combine_Four_AllSuccess_ReturnsTuple()
    {
        Result<(int, string, bool, long)> result = Result<int>.Combine(Ok(1), Result<string>.Success("b"), Result<bool>.Success(true), Result<long>.Success(4L));

        result.Value.Should().Be((1, "b", true, 4L));
    }

    [Fact]
    public void Combine_Four_EveryFailure_CollectsAllErrorsInOrder()
    {
        Result<(int, int, int, int)> result = Result<int>.Combine(Fail(1), Fail(2), Fail(3), Fail(4));

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E3", "E4");
    }

    [Fact]
    public void Combine_Five_AllSuccess_ReturnsTuple()
    {
        Result<(int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Ok(2), Ok(3), Ok(4), Ok(5));

        result.Value.Should().Be((1, 2, 3, 4, 5));
    }

    [Fact]
    public void Combine_Five_EveryFailure_CollectsAllErrorsInOrder()
    {
        Result<(int, int, int, int, int)> result = Result<int>.Combine(Fail(1), Fail(2), Fail(3), Fail(4), Fail(5));

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E3", "E4", "E5");
    }

    [Fact]
    public void Combine_Five_SingleFailure_ReturnsOnlyThatError()
    {
        Result<(int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Ok(2), Ok(3), Ok(4), Fail(5));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("E5");
    }

    [Fact]
    public void Combine_Six_AllSuccess_ReturnsTuple()
    {
        Result<(int, int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Ok(2), Ok(3), Ok(4), Ok(5), Ok(6));

        result.Value.Should().Be((1, 2, 3, 4, 5, 6));
    }

    [Fact]
    public void Combine_Six_EveryFailure_CollectsAllErrorsInOrder()
    {
        Result<(int, int, int, int, int, int)> result = Result<int>.Combine(Fail(1), Fail(2), Fail(3), Fail(4), Fail(5), Fail(6));

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E3", "E4", "E5", "E6");
    }

    [Fact]
    public void Combine_Seven_AllSuccess_ReturnsTuple()
    {
        Result<(int, int, int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Ok(2), Ok(3), Ok(4), Ok(5), Ok(6), Ok(7));

        result.Value.Should().Be((1, 2, 3, 4, 5, 6, 7));
    }

    [Fact]
    public void Combine_Seven_EveryFailure_CollectsAllErrorsInOrder()
    {
        Result<(int, int, int, int, int, int, int)> result = Result<int>.Combine(Fail(1), Fail(2), Fail(3), Fail(4), Fail(5), Fail(6), Fail(7));

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E3", "E4", "E5", "E6", "E7");
    }

    [Fact]
    public void Combine_Eight_AllSuccess_ReturnsTuple()
    {
        Result<(int, int, int, int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Ok(2), Ok(3), Ok(4), Ok(5), Ok(6), Ok(7), Ok(8));

        result.Value.Should().Be((1, 2, 3, 4, 5, 6, 7, 8));
    }

    [Fact]
    public void Combine_Eight_EveryFailure_CollectsAllErrorsInOrder()
    {
        Result<(int, int, int, int, int, int, int, int)> result = Result<int>.Combine(Fail(1), Fail(2), Fail(3), Fail(4), Fail(5), Fail(6), Fail(7), Fail(8));

        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8");
    }

    [Fact]
    public void Combine_Eight_MixedFailures_SkipsSuccessfulPositions()
    {
        Result<(int, int, int, int, int, int, int, int)> result = Result<int>.Combine(Ok(1), Fail(2), Ok(3), Fail(4), Ok(5), Fail(6), Ok(7), Fail(8));

        result.Errors.Select(e => e.Code).Should().Equal("E2", "E4", "E6", "E8");
    }
}
