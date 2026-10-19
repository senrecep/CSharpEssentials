using CSharpEssentials.Idempotency;
using FluentAssertions;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyRecordKeyTests
{
    [Fact]
    public void ToStorageKey_Should_Length_Prefix_Every_Part()
    {
        IdempotencyRecordKey key = new("http", "alice", "order-42");

        key.ToStorageKey().Should().Be("idem:v3:4:http|5:alice|8:order-42");
    }

    [Fact]
    public void ToStorageKey_Should_Mark_A_Global_Key()
    {
        IdempotencyRecordKey key = new("http", IdempotencyScope.Global, "order-42");

        key.IsGlobal.Should().BeTrue();
        key.Scope.Should().BeNull();
        key.ToStorageKey().Should().Be("idem:v3:4:http|-|8:order-42");
    }

    [Fact]
    public void ToStorageKey_Should_Count_Utf8_Bytes()
    {
        IdempotencyRecordKey key = new("http", "é", "ключ");

        key.ToStorageKey().Should().Be("idem:v3:4:http|2:é|8:ключ");
    }

    [Fact]
    public void ToStorageKey_Should_Use_The_Given_Prefix()
    {
        IdempotencyRecordKey key = new("http", "alice", "k");

        key.ToStorageKey("app:").Should().Be("app:4:http|5:alice|1:k");
        key.ToStorageKey("").Should().Be("4:http|5:alice|1:k");
    }

    [Fact]
    public void Global_Key_Should_Never_Collide_With_A_Scoped_Key()
    {
        string global = new IdempotencyRecordKey("http", (string?)null, "k").ToStorageKey();
        string[] scopes = ["-", "--", " ", "|", "0:", "1:-", "-|1:k", "global", "(global)", "null"];

        foreach (string scope in scopes)
            new IdempotencyRecordKey("http", scope, "k").ToStorageKey().Should().NotBe(global, $"scope '{scope}' is not global");
    }

    [Fact]
    public void Distinct_Keys_Should_Never_Share_A_Storage_Key()
    {
        IdempotencyRecordKey[] keys =
        [
            new("a|1:b", "c", "d"),
            new("a", "b|1:c", "d"),
            new("a", "b", "c|1:d"),
            new("a", "b", "c"),
            new("a", "-", "b"),
            new("a", (string?)null, "b"),
            new("a", (string?)null, "-|1:b"),
            new("a|-", (string?)null, "b"),
            new("1:a", "b", "c"),
            new("a", "1:b", "c"),
            new("ab", "c", "d"),
            new("a", "bc", "d"),
        ];

        keys.Select(key => key.ToStorageKey()).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Keys_With_The_Same_Parts_Should_Be_Equal()
    {
        IdempotencyRecordKey fromString = new("http", "alice", "k");
        IdempotencyRecordKey fromScope = new("http", IdempotencyScope.Of("alice"), "k");

        fromString.Should().Be(fromScope);
        fromString.GetHashCode().Should().Be(fromScope.GetHashCode());
        new IdempotencyRecordKey("http", (string?)null, "k").Should().NotBe(new IdempotencyRecordKey("http", "-", "k"));
    }

    [Fact]
    public void Constructor_Should_Reject_Empty_Parts()
    {
        Action emptyNamespace = () => _ = new IdempotencyRecordKey("", "s", "k");
        Action nullNamespace = () => _ = new IdempotencyRecordKey(null!, "s", "k");
        Action emptyScope = () => _ = new IdempotencyRecordKey("ns", "", "k");
        Action emptyKey = () => _ = new IdempotencyRecordKey("ns", "s", "");
        Action nullKey = () => _ = new IdempotencyRecordKey("ns", "s", null!);

        emptyNamespace.Should().Throw<ArgumentException>();
        nullNamespace.Should().Throw<ArgumentException>();
        emptyScope.Should().Throw<ArgumentException>();
        emptyKey.Should().Throw<ArgumentException>();
        nullKey.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToStorageKey_Should_Reject_A_Lone_Surrogate()
    {
        IdempotencyRecordKey key = new("http", "alice", "k\uD800");

        Action act = () => key.ToStorageKey();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToStorageKey_Should_Reject_A_Null_Prefix()
    {
        Action act = () => new IdempotencyRecordKey("http", "alice", "k").ToStorageKey(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
