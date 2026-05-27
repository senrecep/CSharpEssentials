using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeFromAsyncTests
{
    #region Maybe<T>.FromAsync — Task<T?>

    [Fact]
    public async Task FromAsync_Task_WithNonNullValue_ShouldHaveValue()
    {
        Task<string?> task = Task.FromResult<string?>("hello");

        Maybe<string> result = await Maybe<string>.FromAsync(task);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public async Task FromAsync_Task_WithNullValue_ShouldHaveNoValue()
    {
        Task<string?> task = Task.FromResult<string?>(null);

        Maybe<string> result = await Maybe<string>.FromAsync(task);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Maybe<T>.FromAsync — Func<Task<T?>>

    [Fact]
    public async Task FromAsync_FuncTask_WithNonNullValue_ShouldHaveValue()
    {
        Task<string?> task = Task.FromResult<string?>("async-value");

        Maybe<string> result = await Maybe<string>.FromAsync(() => task);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("async-value");
    }

    [Fact]
    public async Task FromAsync_FuncTask_WithNullValue_ShouldHaveNoValue()
    {
        Task<string?> task = Task.FromResult<string?>(null);

        Maybe<string> result = await Maybe<string>.FromAsync(() => task);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region Maybe.From static — Task<T?> via extension

    [Fact]
    public async Task From_TaskExtension_WithNonNullValue_ShouldHaveValue()
    {
        Task<string?> task = Task.FromResult<string?>("world");

        Maybe<string> result = await CSharpEssentials.Maybe.Maybe.From<string>(task);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("world");
    }

    [Fact]
    public async Task From_TaskExtension_WithNullValue_ShouldHaveNoValue()
    {
        Task<string?> task = Task.FromResult<string?>(null);

        Maybe<string> result = await CSharpEssentials.Maybe.Maybe.From<string>(task);

        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
