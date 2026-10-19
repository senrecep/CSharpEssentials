using CSharpEssentials.Idempotency;
using FluentAssertions;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyRecordKeyTests
{
    [Fact]
    public void ToStorageKey_Should_Length_Prefix_Every_Part()
    {
        IdempotencyRecordKey key = Raw("http", "alice", "order-42");

        key.ToStorageKey().Should().Be("idem:v3:4:http|5:alice|8:order-42");
    }

    [Fact]
    public void ToStorageKey_Should_Write_Global_As_The_Scope_Of_A_Global_Key()
    {
        IdempotencyRecordKey key = new("http", IdempotencyScope.Global, "order-42");

        key.IsGlobal.Should().BeTrue();
        key.Scope.Should().BeNull();
        key.ToStorageKey().Should().Be("idem:v3:4:http|6:global|8:order-42");
    }

    [Fact]
    public void ToStorageKey_Should_Count_Utf8_Bytes()
    {
        IdempotencyRecordKey key = Raw("http", "é", "ключ");

        key.ToStorageKey().Should().Be("idem:v3:4:http|2:é|8:ключ");
    }

    [Fact]
    public void ToStorageKey_Should_Use_The_Given_Prefix()
    {
        IdempotencyRecordKey key = Raw("http", "alice", "k");

        key.ToStorageKey("app:").Should().Be("app:4:http|5:alice|1:k");
        key.ToStorageKey("").Should().Be("4:http|5:alice|1:k");
    }

    [Fact]
    public void Global_Key_Should_Never_Collide_With_A_Scoped_Key()
    {
        string global = new IdempotencyRecordKey("http", null, "k").ToStorageKey();
        string[] scopes = ["-", "--", " ", "|", "0:", "1:-", "-|1:k", "global:", "(global)", "null"];

        foreach (string scope in scopes)
            Raw("http", scope, "k").ToStorageKey().Should().NotBe(global, $"scope '{scope}' is not global");
    }

    [Fact]
    public void Constructor_Should_Reject_The_Reserved_Global_Scope()
    {
        Action act = () => Raw("http", "global", "k");

        act.Should().Throw<ArgumentException>().WithParameterName("scope");
    }

    [Fact]
    public void Scope_Of_Should_Reject_The_Reserved_Global_Scope()
    {
        Action act = () => IdempotencyScope.Of("global");

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Scope_Factories_Should_Never_Produce_The_Global_Scope_Part()
    {
        IdempotencyScope[] scopes =
        [
            IdempotencyScope.User("global"),
            IdempotencyScope.Tenant("global"),
            IdempotencyScope.Client("global"),
            IdempotencyScope.UserAndTenant("global", "global"),
            IdempotencyScope.Custom("org", "global"),
        ];
        string global = new IdempotencyRecordKey("http", IdempotencyScope.Global, "k").ToStorageKey();

        foreach (IdempotencyScope scope in scopes)
        {
            scope.Value.Should().Contain(":").And.NotBe("global");
            new IdempotencyRecordKey("http", scope, "k").ToStorageKey().Should().NotBe(global);
        }
    }

    [Fact]
    public void Constructor_Should_Treat_Null_And_Global_Scope_Alike()
    {
        IdempotencyRecordKey fromNull = new("http", null, "k");
        IdempotencyRecordKey fromGlobal = new("http", IdempotencyScope.Global, "k");

        fromNull.Should().Be(fromGlobal);
        fromNull.IsGlobal.Should().BeTrue();
    }

    [Fact]
    public void Distinct_Keys_Should_Never_Share_A_Storage_Key()
    {
        IdempotencyRecordKey[] keys =
        [
            Raw("a|1:b", "c", "d"),
            Raw("a", "b|1:c", "d"),
            Raw("a", "b", "c|1:d"),
            Raw("a", "b", "c"),
            Raw("a", "-", "b"),
            Raw("a", null, "b"),
            Raw("a", null, "-|1:b"),
            Raw("a|-", null, "b"),
            Raw("1:a", "b", "c"),
            Raw("a", "1:b", "c"),
            Raw("ab", "c", "d"),
            Raw("a", "bc", "d"),
        ];

        keys.Select(key => key.ToStorageKey()).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Keys_With_The_Same_Parts_Should_Be_Equal()
    {
        IdempotencyRecordKey fromString = Raw("http", "user:alice", "k");
        IdempotencyRecordKey fromScope = new("http", IdempotencyScope.User("alice"), "k");

        fromString.Should().Be(fromScope);
        fromString.GetHashCode().Should().Be(fromScope.GetHashCode());
        new IdempotencyRecordKey("http", null, "k").Should().NotBe(Raw("http", "-", "k"));
    }

    [Fact]
    public void Constructor_Should_Reject_Empty_Parts()
    {
        Action emptyNamespace = () => Raw("", "s", "k");
        Action nullNamespace = () => Raw(null!, "s", "k");
        Action emptyScope = () => Raw("ns", "", "k");
        Action emptyKey = () => Raw("ns", "s", "");
        Action nullKey = () => Raw("ns", "s", null!);

        emptyNamespace.Should().Throw<ArgumentException>();
        nullNamespace.Should().Throw<ArgumentException>();
        emptyScope.Should().Throw<ArgumentException>();
        emptyKey.Should().Throw<ArgumentException>();
        nullKey.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_Should_Reject_A_Lone_Surrogate_In_The_Key()
    {
        string[] keys = ["k\uD800", "k\uDC00", "\uDC00\uD800", "k\uD800x"];

        foreach (string key in keys)
        {
            Action act = () => _ = new IdempotencyRecordKey("http", IdempotencyScope.User("alice"), key);

            act.Should().Throw<ArgumentException>().WithParameterName(nameof(key));
        }
    }

    [Fact]
    public void Constructor_Should_Reject_A_Lone_Surrogate_In_The_Namespace()
    {
        Action act = () => _ = new IdempotencyRecordKey("http\uD800", IdempotencyScope.User("alice"), "k");

        act.Should().Throw<ArgumentException>().WithMessage("*unpaired surrogate*");
    }

    [Fact]
    public void Constructor_Should_Reject_A_Lone_Surrogate_In_A_Storage_Scope()
    {
        Action act = () => Raw("http", "user:\uD800", "k");

        act.Should().Throw<ArgumentException>().WithParameterName("scope");
    }

    [Fact]
    public void Constructor_Should_Accept_A_Surrogate_Pair()
    {
        IdempotencyRecordKey key = new("http\uD83D\uDE00", IdempotencyScope.User("\uD83D\uDE00"), "k\uD83D\uDE00");

        key.ToStorageKey().Should().Be("idem:v3:8:http\uD83D\uDE00|9:user:\uD83D\uDE00|5:k\uD83D\uDE00");
    }

    [Fact]
    public void ToStorageKey_Should_Reject_A_Null_Prefix()
    {
        Action act = () => Raw("http", "alice", "k").ToStorageKey(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static IdempotencyRecordKey Raw(string @namespace, string? scope, string key) =>
        IdempotencyRecordKey.FromStorageScope(@namespace, scope, key);
}
