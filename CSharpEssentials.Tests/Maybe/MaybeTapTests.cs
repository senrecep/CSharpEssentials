using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeTapTests
{
    #region Tap(Action<T>)

    [Fact]
    public void Tap_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        Maybe<int> result = maybe.Tap(x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = maybe.Tap(_ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Tap(Action)

    [Fact]
    public void Tap_Action_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        bool executed = false;

        Maybe<int> result = maybe.Tap(() => executed = true);

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_Action_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = maybe.Tap(() => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region TapAsync(Func<T, Task>)

    [Fact]
    public async Task TapAsync_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        Maybe<int> result = await maybe.TapAsync(async x =>
        {
            await Task.Yield();
            captured = x;
        });

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = await maybe.TapAsync(async _ =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region TapAsync(Func<Task>)

    [Fact]
    public async Task TapAsync_Action_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        bool executed = false;

        Maybe<int> result = await maybe.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Action_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = await maybe.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapAsync(Action<T>)

    [Fact]
    public async Task TapAsync_Task_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapAsync(x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Task_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(_ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapAsync(Action)

    [Fact]
    public async Task TapAsync_Task_Action_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(() => executed = true);

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Task_Action_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(() => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapAsync(Func<T, Task>)

    [Fact]
    public async Task TapAsync_Task_FuncT_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapAsync(async x =>
        {
            await Task.Yield();
            captured = x;
        });

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Task_FuncT_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async _ =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapAsync(Func<Task>)

    [Fact]
    public async Task TapAsync_Task_Func_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_Task_Func_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapAsync(Action<T>)

    [Fact]
    public async Task TapAsync_ValueTask_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapAsync(x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_ValueTask_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(_ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapAsync(Action)

    [Fact]
    public async Task TapAsync_ValueTask_Action_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(() => executed = true);

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_ValueTask_Action_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(() => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapAsync(Func<T, Task>)

    [Fact]
    public async Task TapAsync_ValueTask_FuncT_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapAsync(async x =>
        {
            await Task.Yield();
            captured = x;
        });

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_ValueTask_FuncT_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async _ =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapAsync(Func<Task>)

    [Fact]
    public async Task TapAsync_ValueTask_Func_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeTrue();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_ValueTask_Func_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
