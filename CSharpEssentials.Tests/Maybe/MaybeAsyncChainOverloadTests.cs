using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeAsyncChainOverloadTests
{
    private static Task<Maybe<int>> SomeTask(int value) => Task.FromResult(Maybe<int>.From(value));
    private static Task<Maybe<int>> NoneTask() => Task.FromResult(Maybe<int>.None);
    private static ValueTask<Maybe<int>> SomeValueTask(int value) => new(Maybe<int>.From(value));
    private static ValueTask<Maybe<int>> NoneValueTask() => new(Maybe<int>.None);

    #region Bind / Map / Where / Select

    [Fact]
    public void Bind_WithContext_ShouldPassContextToSelector()
    {
        Maybe<int>.From(2).Bind((x, ctx) => Maybe<int>.From(x + ctx), 40).Value.Should().Be(42);
        Maybe<int>.From(2).Bind((x, ctx) => Maybe<int>.None, 40).HasNoValue.Should().BeTrue();
        Maybe<int>.None.Bind((x, ctx) => Maybe<int>.From(x + ctx), 40).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task MapAsync_ValueTaskSource_ValueTaskSelector_ShouldMapOnlySome()
    {
        (await SomeValueTask(2).MapAsync(x => new ValueTask<string>($"v{x}"))).Value.Should().Be("v2");
        (await NoneValueTask().MapAsync(x => new ValueTask<string>($"v{x}"))).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task WhereAsync_ValueTaskSource_ValueTaskPredicate_ShouldFilter()
    {
        (await SomeValueTask(2).WhereAsync(x => new ValueTask<bool>(x > 1))).Value.Should().Be(2);
        (await SomeValueTask(2).WhereAsync(x => new ValueTask<bool>(x > 5))).HasNoValue.Should().BeTrue();
        (await NoneValueTask().WhereAsync(_ => new ValueTask<bool>(true))).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Select_ShouldMapValue()
    {
        Maybe<int>.From(2).Select(x => x * 3).Value.Should().Be(6);
        Maybe<int>.None.Select(x => x * 3).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void SelectMany_SingleSelector_ShouldBind()
    {
        Maybe<int>.From(2).SelectMany(x => Maybe<string>.From($"v{x}")).Value.Should().Be("v2");
        Maybe<int>.From(2).SelectMany(_ => Maybe<string>.None).HasNoValue.Should().BeTrue();
        Maybe<int>.None.SelectMany(x => Maybe<string>.From($"v{x}")).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void StaticFrom_Factory_ShouldWrapResultOrNone()
    {
        CSharpEssentials.Maybe.Maybe.From(() => "x").Value.Should().Be("x");
        CSharpEssentials.Maybe.Maybe.From<string>(() => null).HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void QuerySyntax_WithTwoFroms_ShouldProjectWhenBothHaveValues()
    {
        Maybe<int> a = Maybe<int>.From(2);
        Maybe<int> b = Maybe<int>.From(5);
        Maybe<int> none = Maybe<int>.None;

        Maybe<int> both = from x in a from y in b select x * y;
        Maybe<int> secondMissing = from x in a from y in none select x * y;
        Maybe<int> firstMissing = from x in none from y in b select x * y;

        both.Value.Should().Be(10);
        secondMissing.HasNoValue.Should().BeTrue();
        firstMissing.HasNoValue.Should().BeTrue();
    }

    #endregion

    #region ExecuteAsync / ExecuteNoValueAsync on tasks

    [Fact]
    public async Task ExecuteAsync_ValueTaskSource_Action_ShouldRunOnlyForSome()
    {
        var seen = new List<int>();

        await SomeValueTask(3).ExecuteAsync(x => seen.Add(x));
        await NoneValueTask().ExecuteAsync(x => seen.Add(x));

        seen.Should().Equal(3);
    }

    [Fact]
    public async Task ExecuteAsync_ValueTaskSource_ValueTaskAction_ShouldRunOnlyForSome()
    {
        var seen = new List<int>();

        await SomeValueTask(3).ExecuteAsync(x => { seen.Add(x); return ValueTask.CompletedTask; });
        await NoneValueTask().ExecuteAsync(x => { seen.Add(x); return ValueTask.CompletedTask; });

        seen.Should().Equal(3);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_TaskSource_Action_ShouldRunOnlyForNone()
    {
        int calls = 0;

        await SomeTask(3).ExecuteNoValueAsync(() => calls++);
        calls.Should().Be(0);

        await NoneTask().ExecuteNoValueAsync(() => calls++);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_TaskSource_TaskAction_ShouldRunOnlyForNone()
    {
        int calls = 0;

        await SomeTask(3).ExecuteNoValueAsync(() => { calls++; return Task.CompletedTask; });
        calls.Should().Be(0);

        await NoneTask().ExecuteNoValueAsync(() => { calls++; return Task.CompletedTask; });
        calls.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_ValueTaskSource_Action_ShouldRunOnlyForNone()
    {
        int calls = 0;

        await SomeValueTask(3).ExecuteNoValueAsync(() => calls++);
        calls.Should().Be(0);

        await NoneValueTask().ExecuteNoValueAsync(() => calls++);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_ValueTaskSource_ValueTaskAction_ShouldRunOnlyForNone()
    {
        int calls = 0;

        await SomeValueTask(3).ExecuteNoValueAsync(() => { calls++; return ValueTask.CompletedTask; });
        calls.Should().Be(0);

        await NoneValueTask().ExecuteNoValueAsync(() => { calls++; return ValueTask.CompletedTask; });
        calls.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_CancelledWhileSourcePending_ShouldThrowWithoutRunning()
    {
        var source = new TaskCompletionSource<Maybe<int>>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        int calls = 0;

        Func<Task> act = () => source.Task.ExecuteNoValueAsync(() => calls++, cts.Token).WaitAsync(TimeSpan.FromSeconds(5));

        await act.Should().ThrowAsync<OperationCanceledException>();
        calls.Should().Be(0);
    }

    #endregion

    #region Equality with objects

    [Fact]
    public void Equals_Object_OfUnrelatedType_ShouldBeFalse()
    {
        Maybe<int>.From(1).Equals((object)"1").Should().BeFalse();
        Maybe<int>.None.Equals((object)"1").Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithObject_ShouldDelegateToEquals()
    {
        object boxedSame = Maybe<int>.From(1);
        object boxedOther = Maybe<int>.From(2);

        (Maybe<int>.From(1) == boxedSame).Should().BeTrue();
        (Maybe<int>.From(1) == boxedOther).Should().BeFalse();
        (Maybe<int>.From(1) != boxedOther).Should().BeTrue();
    }

    #endregion
}
