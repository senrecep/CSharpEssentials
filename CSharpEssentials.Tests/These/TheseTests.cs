using System.Globalization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.These;
using FluentAssertions;

namespace CSharpEssentials.Tests.These;

public class TheseTests
{
    [Fact]
    public void Left_Should_SetIsLeft_True()
    {
        var these = These<string, int>.Left("error");

        these.IsLeft.Should().BeTrue();
        these.IsRight.Should().BeFalse();
        these.IsBoth.Should().BeFalse();
    }

    [Fact]
    public void Right_Should_SetIsRight_True()
    {
        var these = These<string, int>.Right(42);

        these.IsRight.Should().BeTrue();
        these.IsLeft.Should().BeFalse();
        these.IsBoth.Should().BeFalse();
    }

    [Fact]
    public void Both_Should_SetIsBoth_True()
    {
        var these = These<string, int>.Both("warn", 42);

        these.IsBoth.Should().BeTrue();
        these.IsLeft.Should().BeFalse();
        these.IsRight.Should().BeFalse();
    }

    [Fact]
    public void Map_Should_TransformValue_When_IsRight()
    {
        var these = These<string, int>.Right(10).Map(x => x * 2);

        these.GetRight().Value.Should().Be(20);
    }

    [Fact]
    public void Map_Should_TransformValue_When_IsBoth()
    {
        var these = These<string, int>.Both("warn", 10).Map(x => x * 2);

        these.IsBoth.Should().BeTrue();
        these.GetRight().Value.Should().Be(20);
    }

    [Fact]
    public void Map_Should_PassThrough_When_IsLeft()
    {
        var these = These<string, int>.Left("err").Map(x => x * 2);

        these.IsLeft.Should().BeTrue();
    }

    [Fact]
    public void MapLeft_Should_TransformError_When_IsLeft()
    {
        var these = These<string, int>.Left("err").MapLeft(e => e.ToUpper(CultureInfo.InvariantCulture));

        these.GetLeft().Value.Should().Be("ERR");
    }

    [Fact]
    public void MapLeft_Should_TransformError_When_IsBoth()
    {
        var these = These<string, int>.Both("err", 5).MapLeft(e => e.ToUpper(CultureInfo.InvariantCulture));

        these.IsBoth.Should().BeTrue();
        these.GetLeft().Value.Should().Be("ERR");
    }

    [Fact]
    public void MapLeft_Should_PassThrough_When_IsRight()
    {
        var these = These<string, int>.Right(1).MapLeft(e => e.ToUpper(CultureInfo.InvariantCulture));

        these.IsRight.Should().BeTrue();
    }

    [Fact]
    public void FlatMap_Should_Chain_When_IsRight()
    {
        var these = These<string, int>.Right(5)
            .FlatMap(x => These<string, string>.Right(x.ToString(CultureInfo.InvariantCulture)));

        these.GetRight().Value.Should().Be("5");
    }

    [Fact]
    public void FlatMap_Should_Chain_When_IsBoth()
    {
        var these = These<string, int>.Both("w", 5)
            .FlatMap(x => These<string, string>.Right(x.ToString(CultureInfo.InvariantCulture)));

        these.IsRight.Should().BeTrue();
        these.GetRight().Value.Should().Be("5");
    }

    [Fact]
    public void FlatMap_Should_ReturnLeft_When_IsLeft()
    {
        var these = These<string, int>.Left("err")
            .FlatMap(x => These<string, string>.Right(x.ToString(CultureInfo.InvariantCulture)));

        these.IsLeft.Should().BeTrue();
    }

