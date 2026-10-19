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

        IdempotencyScopes.User.Resolve(principal).Should().Be(IdempotencyScope.Of("u1"));
    }

    [Fact]
    public void User_Should_Fall_Back_To_Sub()
    {
        ClaimsPrincipal principal = Authenticated(new Claim("sub", "u2"));

        IdempotencyScopes.User.Resolve(principal).Should().Be(IdempotencyScope.Of("u2"));
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

        resolver.Resolve(Authenticated(new Claim(TenantClaim, "t1"))).Should().Be(IdempotencyScope.Of("t1"));
        resolver.Resolve(Authenticated(new Claim(ClaimTypes.NameIdentifier, "u1"))).Should().BeNull();
    }

    [Fact]
    public void UserAndTenant_Should_Combine_Both_With_Length_Prefixes()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.UserAndTenant(TenantClaim);

        IdempotencyScope? scope = resolver.Resolve(Authenticated(new Claim("sub", "u1"), new Claim(TenantClaim, "t1")));

        scope.Should().Be(IdempotencyScope.Of("2:u1|2:t1"));
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

        IdempotencyScopes.Client.Resolve(principal).Should().Be(IdempotencyScope.Of("c1"));
    }

    [Fact]
    public void Client_Should_Fall_Back_To_Azp()
    {
        ClaimsPrincipal principal = Authenticated(new Claim(IdempotencyScopes.AuthorizedPartyClaimType, "c2"));

        IdempotencyScopes.Client.Resolve(principal).Should().Be(IdempotencyScope.Of("c2"));
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
        IIdempotencyScopeResolver resolver = IdempotencyScopes.Custom(principal => principal?.FindFirst("org")?.Value);

        resolver.Resolve(new ClaimsPrincipal(new ClaimsIdentity([new Claim("org", "o1")]))).Should().Be(IdempotencyScope.Of("o1"));
        resolver.Resolve(Authenticated()).Should().BeNull();
        resolver.Resolve(null).Should().BeNull();
    }

    [Fact]
    public void Custom_Should_Treat_An_Empty_Result_As_Unresolved()
    {
        IIdempotencyScopeResolver resolver = IdempotencyScopes.Custom(_ => "");

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
        IdempotencyScope.Of("u1").IsGlobal.Should().BeFalse();
        IdempotencyScope.Of("u1").Value.Should().Be("u1");
    }

    [Fact]
    public void Scope_ToString_Should_Show_The_Value_Or_Global()
    {
        IdempotencyScope.Of("u1").ToString().Should().Be("u1");
        IdempotencyScope.Global.ToString().Should().Be("(global)");
    }

    [Fact]
    public void Factories_Should_Reject_Invalid_Arguments()
    {
        Action emptyScope = () => IdempotencyScope.Of("");
        Action nullScope = () => IdempotencyScope.Of(null!);
        Action emptyTenant = () => IdempotencyScopes.Tenant("");
        Action emptyUserAndTenant = () => IdempotencyScopes.UserAndTenant("");
        Action nullCustom = () => IdempotencyScopes.Custom(null!);

        emptyScope.Should().Throw<ArgumentException>();
        nullScope.Should().Throw<ArgumentException>();
        emptyTenant.Should().Throw<ArgumentException>();
        emptyUserAndTenant.Should().Throw<ArgumentException>();
        nullCustom.Should().Throw<ArgumentNullException>();
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) => new(new ClaimsIdentity(claims, "Test"));
}
