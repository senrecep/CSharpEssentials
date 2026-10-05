using CSharpEssentials.AspNetCore;
using FluentAssertions;
using static CSharpEssentials.Tests.AspNetCore.EnumBinding.EnumBindingAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// <c>AddEnhancedProblemDetails()</c> + <c>UseEnumBinding()</c> behavior shared by Minimal API,
/// MVC [ApiController] and plain MVC controllers.
/// </summary>
public class EnumBindingMiddlewareTests
{
    private static Task<EnumBindingHost> Start(EbHostKind kind, Action<EnumBindingOptions>? configure = null) =>
        EnumBindingHost.StartAsync(kind, useEnumBinding: true, configure);

    public static TheoryData<EbHostKind> AllHostKinds => Hosts(AllHosts);

    public static TheoryData<EbHostKind> MvcHostKinds => Hosts(MvcHosts);

    public static TheoryData<EbHostKind, string, string> AcceptedScalarSpellings => Cross(AllHosts,
        ("in_progress", "InProgress"),
        ("InProgress", "InProgress"),
        ("inprogress", "InProgress"),
        ("INPROGRESS", "InProgress"),
        ("IN_PROGRESS", "InProgress"),
        ("1", "InProgress"),
        ("http_error", "HTTPError"),
        ("HTTPError", "HTTPError"),
        ("%20in_progress%20", "InProgress"));

    public static TheoryData<EbHostKind, string> RejectedScalarValues => Cross(AllHosts,
        "99", "garbage", "Active,InProgress", "in-progress");

    public static TheoryData<EbHostKind, string, string> AcceptedRouteSpellings => Cross(AllHosts,
        ("in_progress", "InProgress"),
        ("inprogress", "InProgress"),
        ("http_error", "HTTPError"),
        ("2", "HTTPError"));

    public static TheoryData<EbHostKind, string, string> AcceptedFlagSpellings => Cross(AllHosts,
        ("read,write", "Read, Write"),
        ("Read,%20Write", "Read, Write"),
        ("read", "Read"),
        ("WRITE,delete", "Write, Delete"),
        ("3", "Read, Write"),
        ("none", "None"));

    public static TheoryData<EbHostKind, string> RejectedFlagValues => Cross(AllHosts,
        "99", "read,,write", "read,garbage", "active");

    // ---------- query scalar ----------

    [Theory]
    [MemberData(nameof(AcceptedScalarSpellings))]
    public async Task Query_Should_BindMember_When_SpellingIsAccepted(EbHostKind kind, string value, string expected)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync($"/status?status={value}");