    [Fact]
    public void Tap_Should_ExecuteAction_When_IsRight()
    {
        bool called = false;

        These<string, int>.Right(42).Tap(_ => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void Tap_Should_ExecuteAction_When_IsBoth()
    {
        bool called = false;

        These<string, int>.Both("w", 42).Tap(_ => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void Tap_Should_NotExecuteAction_When_IsLeft()
    {
        bool called = false;

        These<string, int>.Left("err").Tap(_ => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void TapLeft_Should_ExecuteAction_When_IsLeft()
    {
        bool called = false;

        These<string, int>.Left("err").TapLeft(_ => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void TapLeft_Should_ExecuteAction_When_IsBoth()
    {
        bool called = false;

        These<string, int>.Both("err", 1).TapLeft(_ => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void TapLeft_Should_NotExecuteAction_When_IsRight()
    {
        bool called = false;

        These<string, int>.Right(42).TapLeft(_ => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void Match_Should_InvokeRightBranch_When_IsRight()
    {
        string result = These<string, int>.Right(42).Match(
            onLeft:  e      => $"left:{e}",
            onRight: v      => $"right:{v}",
            onBoth:  (e, v) => $"both:{e},{v}");

        result.Should().Be("right:42");
    }

    [Fact]
    public void Match_Should_InvokeLeftBranch_When_IsLeft()
    {
        string result = These<string, int>.Left("err").Match(
            onLeft:  e      => $"left:{e}",
            onRight: v      => $"right:{v}",
            onBoth:  (e, v) => $"both:{e},{v}");

        result.Should().Be("left:err");
    }

    [Fact]
    public void Match_Should_InvokeBothBranch_When_IsBoth()
    {
        string result = These<string, int>.Both("w", 1).Match(
            onLeft:  e      => "left",
            onRight: v      => "right",
            onBoth:  (e, v) => $"both:{e},{v}");

        result.Should().Be("both:w,1");
    }

    [Fact]
    public void GetRight_Should_ReturnNone_When_IsLeft()
    {
        These<string, int>.Left("err").GetRight().HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void GetRight_Should_ReturnValue_When_IsRight()
    {
        These<string, int>.Right(42).GetRight().Value.Should().Be(42);
    }

    [Fact]
    public void GetRight_Should_ReturnValue_When_IsBoth()
    {
        These<string, int>.Both("w", 7).GetRight().Value.Should().Be(7);
    }

    [Fact]
    public void GetLeft_Should_ReturnNone_When_IsRight()
    {
        These<string, int>.Right(42).GetLeft().HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void GetLeft_Should_ReturnValue_When_IsLeft()
    {
        These<string, int>.Left("err").GetLeft().Value.Should().Be("err");
    }

    [Fact]
    public void GetLeft_Should_ReturnValue_When_IsBoth()
    {
        These<string, int>.Both("w", 7).GetLeft().Value.Should().Be("w");
    }

    [Fact]
    public void FromResult_Should_ReturnRight_When_Success()
    {
        Result<int> result = 42;

        These<Error, int> these = TheseExtensions.FromResult(result);

        these.IsRight.Should().BeTrue();
        these.GetRight().Value.Should().Be(42);
    }

    [Fact]
    public void FromResult_Should_ReturnLeft_When_Failure()
    {
        Result<int> result = Error.Failure("E.Fail", "fail");

        These<Error, int> these = TheseExtensions.FromResult(result);

        these.IsLeft.Should().BeTrue();
    }

    [Fact]
    public void ToResult_Should_ReturnSuccess_When_IsRight()
    {
        These<Error, int> these = These<Error, int>.Right(42);

        Result<int> result = these.ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_Should_ReturnFailure_When_IsLeft()
    {
        These<Error, int> these = These<Error, int>.Left(Error.Failure("E", "fail"));

        Result<int> result = these.ToResult();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToResult_Should_ReturnFailure_When_IsBoth()
    {
        These<Error, int> these = These<Error, int>.Both(Error.Failure("E", "warn"), 42);

        Result<int> result = these.ToResult();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToResultLenient_Should_ReturnSuccess_When_IsBoth()
    {
        These<Error, int> these = These<Error, int>.Both(Error.Failure("E", "warn"), 42);

        Result<int> result = these.ToResultLenient();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResultLenient_Should_ReturnSuccess_When_IsRight()
    {
        These<Error, int> these = These<Error, int>.Right(42);

        Result<int> result = these.ToResultLenient();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResultLenient_Should_ReturnFailure_When_IsLeft()
    {
        These<Error, int> these = These<Error, int>.Left(Error.Failure("E", "fail"));

        Result<int> result = these.ToResultLenient();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Partition_Should_HandleEmptyCollection()
    {
        var items = Array.Empty<These<string, int>>();

        var (lefts, rights, boths) = items.Partition();

        lefts.Should().BeEmpty();
        rights.Should().BeEmpty();
        boths.Should().BeEmpty();
    }

    [Fact]
    public void Partition_Should_SeparateIntoThreeGroups()
    {
        var items = new[]
        {
            These<string, int>.Left("e1"),
            These<string, int>.Right(1),
            These<string, int>.Both("w1", 2),
            These<string, int>.Left("e2"),
            These<string, int>.Right(3),
        };

        var (lefts, rights, boths) = items.Partition();

        lefts.Should().HaveCount(2);
        rights.Should().HaveCount(2);
        boths.Should().HaveCount(1);
    }
}
