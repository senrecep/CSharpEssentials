using System.Runtime.CompilerServices;
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultDefaultTests
{
    private static readonly Error Failure = Error.Failure("E1", "Failed");

    [Fact]
    public void Default_Result_Should_BeFailure_With_ResultDefaultError()
    {
        Result result = default;

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Result.Default");
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.LastError.Should().Be(result.FirstError);
        result.ErrorsOrEmptyArray.Should().Equal(result.Errors);
    }

    [Fact]
    public void Default_ResultOfValueType_Should_BeFailure_With_ResultDefaultError()
    {
        Result<int> result = default;

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Result.Default");
        result.Value.Should().Be(0);
    }

    [Fact]
    public void Default_ResultOfReferenceType_Should_BeFailure_Through_IResultBase()
    {
        IResultBase result = default(Result<string>);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Result.Default");
    }

    [Fact]
    public void Success_Should_StayASuccess()
    {
        Result fromFactory = Result.Success();
        Result fromBool = true;
        Result<int> fromValue = 0;

        fromFactory.IsSuccess.Should().BeTrue();
        fromFactory.ErrorsOrEmptyArray.Should().BeEmpty();
        fromBool.IsSuccess.Should().BeTrue();
        fromValue.IsSuccess.Should().BeTrue();
        fromValue.Value.Should().Be(0);
    }

    [Fact]
    public void Default_Should_NotEqual_Success()
    {
        default(Result).Should().NotBe(Result.Success());
        default(Result<int>).Should().NotBe(Result<int>.Success(0));
        default(Result).Should().Be(default(Result));
    }

    [Fact]
    public void Default_Result_Combinators_Should_Throw()
    {
        Result result = default;

        Action match = () => result.Match(() => 1, _ => 2);
        Action map = () => result.Map(() => 1);
        Action bind = () => result.Bind(() => Result<int>.Success(1));
        Action mapError = () => result.MapError(e => e);

        match.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        map.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        bind.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        mapError.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
    }

    [Fact]
    public void Default_ResultT_Combinators_Should_Throw()
    {
        Result<int> result = default;

        Action match = () => result.Match(v => v, _ => -1);
        Action map = () => result.Map(v => v + 1);
        Action bind = () => result.Bind(v => Result<int>.Success(v));
        Action then = () => result.Then(_ => Result.Success());

        match.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        map.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        bind.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        then.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
    }

    private static Func<Task> Sync(Action action) => () =>
    {
        action();
        return Task.CompletedTask;
    };

    public static TheoryData<string, Func<Task>> ErrorForwardingOverloads => new()
    {
        { "Result.MatchAsync", () => default(Result).MatchAsync(() => Task.FromResult(1), _ => Task.FromResult(2)) },
        { "Result.MatchFirst", Sync(() => default(Result).MatchFirst(() => 1, _ => 2)) },
        { "Result.MatchFirstAsync", () => default(Result).MatchFirstAsync(() => Task.FromResult(1), _ => Task.FromResult(2)) },
        { "Result.MapAsync", () => default(Result).MapAsync(() => Task.FromResult(1)) },
        { "Result.BindAsync", () => default(Result).BindAsync(() => Task.FromResult(Result<int>.Success(1))) },
        { "Result.MapError(Error[])", Sync(() => default(Result).MapError((Error[] errors) => errors)) },
        { "Result.Switch", Sync(() => default(Result).Switch(() => { }, _ => { })) },
        { "Result.TapError", Sync(() => default(Result).TapError((Error[] _) => { })) },
        { "Result.Compensate", Sync(() => default(Result).Compensate((Error[] _) => Result.Success())) },
        { "Result<int>.MatchAsync", () => default(Result<int>).MatchAsync(v => Task.FromResult(v), _ => Task.FromResult(-1)) },
        { "Result<int>.MatchFirst", Sync(() => default(Result<int>).MatchFirst(v => v, _ => -1)) },
        { "Result<int>.MapAsync", () => default(Result<int>).MapAsync(v => Task.FromResult(v + 1)) },
        { "Result<int>.BindAsync", () => default(Result<int>).BindAsync(v => Task.FromResult(Result<int>.Success(v))) },
        { "Result<int>.ThenAsync", () => default(Result<int>).ThenAsync(_ => Task.FromResult(Result.Success())) },
        { "Result<int>.MapError(Error[])", Sync(() => default(Result<int>).MapError((Error[] errors) => errors)) },
        { "Result<int>.Switch", Sync(() => default(Result<int>).Switch(_ => { }, _ => { })) },
        { "Result<int>.Else", Sync(() => default(Result<int>).Else((Error[] _) => 1)) },
        { "Result<int>.Recover", Sync(() => default(Result<int>).Recover(ErrorType.Unexpected, _ => Result<int>.Success(1))) },
        { "Task<Result<int>>.BindAsync", () => Task.FromResult(default(Result<int>)).BindAsync(v => Task.FromResult(Result<int>.Success(v))) },
        { "Task<Result<int>>.MapAsync", () => Task.FromResult(default(Result<int>)).MapAsync(v => Task.FromResult(v + 1)) },
        { "Task<Result<int>>.Bind", () => Task.FromResult(default(Result<int>)).BindAsync(v => Result<int>.Success(v)) },
    };

    [Theory]
    [MemberData(nameof(ErrorForwardingOverloads))]
    public async Task Default_ErrorForwardingOverloads_Should_Throw(string name, Func<Task> call)
    {
        Func<Task> act = async () => await call();

        await act.Should().ThrowExactlyAsync<InvalidOperationException>(name).WithMessage("*default-initialized*");
    }

    [Fact]
    public void Default_PassThroughAndAggregation_Should_KeepResultDefault()
    {
        default(Result).Then(() => Result.Success()).FirstError.Code.Should().Be("Result.Default");
        default(Result<int>).Ensure(v => v > 0, Failure).FirstError.Code.Should().Be("Result.Default");
        Result.And(default(Result), Result.Success()).FirstError.Code.Should().Be("Result.Default");
        new[] { default(Result<int>), 1 }.Partition().Errors.Should().ContainSingle().Which.Code.Should().Be("Result.Default");
    }

    [Fact]
    public void Failure_Combinators_Should_StillPropagateErrors()
    {
        Result<int> result = Failure;

        result.Map(v => v + 1).FirstError.Should().Be(Failure);
        result.ToResult().FirstError.Should().Be(Failure);
        Result.Failure(Failure).Match(() => "ok", errors => errors[0].Code).Should().Be("E1");
    }

    [Fact]
    public void Default_TryGet_Should_ReturnFalse_With_ResultDefaultError()
    {
        bool success = default(Result<int>).TryGet(out int value, out Error[]? errors);

        success.Should().BeFalse();
        value.Should().Be(0);
        errors.Should().ContainSingle().Which.Code.Should().Be("Result.Default");
    }

    [Fact]
    public void Default_Json_Should_RoundTrip_AsFailure()
    {
        string json = JsonSerializer.Serialize(default(Result<int>));

        Result<int> roundTripped = JsonSerializer.Deserialize<Result<int>>(json);

        roundTripped.IsFailure.Should().BeTrue();
        roundTripped.FirstError.Code.Should().Be("Result.Default");
    }

    [Fact]
    public void Success_Json_Should_RoundTrip_AsSuccess()
    {
        string json = JsonSerializer.Serialize(Result.Success());

        Result roundTripped = JsonSerializer.Deserialize<Result>(json);

        roundTripped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Default_ToProblemDetails_Should_Report500()
    {
        EnhancedProblemDetails problem = default(Result).ToProblemDetails();

        problem.Status.Should().Be(500);
    }

    [Fact]
    public void Layout_Should_KeepTheSingleReferenceField()
    {
        Unsafe.SizeOf<Result>().Should().Be(IntPtr.Size);
        Unsafe.SizeOf<Result<int>>().Should().Be(2 * IntPtr.Size);
    }
}