        response.Body.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(RejectedScalarValues))]
    public async Task Query_Should_Return400Problem_When_ValueIsInvalid(EbHostKind kind, string value)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync($"/status?status={value}");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_ListAllowedNamesInDescription_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?status=garbage");

        response.ShouldBeEnumBindingProblem().Single().Description
            .Should().Be("'status' must be one of: active, in_progress, http_error.");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_WriteErrorCodes_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?status=garbage");

        response.Body.Should().Contain("\"errorCodes\":[\"status\"]");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_Reject_DefinedNumber_When_AllowIntegerValuesIsFalse(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind, o => o.AllowIntegerValues = false);

        var response = await host.GetAsync("/status?status=1");

        response.ShouldBeEnumBindingProblem().Single().Code.Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_AcceptNames_When_AllowIntegerValuesIsFalse(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind, o => o.AllowIntegerValues = false);

        var response = await host.GetAsync("/status?status=in_progress");

        response.Body.Should().Be("InProgress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_Return400Problem_When_NonNullableValueIsEmpty(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?status=");

        response.ShouldBeEnumBindingProblem().Single().Code.Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_MatchKeyCaseInsensitively(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?STATUS=in_progress");

        response.Body.Should().Be("InProgress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Query_Should_BindFirstNormalizedValue_When_ScalarKeyIsRepeated(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?status=in_progress&status=active");

        response.Body.Should().Be("InProgress");
    }

    // ---------- route ----------

    [Theory]
    [MemberData(nameof(AcceptedRouteSpellings))]
    public async Task Route_Should_BindMember_When_SpellingIsAccepted(EbHostKind kind, string value, string expected)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync($"/items/{value}");

        response.Body.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Route_Should_Return400Problem_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/items/99");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("status");
    }

    // ---------- nullable ----------

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Nullable_Should_BindNull_When_KeyIsMissing(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/nullable");

        response.Body.Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Nullable_Should_BindMember_When_SnakeCase(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/nullable?status=http_error");

        response.Body.Should().Be("HTTPError");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Nullable_Should_Return400Problem_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/nullable?status=garbage");

        response.ShouldBeEnumBindingProblem().Single().Code.Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Nullable_Should_BindNull_When_ValueIsEmpty(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/nullable?status=");

        response.Body.Should().Be("null");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Nullable_Should_BindNull_When_ValueIsWhitespace(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/nullable?status=%20");

        response.Body.Should().Be("null");
    }

    // ---------- arrays / List<T> ----------

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Array_Should_BindEmpty_When_OnlyItemIsBlank(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/array?s=");

        response.Status.Should().Be(200);
        response.Body.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Array_Should_DropBlankItems_When_MixedWithValues(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/array?s=&s=active&s=");

        response.Body.Should().Be("Active");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Array_Should_BindEveryValue_When_SpellingsAreMixed(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/array?s=Active&s=in_progress&s=HTTP_ERROR&s=1");

        response.Body.Should().Be("Active|InProgress|HTTPError|InProgress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Array_Should_Return400Problem_When_OneValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/array?s=active&s=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("s");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task Array_Should_BindEmpty_When_KeyIsMissing(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/array");

        response.Body.Should().BeEmpty();
    }

    // ---------- flags ----------

    [Theory]
    [MemberData(nameof(AcceptedFlagSpellings))]
    public async Task Flags_Should_BindCombination_When_SpellingIsAccepted(EbHostKind kind, string value, string expected)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync($"/flags?perms={value}");

        response.Body.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(RejectedFlagValues))]
    public async Task Flags_Should_Return400Problem_When_ValueIsInvalid(EbHostKind kind, string value)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync($"/flags?perms={value}");

        response.ShouldBeEnumBindingProblem().Single().Description
            .Should().Be("'perms' must be one of: none, read, write, delete.");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task NonFlags_Should_Return400Problem_When_ValueIsCommaList(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/status?status=active,in_progress");

        response.ShouldBeEnumBindingProblem().Single().Code.Should().Be("status");
    }

    // ---------- renamed key / custom member name ----------

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task RenamedQueryKey_Should_BindMember_When_SnakeCase(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/renamed?st=in_progress");

        response.Body.Should().Be("InProgress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task RenamedQueryKey_Should_UseRenamedKeyAsErrorCode_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/renamed?st=garbage");

        response.ShouldBeEnumBindingProblem().Single().Code.Should().Be("st");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task RenamedQueryKey_Should_IgnoreParameterName(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/renamed?st=active&status=garbage");

        response.Body.Should().Be("Active");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task JsonStringEnumMemberName_Should_BindMember(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/custom?value=custom");

        response.Body.Should().Be("Original");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task JsonStringEnumMemberName_Should_StillAcceptCSharpMemberName(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/custom?value=original");

        response.Body.Should().Be("Original");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task JsonStringEnumMemberName_Should_BeListedInDescription_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/custom?value=garbage");

        response.ShouldBeEnumBindingProblem().Single().Description.Should().Be("'value' must be one of: custom, plain.");
    }

    // ---------- CanBind ----------

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task NonStringEnum_Should_NotBeNormalized_ByDefault(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/plain?plain=in_progress");

        response.Body.Should().NotBe("InProgress").And.NotContain("must be one of");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task NonStringEnum_Should_BeNormalized_When_CanBindAcceptsEveryEnum(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind, o => o.CanBind = t => t.IsEnum);

        var response = await host.GetAsync("/plain?plain=in_progress");

        response.Body.Should().Be("InProgress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task StringEnum_Should_NotBeNormalized_When_CanBindExcludesIt(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind, o => o.CanBind = _ => false);

        var response = await host.GetAsync("/status?status=in_progress");

        response.Body.Should().NotBe("InProgress").And.NotContain("must be one of");
    }

    // ---------- multiple errors / unaffected endpoints ----------

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task MultipleInvalidKeys_Should_ReturnOneErrorPerKey(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/multi?a=x&b=y");

        response.ShouldBeEnumBindingProblem().Select(e => e.Code).Should().Equal("a", "b");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task MultipleKeys_Should_BindEach_When_AllAreValid(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/multi?a=http_error&b=read,delete");

        response.Body.Should().Be("HTTPError|Read, Delete");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task EndpointWithoutEnum_Should_BeUnaffected(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/none?x=in_progress&status=garbage");

        response.Body.Should().Be("ok:in_progress");
    }

    [Theory]
    [MemberData(nameof(AllHostKinds))]
    public async Task UnknownRoute_Should_Return404(EbHostKind kind)
    {
        await using EnumBindingHost host = await Start(kind);

        var response = await host.GetAsync("/does-not-exist?status=garbage");

        response.Status.Should().Be(404);
    }
}
