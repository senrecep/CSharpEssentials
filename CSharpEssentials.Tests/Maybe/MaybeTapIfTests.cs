using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeTapIfTests
{
    #region TapIf(bool, Action<T>)

    [Fact]
    public void TapIf_BoolTrue_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        Maybe<int> result = maybe.TapIf(true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TapIf_BoolFalse_WithValue_ShouldNotExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        bool executed = false;

        Maybe<int> result = maybe.TapIf(false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TapIf_BoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = maybe.TapIf(true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region TapIf(Func<bool>, Action<T>)

    [Fact]
    public void TapIf_FuncBoolTrue_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        Maybe<int> result = maybe.TapIf(() => true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TapIf_FuncBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        bool executed = false;

        Maybe<int> result = maybe.TapIf(() => false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TapIf_FuncBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = maybe.TapIf(() => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region TapIf(Func<T, bool>, Action<T>)

    [Fact]
    public void TapIf_FuncTBoolTrue_WithValue_ShouldExecuteAction()
    {
        var maybe = Maybe<int>.From(42);
        int captured = 0;

        Maybe<int> result = maybe.TapIf(x => x > 10, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TapIf_FuncTBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        var maybe = Maybe<int>.From(5);
        bool executed = false;

        Maybe<int> result = maybe.TapIf(x => x > 10, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void TapIf_FuncTBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool executed = false;

        Maybe<int> result = maybe.TapIf(_ => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapIfAsync(bool, Action<T>)

    [Fact]
    public async Task TapIfAsync_Task_BoolTrue_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_Task_BoolFalse_WithValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_Task_BoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapIfAsync(Func<bool>, Action<T>)

    [Fact]
    public async Task TapIfAsync_Task_FuncBoolTrue_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(() => true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_Task_FuncBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(() => false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_Task_FuncBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(() => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> TapIfAsync(Func<T, bool>, Action<T>)

    [Fact]
    public async Task TapIfAsync_Task_FuncTBoolTrue_WithValue_ShouldExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(x => x > 10, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_Task_FuncTBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(x => x > 10, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task TapIfAsync_Task_FuncTBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(_ => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapIfAsync(bool, Action<T>)

    [Fact]
    public async Task TapIfAsync_ValueTask_BoolTrue_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_BoolFalse_WithValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_BoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapIfAsync(Func<bool>, Action<T>)

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncBoolTrue_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(() => true, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(() => false, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(() => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> TapIfAsync(Func<T, bool>, Action<T>)

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncTBoolTrue_WithValue_ShouldExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(42));
        int captured = 0;

        Maybe<int> result = await maybeTask.TapIfAsync(x => x > 10, x => captured = x);

        captured.Should().Be(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncTBoolFalse_WithValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(x => x > 10, _ => executed = true);

        executed.Should().BeFalse();
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task TapIfAsync_ValueTask_FuncTBoolTrue_WithNoValue_ShouldNotExecuteAction()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);
        bool executed = false;

        Maybe<int> result = await maybeTask.TapIfAsync(_ => true, _ => executed = true);

        executed.Should().BeFalse();
        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
