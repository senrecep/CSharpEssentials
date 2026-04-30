using CSharpEssentials.Core;
using FluentAssertions;
using static CSharpEssentials.Tests.TestData;

namespace CSharpEssentials.Tests.Core;

public class GeneralExtensionsTests
{
    [Fact]
    public void IsEmpty_WithNull_ShouldReturnTrue()
    {
        Strings.Null.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithEmptyString_ShouldReturnTrue()
    {
        Strings.Empty.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithWhitespace_ShouldReturnTrue()
    {
        Strings.Whitespace.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithNonEmptyString_ShouldReturnFalse()
    {
        Strings.PascalCase.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotEmpty_WithNull_ShouldReturnFalse()
    {
        Strings.Null.IsNotEmpty().Should().BeFalse();
    }

    [Fact]
    public void IsNotEmpty_WithNonEmptyString_ShouldReturnTrue()
    {
        Strings.PascalCase.IsNotEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNull_WithNull_ShouldReturnTrue()
    {
        string? value = null;
        value.IsNull().Should().BeTrue();
    }

    [Fact]
    public void IsNull_WithNotNull_ShouldReturnFalse()
    {
        "test".IsNull().Should().BeFalse();
    }

    [Fact]
    public void IsNotNull_WithNotNull_ShouldReturnTrue()
    {
        "test".IsNotNull().Should().BeTrue();
    }

    [Fact]
    public void IsNotNull_WithNull_ShouldReturnFalse()
    {
        string? value = null;
        value.IsNotNull().Should().BeFalse();
    }

    [Fact]
    public void IsTrue_WithTrue_ShouldReturnTrue()
    {
        true.IsTrue().Should().BeTrue();
    }

    [Fact]
    public void IsTrue_WithFalse_ShouldReturnFalse()
    {
        false.IsTrue().Should().BeFalse();
    }

    [Fact]
    public void IsFalse_WithFalse_ShouldReturnTrue()
    {
        false.IsFalse().Should().BeTrue();
    }

    [Fact]
    public void IsFalse_WithTrue_ShouldReturnFalse()
    {
        true.IsFalse().Should().BeFalse();
    }

    [Fact]
    public void ExplicitCast_ShouldCastCorrectly()
    {
        object value = 42;
        int result = value.ExplicitCast<int>();
        result.Should().Be(42);
    }

    [Fact]
    public void ToStringFromGuid_And_ToGuidFromString_ShouldRoundTrip()
    {
        Guid guid = Guids.ValidGuid;
        string str = guid.ToStringFromGuid();
        Guid result = str.ToGuidFromString();

        result.Should().Be(guid);
    }

    [Fact]
    public void ToGuidFromString_WithReadOnlySpan_ShouldWork()
    {
        Guid guid = Guids.ValidGuid;
        string str = guid.ToStringFromGuid();
        ReadOnlySpan<char> span = str;
        Guid result = span.ToGuidFromString();

        result.Should().Be(guid);
    }

    [Fact]
    public void MsToDateTime_ShouldConvertCorrectly()
    {
        long timestamp = Dates.UnixTimestampMs;
        DateTime result = timestamp.MsToDateTime();

        result.Should().BeCloseTo(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetTypeGroup_ShouldGroupCorrectly()
    {
        TestValue value = TestValue.Value150;
        GroupValue result = value.GetTypeGroup<GroupValue, TestValue>(100);

        result.Should().Be(GroupValue.Group100);
    }

    private enum TestValue { Value100 = 100, Value150 = 150, Value200 = 200 }
    private enum GroupValue { Group100 = 100, Group200 = 200 }

    [Fact]
    public async Task AsTask_ShouldWrapInTask()
    {
        int value = 42;
        Task<int> task = value.AsTask();

        int result = await task;
        result.Should().Be(42);
        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task AsValueTask_ShouldWrapInValueTask()
    {
        int value = 42;
        ValueTask<int> valueTask = value.AsValueTask();

        int result = await valueTask;
        result.Should().Be(42);
        valueTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task WithCancellation_WithTask_WhenNotCancelled_ShouldComplete()
    {
        var task = Task.Delay(10);
        await task.WithCancellation(CancellationToken.None);

        task.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task WithCancellation_WithTask_WhenCancelled_ShouldThrow()
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(10);
        var task = Task.Delay(1000);

        await Assert.ThrowsAsync<OperationCanceledException>(() => task.WithCancellation(cts.Token));
    }

    [Fact]
    public async Task WithCancellation_WithTaskT_ShouldWork()
    {
        var task = Task.FromResult(42);
        int result = await task.WithCancellation(CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public async Task WithCancellation_WithValueTask_ShouldWork()
    {
        var valueTask = ValueTask.FromResult(42);
        int result = await valueTask.WithCancellation(CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public void IfTrue_WhenTrue_ShouldExecuteAction()
    {
        bool executed = false;
        bool result = true.IfTrue(() => executed = true);

        executed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public void IfTrue_WhenFalse_ShouldNotExecuteAction()
    {
        bool executed = false;
        bool result = false.IfTrue(() => executed = true);

        executed.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void IfFalse_WhenFalse_ShouldExecuteAction()
    {
        bool executed = false;
        bool result = false.IfFalse(() => executed = true);

        executed.Should().BeTrue();
        result.Should().BeFalse();
    }

    [Fact]
    public void IfNotNull_WithNotNull_ShouldExecuteAction()
    {
        bool executed = false;
        bool result = "test".IfNotNull(_ => executed = true);

        executed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public void IfNotNull_WithNull_ShouldNotExecuteAction()
    {
        bool executed = false;
        string? value = null;
        bool result = value.IfNotNull(_ => executed = true);

        executed.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void IfNotNull_WithElseAction_ShouldExecuteCorrectly()
    {
        bool ifExecuted = false;
        bool elseExecuted = false;
        bool result = "test".IfNotNull(_ => ifExecuted = true, () => elseExecuted = true);

        ifExecuted.Should().BeTrue();
        elseExecuted.Should().BeFalse();
        result.Should().BeTrue();
    }

    [Fact]
    public void IfNull_WithNull_ShouldExecuteAction()
    {
        bool executed = false;
        string? value = null;
        bool result = value.IfNull(() => executed = true);

        executed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public void IfNull_WithNotNull_ShouldNotExecuteAction()
    {
        bool executed = false;
        bool result = "test".IfNull(() => executed = true);

        executed.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void IfNull_WithElseAction_ShouldExecuteCorrectly()
    {
        bool ifExecuted = false;
        bool elseExecuted = false;
        string? value = null;
        bool result = value.IfNull(() => ifExecuted = true, _ => elseExecuted = true);

        ifExecuted.Should().BeTrue();
        elseExecuted.Should().BeFalse();
        result.Should().BeTrue();
    }
}
