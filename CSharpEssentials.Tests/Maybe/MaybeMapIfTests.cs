using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeMapIfTests
{
    #region MapIf(bool, Func<T, T>)

    [Fact]
    public void MapIf_BoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.MapIf(true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void MapIf_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.MapIf(false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void MapIf_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.MapIf(true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapIf(Func<bool>, Func<T, T>)

    [Fact]
    public void MapIf_FuncBoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.MapIf(() => true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void MapIf_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.MapIf(() => false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void MapIf_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.MapIf(() => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapIf(Func<T, bool>, Func<T, T>)

    [Fact]
    public void MapIf_FuncTBoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = maybe.MapIf(x => x > 3, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void MapIf_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(2);

        Maybe<int> result = maybe.MapIf(x => x > 3, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public void MapIf_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = maybe.MapIf(_ => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapIfAsync(bool, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_BoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.MapIfAsync(true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.MapIfAsync(false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.MapIfAsync(true, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapIfAsync(Func<bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_FuncBoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.MapIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.MapIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.MapIfAsync(() => true, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region MapIfAsync(Func<T, bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_FuncTBoolTrue_WithValue_ShouldMap()
    {
        var maybe = Maybe<int>.From(5);

        Maybe<int> result = await maybe.MapIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        var maybe = Maybe<int>.From(2);

        Maybe<int> result = await maybe.MapIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task MapIfAsync_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.MapIfAsync(_ => true, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(bool, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_Task_BoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_Task_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(Func<bool>, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_Task_FuncBoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_Task_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(Func<T, bool>, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_Task_FuncTBoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task MapIfAsync_Task_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(_ => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(bool, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_Task_AsyncBoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_AsyncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(Func<bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_Task_AsyncFuncBoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_AsyncFuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region Task<Maybe<T>> MapIfAsync(Func<T, bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_Task_AsyncFuncTBoolTrue_WithValue_ShouldMap()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_Task_AsyncFuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(bool, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_ValueTask_BoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_BoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_BoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(Func<bool>, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncBoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => false, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncBoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(Func<T, bool>, Func<T, T>)

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncTBoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, x => x * 2);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, _ => 999);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_FuncTBoolTrue_WithNoValue_ShouldReturnNone()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        Maybe<int> result = await maybeTask.MapIfAsync(_ => true, _ => 999);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(bool, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncBoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(Func<bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncFuncBoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => true, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncFuncBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(() => false, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region ValueTask<Maybe<T>> MapIfAsync(Func<T, bool>, Func<T, Task<T>>)

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncFuncTBoolTrue_WithValue_ShouldMap()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(5));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, async x =>
        {
            await Task.Yield();
            return x * 2;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapIfAsync_ValueTask_AsyncFuncTBoolFalse_WithValue_ShouldReturnOriginal()
    {
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.From(2));

        Maybe<int> result = await maybeTask.MapIfAsync(x => x > 3, async _ =>
        {
            await Task.Yield();
            return 999;
        });

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    #endregion
}
