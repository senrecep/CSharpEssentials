using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeAsMaybeAsyncTests
{
    #region AsMaybeAsync — ValueTask<T?>

    [Fact]
    public async Task AsMaybeAsync_ValueTask_WithNonNullValue_ShouldHaveValue()
    {
        ValueTask<string?> task = ValueTask.FromResult<string?>("hello");

        Maybe<string> result = await task.AsMaybeAsync();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public async Task AsMaybeAsync_ValueTask_WithNullValue_ShouldHaveNoValue()
    {
        ValueTask<string?> task = ValueTask.FromResult<string?>(null);

        Maybe<string> result = await task.AsMaybeAsync();

        result.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region AsMaybeAsync — Task<T?> where T : class

    [Fact]
    public async Task AsMaybeAsync_Task_WithNonNullValue_ShouldHaveValue()
    {
        Task<string?> task = Task.FromResult<string?>("world");

        Maybe<string> result = await task.AsMaybeAsync();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("world");
    }

    [Fact]
    public async Task AsMaybeAsync_Task_WithNullValue_ShouldHaveNoValue()
    {
        Task<string?> task = Task.FromResult<string?>(null);

        Maybe<string> result = await task.AsMaybeAsync();

        result.HasNoValue.Should().BeTrue();
    }

    #endregion
}
