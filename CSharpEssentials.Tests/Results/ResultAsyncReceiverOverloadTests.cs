using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultAsyncReceiverOverloadTests
{
    private static readonly Error First = Error.Failure("E1", "First error");
    private static readonly Error Second = Error.Failure("E2", "Second error");
    private static readonly Error Replacement = Error.Conflict("REPLACED", "Replacement error");

    private static Result Failed => new[] { First, Second };

    private static ValueTask<Result> Vt(Result result) => new(result);

    private static Task<Result> AsTask(Result result) => Task.FromResult(result);

    #region ElseAsync on ValueTask<Result>

    [Fact]
    public async Task ElseAsync_ValueTask_ErrorFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], Error> onFailure = errors => Error.Conflict("COUNT", $"{errors.Length}");

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Should().ContainSingle().Which.Description.Should().Be("2");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ErrorsFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], IEnumerable<Error>> onFailure = errors => errors.AsEnumerable().Reverse();

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Select(e => e.Code).Should().Equal("E2", "E1");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_Error_ReplacesErrorsOnFailure()
    {
        Result failed = await Vt(Failed).ElseAsync(Replacement);
        Result succeeded = await Vt(Result.Success()).ElseAsync(Replacement);

        failed.Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_AsyncErrorFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], Task<Error>> onFailure = errors => Task.FromResult(Error.Conflict("LAST", errors[^1].Code));

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Should().ContainSingle().Which.Description.Should().Be("E2");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_AsyncErrorsFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], Task<IEnumerable<Error>>> onFailure = errors => Task.FromResult<IEnumerable<Error>>([.. errors, Replacement]);

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "REPLACED");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_TaskOfError_ReplacesErrorsOnFailure()
    {
        Result failed = await Vt(Failed).ElseAsync(Task.FromResult(Replacement));
        Result succeeded = await Vt(Result.Success()).ElseAsync(Task.FromResult(Replacement));

        failed.Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ValueTaskErrorFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], ValueTask<Error>> onFailure = errors => ValueTask.FromResult(Error.Conflict("COUNT", $"{errors.Length}"));

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Should().ContainSingle().Which.Description.Should().Be("2");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ValueTaskErrorsFactory_ReplacesErrorsOnFailure()
    {
        Func<Error[], ValueTask<IEnumerable<Error>>> onFailure = errors => ValueTask.FromResult(errors.AsEnumerable().Reverse());

        Result failed = await Vt(Failed).ElseAsync(onFailure);
        Result succeeded = await Vt(Result.Success()).ElseAsync(onFailure);

        failed.Errors.Select(e => e.Code).Should().Equal("E2", "E1");
        succeeded.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ElseAsync_ValueTask_ValueTaskOfError_ReplacesErrorsOnFailure()
    {
        Result failed = await Vt(Failed).ElseAsync(ValueTask.FromResult(Replacement));
        Result succeeded = await Vt(Result.Success()).ElseAsync(ValueTask.FromResult(Replacement));

        failed.Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
        succeeded.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ElseDoAsync on ValueTask<Result>

    [Fact]
    public async Task ElseDoAsync_ValueTask_TaskAction_RunsOnlyOnFailure()
    {
        var seen = new List<string>();
        Func<Error[], Task> onFailure = errors =>
        {
            seen.AddRange(errors.Select(e => e.Code));
            return Task.CompletedTask;
        };

        Result succeeded = await Vt(Result.Success()).ElseDoAsync(onFailure);
        Result failed = await Vt(Failed).ElseDoAsync(onFailure);

        seen.Should().Equal("E1", "E2");
        succeeded.IsSuccess.Should().BeTrue();
        failed.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ElseDoAsync_ValueTask_ValueTaskAction_RunsOnlyOnFailure()
    {
        var seen = new List<string>();
        Func<Error[], ValueTask> onFailure = errors =>
        {
            seen.AddRange(errors.Select(e => e.Code));
            return ValueTask.CompletedTask;
        };

        Result succeeded = await Vt(Result.Success()).ElseDoAsync(onFailure);
        Result failed = await Vt(Failed).ElseDoAsync(onFailure);

        seen.Should().Equal("E1", "E2");
        succeeded.IsSuccess.Should().BeTrue();
        failed.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ElseDoFirstAsync_ValueTask_TaskAction_RunsOnlyOnFailureWithFirstError()
    {
        var seen = new List<string>();
        Func<Error, Task> onFirstFailure = error =>
        {
            seen.Add(error.Code);
            return Task.CompletedTask;
        };

        Result succeeded = await Vt(Result.Success()).ElseDoFirstAsync(onFirstFailure);
        Result failed = await Vt(Failed).ElseDoFirstAsync(onFirstFailure);

        seen.Should().Equal("E1");
        succeeded.IsSuccess.Should().BeTrue();
        failed.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ElseDoFirstAsync_ValueTask_ValueTaskAction_RunsOnlyOnFailureWithFirstError()
    {
        var seen = new List<string>();
        Func<Error, ValueTask> onFirstFailure = error =>
        {
            seen.Add(error.Code);
            return ValueTask.CompletedTask;
        };

        Result succeeded = await Vt(Result.Success()).ElseDoFirstAsync(onFirstFailure);
        Result failed = await Vt(Failed).ElseDoFirstAsync(onFirstFailure);

        seen.Should().Equal("E1");
        succeeded.IsSuccess.Should().BeTrue();
        failed.Errors.Should().HaveCount(2);
    }

    #endregion

    #region FinallyAsync

    [Fact]
    public void Finally_Action_RunsForBothStatesAndReturnsSameResult()
    {
        var seen = new List<bool>();

        Result success = Result.Success().Finally(r => seen.Add(r.IsSuccess));
        Result failure = Failed.Finally(r => seen.Add(r.IsSuccess));

        seen.Should().Equal(true, false);
        success.IsSuccess.Should().BeTrue();
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task FinallyAsync_Task_Func_ReturnsProjection()
    {
        Func<Result, string> func = r => r.IsSuccess ? "ok" : r.FirstError.Code;

        string onSuccess = await AsTask(Result.Success()).FinallyAsync(func);
        string onFailure = await AsTask(Failed).FinallyAsync(func);

        onSuccess.Should().Be("ok");
        onFailure.Should().Be("E1");
    }

    [Fact]
    public async Task FinallyAsync_Task_AsyncFunc_ReturnsProjection()
    {
        Func<Result, Task<int>> func = r => Task.FromResult(r.ErrorsOrEmptyArray.Length);

        int count = await AsTask(Failed).FinallyAsync(func);

        count.Should().Be(2);
    }

    [Fact]
    public async Task FinallyAsync_Task_Action_RunsAndReturnsSameResult()
    {
        Result? observed = null;
        Action<Result> action = r => observed = r;

        Result result = await AsTask(Failed).FinallyAsync(action);

        observed.Should().Be(Failed);
        result.Should().Be(Failed);
    }

    [Fact]
    public async Task FinallyAsync_Task_AsyncAction_RunsAndReturnsSameResult()
    {
        int calls = 0;
        Func<Result, Task> action = _ =>
        {
            calls++;
            return Task.CompletedTask;
        };

        Result result = await AsTask(Result.Success()).FinallyAsync(action);

        calls.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_Func_ReturnsProjection()
    {
        Func<Result, int> func = r => r.ErrorsOrEmptyArray.Length;

        int count = await Vt(Failed).FinallyAsync(func);

        count.Should().Be(2);
    }

    [Fact]
    public async Task FinallyAsync_ValueTask_Action_RunsAndReturnsSameResult()
    {
        int calls = 0;
        Action<Result> action = _ => calls++;

        Result result = await Vt(Failed).FinallyAsync(action);

        calls.Should().Be(1);
        result.Should().Be(Failed);
    }

    #endregion

    #region Map and Bind

    [Fact]
    public async Task MapAsync_ValueTask_Func_MapsSuccessAndKeepsErrors()
    {
        Func<int> map = () => 42;

        Result<int> success = await Vt(Result.Success()).MapAsync(map);
        Result<int> failure = await Vt(Failed).MapAsync(map);

        success.Value.Should().Be(42);
        failure.Errors.Select(e => e.Code).Should().Equal("E1", "E2");
    }

    [Fact]
    public async Task BindAsync_Task_FuncOfResultT_BindsSuccessAndKeepsErrors()
    {
        Func<Result<string>> func = () => "bound";

        Result<string> success = await AsTask(Result.Success()).BindAsync(func);
        Result<string> failure = await AsTask(Failed).BindAsync(func);

        success.Value.Should().Be("bound");
        failure.Errors.Should().HaveCount(2);
    }

    #endregion

    #region Match on ValueTask<Result>

    [Fact]
    public async Task MatchAsync_ValueTask_SyncHandlers_PicksBranch()
    {
        Func<string> onSuccess = () => "ok";
        Func<Error[], string> onFailure = errors => $"{errors.Length}";

        (await Vt(Result.Success()).MatchAsync(onSuccess, onFailure)).Should().Be("ok");
        (await Vt(Failed).MatchAsync(onSuccess, onFailure)).Should().Be("2");
    }

    [Fact]
    public async Task MatchAsync_ValueTask_AsyncHandlers_PicksBranch()
    {
        Func<Task<string>> onSuccess = () => Task.FromResult("ok");
        Func<Error[], Task<string>> onFailure = errors => Task.FromResult($"{errors.Length}");

        (await Vt(Result.Success()).MatchAsync(onSuccess, onFailure)).Should().Be("ok");
        (await Vt(Failed).MatchAsync(onSuccess, onFailure)).Should().Be("2");
    }

    [Fact]
    public async Task MatchFirstAsync_ValueTask_SyncHandlers_ReceivesFirstError()
    {
        Func<string> onSuccess = () => "ok";
        Func<Error, string> onFailure = error => error.Code;

        (await Vt(Result.Success()).MatchFirstAsync(onSuccess, onFailure)).Should().Be("ok");
        (await Vt(Failed).MatchFirstAsync(onSuccess, onFailure)).Should().Be("E1");
    }

    [Fact]
    public async Task MatchFirstAsync_ValueTask_AsyncHandlers_ReceivesFirstError()
    {
        Func<Task<string>> onSuccess = () => Task.FromResult("ok");
        Func<Error, Task<string>> onFailure = error => Task.FromResult(error.Code);

        (await Vt(Result.Success()).MatchFirstAsync(onSuccess, onFailure)).Should().Be("ok");
        (await Vt(Failed).MatchFirstAsync(onSuccess, onFailure)).Should().Be("E1");
    }

    [Fact]
    public async Task MatchLastAsync_ValueTask_AsyncHandlers_ReceivesLastError()
    {
        Func<Task<string>> onSuccess = () => Task.FromResult("ok");
        Func<Error, Task<string>> onFailure = error => Task.FromResult(error.Code);

        (await Vt(Result.Success()).MatchLastAsync(onSuccess, onFailure)).Should().Be("ok");
        (await Vt(Failed).MatchLastAsync(onSuccess, onFailure)).Should().Be("E2");
    }

    #endregion

    #region Switch on ValueTask<Result>

    [Fact]
    public async Task SwitchAsync_ValueTask_SyncHandlers_RunsMatchingBranch()
    {
        var log = new List<string>();
        Action onSuccess = () => log.Add("ok");
        Action<Error[]> onFailure = errors => log.Add($"{errors.Length}");

        await Vt(Result.Success()).SwitchAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "2");
    }

    [Fact]
    public async Task SwitchAsync_ValueTask_AsyncHandlers_RunsMatchingBranch()
    {
        var log = new List<string>();
        Func<Task> onSuccess = () =>
        {
            log.Add("ok");
            return Task.CompletedTask;
        };
        Func<Error[], Task> onFailure = errors =>
        {
            log.Add($"{errors.Length}");
            return Task.CompletedTask;
        };

        await Vt(Result.Success()).SwitchAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "2");
    }

    [Fact]
    public async Task SwitchFirstAsync_ValueTask_SyncHandlers_ReceivesFirstError()
    {
        var log = new List<string>();
        Action onSuccess = () => log.Add("ok");
        Action<Error> onFailure = error => log.Add(error.Code);

        await Vt(Result.Success()).SwitchFirstAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchFirstAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "E1");
    }

    [Fact]
    public async Task SwitchFirstAsync_ValueTask_AsyncHandlers_ReceivesFirstError()
    {
        var log = new List<string>();
        Func<Task> onSuccess = () =>
        {
            log.Add("ok");
            return Task.CompletedTask;
        };
        Func<Error, Task> onFailure = error =>
        {
            log.Add(error.Code);
            return Task.CompletedTask;
        };

        await Vt(Result.Success()).SwitchFirstAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchFirstAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "E1");
    }

    [Fact]
    public async Task SwitchLastAsync_ValueTask_SyncHandlers_ReceivesLastError()
    {
        var log = new List<string>();
        Action onSuccess = () => log.Add("ok");
        Action<Error> onFailure = error => log.Add(error.Code);

        await Vt(Result.Success()).SwitchLastAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchLastAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "E2");
    }

    [Fact]
    public async Task SwitchLastAsync_ValueTask_AsyncHandlers_ReceivesLastError()
    {
        var log = new List<string>();
        Func<Task> onSuccess = () =>
        {
            log.Add("ok");
            return Task.CompletedTask;
        };
        Func<Error, Task> onFailure = error =>
        {
            log.Add(error.Code);
            return Task.CompletedTask;
        };

        await Vt(Result.Success()).SwitchLastAsync(onSuccess, onFailure);
        await Vt(Failed).SwitchLastAsync(onSuccess, onFailure);

        log.Should().Equal("ok", "E2");
    }

    #endregion

    #region Tap

    [Fact]
    public async Task TapAsync_Task_Action_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Action action = () => calls++;

        Result success = await AsTask(Result.Success()).TapAsync(action);
        Result failure = await AsTask(Failed).TapAsync(action);

        calls.Should().Be(1);
        success.IsSuccess.Should().BeTrue();
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task TapAsync_ValueTask_Action_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Action action = () => calls++;

        await Vt(Result.Success()).TapAsync(action);
        Result failure = await Vt(Failed).TapAsync(action);

        calls.Should().Be(1);
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task TapAsync_ValueTask_ConditionAndAction_RunsOnlyWhenConditionHolds()
    {
        int calls = 0;
        Action action = () => calls++;

        await Vt(Result.Success()).TapAsync(() => false, action);
        calls.Should().Be(0);

        await Vt(Result.Success()).TapAsync(() => true, action);
        calls.Should().Be(1);

        await Vt(Failed).TapAsync(() => true, action);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapAsync_ValueTask_BoolConditionAndAction_RunsOnlyWhenConditionHolds()
    {
        int calls = 0;
        Action action = () => calls++;

        await Vt(Result.Success()).TapAsync(false, action);
        calls.Should().Be(0);

        await Vt(Result.Success()).TapAsync(true, action);
        calls.Should().Be(1);

        await Vt(Failed).TapAsync(true, action);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapAsync_ValueTask_ValueTaskAction_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Func<ValueTask> action = () =>
        {
            calls++;
            return ValueTask.CompletedTask;
        };

        Result success = await Vt(Result.Success()).TapAsync(action);
        calls.Should().Be(1);

        Result failure = await Vt(Failed).TapAsync(action);
        calls.Should().Be(1);
        success.IsSuccess.Should().BeTrue();
        failure.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task TapAsync_ValueTask_BoolConditionAndValueTaskAction_RunsOnlyWhenConditionHolds()
    {
        int calls = 0;
        Func<ValueTask> action = () =>
        {
            calls++;
            return ValueTask.CompletedTask;
        };

        await Vt(Result.Success()).TapAsync(false, action);
        calls.Should().Be(0);

        await Vt(Result.Success()).TapAsync(true, action);
        calls.Should().Be(1);

        await Vt(Failed).TapAsync(true, action);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task TapAsync_ValueTask_ConditionAndValueTaskAction_RunsOnlyWhenConditionHolds()
    {
        int calls = 0;
        Func<ValueTask> action = () =>
        {
            calls++;
            return ValueTask.CompletedTask;
        };

        await Vt(Result.Success()).TapAsync(() => false, action);
        calls.Should().Be(0);

        await Vt(Result.Success()).TapAsync(() => true, action);
        calls.Should().Be(1);

        await Vt(Failed).TapAsync(() => true, action);
        calls.Should().Be(1);
    }

    #endregion

    #region Then on ValueTask<Result>

    [Fact]
    public async Task ThenAsync_ValueTask_FuncOfResult_ChainsOnSuccess()
    {
        Func<Result> onSuccess = () => Replacement;

        Result success = await Vt(Result.Success()).ThenAsync(onSuccess);
        Result failure = await Vt(Failed).ThenAsync(onSuccess);

        success.FirstError.Code.Should().Be("REPLACED");
        failure.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task ThenDoAsync_ValueTask_Action_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Action action = () => calls++;

        Result success = await Vt(Result.Success()).ThenDoAsync(action);
        Result failure = await Vt(Failed).ThenDoAsync(action);

        calls.Should().Be(1);
        success.IsSuccess.Should().BeTrue();
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ThenAsync_ValueTask_AsyncFuncOfResult_ChainsOnSuccess()
    {
        Func<Task<Result>> onSuccess = () => Task.FromResult<Result>(Replacement);

        Result success = await Vt(Result.Success()).ThenAsync(onSuccess);
        Result failure = await Vt(Failed).ThenAsync(onSuccess);

        success.FirstError.Code.Should().Be("REPLACED");
        failure.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task ThenDoAsync_ValueTask_AsyncAction_RunsOnlyOnSuccess()
    {
        int calls = 0;
        Func<Task> action = () =>
        {
            calls++;
            return Task.CompletedTask;
        };

        await Vt(Result.Success()).ThenDoAsync(action);
        Result failure = await Vt(Failed).ThenDoAsync(action);

        calls.Should().Be(1);
        failure.Errors.Should().HaveCount(2);
    }

    #endregion

    #region BindIfAsync

    [Fact]
    public async Task BindIfAsync_Task_Condition_BindsOnlyWhenTrue()
    {
        Func<Result> func = () => Replacement;

        Result bound = await AsTask(Result.Success()).BindIfAsync(true, func);
        Result skipped = await AsTask(Result.Success()).BindIfAsync(false, func);

        bound.FirstError.Code.Should().Be("REPLACED");
        skipped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindIfAsync_Task_Predicate_BindsOnlyWhenTrue()
    {
        Func<Result> func = () => Replacement;

        Result bound = await AsTask(Result.Success()).BindIfAsync(() => true, func);
        Result skipped = await AsTask(Result.Success()).BindIfAsync(() => false, func);

        bound.FirstError.Code.Should().Be("REPLACED");
        skipped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_Condition_BindsOnlyWhenTrue()
    {
        Func<Result> func = () => Replacement;

        Result bound = await Vt(Result.Success()).BindIfAsync(true, func);
        Result failure = await Vt(Failed).BindIfAsync(true, func);

        bound.FirstError.Code.Should().Be("REPLACED");
        failure.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_Predicate_BindsOnlyWhenTrue()
    {
        Func<Result> func = () => Replacement;

        Result bound = await Vt(Result.Success()).BindIfAsync(() => true, func);
        Result skipped = await Vt(Result.Success()).BindIfAsync(() => false, func);

        bound.FirstError.Code.Should().Be("REPLACED");
        skipped.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region TryCatchAsync

    [Fact]
    public async Task TryCatchAsync_Task_FuncOfResult_ConvertsExceptionToGivenError()
    {
        Func<Result> func = () => throw new InvalidOperationException("boom");

        Result caught = await AsTask(Result.Success()).TryCatchAsync(func, Replacement);
        Result failure = await AsTask(Failed).TryCatchAsync(func, Replacement);

        caught.Errors.Should().ContainSingle().Which.Code.Should().Be("REPLACED");
        failure.FirstError.Code.Should().Be("E1");
    }

    [Fact]
    public async Task TryCatchAsync_Task_FuncOfResultT_ConvertsExceptionToExceptionError()
    {
        Func<Result<int>> func = () => throw new InvalidOperationException("boom");

        Result<int> caught = await AsTask(Result.Success()).TryCatchAsync(func);

        caught.FirstError.Code.Should().Be(nameof(InvalidOperationException));
        caught.FirstError.Description.Should().Be("boom");
    }

    [Fact]
    public async Task TryCatchAsync_ValueTask_FuncOfResult_ReturnsFuncResultWhenNoException()
    {
        Func<Result> func = () => Result.Success();
        Func<Result> throwing = () => throw new InvalidOperationException("boom");

        Result ok = await Vt(Result.Success()).TryCatchAsync(func);
        Result caught = await Vt(Result.Success()).TryCatchAsync(throwing, Replacement);

        ok.IsSuccess.Should().BeTrue();
        caught.FirstError.Code.Should().Be("REPLACED");
    }

    [Fact]
    public async Task TryCatchAsync_ValueTask_FuncOfResultT_ReturnsValueOrError()
    {
        Func<Result<int>> func = () => 7;
        Func<Result<int>> throwing = () => throw new InvalidOperationException("boom");

        Result<int> ok = await Vt(Result.Success()).TryCatchAsync(func);
        Result<int> caught = await Vt(Result.Success()).TryCatchAsync(throwing, Replacement);
        Result<int> failure = await Vt(Failed).TryCatchAsync(func);

        ok.Value.Should().Be(7);
        caught.FirstError.Code.Should().Be("REPLACED");
        failure.Errors.Should().HaveCount(2);
    }

    #endregion

    [Fact]
    public void Try_ActionWithHandler_ReturnsSuccessOrHandledError()
    {
        Action noop = () => { };
        Action throwing = () => throw new InvalidOperationException("boom");

        Result ok = Result.Try(noop, ex => Error.Failure("HANDLED", ex.Message));
        Result caught = Result.Try(throwing, ex => Error.Failure("HANDLED", ex.Message));

        ok.IsSuccess.Should().BeTrue();
        caught.FirstError.Code.Should().Be("HANDLED");
        caught.FirstError.Description.Should().Be("boom");
    }
}
