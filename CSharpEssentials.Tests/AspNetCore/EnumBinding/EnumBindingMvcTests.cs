using FluentAssertions;
using static CSharpEssentials.Tests.AspNetCore.EnumBinding.EnumBindingAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// MVC specific enum binding behavior: <c>[FromQuery]</c> complex DTOs with enum properties.
/// </summary>
public class EnumBindingMvcTests
{
    public static TheoryData<EbHostKind> MvcHostKinds => Hosts(MvcHosts);

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_BindProperty_When_KeyIsUnprefixed(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?Status=in_progress");

        response.Body.Should().Be("InProgress|null");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_BindProperty_When_KeyIsPrefixed(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?dto.Status=http_error&dto.Optional=in_progress");

        response.Body.Should().Be("HTTPError|InProgress");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_BindNullablePropertyNull_When_ValueIsEmpty(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?status=active&optional=");

        response.Body.Should().Be("Active|null");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_UseUnprefixedKeyAsErrorCode_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?Status=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("Status");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_UsePrefixedKeyAsErrorCode_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?dto.Status=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("dto.Status");
    }

    [Fact]
    public async Task ApiController_NonStringEnum_Should_KeepFrameworkModelStateProblem()
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(EbHostKind.MvcApiController);

        var response = await host.GetAsync("/plain?plain=in_progress");

        response.Status.Should().Be(400);
        response.Body.Should().Contain("The value 'in_progress' is not valid.");
    }

    [Fact]
    public async Task Controller_NonStringEnum_Should_RunActionWithInvalidModelState()
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(EbHostKind.MvcController);

        var response = await host.GetAsync("/plain?plain=in_progress");

        response.Body.Should().Be("invalid");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_IgnoreUnprefixedKey_When_PrefixedKeyIsPresent(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?dto.Status=in_progress&Status=garbage");

        response.Body.Should().Be("InProgress|null");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Dto_Should_IgnoreUnprefixedKey_When_AnyOtherPrefixedKeyIsPresent(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/dto?dto.Optional=http_error&Status=garbage");

        response.Status.Should().Be(200);
        response.Body.Should().EndWith("|HTTPError");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Nested_Should_BindProperty_When_KeyIsUnprefixed(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/nested?inner.status=in_progress");

        response.Body.Should().Be("InProgress|null");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Nested_Should_BindProperty_When_KeyIsPrefixed(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/nested?dto.inner.status=in_progress&dto.inner.optional=http_error");

        response.Body.Should().Be("InProgress|HTTPError");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Nested_Should_UseUnprefixedPathAsErrorCode_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/nested?inner.status=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("Inner.Status");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Nested_Should_UsePrefixedPathAsErrorCode_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/nested?dto.inner.status=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("dto.Inner.Status");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task Nested_Should_IgnoreUnprefixedPath_When_PrefixedKeyIsPresent(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/nested?dto.inner.status=in_progress&inner.status=garbage");

        response.Body.Should().Be("InProgress|null");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task SelfReferencingType_Should_BindFirstLevelEnum_When_RequestIsValid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/cycle?status=http_error");

        response.Body.Should().Be("HTTPError");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task SelfReferencingType_Should_Return400Problem_When_FirstLevelValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/cycle?status=garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("Status");
    }

    [Theory]
    [MemberData(nameof(MvcHostKinds))]
    public async Task BindNeverProperty_Should_NotBeValidated_When_ValueIsInvalid(EbHostKind kind)
    {
        await using EnumBindingHost host = await EnumBindingHost.StartAsync(kind);

        var response = await host.GetAsync("/excluded?status=in_progress&hidden=garbage");

        response.Body.Should().Be("InProgress|Active");
    }
}
