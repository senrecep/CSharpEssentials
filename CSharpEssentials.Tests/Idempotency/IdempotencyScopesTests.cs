using System.Security.Claims;
using CSharpEssentials.Idempotency;
using FluentAssertions;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyScopesTests
{
    private const string TenantClaim = "tenant_id";

    [Fact]
    public void User_Should_Read_NameIdentifier()
    {
        ClaimsPrincipal principal = Authenticated(new Claim(ClaimTypes.NameIdentifier, "u1"), new Claim("sub", "u2"));

        IdempotencyScopes.User.Resolve(principal).Should().Be(IdempotencyScope.User("u1"));
    }

    [Fact]
    public void User_Should_Fall_Back_To_Sub()
    {
        ClaimsPrincipal principal = Authenticated(new Claim("sub", "u2"));

        IdempotencyScopes.User.Resolve(principal).Should().Be(IdempotencyScope.User("u2"));
    }

    [Fact]
    public void User_Should_Be_Unresolved_For_A_Missing_Or_Empty_Claim()
    {
        IdempotencyScopes.User.Resolve(Authenticated()).Should().BeNull();
        IdempotencyScopes.User.Resolve(Authenticated(new Claim(ClaimTypes.NameIdentifier, ""))).Should().BeNull();
    }

    [Fact]
    public void Claim_Resolvers_Should_Be_Unresolved_For_Anonymous_And_Null_Principals()
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, "u1"),
            new(TenantClaim, "t1"),
            new(IdempotencyScopes.ClientIdClaimType, "c1"),
        ];
        ClaimsPrincipal anonymous = new(new ClaimsIdentity(claims));
        IIdempotencyScopeResolver[] resolvers =
        [
            IdempotencyScopes.User,
            IdempotencyScopes.Client,
            IdempotencyScopes.Tenant(TenantClaim),
            IdempotencyScopes.UserAndTenant(TenantClaim),
        ];

        foreach (IIdempotencyScopeResolver resolver in resolvers)
        {
            resolver.Resolve(anonymous).Should().BeNull();
            resolver.Resolve(null).Should().BeNull();
        }
    }

    [Fact]
    public void Tenant_Should_Read_The_Given_Claim()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.Tenant(TenantClaim);

        resolver.Resolve(Authenticated(new Claim(TenantClaim, "t1"))).Should().Be(IdempotencyScope.Tenant("t1"));
        resolver.Resolve(Authenticated(new Claim(ClaimTypes.NameIdentifier, "u1"))).Should().BeNull();
    }

    [Fact]
    public void UserAndTenant_Should_Length_Prefix_The_Tenant_Part()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.UserAndTenant(TenantClaim);

        IdempotencyScope? scope = resolver.Resolve(Authenticated(new Claim("sub", "u1"), new Claim(TenantClaim, "t1")));

        scope.Should().Be(IdempotencyScope.UserAndTenant("u1", "t1"));
        scope.Value.Value.Should().Be("9:tenant:t1/user:u1");
    }

    [Fact]
    public void UserAndTenant_Should_Be_Unresolved_When_Either_Is_Missing()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.UserAndTenant(TenantClaim);

        resolver.Resolve(Authenticated(new Claim("sub", "u1"))).Should().BeNull();
        resolver.Resolve(Authenticated(new Claim(TenantClaim, "t1"))).Should().BeNull();
    }

    [Fact]
    public void UserAndTenant_Should_Not_Collide_When_Values_Contain_Separators()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.UserAndTenant(TenantClaim);

        IdempotencyScope? first = resolver.Resolve(Authenticated(new Claim("sub", "a|b"), new Claim(TenantClaim, "c")));
        IdempotencyScope? second = resolver.Resolve(Authenticated(new Claim("sub", "a"), new Claim(TenantClaim, "b|c")));

        first.Should().NotBe(second);
    }

    [Fact]
    public void Client_Should_Read_ClientId()
    {
        ClaimsPrincipal principal = Authenticated(
            new Claim(IdempotencyScopes.ClientIdClaimType, "c1"),
            new Claim(IdempotencyScopes.AuthorizedPartyClaimType, "c2"));

        IdempotencyScopes.Client.Resolve(principal).Should().Be(IdempotencyScope.Client("c1"));
    }

    [Fact]
    public void Client_Should_Fall_Back_To_Azp()
    {
        ClaimsPrincipal principal = Authenticated(new Claim(IdempotencyScopes.AuthorizedPartyClaimType, "c2"));

        IdempotencyScopes.Client.Resolve(principal).Should().Be(IdempotencyScope.Client("c2"));
    }

    [Fact]
    public void Client_Should_Ignore_The_User()
    {
        ClaimsPrincipal principal = Authenticated(new Claim(ClaimTypes.NameIdentifier, "u1"));

        IdempotencyScopes.Client.Resolve(principal).Should().BeNull();
    }

    [Fact]
    public void Custom_Should_Use_The_Delegate()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.Custom("org", principal => principal?.FindFirst("org")?.Value);

        resolver.Resolve(new ClaimsPrincipal(new ClaimsIdentity([new Claim("org", "o1")]))).Should().Be(IdempotencyScope.Custom("org", "o1"));
        resolver.Resolve(Authenticated()).Should().BeNull();
        resolver.Resolve(null).Should().BeNull();
    }

    [Fact]
    public void Custom_Should_Treat_An_Empty_Result_As_Unresolved()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.Custom("org", _ => "");

        resolver.Resolve(Authenticated()).Should().BeNull();
    }

    [Fact]
    public void Global_Should_Resolve_For_Every_Caller()
    {
        IdempotencyScopes.Global.Resolve(null).Should().Be(IdempotencyScope.Global);
        IdempotencyScopes.Global.Resolve(new ClaimsPrincipal(new ClaimsIdentity())).Should().Be(IdempotencyScope.Global);
        IdempotencyScopes.Global.Resolve(Authenticated(new Claim("sub", "u1"))).Should().Be(IdempotencyScope.Global);
    }

    [Fact]
    public void Global_Should_Differ_From_Unresolved()
    {
        IdempotencyScope? global = IdempotencyScopes.Global.Resolve(null);
        IdempotencyScope? unresolved = IdempotencyScopes.User.Resolve(null);

        global.Should().NotBeNull();
        global.Value.IsGlobal.Should().BeTrue();
        global.Value.Value.Should().BeNull();
        unresolved.Should().BeNull();
    }

    [Fact]
    public void Scope_Should_Default_To_Global()
    {
        IdempotencyScope scope = default;

        scope.Should().Be(IdempotencyScope.Global);
        scope.IsGlobal.Should().BeTrue();
        IdempotencyScope.User("u1").IsGlobal.Should().BeFalse();
    }

    [Fact]
    public void Scope_ToString_Should_Show_The_Value_Or_Global()
    {
        IdempotencyScope.User("u1").ToString().Should().Be("user:u1");
        IdempotencyScope.Global.ToString().Should().Be("(global)");
    }

    [Fact]
    public void Factories_Should_Reject_Invalid_Arguments()
    {
        Action emptyScope = () => IdempotencyScope.Of("");
        Action nullScope = () => IdempotencyScope.Of(null!);
        Action emptyTenant = () => IdempotencyScopes.Tenant("");
        Action emptyUserAndTenant = () => IdempotencyScopes.UserAndTenant("");
        Action emptyCustomKind = () => IdempotencyScopes.Custom("", _ => "v");
        Action nullCustomKind = () => IdempotencyScopes.Custom(null!, _ => "v");
        Action nullCustom = () => IdempotencyScopes.Custom("org", null!);

        emptyScope.Should().Throw<ArgumentException>();
        nullScope.Should().Throw<ArgumentException>();
        emptyTenant.Should().Throw<ArgumentException>();
        emptyUserAndTenant.Should().Throw<ArgumentException>();
        emptyCustomKind.Should().Throw<ArgumentException>();
        nullCustomKind.Should().Throw<ArgumentException>();
        nullCustom.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Scope_Factories_Should_Prefix_The_Kind()
    {
        IdempotencyScope.User("42").Value.Should().Be("user:42");
        IdempotencyScope.Tenant("42").Value.Should().Be("tenant:42");
        IdempotencyScope.Client("42").Value.Should().Be("client:42");
        IdempotencyScope.UserAndTenant("u1", "t1").Value.Should().Be("9:tenant:t1/user:u1");
        IdempotencyScope.Custom("org", "42").Value.Should().Be("org:42");
    }

    [Fact]
    public void Tenant_With_A_User_Like_Id_Should_Not_Collide_With_User()
    {
        IdempotencyScope tenant = IdempotencyScope.Tenant("user:42");
        IdempotencyScope user = IdempotencyScope.User("42");

        tenant.Value.Should().Be("tenant:user:42");
        tenant.Should().NotBe(user);
        new IdempotencyRecordKey("http", tenant, "k").ToStorageKey()
            .Should().NotBe(new IdempotencyRecordKey("http", user, "k").ToStorageKey());
    }

    [Fact]
    public void Custom_Should_Not_Mimic_A_Built_In_Length_Prefix()
    {
        Action allDigitKind = () => IdempotencyScope.Custom("9", "tenant:t1/user:u1");
        Action allDigitResolverKind = () => IdempotencyScopes.Custom("9", _ => "tenant:t1/user:u1");

        allDigitKind.Should().Throw<ArgumentException>().WithParameterName("kind");
        allDigitResolverKind.Should().Throw<ArgumentException>().WithParameterName("kind");
    }

    [Theory]
    [InlineData("")]
    [InlineData("a:b")]
    [InlineData(":")]
    [InlineData("123")]
    [InlineData("a/b")]
    [InlineData("/")]
    public void Custom_Should_Reject_An_Invalid_Kind(string kind)
    {
        Action scope = () => IdempotencyScope.Custom(kind, "v");
        Action resolver = () => IdempotencyScopes.Custom(kind, _ => "v");

        scope.Should().Throw<ArgumentException>().WithParameterName(nameof(kind));
        resolver.Should().Throw<ArgumentException>().WithParameterName(nameof(kind));
    }

    [Fact]
    public void Scope_Factories_Should_Reject_Empty_Ids()
    {
        Action[] factories =
        [
            () => IdempotencyScope.User(""),
            () => IdempotencyScope.User(null!),
            () => IdempotencyScope.Tenant(""),
            () => IdempotencyScope.Client(""),
            () => IdempotencyScope.UserAndTenant("", "t1"),
            () => IdempotencyScope.UserAndTenant("u1", ""),
            () => IdempotencyScope.Custom("org", ""),
            () => IdempotencyScope.Custom(null!, "v"),
        ];

        foreach (Action factory in factories)
            factory.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Custom_Should_Reject_The_Reserved_User_Kind() => AssertReservedKind("user");

    [Fact]
    public void Custom_Should_Reject_The_Reserved_Tenant_Kind() => AssertReservedKind("tenant");

    [Fact]
    public void Custom_Should_Reject_The_Reserved_Client_Kind() => AssertReservedKind("client");

    [Fact]
    public void Custom_Should_Reject_The_Reserved_Global_Kind() => AssertReservedKind("global");

    [Fact]
    public void Custom_Should_Reject_Reserved_Kinds_In_Any_Casing()
    {
        string[] kinds = ["User", "USER", "Tenant", "cLiEnT", "GLOBAL", "Global"];

        foreach (string kind in kinds)
            AssertReservedKind(kind);
    }

    [Fact]
    public void Custom_Should_Accept_A_Kind_That_Only_Contains_A_Reserved_Kind()
    {
        IdempotencyScope.Custom("users", "1").Value.Should().Be("users:1");
        IdempotencyScope.Custom("global-org", "1").Value.Should().Be("global-org:1");
    }

    [Fact]
    public void User_Should_Reject_A_Lone_Surrogate()
    {
        Action act = () => IdempotencyScope.User("u\uD800");

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void Tenant_Should_Reject_A_Lone_Surrogate()
    {
        Action act = () => IdempotencyScope.Tenant("\uDC00t");

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void Client_Should_Reject_A_Lone_Surrogate()
    {
        Action act = () => IdempotencyScope.Client("c\uD800c");

        act.Should().Throw<ArgumentException>().WithParameterName("id");
    }

    [Fact]
    public void UserAndTenant_Should_Reject_A_Lone_Surrogate_In_Either_Id()
    {
        Action user = () => IdempotencyScope.UserAndTenant("u\uD800", "t1");
        Action tenant = () => IdempotencyScope.UserAndTenant("u1", "t\uDC00");

        user.Should().Throw<ArgumentException>().WithParameterName("userId");
        tenant.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Fact]
    public void Custom_Should_Reject_A_Lone_Surrogate_In_The_Kind()
    {
        Action scope = () => IdempotencyScope.Custom("org\uD800", "v");
        Action resolver = () => IdempotencyScopes.Custom("org\uD800", _ => "v");

        scope.Should().Throw<ArgumentException>().WithParameterName("kind");
        resolver.Should().Throw<ArgumentException>().WithParameterName("kind");
    }

    [Fact]
    public void Custom_Should_Reject_A_Lone_Surrogate_In_The_Value()
    {
        Action act = () => IdempotencyScope.Custom("org", "v\uDC00");

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void Scope_Factories_Should_Accept_A_Surrogate_Pair()
    {
        IdempotencyScope.User("\uD83D\uDE00").Value.Should().Be("user:\uD83D\uDE00");
        IdempotencyScope.UserAndTenant("\uD83D\uDE00", "\uD83D\uDE00").Value.Should().Be("11:tenant:\uD83D\uDE00/user:\uD83D\uDE00");
        IdempotencyScope.Custom("\uD83D\uDE00", "v").Value.Should().Be("\uD83D\uDE00:v");
    }

    private static void AssertReservedKind(string kind)
    {
        Action scope = () => IdempotencyScope.Custom(kind, "v");
        Action resolver = () => IdempotencyScopes.Custom(kind, _ => "v");

        scope.Should().Throw<ArgumentException>().WithParameterName(nameof(kind)).WithMessage("*reserved*");
        resolver.Should().Throw<ArgumentException>().WithParameterName(nameof(kind)).WithMessage("*reserved*");
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) => new(new ClaimsIdentity(claims, "Test"));
}
