using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeExecuteAsyncTests
{
    #region Execute async — Func<T, Task>

    [Fact]
    public async Task Execute_FuncTask_WithValue_ShouldInvokeAction()
    {
        var maybe = Maybe<int>.From(5);
        int captured = 0;

        await maybe.Execute(v => { captured = v; return Task.CompletedTask; });

        captured.Should().Be(5);
    }

    [Fact]
    public async Task Execute_FuncTask_WithNoValue_ShouldNotInvokeAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Execute(v => { captured = v; return Task.CompletedTask; });

        captured.Should().Be(0);
    }

    #endregion

    #region Execute async — Func<T, ValueTask>

    [Fact]
    public async Task Execute_FuncValueTask_WithValue_ShouldInvokeAction()
    {
        var maybe = Maybe<int>.From(10);
        int captured = 0;

        await maybe.Execute(v => { captured = v; return ValueTask.CompletedTask; });

        captured.Should().Be(10);
    }

    [Fact]
    public async Task Execute_FuncValueTask_WithNoValue_ShouldNotInvokeAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int captured = 0;

        await maybe.Execute(v => { captured = v; return ValueTask.CompletedTask; });

        captured.Should().Be(0);
    }

    #endregion

    #region ExecuteNoValue async — Func<Task>

    [Fact]
    public async Task ExecuteNoValue_FuncTask_WithNoValue_ShouldInvokeAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool invoked = false;

        await maybe.ExecuteNoValue(() => { invoked = true; return Task.CompletedTask; });

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNoValue_FuncTask_WithValue_ShouldNotInvokeAction()
    {
        var maybe = Maybe<int>.From(3);
        bool invoked = false;

        await maybe.ExecuteNoValue(() => { invoked = true; return Task.CompletedTask; });

        invoked.Should().BeFalse();
    }

    #endregion

    #region ExecuteNoValue async — Func<ValueTask>

    [Fact]
    public async Task ExecuteNoValue_FuncValueTask_WithNoValue_ShouldInvokeAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool invoked = false;

        await maybe.ExecuteNoValue(() => { invoked = true; return ValueTask.CompletedTask; });

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNoValue_FuncValueTask_WithValue_ShouldNotInvokeAction()
    {
        var maybe = Maybe<int>.From(7);
        bool invoked = false;

        await maybe.ExecuteNoValue(() => { invoked = true; return ValueTask.CompletedTask; });

        invoked.Should().BeFalse();
    }

    #endregion

    #region Execute extension on Task<Maybe<T>> — Action<T>

    [Fact]
    public async Task Execute_TaskMaybe_Action_WithValue_ShouldInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(20));
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; });

        captured.Should().Be(20);
    }

    [Fact]
    public async Task Execute_TaskMaybe_Action_WithNoValue_ShouldNotInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; });

        captured.Should().Be(0);
    }

    #endregion

    #region Execute extension on Task<Maybe<T>> — Func<T, Task>

    [Fact]
    public async Task Execute_TaskMaybe_FuncTask_WithValue_ShouldInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(15));
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; return Task.CompletedTask; });

        captured.Should().Be(15);
    }

    [Fact]
    public async Task Execute_TaskMaybe_FuncTask_WithNoValue_ShouldNotInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; return Task.CompletedTask; });

        captured.Should().Be(0);
    }

    #endregion

    #region Execute extension on ValueTask<Maybe<T>> — Func<T, ValueTask>

    [Fact]
    public async Task Execute_ValueTaskMaybe_FuncValueTask_WithValue_ShouldInvokeAction()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.From(8));
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; return ValueTask.CompletedTask; });

        captured.Should().Be(8);
    }

    [Fact]
    public async Task Execute_ValueTaskMaybe_FuncValueTask_WithNoValue_ShouldNotInvokeAction()
    {
        ValueTask<Maybe<int>> maybeTask = ValueTask.FromResult(Maybe<int>.None);
        int captured = 0;

        await maybeTask.Execute(v => { captured = v; return ValueTask.CompletedTask; });

        captured.Should().Be(0);
    }

    #endregion

    #region ExecuteNoValue extension on Task<Maybe<T>>

    [Fact]
    public async Task ExecuteNoValue_TaskMaybe_Action_WithNoValue_ShouldInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool invoked = false;

        await maybeTask.ExecuteNoValue(() => { invoked = true; });

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNoValue_TaskMaybe_Action_WithValue_ShouldNotInvokeAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(1));
        bool invoked = false;

        await maybeTask.ExecuteNoValue(() => { invoked = true; });

        invoked.Should().BeFalse();
    }

    #endregion
}
