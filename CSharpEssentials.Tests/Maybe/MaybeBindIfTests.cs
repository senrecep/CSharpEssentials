using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeBindIfTests
{
    #region BindIf(bool, Func<T, Maybe<T>>)

    [Fact]
    public void BindIf_BoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.BindIf(true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void BindIf_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.BindIf(false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void BindIf_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.BindIf(true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindIf(Func<bool>, Func<T, Maybe<T>>)

    [Fact]
    public void BindIf_FuncBoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.BindIf(() => true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void BindIf_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.BindIf(() => false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void BindIf_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.BindIf(() => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindIf(Func<T, bool>, Func<T, Maybe<T>>)

    [Fact]
    public void BindIf_FuncTBoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.BindIf(x => x > 3, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void BindIf_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(2);

        Maybe<int> result = maybe.BindIf(x => x > 3, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void BindIf_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.BindIf(_ => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindIfAsync(bool, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_BoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.BindIfAsync(true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.BindIfAsync(false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.BindIfAsync(true, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindIfAsync(Func<bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_FuncBoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.BindIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.BindIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.BindIfAsync(() => true, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region BindIfAsync(Func<T, bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_FuncTBoolTrue_WithValue_ShouldBind()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.BindIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(2);

        Maybe<int> result = await maybe.BindIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task BindIfAsync_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.BindIfAsync(_ => true, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(bool, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_Task_BoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_Task_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(Func<bool>, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_Task_FuncBoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_Task_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(Func<T, bool>, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_Task_FuncTBoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task BindIfAsync_Task_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(_ => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(bool, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_Task_AsyncBoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_AsyncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(Func<bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_Task_AsyncFuncBoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_AsyncFuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region Task<Maybe<T>> BindIfAsync(Func<T, bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_Task_AsyncFuncTBoolTrue_WithValue_ShouldBind()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_Task_AsyncFuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(bool, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_BoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(Func<bool>, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncBoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => false, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(Func<T, bool>, Func<T, Maybe<T>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncTBoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, x => Maybe<int>.From(x * 2));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, _ => Maybe<int>.From(999));

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.BindIfAsync(_ => true, _ => Maybe<int>.From(999));

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(bool, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncBoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(Func<bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncFuncBoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncFuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region ValueTask<Maybe<T>> BindIfAsync(Func<T, bool>, Func<T, Task<Maybe<T>>>)

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncFuncTBoolTrue_WithValue_ShouldBind()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return Maybe<int>.From(x * 2);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task BindIfAsync_ValueTask_AsyncFuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.BindIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return Maybe<int>.From(999);
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion
}
