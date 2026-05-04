using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultCombineTests
{
    [Fact]
    public void Combine_TwoSuccessResults_ShouldReturnTuple()
    {
        Result<int> first = 42;
        Result<string> second = "hello";

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Item1.Should().Be(42);
        combined.Value.Item2.Should().Be("hello");
    }

    [Fact]
    public void Combine_FirstFailure_ShouldReturnFailureWithErrors()
    {
        Result<int> first = Error.Failure("First.Error", "First error");
        Result<string> second = "hello";

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "First.Error");
    }

    [Fact]
    public void Combine_SecondFailure_ShouldReturnFailureWithErrors()
    {
        Result<int> first = 42;
        Result<string> second = Error.Failure("Second.Error", "Second error");

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().ContainSingle(e => e.Code == "Second.Error");
    }

    [Fact]
    public void Combine_BothFailure_ShouldReturnFailureWithAllErrors()
    {
        Result<int> first = Error.Failure("First.Error", "First error");
        Result<string> second = Error.Failure("Second.Error", "Second error");

        Result<(int, string)> combined = Result<int>.Combine(first, second);

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
        combined.Errors.Should().Contain(e => e.Code == "First.Error");
        combined.Errors.Should().Contain(e => e.Code == "Second.Error");
    }

    [Fact]
    public void Combine_ThreeSuccessResults_ShouldReturnTuple()
    {
        Result<int> first = 1;
        Result<string> second = "two";
        Result<bool> third = true;

        Result<(int, string, bool)> combined = Result<int>.Combine(first, second, third);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "two", true));
    }

    [Fact]
    public void Combine_FourSuccessResults_ShouldReturnTuple()
    {
        Result<int> r1 = 1;
        Result<int> r2 = 2;
        Result<int> r3 = 3;
        Result<int> r4 = 4;

        Result<(int, int, int, int)> combined = Result<int>.Combine(r1, r2, r3, r4);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3, 4));
    }

    [Fact]
    public void Combine_FiveSuccessResults_ShouldReturnTuple()
    {
        Result<int> r1 = 1;
        Result<int> r2 = 2;
        Result<int> r3 = 3;
        Result<int> r4 = 4;
        Result<int> r5 = 5;

        Result<(int, int, int, int, int)> combined = Result<int>.Combine(r1, r2, r3, r4, r5);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3, 4, 5));
    }

    [Fact]
    public void Combine_SixSuccessResults_ShouldReturnTuple()
    {
        Result<int> r1 = 1;
        Result<int> r2 = 2;
        Result<int> r3 = 3;
        Result<int> r4 = 4;
        Result<int> r5 = 5;
        Result<int> r6 = 6;

        Result<(int, int, int, int, int, int)> combined = Result<int>.Combine(r1, r2, r3, r4, r5, r6);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3, 4, 5, 6));
    }

    [Fact]
    public void Combine_SevenSuccessResults_ShouldReturnTuple()
    {
        Result<int> r1 = 1;
        Result<int> r2 = 2;
        Result<int> r3 = 3;
        Result<int> r4 = 4;
        Result<int> r5 = 5;
        Result<int> r6 = 6;
        Result<int> r7 = 7;

        Result<(int, int, int, int, int, int, int)> combined = Result<int>.Combine(r1, r2, r3, r4, r5, r6, r7);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3, 4, 5, 6, 7));
    }

    [Fact]
    public void Combine_EightSuccessResults_ShouldReturnTuple()
    {
        Result<int> r1 = 1;
        Result<int> r2 = 2;
        Result<int> r3 = 3;
        Result<int> r4 = 4;
        Result<int> r5 = 5;
        Result<int> r6 = 6;
        Result<int> r7 = 7;
        Result<int> r8 = 8;

        Result<(int, int, int, int, int, int, int, int)> combined = Result<int>.Combine(r1, r2, r3, r4, r5, r6, r7, r8);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, 2, 3, 4, 5, 6, 7, 8));
    }
}
