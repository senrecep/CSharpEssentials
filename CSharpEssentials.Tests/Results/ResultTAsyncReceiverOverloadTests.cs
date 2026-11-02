using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultTAsyncReceiverOverloadTests
{
    private static readonly Error First = Error.Failure("E1", "First error");
    private static readonly Error Second = Error.Failure("E2", "Second error");
    private static readonly Error Replacement = Error.Conflict("REPLACED", "Replacement error");

    private static Result<int> Ok => 5;

    private static Result<int> Failed => new[] { First, Second };

    private static ValueTask<Result<int>> Vt(Result<int> result) => new(result);

    private static Task<Result<int>> AsTask(Result<int> result) => Task.FromResult(result);

    #region ElseAsync on ValueTask<Result<T>>

    [Fact]
    public async Task ElseAsync_ValueTask_ValueFactory_RecoversFailure()
    {
        Func<Error[], int> onFailure = errors => errors.Length * 10;

        (await Vt(Failed).ElseAsync(onFailure)).Value.Should().Be(20);
        (await Vt(Ok).ElseAsync(onFailure)).Value.Should().Be(5);
    }

    [Fact]
    public async Task ElseAsync_ValueTask_Value_RecoversFailure()
    {
        (await Vt(Failed).ElseAsync(99)).Value.Should().Be(99);
        (await Vt(Ok).ElseAsync(99)).Value.Should().Be(5);
    }

    [Fact]
    public async Task ElseAsync_ValueTask_AsyncValueFactory_RecoversFailure()
    {
        Func<Error[], Task<int>> onFailure = errors => Task.FromResult(errors.Length);

        (await Vt(Failed).ElseAsync(onFailure)).Value.Should().Be(2);
        (await Vt(Ok).ElseAsync(onFailure)).Value.Should().Be(5);
    }

    [Fact]
    public async Task ElseAsync_ValueTask_TaskOfValue_RecoversFailure()
    {
        (await Vt(Failed).ElseAsync(Task.FromResult(77))).Value.Should().Be(77);
        (await Vt(Ok).ElseAsync(Task.FromResult(77))).Value.Should().Be(5);
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ErrorFactory_ReplacesErrors()
    {
        Func<Error[], Error> onFailure = errors => Error.Conflict("LAST", errors[^1].Code);

        Result<int> result = await Vt(Failed).ElseAsync(onFailure);

        result.Errors.Should().ContainSingle().Which.Description.Should().Be("E2");
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ErrorsFactory_ReplacesErrors()
    {
        Func<Error[], Error[]> onFailure = errors => [Replacement, .. errors];

        Result<int> result = await Vt(Failed).ElseAsync(onFailure);

        result.Errors.Select(e => e.Code).Should().Equal("REPLACED", "E1", "E2");
    }

    [Fact]
    public async Task ElseAsync_ValueTask_Error_ReplacesErrors()
    {
        (await Vt(Failed).ElseAsync(Replacement)).Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
        (await Vt(Ok).ElseAsync(Replacement)).Value.Should().Be(5);
    }

    [Fact]
    public async Task ElseAsync_ValueTask_AsyncErrorFactory_ReplacesErrors()
    {
        Func<Error[], Task<Error>> onFailure = _ => Task.FromResult(Replacement);

        Result<int> result = await Vt(Failed).ElseAsync(onFailure);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
    }

    [Fact]
    public async Task ElseAsync_ValueTask_AsyncErrorsFactory_ReplacesErrors()
    {
        Func<Error[], Task<Error[]>> onFailure = errors => Task.FromResult<Error[]>([errors[^1]]);

        Result<int> result = await Vt(Failed).ElseAsync(onFailure);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("E2");
    }

    [Fact]
    public async Task ElseAsync_ValueTask_TaskOfError_ReplacesErrors()
    {
        (await Vt(Failed).ElseAsync(Task.FromResult(Replacement))).FirstError.Code.Should().Be("REPLACED");
        (await Vt(Ok).ElseAsync(Task.FromResult(Replacement))).Value.Should().Be(5);
    }

    #endregion

    #region Match

    [Fact]
    public async Task MatchLastAsync_Task_SyncHandlers_ReceivesLastError()
    {
        Func<int, string> onSuccess = v => $"v{v}";
        Func<Error, string> onError = e => e.Code;

        (await AsTask(Ok).MatchLastAsync(onSuccess, onError)).Should().Be("v5");
        (await AsTask(Failed).MatchLastAsync(onSuccess, onError)).Should().Be("E2");
    }

    [Fact]
    public async Task MatchLastAsync_Task_AsyncHandlers_ReceivesLastError()
    {
        Func<int, Task<string>> onSuccess = v => Task.FromResult($"v{v}");
        Func<Error, Task<string>> onError = e => Task.FromResult(e.Code);

        (await AsTask(Ok).MatchLastAsync(onSuccess, onError)).Should().Be("v5");
        (await AsTask(Failed).MatchLastAsync(onSuccess, onError)).Should().Be("E2");
    }

    [Fact]
    public async Task MatchAsync_ValueTask_SyncHandlers_PicksBranch()
    {
        Func<int, string> onSuccess = v => $"v{v}";
        Func<Error[], string> onError = e => $"{e.Length}";

        (await Vt(Ok).MatchAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchAsync(onSuccess, onError)).Should().Be("2");
    }

    [Fact]
    public async Task MatchAsync_ValueTask_AsyncHandlers_PicksBranch()
    {
        Func<int, Task<string>> onSuccess = v => Task.FromResult($"v{v}");
        Func<Error[], Task<string>> onError = e => Task.FromResult($"{e.Length}");

        (await Vt(Ok).MatchAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchAsync(onSuccess, onError)).Should().Be("2");
    }

    [Fact]
    public async Task MatchFirstAsync_ValueTask_SyncHandlers_ReceivesFirstError()
    {
        Func<int, string> onSuccess = v => $"v{v}";
        Func<Error, string> onError = e => e.Code;

        (await Vt(Ok).MatchFirstAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchFirstAsync(onSuccess, onError)).Should().Be("E1");
    }

    [Fact]
    public async Task MatchFirstAsync_ValueTask_AsyncHandlers_ReceivesFirstError()
    {
        Func<int, Task<string>> onSuccess = v => Task.FromResult($"v{v}");
        Func<Error, Task<string>> onError = e => Task.FromResult(e.Code);

        (await Vt(Ok).MatchFirstAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchFirstAsync(onSuccess, onError)).Should().Be("E1");
    }

    [Fact]
    public async Task MatchLastAsync_ValueTask_SyncHandlers_ReceivesLastError()
    {
        Func<int, string> onSuccess = v => $"v{v}";
        Func<Error, string> onError = e => e.Code;

        (await Vt(Ok).MatchLastAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchLastAsync(onSuccess, onError)).Should().Be("E2");
    }

    [Fact]
    public async Task MatchLastAsync_ValueTask_AsyncHandlers_ReceivesLastError()
    {
        Func<int, Task<string>> onSuccess = v => Task.FromResult($"v{v}");
        Func<Error, Task<string>> onError = e => Task.FromResult(e.Code);

        (await Vt(Ok).MatchLastAsync(onSuccess, onError)).Should().Be("v5");
        (await Vt(Failed).MatchLastAsync(onSuccess, onError)).Should().Be("E2");
    }

    #endregion

    #region Switch on ValueTask<Result<T>>

    [Fact]
    public async Task SwitchAsync_ValueTask_SyncHandlers_RunsMatchingBranch()
    {
        var log = new List<string>();
        Action<int> onSuccess = v => log.Add($"v{v}");
        Action<Error[]> onError = e => log.Add($"{e.Length}");

        await Vt(Ok).SwitchAsync(onSuccess, onError);
        await Vt(Failed).SwitchAsync(onSuccess, onError);

        log.Should().Equal("v5", "2");
    }

    [Fact]
    public async Task SwitchAsync_ValueTask_AsyncHandlers_RunsMatchingBranch()
    {
        var log = new List<string>();
        Func<int, Task> onSuccess = v =>
        {
            log.Add($"v{v}");
            return Task.CompletedTask;
        };
        Func<Error[], Task> onError = e =>
        {
            log.Add($"{e.Length}");
            return Task.CompletedTask;
        };

        await Vt(Ok).SwitchAsync(onSuccess, onError);
        await Vt(Failed).SwitchAsync(onSuccess, onError);

        log.Should().Equal("v5", "2");
    }

    [Fact]
    public async Task SwitchFirstAsync_ValueTask_SyncHandlers_ReceivesFirstError()
    {
        var log = new List<string>();
        Action<int> onSuccess = v => log.Add($"v{v}");
        Action<Error> onError = e => log.Add(e.Code);

        await Vt(Ok).SwitchFirstAsync(onSuccess, onError);
        await Vt(Failed).SwitchFirstAsync(onSuccess, onError);

        log.Should().Equal("v5", "E1");
    }

    [Fact]
    public async Task SwitchFirstAsync_ValueTask_AsyncHandlers_ReceivesFirstError()
    {
        var log = new List<string>();
        Func<int, Task> onSuccess = v =>
        {
            log.Add($"v{v}");
            return Task.CompletedTask;
        };
        Func<Error, Task> onError = e =>
        {
            log.Add(e.Code);
            return Task.CompletedTask;
        };

        await Vt(Ok).SwitchFirstAsync(onSuccess, onError);
        await Vt(Failed).SwitchFirstAsync(onSuccess, onError);

        log.Should().Equal("v5", "E1");
    }

    [Fact]
    public async Task SwitchLastAsync_ValueTask_SyncHandlers_ReceivesLastError()
    {
        var log = new List<string>();
        Action<int> onSuccess = v => log.Add($"v{v}");
        Action<Error> onError = e => log.Add(e.Code);

        await Vt(Ok).SwitchLastAsync(onSuccess, onError);
        await Vt(Failed).SwitchLastAsync(onSuccess, onError);

        log.Should().Equal("v5", "E2");
    }

    [Fact]
    public async Task SwitchLastAsync_ValueTask_AsyncHandlers_ReceivesLastError()
    {
        var log = new List<string>();
        Func<int, Task> onSuccess = v =>
        {
            log.Add($"v{v}");
            return Task.CompletedTask;
        };
        Func<Error, Task> onError = e =>
        {
            log.Add(e.Code);
            return Task.CompletedTask;
        };

        await Vt(Ok).SwitchLastAsync(onSuccess, onError);
        await Vt(Failed).SwitchLastAsync(onSuccess, onError);

        log.Should().Equal("v5", "E2");
    }

    #endregion

    #region Then on ValueTask<Result<T>>

    [Fact]
    public async Task ThenAsync_ValueTask_FuncOfResult_ChainsOnSuccess()
    {
        Func<int, Result<string>> onSuccess = v => $"v{v}";

        (await Vt(Ok).ThenAsync(onSuccess)).Value.Should().Be("v5");
        (await Vt(Failed).ThenAsync(onSuccess)).Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ThenAsync_ValueTask_FuncOfValue_MapsOnSuccess()
    {
        Func<int, long> onSuccess = v => v * 2L;

        (await Vt(Ok).ThenAsync(onSuccess)).Value.Should().Be(10L);
        (await Vt(Failed).ThenAsync(onSuccess)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task ThenDoAsync_ValueTask_Action_RunsOnlyOnSuccess()
    {
        var seen = new List<int>();
        Action<int> action = seen.Add;

        Result<int> success = await Vt(Ok).ThenDoAsync(action);
        Result<int> failure = await Vt(Failed).ThenDoAsync(action);

        seen.Should().Equal(5);
        success.Value.Should().Be(5);
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ThenAsync_ValueTask_AsyncFuncOfResult_ChainsOnSuccess()
    {
        Func<int, Task<Result<string>>> onSuccess = v => Task.FromResult<Result<string>>(Replacement);

        (await Vt(Ok).ThenAsync(onSuccess)).FirstError.Code.Should().Be("REPLACED");
        (await Vt(Failed).ThenAsync(onSuccess)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task ThenAsync_ValueTask_AsyncFuncOfValue_MapsOnSuccess()
    {
        Func<int, Task<string>> onSuccess = v => Task.FromResult($"v{v}");

        (await Vt(Ok).ThenAsync(onSuccess)).Value.Should().Be("v5");
        (await Vt(Failed).ThenAsync(onSuccess)).Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ThenDoAsync_ValueTask_AsyncAction_RunsOnlyOnSuccess()
    {
        var seen = new List<int>();
        Func<int, Task> action = v =>
        {
            seen.Add(v);
            return Task.CompletedTask;
        };

        await Vt(Ok).ThenDoAsync(action);
        await Vt(Failed).ThenDoAsync(action);

        seen.Should().Equal(5);
    }

    #endregion

    #region BindIfAsync

    [Fact]
    public async Task BindIfAsync_Task_Condition_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = v => v + 1;

        (await AsTask(Ok).BindIfAsync(true, func)).Value.Should().Be(6);
        (await AsTask(Ok).BindIfAsync(false, func)).Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_Task_Predicate_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = v => v + 1;

        (await AsTask(Ok).BindIfAsync(() => true, func)).Value.Should().Be(6);
        (await AsTask(Ok).BindIfAsync(() => false, func)).Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_Task_ValuePredicate_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = _ => Replacement;

        (await AsTask(Ok).BindIfAsync(v => v > 3, func)).FirstError.Code.Should().Be("REPLACED");
        (await AsTask(Ok).BindIfAsync(v => v > 10, func)).Value.Should().Be(5);
        (await AsTask(Failed).BindIfAsync(v => v > 3, func)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_Condition_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = v => v + 1;

        (await Vt(Ok).BindIfAsync(true, func)).Value.Should().Be(6);
        (await Vt(Ok).BindIfAsync(false, func)).Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_Predicate_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = v => v + 1;

        (await Vt(Ok).BindIfAsync(() => true, func)).Value.Should().Be(6);
        (await Vt(Ok).BindIfAsync(() => false, func)).Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_ValuePredicate_BindsOnlyWhenTrue()
    {
        Func<int, Result<int>> func = v => v * 3;

        (await Vt(Ok).BindIfAsync(v => v == 5, func)).Value.Should().Be(15);
        (await Vt(Ok).BindIfAsync(v => v != 5, func)).Value.Should().Be(5);
    }

    #endregion

    #region TryCatchAsync

    [Fact]
    public async Task TryCatchAsync_Task_FuncOfResult_ConvertsExceptionToError()
    {
        Func<int, Result> throwing = _ => throw new InvalidOperationException("boom");
        Func<int, Result> ok = _ => Result.Success();

        (await AsTask(Ok).TryCatchAsync(throwing, Replacement)).FirstError.Code.Should().Be("REPLACED");
        (await AsTask(Ok).TryCatchAsync(ok)).IsSuccess.Should().BeTrue();
        (await AsTask(Failed).TryCatchAsync(ok)).Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task TryCatchAsync_Task_FuncOfResultT_ConvertsExceptionToError()
    {
        Func<int, Result<string>> throwing = _ => throw new InvalidOperationException("boom");
        Func<int, Result<string>> ok = v => $"v{v}";

        (await AsTask(Ok).TryCatchAsync(throwing)).FirstError.Code.Should().Be(nameof(InvalidOperationException));
        (await AsTask(Ok).TryCatchAsync(ok)).Value.Should().Be("v5");
    }

    [Fact]
    public async Task TryCatchAsync_ValueTask_FuncOfResult_ConvertsExceptionToError()
    {
        Func<int, Result> throwing = _ => throw new InvalidOperationException("boom");

        (await Vt(Ok).TryCatchAsync(throwing, Replacement)).FirstError.Code.Should().Be("REPLACED");
        (await Vt(Failed).TryCatchAsync(throwing, Replacement)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task TryCatchAsync_ValueTask_FuncOfResultT_ConvertsExceptionToError()
    {
        Func<int, Result<string>> throwing = _ => throw new InvalidOperationException("boom");
        Func<int, Result<string>> ok = v => $"v{v}";

        (await Vt(Ok).TryCatchAsync(throwing, Replacement)).FirstError.Code.Should().Be("REPLACED");
        (await Vt(Ok).TryCatchAsync(ok)).Value.Should().Be("v5");
    }

    #endregion

    #region Finally

    [Fact]
    public void Finally_Action_RunsForBothStatesAndReturnsSameResult()
    {
        var seen = new List<bool>();

        Result<int> success = Ok.Finally(r => seen.Add(r.IsSuccess));
        Result<int> failure = Failed.Finally(r => seen.Add(r.IsSuccess));

        seen.Should().Equal(true, false);
        success.Value.Should().Be(5);
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task FinallyAsync_Task_Func_ReturnsProjection()
    {
        Func<Result<int>, string> func = r => r.IsSuccess ? $"v{r.Value}" : r.FirstError.Code;

        (await AsTask(Ok).FinallyAsync(func)).Should().Be("v5");
        (await AsTask(Failed).FinallyAsync(func)).Should().Be("E1");
    }

    [Fact]
    public async Task FinallyAsync_Task_Action_RunsAndReturnsSameResult()
    {
        int calls = 0;
        Action<Result<int>> action = _ => calls++;

        Result<int> result = await AsTask(Failed).FinallyAsync(action);

        calls.Should().Be(1);
        result.Should().Be(Failed);
    }

    [Fact]
    public async Task FinallyAsync_Task_AsyncAction_RunsAndReturnsSameResult()
    {
        var seen = new List<int>();
        Func<Result<int>, Task> action = r =>
        {
            seen.Add(r.Value);
            return Task.CompletedTask;
        };

        Result<int> result = await AsTask(Ok).FinallyAsync(action);

        seen.Should().Equal(5);
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_Func_ReturnsProjection()
    {
        Func<Result<int>, int> func = r => r.ErrorsOrEmptyArray.Length;

        (await Vt(Failed).FinallyAsync(func)).Should().Be(2);
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_Action_RunsAndReturnsSameResult()
    {
        int calls = 0;
        Action<Result<int>> action = _ => calls++;

        Result<int> result = await Vt(Ok).FinallyAsync(action);

        calls.Should().Be(1);
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_AsyncAction_RunsAndReturnsSameResult()
    {
        int calls = 0;
        Func<Result<int>, Task> action = _ =>
        {
            calls++;
            return Task.CompletedTask;
        };

        Result<int> result = await Vt(Failed).FinallyAsync(action);

        calls.Should().Be(1);
        result.Errors.Should().HaveCount(2);
    }

    #endregion

    #region GetValueOrDefault

    [Fact]
    public void GetValueOrDefault_Factory_IsCalledOnlyOnFailure()
    {
        int calls = 0;
        Func<int> factory = () =>
        {
            calls++;
            return 42;
        };

        Ok.GetValueOrDefault(factory).Should().Be(5);
        Failed.GetValueOrDefault(factory).Should().Be(42);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_Task_Overloads_ReturnValueOrFallback()
    {
        (await AsTask(Ok).GetValueOrDefaultAsync()).Should().Be(5);
        (await AsTask(Failed).GetValueOrDefaultAsync()).Should().Be(0);
        (await AsTask(Failed).GetValueOrDefaultAsync(9)).Should().Be(9);
        (await AsTask(Failed).GetValueOrDefaultAsync(() => 11)).Should().Be(11);
        (await AsTask(Ok).GetValueOrDefaultAsync(() => 11)).Should().Be(5);
    }

    [Fact]
    public async Task GetValueOrDefaultAsync_ValueTask_Overloads_ReturnValueOrFallback()
    {
        (await Vt(Ok).GetValueOrDefaultAsync()).Should().Be(5);
        (await Vt(Failed).GetValueOrDefaultAsync()).Should().Be(0);
        (await Vt(Failed).GetValueOrDefaultAsync(9)).Should().Be(9);
        (await Vt(Ok).GetValueOrDefaultAsync(9)).Should().Be(5);
        (await Vt(Failed).GetValueOrDefaultAsync(() => 11)).Should().Be(11);
    }

    #endregion

    #region SelectAsync

    [Fact]
    public async Task SelectAsync_Task_Func_ProjectsSuccessAndKeepsErrors()
    {
        Func<int, string> selector = v => $"v{v}";

        (await AsTask(Ok).SelectAsync(selector)).Value.Should().Be("v5");
        (await AsTask(Failed).SelectAsync(selector)).Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task SelectAsync_Task_AsyncFunc_SkipsSelectorOnFailure()
    {
        int calls = 0;
        Func<int, Task<string>> selector = v =>
        {
            calls++;
            return Task.FromResult($"v{v}");
        };

        Result<string> failure = await AsTask(Failed).SelectAsync(selector);
        Result<string> success = await AsTask(Ok).SelectAsync(selector);

        failure.Errors.Select(e => e.Code).Should().Equal("E1", "E2");
        success.Value.Should().Be("v5");
        calls.Should().Be(1);
    }

    [Fact]
    public async Task SelectAsync_ValueTask_Func_ProjectsSuccessAndKeepsErrors()
    {
        Func<int, string> selector = v => $"v{v}";

        (await Vt(Ok).SelectAsync(selector)).Value.Should().Be("v5");
        (await Vt(Failed).SelectAsync(selector)).FirstError.Code.Should().Be("E1");
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_Task_Action_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Action action = () => calls++;

        await AsTask(Ok).TapAsync(action);
        Result<int> failure = await AsTask(Failed).TapAsync(action);

        calls.Should().Be(1);
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task TapAsync_Task_ConditionAndValueAction_RunsOnlyWhenConditionHolds()
    {
        var seen = new List<int>();
        Action<int> action = seen.Add;

        await AsTask(Ok).TapAsync(() => true, action);
        await AsTask(Result<int>.Success(7)).TapAsync(() => false, action);
        await AsTask(Failed).TapAsync(() => true, action);

        seen.Should().Equal(5);
    }

    [Fact]
    public async Task TapAsync_ValueTask_Action_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Action action = () => calls++;

        Result<int> success = await Vt(Ok).TapAsync(action);
        await Vt(Failed).TapAsync(action);

        calls.Should().Be(1);
        success.Value.Should().Be(5);
    }

    [Fact]
    public async Task TapAsync_ValueTask_ConditionAndValueAction_RunsOnlyWhenConditionHolds()
    {
        var seen = new List<int>();
        Action<int> action = seen.Add;

        await Vt(Ok).TapAsync(() => true, action);
        await Vt(Result<int>.Success(7)).TapAsync(() => false, action);
        await Vt(Failed).TapAsync(() => true, action);

        seen.Should().Equal(5);
    }

    [Fact]
    public async Task TapAsync_Task_BoolConditionAndAsyncAction_RunsOnlyWhenConditionHolds()
    {
        var seen = new List<int>();
        Func<int, Task> action = v =>
        {
            seen.Add(v);
            return Task.CompletedTask;
        };

        Result<int> skipped = await AsTask(Result<int>.Success(7)).TapAsync(false, action);
        Result<int> ran = await AsTask(Ok).TapAsync(true, action);

        seen.Should().Equal(5);
        skipped.Value.Should().Be(7);
        ran.Value.Should().Be(5);
    }

    #endregion

    #region FailIf and Ensure

    [Fact]
    public void FailIf_ErrorFactory_FailsOnlyWhenPredicateHolds()
    {
        Func<int, Error> func = v => Error.Validation("TOO_BIG", $"{v}");

        Ok.FailIf(v => v > 3, func).FirstError.Description.Should().Be("5");
        Ok.FailIf(v => v > 10, func).Value.Should().Be(5);
        Failed.FailIf(_ => true, func).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task FailIfAsync_AsyncErrorFactory_SkipsOnFailure()
    {
        Func<int, Task<bool>> predicate = _ => Task.FromResult(true);
        Func<int, Task<Error>> func = _ => Task.FromResult(Replacement);

        (await Failed.FailIfAsync(predicate, func)).FirstError.Code.Should().Be("E1");
        (await Ok.FailIfAsync(predicate, func)).FirstError.Code.Should().Be("REPLACED");
    }

    [Fact]
    public async Task FailIfAsync_ValueTask_ErrorFactory_FailsOnlyWhenPredicateHolds()
    {
        Func<int, bool> predicate = v => v == 5;
        Func<int, Error> func = _ => Replacement;

        (await Vt(Ok).FailIfAsync(predicate, func)).FirstError.Code.Should().Be("REPLACED");
        (await Vt(10).FailIfAsync(predicate, func)).Value.Should().Be(10);
    }

    [Fact]
    public async Task FailIfAsync_ValueTask_AsyncPredicateAndError_FailsOnlyWhenPredicateHolds()
    {
        Func<int, Task<bool>> predicate = v => Task.FromResult(v == 5);

        (await Vt(Ok).FailIfAsync(predicate, Replacement)).FirstError.Code.Should().Be("REPLACED");
        (await Vt(10).FailIfAsync(predicate, Replacement)).Value.Should().Be(10);
    }

    [Fact]
    public async Task FailIfAsync_ValueTask_AsyncPredicateAndAsyncErrorFactory_FailsOnlyWhenPredicateHolds()
    {
        Func<int, Task<bool>> predicate = v => Task.FromResult(v == 5);
        Func<int, Task<Error>> func = v => Task.FromResult(Error.Validation("BAD", $"{v}"));

        (await Vt(Ok).FailIfAsync(predicate, func)).FirstError.Description.Should().Be("5");
        (await Vt(Failed).FailIfAsync(predicate, func)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public void EnsureNotNull_ErrorFactory_FailsForDefaultResult()
    {
        Func<string, Error> factory = _ => Replacement;
        Result<string> present = "x";
        Result<string> failed = First;

        default(Result<string>).EnsureNotNull(factory).FirstError.Code.Should().Be("REPLACED");
        present.EnsureNotNull(factory).Value.Should().Be("x");
        failed.EnsureNotNull(factory).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task EnsureAsync_AsyncPredicateAndErrorFactory_FailsWhenPredicateIsFalse()
    {
        Func<int, Task<bool>> predicate = v => Task.FromResult(v > 10);
        Func<int, Error> factory = v => Error.Validation("SMALL", $"{v}");

        (await Ok.EnsureAsync(predicate, factory)).FirstError.Description.Should().Be("5");
        (await Result<int>.Success(20).EnsureAsync(predicate, factory)).Value.Should().Be(20);
        (await Failed.EnsureAsync(predicate, factory)).FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task EnsureAsync_Task_AsyncPredicateAndErrorFactory_FailsWhenPredicateIsFalse()
    {
        Func<int, Task<bool>> predicate = v => Task.FromResult(v > 10);
        Func<int, Error> factory = v => Error.Validation("SMALL", $"{v}");

        (await AsTask(Ok).EnsureAsync(predicate, factory)).FirstError.Code.Should().Be("SMALL");
        (await AsTask(20).EnsureAsync(predicate, factory)).Value.Should().Be(20);
    }

    [Fact]
    public async Task EnsureAsync_ValueTask_AsyncPredicateAndErrorFactory_FailsWhenPredicateIsFalse()
    {
        Func<int, Task<bool>> predicate = v => Task.FromResult(v > 10);
        Func<int, Error> factory = v => Error.Validation("SMALL", $"{v}");

        (await Vt(Ok).EnsureAsync(predicate, factory)).FirstError.Code.Should().Be("SMALL");
        (await Vt(20).EnsureAsync(predicate, factory)).Value.Should().Be(20);
    }

    #endregion
}
