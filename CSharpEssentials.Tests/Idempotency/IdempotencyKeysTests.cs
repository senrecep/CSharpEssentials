using System.Security.Claims;
using System.Text.Json;
using CSharpEssentials.Idempotency;
using FluentAssertions;

namespace CSharpEssentials.Tests.Idempotency;

/// <summary>
/// Runs <c>tests/vectors/idempotency-key-vectors.json</c>, the deterministic key, scope and storage key vectors, plus the
/// cases JSON cannot carry (lone surrogates).
/// </summary>
public sealed class IdempotencyKeysTests
{
    private static readonly Lazy<JsonDocument> Vectors = new(() =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Idempotency", "idempotency-key-vectors.json"))));

    private static JsonElement Root => Vectors.Value.RootElement;

    [Fact]
    public void Constants_Should_Match_The_Vectors()
    {
        IdempotencyKeys.UuidNamespace.Should().Be(Guid.Parse(Root.GetProperty("namespaceUuid").GetString()!));
        IdempotencyKeys.MaxKeyLength.Should().Be(Root.GetProperty("maxKeyLength").GetInt32());
        IdempotencyKeys.MessageOperation.Should().Be(Root.GetProperty("messageOperation").GetString());
    }

    [Fact]
    public void CreateUuidV5_Should_Match_The_Rfc9562_Known_Answer()
    {
        JsonElement vector = Root.GetProperty("rfc9562");

        Guid uuid = IdempotencyKeys.CreateUuidV5(
            Guid.Parse(vector.GetProperty("namespace").GetString()!),
            vector.GetProperty("name").GetString()!);

        uuid.ToString().Should().Be(vector.GetProperty("uuidV5").GetString());
    }

    [Fact]
    public void For_Should_Match_Every_Vector()
    {
        JsonElement.ArrayEnumerator vectors = Root.GetProperty("for").EnumerateArray();
        vectors.Should().NotBeEmpty();

        foreach (JsonElement vector in vectors)
        {
            string name = vector.GetProperty("name").GetString()!;
            string operation = vector.GetProperty("operation").GetString()!;
            string[] ids = Ids(vector);

            IdempotencyKeys.For(operation, ids).Should().Be(vector.GetProperty("key").GetString(), name);
            IdempotencyKeys.ToUuidV5(operation, ids).ToString().Should().Be(vector.GetProperty("uuidV5").GetString(), name);
            IdempotencyKeys.CreateUuidV5(IdempotencyKeys.UuidNamespace, vector.GetProperty("canonical").GetString()!)
                .Should().Be(IdempotencyKeys.ToUuidV5(operation, ids), $"{name}: UUIDv5 hashes the canonical string");
        }
    }

    [Fact]
    public void For_Should_Return_The_Canonical_String_Only_When_It_Is_A_Valid_Key()
    {
        foreach (JsonElement vector in Root.GetProperty("for").EnumerateArray())
        {
            string name = vector.GetProperty("name").GetString()!;
            string canonical = vector.GetProperty("canonical").GetString()!;
            string key = vector.GetProperty("key").GetString()!;

            if (IdempotencyKeys.IsValid(canonical))
                key.Should().Be(canonical, name);
            else
                key.Should().StartWith("h:", name).And.HaveLength(45, name);
            IdempotencyKeys.IsValid(key).Should().BeTrue(name);
        }
    }

    [Fact]
    public void Scopes_Should_Match_Every_Vector()
    {
        JsonElement.ArrayEnumerator vectors = Root.GetProperty("scopes").EnumerateArray();
        vectors.Should().NotBeEmpty();

        foreach (JsonElement vector in vectors)
        {
            string name = vector.GetProperty("name").GetString()!;

            ResolveScope(vector).Should().Be(ExpectedScope(vector), name);
        }
    }

    [Fact]
    public void StorageKeys_Should_Match_Every_Vector()
    {
        JsonElement.ArrayEnumerator vectors = Root.GetProperty("storageKeys").EnumerateArray();
        vectors.Should().NotBeEmpty();
        string defaultPrefix = Root.GetProperty("storagePrefix").GetString()!;
        IdempotencyRecordKey.DefaultPrefix.Should().Be(defaultPrefix);

        foreach (JsonElement vector in vectors)
        {
            string name = vector.GetProperty("name").GetString()!;
            string scope = vector.GetProperty("scope").GetString()!;
            string prefix = vector.GetProperty("prefix").GetString()!;
            string expected = vector.GetProperty("storageKey").GetString()!;
            IdempotencyRecordKey key = scope == "global"
                ? new(vector.GetProperty("namespace").GetString()!, IdempotencyScope.Global, vector.GetProperty("key").GetString()!)
                : IdempotencyRecordKey.FromStorageScope(vector.GetProperty("namespace").GetString()!, scope, vector.GetProperty("key").GetString()!);

            key.ToStorageKey(prefix).Should().Be(expected, name);
            if (prefix == defaultPrefix)
                key.ToStorageKey().Should().Be(expected, name);
        }
    }

    [Fact]
    public void FromMessage_Should_Match_Every_Vector()
    {
        JsonElement.ArrayEnumerator vectors = Root.GetProperty("fromMessage").EnumerateArray();
        vectors.Should().NotBeEmpty();

        foreach (JsonElement vector in vectors)
        {
            string name = vector.GetProperty("name").GetString()!;
            string messageId = vector.GetProperty("messageId").GetString()!;
            string consumer = vector.GetProperty("consumer").GetString()!;
            string key = vector.GetProperty("key").GetString()!;

            IdempotencyKeys.FromMessage(messageId, consumer).Should().Be(key, name);
            IdempotencyKeys.For(IdempotencyKeys.MessageOperation, consumer, messageId).Should().Be(key, name);
        }
    }

    [Fact]
    public void Invalid_Vectors_Should_Throw_An_Argument_Error()
    {
        JsonElement.ArrayEnumerator vectors = Root.GetProperty("invalid").EnumerateArray();
        vectors.Should().NotBeEmpty();

        foreach (JsonElement vector in vectors)
        {
            string name = vector.GetProperty("name").GetString()!;
            string? operation = vector.GetProperty("operation").GetString();
            string[] ids = Ids(vector);

            Action key = () => IdempotencyKeys.For(operation!, ids);
            Action uuid = () => IdempotencyKeys.ToUuidV5(operation!, ids);

            key.Should().Throw<ArgumentException>(name);
            uuid.Should().Throw<ArgumentException>(name);
        }
    }

    /// <summary>
    /// The parts are built in code: attribute arguments are stored as UTF-8, which turns a lone surrogate into U+FFFD.
    /// </summary>
    [Fact]
    public void Lone_Surrogate_Should_Throw()
    {
        string[] parts = ["\uD800", "\uDC00", "a\uD800b", "\uDC00\uD800", "\uD83D"];

        foreach (string part in parts)
        {
            Action inId = () => IdempotencyKeys.For("op", part);
            Action inOperation = () => IdempotencyKeys.For(part, "id");
            Action inMessage = () => IdempotencyKeys.FromMessage(part, "consumer");
            Action inUuid = () => IdempotencyKeys.ToUuidV5("op", part);

            inId.Should().Throw<ArgumentException>();
            inOperation.Should().Throw<ArgumentException>();
            inMessage.Should().Throw<ArgumentException>();
            inUuid.Should().Throw<ArgumentException>();
        }
    }

    [Fact]
    public void Surrogate_Pair_Should_Hash()
    {
        string key = IdempotencyKeys.For("op", "😀");

        key.Should().StartWith("h:");
        IdempotencyKeys.IsValid(key).Should().BeTrue();
    }

    [Fact]
    public void Null_Ids_Array_Should_Throw()
    {
        Action act = () => IdempotencyKeys.For("op", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void For_Should_Be_Deterministic_And_Order_Sensitive()
    {
        IdempotencyKeys.For("op", "a", "b").Should().Be(IdempotencyKeys.For("op", "a", "b"));
        IdempotencyKeys.For("op", "a", "b").Should().NotBe(IdempotencyKeys.For("op", "b", "a"));
        IdempotencyKeys.For("op", "ab", "c").Should().NotBe(IdempotencyKeys.For("op", "a", "bc"));
        IdempotencyKeys.FromMessage("m1", "billing").Should().NotBe(IdempotencyKeys.FromMessage("m1", "shipping"));
    }

    [Fact]
    public void ToUuidV5_Should_Set_Version_5_And_The_Rfc_Variant()
    {
        string uuid = IdempotencyKeys.ToUuidV5("op", "a").ToString();

        uuid[14].Should().Be('5');
        uuid[19].Should().BeOneOf('8', '9', 'a', 'b');
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("a", true)]
    [InlineData("!~", true)]
    [InlineData("order-42:v1|x", true)]
    [InlineData("has space", false)]
    [InlineData("quote\"", false)]
    [InlineData("back\\slash", false)]
    [InlineData("tab\t", false)]
    [InlineData("café", false)]
    [InlineData("\u007F", false)]
    public void IsValid_Should_Allow_Printable_Ascii_Except_Quote_And_Backslash(string? key, bool expected) =>
        IdempotencyKeys.IsValid(key).Should().Be(expected);

    [Fact]
    public void IsValid_Should_Check_The_Length()
    {
        IdempotencyKeys.IsValid(new string('a', IdempotencyKeys.MaxKeyLength)).Should().BeTrue();
        IdempotencyKeys.IsValid(new string('a', IdempotencyKeys.MaxKeyLength + 1)).Should().BeFalse();
        IdempotencyKeys.IsValid("abcd", maxLength: 3).Should().BeFalse();
        IdempotencyKeys.IsValid("abc", maxLength: 3).Should().BeTrue();
    }

    private static string[] Ids(JsonElement vector) =>
        [.. vector.GetProperty("ids").EnumerateArray().Select(id => id.GetString()!)];

    private static IdempotencyScope? ExpectedScope(JsonElement vector)
    {
        string? scope = vector.GetProperty("scope").GetString();
        if (scope is null)
            return null;
        return scope == "global" ? IdempotencyScope.Global : IdempotencyScope.Of(scope);
    }

    private static IdempotencyScope? ResolveScope(JsonElement vector)
    {
        const string tenantClaim = "tenant_id";
        string kind = vector.GetProperty("kind").GetString()!;

        return kind switch
        {
            "user" => IdempotencyScopes.User.Resolve(Principal((ClaimTypes.NameIdentifier, Optional(vector, "id")))),
            "tenant" => IdempotencyScopes.Tenant(tenantClaim).Resolve(Principal((tenantClaim, Optional(vector, "id")))),
            "client" => IdempotencyScopes.Client.Resolve(Principal((IdempotencyScopes.ClientIdClaimType, Optional(vector, "id")))),
            "custom" => IdempotencyScopes
                .Custom(vector.GetProperty("customKind").GetString()!, _ => Optional(vector, "value"))
                .Resolve(Principal()),
            "global" => IdempotencyScopes.Global.Resolve(Principal()),
            "userAndTenant" => IdempotencyScopes.UserAndTenant(tenantClaim).Resolve(Principal(
                (ClaimTypes.NameIdentifier, Optional(vector, "userId")),
                (tenantClaim, Optional(vector, "tenantId")))),
            _ => throw new InvalidOperationException($"Unknown scope kind '{kind}'."),
        };
    }

    private static string? Optional(JsonElement vector, string property) => vector.GetProperty(property).GetString();

    private static ClaimsPrincipal Principal(params (string Type, string? Value)[] claims) =>
        new(new ClaimsIdentity(
            claims.Where(claim => claim.Value is not null).Select(claim => new Claim(claim.Type, claim.Value!)),
            "Test"));
}
