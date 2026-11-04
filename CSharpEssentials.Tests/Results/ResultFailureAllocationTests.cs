using System.Runtime.CompilerServices;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;
using Xunit;

namespace CSharpEssentials.Tests.Results;

public class ResultFailureAllocationTests
{
    private static readonly Error SampleError = Error.Failure("CODE", "Message");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Error[] AllocateErrors(int count) => new Error[count];

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<int> FromImplicitError(Error error) => error;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result FromImplicitErrorNonGeneric(Error error) => error;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<int> FromFailureError(Error error) => Result<int>.Failure(error);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result FromFailureErrorNonGeneric(Error error) => Result.Failure(error);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<int> FromStaticFailureError(Error error) => Result.Failure<int>(error);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Result<int> FromStaticFromError(Error error) => Result.From<int>(error);

    private static long Measure(Action action)
    {
        action();
        action();
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    private static long SingleErrorArrayBytes() => Measure(() => AllocateErrors(1));

    [Fact]
    public void SingleErrorPaths_ShouldAllocateOnlyTheErrorArray()
    {
        long expected = SingleErrorArrayBytes();

        Measure(() => FromImplicitError(SampleError)).Should().Be(expected);
        Measure(() => FromImplicitErrorNonGeneric(SampleError)).Should().Be(expected);
        Measure(() => FromFailureError(SampleError)).Should().Be(expected);
        Measure(() => FromFailureErrorNonGeneric(SampleError)).Should().Be(expected);
        Measure(() => FromStaticFailureError(SampleError)).Should().Be(expected);
        Measure(() => FromStaticFromError(SampleError)).Should().Be(expected);
    }

    [Fact]
    public void SingleErrorPaths_ShouldCarryTheError()
    {
        FromImplicitError(SampleError).Errors.Should().Equal(SampleError);
        FromImplicitErrorNonGeneric(SampleError).Errors.Should().Equal(SampleError);
        FromFailureError(SampleError).Errors.Should().Equal(SampleError);
        FromFailureErrorNonGeneric(SampleError).Errors.Should().Equal(SampleError);
        FromStaticFailureError(SampleError).Errors.Should().Equal(SampleError);
        FromStaticFromError(SampleError).Errors.Should().Equal(SampleError);
    }

    [Fact]
    public void CombineAll_ArrayWithThreeFailures_ShouldAllocateOnlyTheCombinedArray()
    {
        Result[] source = [Result.Failure(SampleError), Result.Failure(SampleError), Result.Failure(SampleError)];
        long expected = Measure(() => AllocateErrors(3));

        Measure(() => source.CombineAll()).Should().Be(expected);
    }

    [Fact]
    public void CombineAll_ArrayAllSuccess_ShouldNotAllocate()
    {
        Result[] source = [Result.Success(), Result.Success(), Result.Success()];

        Measure(() => source.CombineAll()).Should().Be(0);
    }

    [Fact]
    public void CombineAll_Array_ShouldKeepOrderAcrossMultiErrorResults()
    {
        Error a = Error.Failure("A", "a");
        Error b = Error.Failure("B", "b");
        Error c = Error.Failure("C", "c");
        Result[] source = [Result.Failure(new[] { a, b }), Result.Success(), Result.Failure(c)];

        Result combined = source.CombineAll();

        combined.Errors.Should().Equal(a, b, c);
    }

    [Fact]
    public void CombineAll_LazyEnumerable_ShouldKeepOrderAndSucceedWhenEmpty()
    {
        Error a = Error.Failure("A", "a");
        Error b = Error.Failure("B", "b");

        Yield(Result.Failure(a), Result.Success(), Result.Failure(b)).CombineAll().Errors.Should().Equal(a, b);
        Yield(Result.Success(), Result.Success()).CombineAll().IsSuccess.Should().BeTrue();
        Yield<IResultBase>(Result<int>.Failure(a), Result.Success(), Result<string>.Failure(b)).CombineAll().Errors.Should().Equal(a, b);
        Yield<IResultBase>(Result<int>.Success(1)).CombineAll().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineAll_NullSource_ShouldThrow()
    {
        FluentActions.Invoking(() => ((IEnumerable<Result>)null!).CombineAll()).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => ((IEnumerable<IResultBase>)null!).CombineAll()).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_OnFailure_ShouldPassTheStoredErrors()
    {
        Error[] errors = [SampleError];

        Result<int>.Failure(errors).Match(_ => Array.Empty<Error>(), e => e).Should().BeSameAs(errors);
        Result.Failure(errors).Match(() => Array.Empty<Error>(), e => e).Should().BeSameAs(errors);
        Result<int>.Success(7).Match(v => v, _ => -1).Should().Be(7);
        Result.Success().Match(() => 1, _ => -1).Should().Be(1);
    }

    private static IEnumerable<T> Yield<T>(params T[] items)
    {
        foreach (T item in items)
            yield return item;
    }
}
